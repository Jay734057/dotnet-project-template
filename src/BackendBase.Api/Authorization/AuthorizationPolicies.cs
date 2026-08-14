using Microsoft.AspNetCore.Authorization;

namespace BackendBase.Api.Authorization;

/// <summary>
/// Central definition of the API's authorization policies and the roles that
/// satisfy them. Controllers reference these constants with
/// <c>[Authorize(Policy = ...)]</c> so authorization rules live in one place
/// rather than being scattered as inline role strings across endpoints.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Policy for read operations (GET). Any known role satisfies it.</summary>
    public const string ProductsRead = "Products.Read";

    /// <summary>Policy for write operations (POST/PUT/DELETE). Writer or Admin only.</summary>
    public const string ProductsWrite = "Products.Write";

    public static class Roles
    {
        public const string Reader = "Reader";
        public const string Writer = "Writer";
        public const string Admin = "Admin";
    }

    /// <summary>Registers every policy with the authorization system.</summary>
    public static AuthorizationOptions AddApplicationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(ProductsRead, policy =>
            policy.RequireRole(Roles.Reader, Roles.Writer, Roles.Admin));

        options.AddPolicy(ProductsWrite, policy =>
            policy.RequireRole(Roles.Writer, Roles.Admin));

        return options;
    }
}
