using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Scheduler.Services;
using Quartz;
using System;
using System.Threading.Tasks;

namespace MIT.Fwk.Scheduler.Manager
{
    # nullable enable
    /// <summary>
    /// Quartz job for processing log files and saving them to MongoDB
    /// </summary>
    public class MongoLogBusManager : IJob, ICronScheduled
    {
        private readonly IServiceCollection _services;
        private ILogFileProcessor? _logFileProcessor;
        private IConfiguration? _configuration;
        private MongoLogOptions? _options;
        private ILogger<MongoLogBusManager>? _logger;

        public MongoLogBusManager()
        {
            _services = new ServiceCollection();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ServiceProvider sp = services.BuildServiceProvider();

            _configuration = sp.GetService<IConfiguration>();
            _logger = sp.GetService<ILogger<MongoLogBusManager>>();

            // Register MongoLog processing services
            services.AddSingleton(_configuration!);
            services.AddSingleton(sp.GetService<IOptions<MongoLogOptions>>()!);
            services.AddSingleton<ILogFileReader, LogFileReader>();
            services.AddSingleton<ILogLineParser, LogLineParser>();
            services.AddSingleton<ILogTransformer, LogTransformer>();
            services.AddSingleton<IMongoLogRepository, MongoLogRepository>();
            services.AddSingleton<ILogFileProcessor, LogFileProcessor>();

            // Rebuild service provider with new services
            sp = services.BuildServiceProvider();
            _logFileProcessor = sp.GetRequiredService<ILogFileProcessor>();
            _options = sp.GetService<IOptions<MongoLogOptions>>()?.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string jobName = context.JobDetail.Key.Name;
            _logger?.LogInformation("Executing job: {JobName}", jobName);

            if (_logFileProcessor == null)
            {
                _logger?.LogError("LogFileProcessor not initialized. Cannot execute job.");
                return;
            }

            try
            {
                await _logFileProcessor.ProcessLogsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing MongoLogBusManager job");
                throw;
            }
        }

        public string GetCronOptions()
        {
            return _options?.CronExpression
                ?? _configuration?["Scheduler:QueueManager"]
                ?? "0 0/5 * * * ?"; // Default: every 5 minutes
        }

        public Type JobType()
        {
            return typeof(MongoLogBusManager);
        }

        public string ScheduleName()
        {
            return nameof(MongoLogBusManager);
        }

        public DateTimeOffset StartAt()
        {
            throw new NotImplementedException();
        }

        public bool StartNow()
        {
            return true;
        }
    }
}
