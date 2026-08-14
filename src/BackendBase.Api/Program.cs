using System.Reflection;
using System.Text;
using BackendBase.Api.Authorization;
using BackendBase.Api.Infrastructure;
using BackendBase.Api.Middleware;
using BackendBase.Api.Options;
using BackendBase.Api.Security;
using BackendBase.Application;
using BackendBase.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- Strongly-typed configuration (Options pattern), validated on startup ----
builder.Services
    .AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName));

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var apiOptions = builder.Configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

// --- Application + Infrastructure layers -------------------------------------
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Dev-only helper used by DevTokenController (which is itself dev-only).
builder.Services.AddSingleton<JwtTokenService>();

// --- Authentication (JWT bearer) ---------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// --- Authorization (policy-based) --------------------------------------------
builder.Services.AddAuthorization(options => options.AddApplicationPolicies());

// --- MVC / controllers -------------------------------------------------------
builder.Services.AddControllers(mvc =>
    mvc.Conventions.Add(new DevOnlyControllerConvention(builder.Environment.IsDevelopment())));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(apiOptions.Version, new OpenApiInfo
    {
        Title = apiOptions.Title,
        Version = apiOptions.Version,
        Description =
            "Base backend API (Clean Architecture + CQRS). Product catalog with CRUD, " +
            "name search (paged & sorted), FluentValidation, and JWT/role-based authorization.\n\n" +
            "**Getting a token (local dev):** call `POST /api/dev/token` to mint a JWT, then click " +
            "**Authorize** above and paste the `accessToken`. Read endpoints need any role; " +
            "write endpoints need `Writer` or `Admin`.",
    });

    // Pull XML doc comments from every layer so DTOs, commands, and endpoints
    // are documented in Swagger for frontend consumers.
    foreach (var xml in new[]
             {
                 "BackendBase.Api.xml",
                 "BackendBase.Application.xml",
                 "BackendBase.Domain.xml",
             })
    {
        var path = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(path))
        {
            options.IncludeXmlComments(path, includeControllerXmlComments: true);
        }
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT here (without the 'Bearer ' prefix).",
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>(),
    });
});

var app = builder.Build();

// Global exception handling: the one place exceptions become HTTP responses.
app.UseExceptionHandling();

// Swagger is dev-only by design: it documents internal contracts that
// shouldn't be discoverable outside a developer's machine.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint($"/swagger/{apiOptions.Version}/swagger.json", $"{apiOptions.Title} {apiOptions.Version}");
        ui.DocumentTitle = apiOptions.Title;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program>-based integration tests can spin
// this API up in-memory once the test project grows past unit tests.
public partial class Program
{
}
