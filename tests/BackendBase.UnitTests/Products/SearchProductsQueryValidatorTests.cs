using BackendBase.Application.Products.Queries.SearchProducts;
using FluentValidation.TestHelper;

namespace BackendBase.UnitTests.Products;

public class SearchProductsQueryValidatorTests
{
    private readonly SearchProductsQueryValidator _validator = new();

    [Fact]
    public void Defaults_are_valid()
    {
        var result = _validator.TestValidate(new SearchProductsQuery());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Page_below_one_fails()
    {
        var result = _validator.TestValidate(new SearchProductsQuery(Page: 0));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageSize_out_of_range_fails(int pageSize)
    {
        var result = _validator.TestValidate(new SearchProductsQuery(PageSize: pageSize));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Unknown_sort_field_fails()
    {
        var result = _validator.TestValidate(new SearchProductsQuery(SortBy: "color"));

        result.ShouldHaveValidationErrorFor(q => q.SortBy);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("price")]
    [InlineData("createdAt")]
    public void Allowed_sort_fields_pass(string sortBy)
    {
        var result = _validator.TestValidate(new SearchProductsQuery(SortBy: sortBy));

        result.ShouldNotHaveValidationErrorFor(q => q.SortBy);
    }
}
