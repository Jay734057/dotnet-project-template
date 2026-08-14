namespace BackendBase.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist. The API's exception-handling
/// middleware maps this to an HTTP 404 response, so handlers can simply throw
/// it instead of returning nullable results and forcing every caller to
/// null-check.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Convenience factory producing a consistent "&lt;entity&gt; with id
    /// &lt;key&gt; was not found." message.
    /// </summary>
    public static NotFoundException For(string entityName, object key) =>
        new($"{entityName} with id '{key}' was not found.");
}
