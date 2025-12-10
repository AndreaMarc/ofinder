using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Helpers;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MIT.Fwk.Scheduler
{
    # nullable enable
    /// <summary>
    /// Responsible for starting and gracefully stopping the Quartz.NET scheduler.
    /// </summary>
    public class QuartzStartup
    {
        private IScheduler? _scheduler; // after Start, and until shutdown completes, references the scheduler object
        private readonly IServiceCollection _services;
        private readonly QuartzSchedulerOptions _options;

        public QuartzStartup(IServiceCollection services, IOptions<QuartzSchedulerOptions> options)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Starts the scheduler, defines the jobs and the triggers
        /// </summary>
        public void Start()
        {
            if (_scheduler != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            NameValueCollection properties = new()
            {
                // json serialization is the one supported under .NET Core (binary isn't)
                ["quartz.serializer.type"] = _options.SerializerType,
            };

            StdSchedulerFactory schedulerFactory = new(properties);
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.Start().Wait();

            List<object> schedules = ReflectionHelper.ResolveAll<IScheduled>();
            string[] schedulesToStart = _options.ToStart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (IScheduled sched in schedules)
            {
                // Fire only for jobs in the ToStart property
                if (!schedulesToStart.Contains(sched.ScheduleName(), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                sched.ConfigureServices(_services);

                IJobDetail jobDetail = JobBuilder.Create()
                    .OfType(sched.JobType())
                    .WithIdentity(sched.ScheduleName())
                    .Build();

                ITrigger trigger = CreateTrigger(sched);

                _scheduler.ScheduleJob(jobDetail, trigger).Wait();
            }
        }

        /// <summary>
        /// Initiates shutdown of the scheduler, and waits until jobs exit gracefully (within allotted timeout)
        /// </summary>
        public void Stop()
        {
            if (_scheduler == null)
            {
                return;
            }

            // give running jobs configured timeout (default 30 sec) to stop gracefully
            if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(_options.ShutdownTimeoutMs))
            {
                _scheduler = null;
            }
            else
            {
                // jobs didn't exit in timely fashion - log a warning...
                Console.WriteLine($"Warning: Scheduler shutdown timed out after {_options.ShutdownTimeoutMs}ms. Some jobs may not have completed gracefully.");
            }
        }

        #region Private Helper Methods - Trigger Factory

        private ITrigger CreateTrigger(IScheduled sched)
        {
            return sched switch
            {
                ICronScheduled cronScheduled => CreateCronTrigger(cronScheduled),
                ISimpleScheduled simpleScheduled => CreateSimpleTrigger(simpleScheduled),
                _ => throw new InvalidOperationException($"Unknown scheduled job type: {sched.GetType().Name}")
            };
        }

        private ITrigger CreateCronTrigger(ICronScheduled job)
        {
            TriggerBuilder builder = TriggerBuilder.Create()
                .WithIdentity(job.ScheduleName())
                .WithCronSchedule(job.GetCronOptions());

            if (job.StartNow())
            {
                builder.StartNow();
            }
            else
            {
                builder.StartAt(job.StartAt());
            }

            return builder.Build();
        }

        private ITrigger CreateSimpleTrigger(ISimpleScheduled job)
        {
            TriggerBuilder builder = TriggerBuilder.Create()
                .WithIdentity(job.ScheduleName());

            // Configure schedule
            if (job.RepeatForever())
            {
                builder.WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(job.WithIntervalInMinutes())
                    .RepeatForever());
            }
            else
            {
                builder.WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(job.WithIntervalInMinutes())
                    .WithRepeatCount(job.WithRepeatCount()));
            }

            // Configure start time
            if (job.StartNow())
            {
                builder.StartNow();
            }
            else
            {
                builder.StartAt(job.StartAt());
            }

            return builder.Build();
        }

        #endregion
    }
}
