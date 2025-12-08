using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Jobs;

namespace SoftwareCenter.Kernel.Services
{
    public class JobSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ModuleLoader _moduleLoader;
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly List<JobRunner> _activeJobs = new List<JobRunner>();

        public JobSchedulerService(
            IServiceProvider serviceProvider,
            ModuleLoader moduleLoader,
            ILogger<JobSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _moduleLoader = moduleLoader;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Discovery Phase: Find all types implementing IJob
            DiscoverJobs();

            _logger.LogInformation($"JobScheduler started. Managing {_activeJobs.Count} jobs.");

            // 2. Execution Loop
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow;

                foreach (var runner in _activeJobs)
                {
                    if (runner.ShouldRun(now))
                    {
                        // Fire and forget - don't await the job so we don't block the scheduler loop
                        _ = ExecuteJobAsync(runner, stoppingToken);
                    }
                }

                // Check schedule every minute
                // Align to the start of the next minute for cleaner scheduling
                var delay = TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(now.Second);
                await Task.Delay(delay, stoppingToken);
            }
        }

        private void DiscoverJobs()
        {
            var assemblies = _moduleLoader.GetLoadedAssemblies();
            var jobInterface = typeof(IJob);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var jobTypes = assembly.GetTypes()
                        .Where(t => jobInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in jobTypes)
                    {
                        try
                        {
                            // Create a singleton instance of the job definition to access the CronExpression
                            var jobInstance = (IJob)Activator.CreateInstance(type);
                            _activeJobs.Add(new JobRunner(jobInstance, type));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to register job '{type.Name}'.");
                        }
                    }
                }
                catch { /* Ignore assembly load errors */ }
            }
        }

        private async Task ExecuteJobAsync(JobRunner runner, CancellationToken token)
        {
            runner.MarkRunning();

            using (var scope = _serviceProvider.CreateScope())
            {
                var traceId = Guid.NewGuid();
                var traceContext = new TraceContext { TraceId = traceId };

                try
                {
                    _logger.LogInformation($"[Scheduler] Starting job '{runner.JobType.Name}' (TraceId: {traceId})");

                    // Dynamic resolution of IJobHandler<TJob>
                    var handlerType = typeof(IJobHandler<>).MakeGenericType(runner.JobType);
                    var handler = scope.ServiceProvider.GetService(handlerType);

                    if (handler != null)
                    {
                        // Invoke ExecuteAsync(TJob job, ITraceContext traceContext)
                        var method = handlerType.GetMethod("ExecuteAsync");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, new object[] { runner.JobInstance, traceContext });
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[Scheduler] No handler registered for job '{runner.JobType.Name}'.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Scheduler] Error executing job '{runner.JobType.Name}' (TraceId: {traceId})");
                }
                finally
                {
                    runner.MarkComplete();
                }
            }
        }

        /// <summary>
        /// Internal wrapper to track job state and schedule.
        /// </summary>
        private class JobRunner
        {
            public IJob JobInstance { get; }
            public Type JobType { get; }
            public DateTimeOffset LastRun { get; private set; } = DateTimeOffset.MinValue;
            public bool IsRunning { get; private set; }

            private readonly SimpleCronParser _cronParser;

            public JobRunner(IJob job, Type type)
            {
                JobInstance = job;
                JobType = type;
                _cronParser = new SimpleCronParser(job.CronExpression);
            }

            public bool ShouldRun(DateTimeOffset now)
            {
                if (IsRunning) return false; // Prevent overlapping execution

                // Check if the current minute matches the Cron expression
                // and we haven't already run in this minute.
                bool matchesSchedule = _cronParser.IsMatch(now);
                bool notRunThisMinute = LastRun.Minute != now.Minute || (now - LastRun).TotalMinutes >= 1;

                return matchesSchedule && notRunThisMinute;
            }

            public void MarkRunning() => IsRunning = true;

            public void MarkComplete()
            {
                IsRunning = false;
                LastRun = DateTimeOffset.UtcNow;
            }
        }
    }
}