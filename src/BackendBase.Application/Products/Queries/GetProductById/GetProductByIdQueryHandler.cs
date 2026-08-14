using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Dtos;
using BackendBase.Domain.Exceptions;
using MediatR;

namespace BackendBase.Application.Products.Queries.GetProductById;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/>. Throws
/// <see cref="NotFoundException"/> (→ 404) when the product does not exist.
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Domain.Entities.Product), request.Id);

        return ProductResponse.FromEntity(product);
    }
}
