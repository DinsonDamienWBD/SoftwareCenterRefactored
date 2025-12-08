using System.Threading.Tasks;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Dispatches commands to their respective handlers.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Dispatches a command and waits for a result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="command">The command to dispatch.</param>
        /// <param name="traceContext">An optional existing trace context to continue a pipeline.</param>
        /// <returns>The result from the command handler.</returns>
        Task<TResult> Dispatch<TResult>(ICommand<TResult> command, ITraceContext traceContext = null);
    }
}
