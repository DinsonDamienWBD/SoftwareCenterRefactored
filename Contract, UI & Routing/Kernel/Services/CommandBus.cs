using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// A command bus that uses the .NET dependency injection container to find and execute command handlers.
    /// </summary>
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBus"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
        public CommandBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public Task<TResult> Dispatch<TResult>(ICommand<TResult> command, ITraceContext traceContext = null)
        {
            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"No handler found for command type {commandType.Name}");
            }

            // Create a new trace context if one is not provided.
            var context = traceContext ?? new TraceContext();

            // Use dynamic invocation to call the Handle method with the command and context.
            return (Task<TResult>)((dynamic)handler).Handle((dynamic)command, context);
        }
    }
}
