using BackendBase.Application.Products.Dtos;
using MediatR;

namespace BackendBase.Application.Products.Commands.CreateProduct;

/// <summary>
/// Creates a new product. Returns the created product, including its
/// server-assigned id and timestamps.
/// </summary>
/// <param name="Name">Product name. Required, 1–200 characters.</param>
/// <param name="Description">Optional description, up to 2000 characters.</param>
/// <param name="Price">Unit price. Must be zero or greater.</param>
public record CreateProductCommand(string Name, string? Description, decimal Price)
    : IRequest<ProductResponse>;
