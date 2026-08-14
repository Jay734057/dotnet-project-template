namespace BackendBase.Api.Options;

/// <summary>
/// General API settings, bound from the "Api" section of appsettings. Example of
/// the strongly-typed Options pattern this project expects for every config
/// value — add new settings here, never as raw <c>IConfiguration["Key"]</c> lookups.
/// </summary>
public class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>Human-readable API title shown in Swagger.</summary>
    public string Title { get; set; } = "BackendBase API";

    /// <summary>API version string shown in Swagger.</summary>
    public string Version { get; set; } = "v1";
}
