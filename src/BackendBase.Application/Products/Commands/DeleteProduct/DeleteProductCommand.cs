using MediatR;

namespace BackendBase.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Deletes a product by id. Throws a not-found error if it does not exist.
/// </summary>
/// <param name="Id">Id of the product to delete.</param>
public record DeleteProductCommand(Guid Id) : IRequest;
