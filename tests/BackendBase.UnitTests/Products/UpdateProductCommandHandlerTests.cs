using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Commands.UpdateProduct;
using BackendBase.Domain.Entities;
using BackendBase.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace BackendBase.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_updates_existing_product()
    {
        var id = Guid.NewGuid();
        var existing = new Product { Id = id, Name = "Old", Description = "old", Price = 1m };
        _repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new UpdateProductCommandHandler(_repository.Object, _unitOfWork.Object);
        var command = new UpdateProductCommand(id, "New", "new desc", 25m);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Name.Should().Be("New");
        response.Description.Should().Be("new desc");
        response.Price.Should().Be(25m);

        _repository.Verify(r => r.Update(existing), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_not_found_when_missing()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UpdateProductCommandHandler(_repository.Object, _unitOfWork.Object);
        var command = new UpdateProductCommand(Guid.NewGuid(), "New", null, 25m);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
