using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Errors;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Logs;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Default implementation of <see cref="IErrorHandler"/> that logs errors
    /// by dispatching a <see cref="LogCommand"/> through the command bus.
    /// </summary>
    public class DefaultErrorHandler : IErrorHandler
    {
        private readonly ICommandBus _commandBus;

        public DefaultErrorHandler(ICommandBus commandBus)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
        }

        public async Task HandleError(Exception exception, ITraceContext traceContext, string message = null, bool isCritical = false)
        {
            var logLevel = isCritical ? LogLevel.Critical : LogLevel.Error;
            var logMessage = message ?? "An unhandled error occurred.";

            var structuredData = new Dictionary<string, object>
            {
                { "ExceptionType", exception?.GetType().FullName },
                { "ExceptionMessage", exception?.Message },
                { "StackTrace", exception?.StackTrace }
            };

            var logCommand = new LogCommand(logLevel, logMessage, traceContext, exception, structuredData);

            await _commandBus.Dispatch(logCommand, traceContext);
        }
    }
}
