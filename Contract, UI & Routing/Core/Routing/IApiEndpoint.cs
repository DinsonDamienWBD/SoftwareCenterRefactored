namespace SoftwareCenter.Core.Routing;

/// <summary>
/// A marker interface for a class that defines API endpoints.
/// The Kernel can use reflection to find classes implementing this interface
/// and automatically register their public methods as API routes.
/// </summary>
public interface IApiEndpoint { }