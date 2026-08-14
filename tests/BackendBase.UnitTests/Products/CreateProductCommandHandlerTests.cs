using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Commands.CreateProduct;
using BackendBase.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BackendBase.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_persists_product_and_returns_response()
    {
        Product? added = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(_repository.Object, _unitOfWork.Object);
        var command = new CreateProductCommand("  Keyboard  ", "  desc  ", 49.99m);

        var response = await handler.Handle(command, CancellationToken.None);

        // Values are trimmed and mapped through to the response.
        response.Name.Should().Be("Keyboard");
        response.Description.Should().Be("desc");
        response.Price.Should().Be(49.99m);
        response.Id.Should().NotBeEmpty();

        added.Should().NotBeNull();
        added!.Id.Should().Be(response.Id);

        // Exactly one atomic commit per request.
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
