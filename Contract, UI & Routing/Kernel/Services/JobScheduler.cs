﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Jobs;
using SoftwareCenter.Core.Logging;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// A standard, in-memory implementation of the IJobScheduler interface for managing background jobs
    /// provided by modules. This implementation uses `System.Threading.Timer` for scheduling and supports
    /// pausing, resuming, and manual triggering of jobs.
    /// </summary>
    public class JobScheduler : IJobScheduler, IDisposable
    {
        private readonly ConcurrentDictionary<string, (IJob Job, Timer Timer, bool IsPaused)> _jobs = new();
        private readonly IKernelLogger _logger;

        public JobScheduler(IKernelLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void Register(IJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            var timer = new Timer(async _ => await ExecuteJob(job.Name), null, Timeout.Infinite, Timeout.Infinite);
            
            if (_jobs.TryRemove(job.Name, out var oldJob))
            {
                oldJob.Timer.Dispose();
            }

            _jobs[job.Name] = (job, timer, IsPaused: false);
            
            // Start the timer
            timer.Change(TimeSpan.Zero, job.Interval);
        }

        public async void TriggerAsync(string jobName)
        {
            await ExecuteJob(jobName, isManualTrigger: true);
        }

        public void Pause(string jobName)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
            {
                entry.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                _jobs[jobName] = (entry.Job, entry.Timer, IsPaused: true);
            }
        }

        public void Resume(string jobName)
        {
            if (_jobs.TryGetValue(jobName, out var entry) && entry.IsPaused)
            {
                entry.Timer.Change(TimeSpan.Zero, entry.Job.Interval);
                _jobs[jobName] = (entry.Job, entry.Timer, IsPaused: false);
            }
        }

        private async Task ExecuteJob(string jobName, bool isManualTrigger = false)
        {
            if (_jobs.TryGetValue(jobName, out var entry))
            {
                // If it's a scheduled run (not manual) and the job is paused, skip execution.
                if (!isManualTrigger && entry.IsPaused) return;

                try
                {
                    // Create a new context for each run
                    var context = new JobContext
                    {
                        Trace = new TraceContext(),
                        CancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token // Example timeout
                    };
                    await entry.Job.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogExceptionAsync(ex, $"An unhandled exception occurred in job '{jobName}'.");
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var key in _jobs.Keys)
            {
                if (_jobs.TryRemove(key, out var entry))
                {
                    entry.Timer.Dispose();
                }
            }
            _jobs.Clear();
        }
    }
}