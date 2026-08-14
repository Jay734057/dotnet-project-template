using BackendBase.Application.Products.Commands.CreateProduct;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BackendBase.UnitTests.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new CreateProductCommand("Keyboard", "A nice keyboard", 49.99m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_fails(string name)
    {
        var command = new CreateProductCommand(name, null, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Name_over_200_chars_fails()
    {
        var command = new CreateProductCommand(new string('x', 201), null, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Negative_price_fails()
    {
        var command = new CreateProductCommand("Mouse", null, -0.01m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Zero_price_is_allowed()
    {
        var command = new CreateProductCommand("Freebie", null, 0m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Price);
    }
}
