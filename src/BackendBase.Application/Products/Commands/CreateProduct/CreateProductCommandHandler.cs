using BackendBase.Application.Common.Interfaces;
using BackendBase.Application.Products.Dtos;
using BackendBase.Domain.Entities;
using MediatR;

namespace BackendBase.Application.Products.Commands.CreateProduct;

/// <summary>
/// Handles <see cref="CreateProductCommand"/>: maps the command to a new
/// <see cref="Product"/>, persists it, and returns the created resource.
/// Timestamps are stamped automatically by the persistence layer.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Placeholder for real business logic (e.g. SKU generation, pricing
        // rules, domain events). The plumbing around it is production-shaped.
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
        };

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
