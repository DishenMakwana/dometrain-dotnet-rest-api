using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMovieEndpoint
{
    public const string Name = "GetAllMovie";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll,
                async ([AsParameters] GetAllMoviesRequest request, HttpContext context, IMovieService _movieService, CancellationToken token) =>
                {
                    var userId = context.GetUserId();
                    var options = request.MapToOptions()
                        .WithUser(userId);
                    
                    var movies = await _movieService.GetAllAsync(options, token);
                    var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                    
                    var moviesResponse = movies.MapToResponse(
                        request.Page.GetValueOrDefault(PagedRequest.DefaultPage), 
                        request.Page.GetValueOrDefault(PagedRequest.DefaultPageSize), movieCount);
                    
                    return TypedResults.Ok(moviesResponse);
                })
            .WithName(Name);

        return app;
    }
}