using System.Threading.Tasks;
using SoftwareCenter.Core.Diagnostics;

namespace SoftwareCenter.Core.Jobs
{
    /// <summary>
    /// Defines the handler for a specific type of <see cref="IJob"/>.
    /// </summary>
    /// <typeparam name="TJob">The type of job this handler can execute.</typeparam>
    /// <remarks>
    /// A single job handler should be implemented for each concrete job type. The Kernel's JobScheduler
    /// will discover all implementations of this interface and invoke them according to the job's schedule.
    /// </remarks>
    public interface IJobHandler<TJob> where TJob : IJob
    {
        /// <summary>
        /// Executes the job's logic.
        /// </summary>
        /// <param name="job">The instance of the job to execute. This contains the job's configuration, like its schedule.</param>
        /// <param name="traceContext">A trace context for this specific execution, used for logging and diagnostics.</param>
        /// <returns>A task that represents the asynchronous execution of the job.</returns>
        Task ExecuteAsync(TJob job, ITraceContext traceContext);
    }
}
