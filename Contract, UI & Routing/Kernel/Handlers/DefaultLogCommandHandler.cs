using System;
using System.Threading.Tasks;
using SoftwareCenter.Core.Attributes; // For [HandlerPriority]
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Logs; // Needed for LogLevel enum

namespace SoftwareCenter.Kernel.Handlers
{
    /// <summary>
    /// The default fallback handler for logs. It writes to the Console.
    /// Modules can override this by registering a handler with higher priority.
    /// </summary>
    [HandlerPriority(-100)] // Very low priority to allow overrides
    public class DefaultLogCommandHandler : ICommandHandler<LogCommand>
    {
        public Task Handle(LogCommand command, ITraceContext traceContext)
        {
            var originalColor = Console.ForegroundColor;

            // Apply colors based on LogLevel
            // Note: Assuming SoftwareCenter.Core.Logs.LogLevel has standard names. 
            // We use string comparison or ToString() to be safe if we don't have the Enum definition.
            SetConsoleColor(command.Level);

            // Format: [Time] [Level] [Module] Message (TraceId)
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{timestamp}] [{command.Level}] [{command.InitiatingModuleId}] {command.Message}");

            // Print Trace ID for debugging context
            if (command.TraceId != Guid.Empty)
            {
                Console.WriteLine($"   > TraceId: {command.TraceId}");
            }

            // Print Structured Data if present
            if (command.StructuredData != null && command.StructuredData.Count > 0)
            {
                Console.WriteLine("   > Data:");
                foreach (var kvp in command.StructuredData)
                {
                    Console.WriteLine($"     - {kvp.Key}: {kvp.Value}");
                }
            }

            // Print Exception Details if present
            if (!string.IsNullOrEmpty(command.ExceptionDetails))
            {
                Console.WriteLine($"   > Exception: {command.ExceptionDetails}");
            }

            // Reset color
            Console.ForegroundColor = originalColor;
            return Task.CompletedTask;
        }

        private void SetConsoleColor(LogLevel level)
        {
            // Switch based on the string name of the enum to match your Core definition
            // Adjust these cases if your Enum names differ (e.g. "Err" instead of "Error")
            switch (level.ToString())
            {
                case "Error":
                case "Critical":
                case "Fatal":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "Warning":
                case "Warn":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case "Information":
                case "Info":
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "Debug":
                case "Trace":
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
        }
    }
}