using System.ComponentModel.DataAnnotations;

namespace BackendBase.Api.Options;

/// <summary>
/// JWT bearer settings, bound from the "Jwt" section of appsettings and
/// validated at startup. The <see cref="SigningKey"/> must never be committed —
/// supply it via user-secrets locally or a secrets manager in real environments.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (the "iss" claim). Validated on incoming tokens.</summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Intended audience (the "aud" claim). Validated on incoming tokens.</summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key. Must be at least 32 characters (256 bits).</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Lifetime, in minutes, of tokens minted by the dev token helper.</summary>
    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
