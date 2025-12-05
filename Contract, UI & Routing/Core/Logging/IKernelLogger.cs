namespace SoftwareCenter.Core.Logging;

/// <summary>
/// Defines a contract for a logger that is available early in the application's startup process (in the Kernel).
/// This can be used for logging critical startup and shutdown events before the main host and its logging providers are configured.
/// </summary>
public interface IKernelLogger
{
    void Log(string message);
}