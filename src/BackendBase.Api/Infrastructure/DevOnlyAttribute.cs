namespace BackendBase.Api.Infrastructure;

/// <summary>
/// Marks a controller that should only exist in the Development environment.
/// <see cref="DevOnlyControllerConvention"/> removes such controllers entirely
/// (routes and Swagger) everywhere else.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DevOnlyAttribute : Attribute
{
}
