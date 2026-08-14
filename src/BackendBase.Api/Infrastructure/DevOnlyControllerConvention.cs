using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BackendBase.Api.Infrastructure;

/// <summary>
/// Removes controllers marked <see cref="DevOnlyAttribute"/> from the
/// application model when not running in Development, so their endpoints don't
/// exist at all (not merely hidden) in staging/production.
/// </summary>
public class DevOnlyControllerConvention : IApplicationModelConvention
{
    private readonly bool _isDevelopment;

    public DevOnlyControllerConvention(bool isDevelopment)
    {
        _isDevelopment = isDevelopment;
    }

    public void Apply(ApplicationModel application)
    {
        if (_isDevelopment)
        {
            return;
        }

        var devOnly = application.Controllers
            .Where(c => c.Attributes.OfType<DevOnlyAttribute>().Any())
            .ToList();

        foreach (var controller in devOnly)
        {
            application.Controllers.Remove(controller);
        }
    }
}
