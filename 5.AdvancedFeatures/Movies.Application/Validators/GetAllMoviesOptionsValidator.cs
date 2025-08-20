using FluentValidation;

namespace Movies.Application.Validators;
using Movies.Application.Models;

public class GetAllMoviesOptionsValidator: AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptableSortFields =
    {
        "title",
        "yearofrelease",
    };
    
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);
        
        RuleFor(x => x.SortField).Must(x => x is null || AcceptableSortFields.Contains(x.ToLowerInvariant()))
            .WithMessage($"Sort field must be one of: {string.Join(", ", AcceptableSortFields)}");
    }
}