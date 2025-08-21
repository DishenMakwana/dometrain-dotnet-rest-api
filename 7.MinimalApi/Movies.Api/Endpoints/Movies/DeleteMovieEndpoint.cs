using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";
    
    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete,
                async (Guid id, IMovieService _movieService, IOutputCacheStore _outputCacheStore, CancellationToken token) =>
                {
                    var deleted = await _movieService.DeleteByIdAsync(id, token);
                    if (!deleted)
                    {
                        return Results.NotFound();
                    }

                    await _outputCacheStore.EvictByTagAsync("movies", token);
                    return TypedResults.Ok();
                })
            .WithName(Name);

        return app;
    }
}