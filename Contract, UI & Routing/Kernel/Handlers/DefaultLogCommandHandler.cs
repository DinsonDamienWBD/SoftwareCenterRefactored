using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Logs;
using SoftwareCenter.Core.Attributes;
using SoftwareCenter.Core.Diagnostics;
using System.Linq;

namespace SoftwareCenter.Kernel.Handlers
{
    /// <summary>
    /// Default handler for <see cref="LogCommand"/>.
    /// This handler logs messages using the standard Microsoft.Extensions.Logging infrastructure.
    /// It has a low priority so it can be overridden by more advanced logging modules.
    /// </summary>
    [HandlerPriority(-100)]
    public class DefaultLogCommandHandler : ICommandHandler<LogCommand>
    {
        private readonly ILogger<DefaultLogCommandHandler> _logger;

        public DefaultLogCommandHandler(ILogger<DefaultLogCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(LogCommand command, ITraceContext traceContext)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var logLevel = ConvertCoreLogLevelToMsLogLevel(command.Level);

            // Prepare structured log data
            var logState = new Dictionary<string, object>(command.StructuredData)
            {
                ["TraceId"] = traceContext?.TraceId ?? Guid.Empty,
                ["InitiatingModuleId"] = traceContext?.Items.TryGetValue("ModuleId", out var id) == true ? id as string : "Unknown"
            };

            if (!string.IsNullOrEmpty(command.ExceptionDetails))
            {
                logState["ExceptionDetails"] = command.ExceptionDetails;
            }

            // Log the message using the Microsoft.Extensions.Logging ILogger
            // Use _logger.Log(logLevel, message, args) or specific Log methods
            // For structured logging, we can pass state object directly or format the message
            // For simplicity and compatibility, we'll format the message and include state as an object.
            _logger.Log(logLevel, 
                        "{Message} (TraceId: {TraceId}, Module: {InitiatingModuleId}, Exception: {ExceptionDetails}, Data: {StructuredData})",
                        command.Message,
                        traceContext?.TraceId ?? Guid.Empty,
                        traceContext?.Items.TryGetValue("ModuleId", out var id2) == true ? id2 as string : "Unknown",
                        command.ExceptionDetails,
                        command.StructuredData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) // Ensure serializable
                        );
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Converts SoftwareCenter.Core.Logs.LogLevel to Microsoft.Extensions.Logging.LogLevel.
        /// </summary>
        private Microsoft.Extensions.Logging.LogLevel ConvertCoreLogLevelToMsLogLevel(SoftwareCenter.Core.Logs.LogLevel coreLogLevel)
        {
            return coreLogLevel switch
            {
                SoftwareCenter.Core.Logs.LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
                SoftwareCenter.Core.Logs.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                SoftwareCenter.Core.Logs.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                SoftwareCenter.Core.Logs.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                SoftwareCenter.Core.Logs.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                SoftwareCenter.Core.Logs.LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
                _ => Microsoft.Extensions.Logging.LogLevel.None, // Default to None or throw exception for unknown
            };
        }
    }
}