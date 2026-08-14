using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Commands.DeleteProduct;
using BackendBase.Domain.Entities;
using BackendBase.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace BackendBase.UnitTests.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_removes_existing_product()
    {
        var id = Guid.NewGuid();
        var existing = new Product { Id = id, Name = "Doomed", Price = 1m };
        _repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new DeleteProductCommandHandler(_repository.Object, _unitOfWork.Object);

        await handler.Handle(new DeleteProductCommand(id), CancellationToken.None);

        _repository.Verify(r => r.Remove(existing), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_not_found_when_missing()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new DeleteProductCommandHandler(_repository.Object, _unitOfWork.Object);

        var act = () => handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
    }
}
