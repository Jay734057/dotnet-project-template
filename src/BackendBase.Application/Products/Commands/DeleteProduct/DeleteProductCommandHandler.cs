using BackendBase.Application.Common.Interfaces;
using BackendBase.Domain.Exceptions;
using MediatR;

namespace BackendBase.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Handles <see cref="DeleteProductCommand"/>: loads the product and removes it.
/// Throws <see cref="NotFoundException"/> (→ 404) when the product does not exist.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Domain.Entities.Product), request.Id);

        _repository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
