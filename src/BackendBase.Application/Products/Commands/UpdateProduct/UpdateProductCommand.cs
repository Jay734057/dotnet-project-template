using BackendBase.Application.Products.Dtos;
using MediatR;

namespace BackendBase.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Updates an existing product in full (PUT semantics). Returns the updated
/// product. Throws a not-found error if no product with <paramref name="Id"/>
/// exists.
/// </summary>
/// <param name="Id">Id of the product to update.</param>
/// <param name="Name">New product name. Required, 1–200 characters.</param>
/// <param name="Description">New description, up to 2000 characters.</param>
/// <param name="Price">New unit price. Must be zero or greater.</param>
public record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price)
    : IRequest<ProductResponse>;
