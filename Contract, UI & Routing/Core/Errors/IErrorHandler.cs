using System;
using System.Threading.Tasks;
using SoftwareCenter.Core.Diagnostics;

namespace SoftwareCenter.Core.Errors
{
    /// <summary>
    /// Defines a contract for handling errors that occur within the application.
    /// This allows for centralized and extensible error reporting and processing.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handles a reported error.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="traceContext">The trace context associated with the operation where the error occurred.</param>
        /// <param name="message">An optional, user-friendly message describing the error.</param>
        /// <param name="isCritical">Indicates if the error is critical and might require immediate attention or application shutdown.</param>
        /// <returns>A Task representing the asynchronous error handling operation.</returns>
        Task HandleError(Exception exception, ITraceContext traceContext, string message = null, bool isCritical = false);
    }
}
