using System;

namespace SoftwareCenter.Core.Jobs
{
    /// <summary>
    /// Represents a background job that can be discovered and executed by the Kernel on a recurring schedule.
    /// </summary>
    /// <remarks>
    /// Jobs are stateless by design. Any required parameters or configuration should be provided
    /// to its corresponding <see cref="IJobHandler"/>. State should be managed externally,
    /// for example in the Global Data Store.
    /// </remarks>
    public interface IJob
    {
        /// <summary>
        /// Gets the schedule for the job, defined as a CRON expression.
        /// The CRON expression determines when the job will be triggered.
        /// For example, "0 0 * * *" runs the job every day at midnight.
        /// </summary>
        string CronExpression { get; }
    }
}
