using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        var result = await connection.ExecuteAsync(new CommandDefinition("insert into movies (id, title, slug, yearofrelease) values (@Id, @Title, @Slug, @YearOfRelease);", movie, cancellationToken:token));
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("insert into genres (movieId, name) values (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre }, cancellationToken:token));
            }
        }
        
        transaction.Commit();
        
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        
        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("select id, title, slug, yearofrelease from movies where id = @id;", new { id }, cancellationToken:token));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("select name from genres where movieId = @id;", new { id }, cancellationToken:token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        
        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("select id, title, slug, yearofrelease from movies where slug = @slug;", new { slug }, cancellationToken:token));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("select name from genres where movieId = @id;", new { id = movie.Id }, cancellationToken:token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.QueryAsync(new CommandDefinition("select m.*, string_agg(g.name, ',') as genres from movies m left join genres g on m.id = g.movieid group by id", cancellationToken:token));
        
        return result.Select(row => new Movie
        {
            Id = row.id,
            Title = row.title,
            YearOfRelease = row.yearofrelease,
            Genres = Enumerable.ToList(row.genres?.Split(',')),
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        // check if exist or not
        var existMovie = await ExistByIdAsync(movie.Id);
        
        if(!existMovie)
        {
            return false;
        }
        
        await connection.ExecuteAsync(new CommandDefinition("delete from genres where movieId = @MovieId;", new { MovieId = movie.Id }, cancellationToken:token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("insert into genres (movieId, name) values (@MovieId, @Name);", new { MovieId = movie.Id, Name = genre }, cancellationToken:token));
        }
        
        var result = await connection.ExecuteAsync(new CommandDefinition("update movies set title = @Title, slug = @Slug, yearofrelease = @YearOfRelease where id = @Id;", movie, cancellationToken:token));
        transaction.Commit();
        
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
       await connection.ExecuteAsync(new CommandDefinition("delete from genres where movieId = @id;", new { id }, cancellationToken:token));
       var result = await connection.ExecuteAsync(new CommandDefinition("delete from movies where id = @id;", new { id }, cancellationToken:token));
       
       transaction.Commit();
       return result > 0;
    }

    public async Task<bool> ExistByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("select count(1) from movies where id = @id;", new {id}, cancellationToken:token));
    }
}
