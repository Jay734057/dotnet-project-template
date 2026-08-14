using FluentValidation;

namespace BackendBase.Application.Products.Queries.SearchProducts;

/// <summary>
/// Validates paging and sorting inputs for <see cref="SearchProductsQuery"/> so
/// malformed requests fail fast with a 400 instead of returning surprising
/// results or letting an unknown sort field through.
/// </summary>
public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, SearchProductsQuery.MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {SearchProductsQuery.MaxPageSize}.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => SearchProductsQuery.AllowedSortFields.Contains(sortBy.ToLowerInvariant()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SearchProductsQuery.AllowedSortFields)}.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name filter must not exceed 200 characters.");
    }
}
