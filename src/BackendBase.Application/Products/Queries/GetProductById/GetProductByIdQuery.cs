using BackendBase.Application.Products.Dtos;
using MediatR;

namespace BackendBase.Application.Products.Queries.GetProductById;

/// <summary>
/// Fetches a single product by id. Throws a not-found error if it does not exist.
/// </summary>
/// <param name="Id">Id of the product to fetch.</param>
public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse>;
