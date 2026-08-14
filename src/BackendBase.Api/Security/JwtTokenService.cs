using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendBase.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BackendBase.Api.Security;

/// <summary>
/// Mints signed JWTs from <see cref="JwtOptions"/>. Used by the dev token helper
/// so the secured endpoints can be exercised without a full identity provider.
/// In a real deployment, tokens would instead be issued by your identity
/// provider (Entra ID, Auth0, IdentityServer, …) and this service removed.
/// </summary>
public class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>Creates a signed JWT for the given subject and roles.</summary>
    /// <returns>The encoded token string and its UTC expiry.</returns>
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(string subject, IEnumerable<string> roles)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
