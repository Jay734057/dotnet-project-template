using BackendBase.Api.Authorization;
using BackendBase.Application.Common.Models;
using BackendBase.Application.Products.Commands.CreateProduct;
using BackendBase.Application.Products.Commands.DeleteProduct;
using BackendBase.Application.Products.Commands.UpdateProduct;
using BackendBase.Application.Products.Dtos;
using BackendBase.Application.Products.Queries.GetProductById;
using BackendBase.Application.Products.Queries.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBase.Api.Controllers;

/// <summary>
/// CRUD and search endpoints for products. Controllers stay thin on purpose:
/// each action only dispatches a Command/Query through MediatR and maps the
/// result to an HTTP status code. All business logic lives in the Application
/// layer handlers. Authorization is enforced per-action via policies
/// (see <see cref="AuthorizationPolicies"/>).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>Searches products by name with paging and sorting.</summary>
    /// <remarks>
    /// Requires a token with any known role (Reader, Writer, or Admin).
    /// Omit <c>name</c> to list all products. Results are paged; the response
    /// includes total counts so clients can render pagination.
    /// </remarks>
    /// <response code="200">A page of matching products.</response>
    /// <response code="400">Invalid paging or sorting parameters.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ProductsRead)]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> Search(
        [FromQuery] SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single product by its id.</summary>
    /// <remarks>Requires a token with any known role (Reader, Writer, or Admin).</remarks>
    /// <param name="id">The product's unique identifier.</param>
    /// <response code="200">The requested product.</response>
    /// <response code="404">No product exists with that id.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsRead)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(product);
    }

    /// <summary>Creates a new product.</summary>
    /// <remarks>Requires a token with the Writer or Admin role.</remarks>
    /// <response code="201">The product was created; the body is the created product.</response>
    /// <response code="400">The request body failed validation.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Updates an existing product in full.</summary>
    /// <remarks>Requires a token with the Writer or Admin role. The route id must match the body id.</remarks>
    /// <param name="id">The id of the product to update.</param>
    /// <param name="command">The new product values.</param>
    /// <response code="200">The updated product.</response>
    /// <response code="400">The body failed validation, or the route id does not match the body id.</response>
    /// <response code="404">No product exists with that id.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid id,
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id does not match the request body id.");
        }

        var product = await _mediator.Send(command, cancellationToken);
        return Ok(product);
    }

    /// <summary>Deletes a product by id.</summary>
    /// <remarks>Requires a token with the Writer or Admin role.</remarks>
    /// <param name="id">The id of the product to delete.</param>
    /// <response code="204">The product was deleted.</response>
    /// <response code="404">No product exists with that id.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProductsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
