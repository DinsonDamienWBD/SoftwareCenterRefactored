using System.Threading.Tasks;
using SoftwareCenter.Core.Diagnostics;

namespace SoftwareCenter.Core.Commands
{
    /// <summary>
    /// Defines a handler for a command that does not return a value.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be handled.</typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handles a command asynchronously.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="traceContext">The context for tracing the operation.</param>
        /// <returns>A task that represents the asynchronous handling process.</returns>
        Task Handle(TCommand command, ITraceContext traceContext);
    }

    /// <summary>
    /// Defines a handler for a command that returns a value.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be handled.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Handles a command asynchronously.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="traceContext">The context for tracing the operation.</param>
        /// <returns>A task that represents the asynchronous handling process, which returns a result of type <typeparamref name="TResult"/>.</returns>
        Task<TResult> Handle(TCommand command, ITraceContext traceContext);
    }
}
