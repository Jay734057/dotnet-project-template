using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Dtos;
using BackendBase.Domain.Exceptions;
using MediatR;

namespace BackendBase.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Handles <see cref="UpdateProductCommand"/>: loads the product, applies the
/// new values, and persists them. Throws <see cref="NotFoundException"/> (→ 404)
/// when the product does not exist.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Domain.Entities.Product), request.Id);

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;

        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
