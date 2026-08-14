using BackendBase.Api.Authorization;
using BackendBase.Api.Infrastructure;
using BackendBase.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace BackendBase.Api.Controllers;

/// <summary>
/// DEVELOPMENT-ONLY helper for minting test JWTs so the secured endpoints can be
/// tried out in Swagger without a real identity provider. It is only mapped when
/// the app runs in the Development environment (see Program.cs) and performs NO
/// authentication — it hands out whatever roles you ask for. Delete this
/// controller (and <see cref="JwtTokenService"/>) once a real token issuer is
/// wired in.
/// </summary>
[ApiController]
[Route("api/dev/token")]
[Tags("Dev (local only)")]
[DevOnly]
public class DevTokenController : ControllerBase
{
    private readonly JwtTokenService _tokenService;

    public DevTokenController(JwtTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>Mints a development JWT for the requested roles.</summary>
    /// <remarks>
    /// Copy the returned <c>accessToken</c> into Swagger's <b>Authorize</b>
    /// dialog (just the token — the UI adds the "Bearer " prefix).
    /// Valid roles: <c>Reader</c>, <c>Writer</c>, <c>Admin</c>.
    /// </remarks>
    /// <response code="200">A signed development token.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DevTokenResponse), StatusCodes.Status200OK)]
    public ActionResult<DevTokenResponse> CreateToken(DevTokenRequest request)
    {
        var roles = request.Roles is { Length: > 0 }
            ? request.Roles
            : new[] { AuthorizationPolicies.Roles.Admin };

        var (token, expiresAt) = _tokenService.CreateToken(request.Subject ?? "dev-user", roles);

        return Ok(new DevTokenResponse(token, expiresAt, roles));
    }
}

/// <summary>Request body for the dev token helper.</summary>
/// <param name="Subject">Optional subject ("sub" claim). Defaults to "dev-user".</param>
/// <param name="Roles">Roles to embed. Defaults to ["Admin"] when omitted.</param>
public record DevTokenRequest(string? Subject = null, string[]? Roles = null);

/// <summary>Response from the dev token helper.</summary>
/// <param name="AccessToken">The signed JWT to paste into the Authorize dialog.</param>
/// <param name="ExpiresAt">UTC expiry of the token.</param>
/// <param name="Roles">The roles embedded in the token.</param>
public record DevTokenResponse(string AccessToken, DateTimeOffset ExpiresAt, string[] Roles);
