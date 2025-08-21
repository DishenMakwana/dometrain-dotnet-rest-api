namespace Movies.Contracts.Requests;

public class PagedRequest
{
    public const int DefaultPageSize = 10;
    public const int DefaultPage = 1;
    
    public int? Page { get; init; } = DefaultPage;

    public int? PageSize { get; init; } = DefaultPageSize;
}
