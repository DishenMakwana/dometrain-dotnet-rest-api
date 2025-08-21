using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update,
            async (Guid id, UpdateMovieRequest request, HttpContext context, IMovieService _movieService, IOutputCacheStore _outputCacheStore,
                CancellationToken token) =>
            {
                var movie = request.MapToMovie(id);
                var userId = context.GetUserId();
                var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
                if (updatedMovie is null)
                {
                    return Results.NotFound();
                }

                await _outputCacheStore.EvictByTagAsync("movies", token);
                var response = updatedMovie.MapToResponse();
                return TypedResults.Ok(response);
            }).WithName(Name);

        return app;
    }
}