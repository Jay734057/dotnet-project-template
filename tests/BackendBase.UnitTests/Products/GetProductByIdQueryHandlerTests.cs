using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Queries.GetProductById;
using BackendBase.Domain.Entities;
using BackendBase.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace BackendBase.UnitTests.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _repository = new();

    [Fact]
    public async Task Handle_returns_product_when_found()
    {
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = id, Name = "Monitor", Price = 199m });

        var handler = new GetProductByIdQueryHandler(_repository.Object);

        var response = await handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Monitor");
    }

    [Fact]
    public async Task Handle_throws_not_found_when_missing()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new GetProductByIdQueryHandler(_repository.Object);

        var act = () => handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
