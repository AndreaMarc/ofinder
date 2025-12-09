using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Implementation of ILogService using log4net.
    /// Provides structured logging with file appenders per context.
    /// </summary>
    public class LogService : ILogService
    {
        public const string DEFAULT_LOG_CONTEXT = "MIT";
        public const string DEFAULT_MONGO_CONTEXT = "LOGMONGO";

        private readonly Dictionary<string, ILog> _logInstances = new();
        private readonly string _logPath;
        private readonly string _logLevel;
        private readonly object _lock = new();

        public LogService(IOptions<DatabaseOptions> databaseOptions)
        {
            // Calculate log path (same logic as ConfigurationHelper.LogPath)
            _logPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location)?.Replace('\\', '/') + "/logs/";

            // For now, use default log level until we have a LoggingOptions class
            // TODO: Create LoggingOptions and inject via IOptions<LoggingOptions>
            _logLevel = "Debug";

            // Initialize default loggers
            InitializeDefaultLoggers();
        }

        private void InitializeDefaultLoggers()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
            hierarchy.Threshold = Level.All;

            // Default MIT logger
            Logger defaultLogger = hierarchy.LoggerFactory.CreateLogger(hierarchy, DEFAULT_LOG_CONTEXT);
            defaultLogger.Hierarchy = hierarchy;
            defaultLogger.AddAppender(CreateFileAppender(DEFAULT_LOG_CONTEXT));
            defaultLogger.Repository.Configured = true;
            defaultLogger.Level = Level.Debug;
            ILog defaultLog = new LogImpl(defaultLogger);
            _logInstances.Add(DEFAULT_LOG_CONTEXT, defaultLog);

            // MongoDB logger
            Logger mongoLogger = hierarchy.LoggerFactory.CreateLogger(hierarchy, DEFAULT_MONGO_CONTEXT);
            mongoLogger.Hierarchy = hierarchy;
            mongoLogger.AddAppender(CreateFileAppender(DEFAULT_MONGO_CONTEXT));
            mongoLogger.Repository.Configured = true;
            mongoLogger.Level = Level.All;
            ILog mongoLog = new LogImpl(mongoLogger);
            _logInstances.Add(DEFAULT_MONGO_CONTEXT, mongoLog);
        }

        private void AddContext(string logContext)
        {
            lock (_lock)
            {
                if (_logInstances.ContainsKey(logContext))
                {
                    return;
                }

                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
                hierarchy.Threshold = Level.All;

                Logger customLogger = hierarchy.LoggerFactory.CreateLogger(hierarchy, logContext);
                customLogger.Hierarchy = hierarchy;
                customLogger.AddAppender(CreateFileAppender(logContext));
                customLogger.Repository.Configured = true;

                // Set log level based on configuration
                customLogger.Level = _logLevel.ToLower() switch
                {
                    "info" => Level.Info,
                    "warn" or "warning" => Level.Warn,
                    "err" or "error" => Level.Error,
                    "debug" => Level.Debug,
                    _ => Level.All
                };

                if (logContext == DEFAULT_MONGO_CONTEXT)
                {
                    customLogger.Level = Level.All;
                }

                ILog customLog = new LogImpl(customLogger);
                _logInstances.Add(logContext, customLog);
            }
        }

        private IAppender CreateFileAppender(string contextName)
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }

            PatternLayout patternLayout = new()
            {
                ConversionPattern = "%date{HH:mm:sszz} %level %logger: %message%newline"
            };
            patternLayout.ActivateOptions();

            RollingFileAppender appender = new()
            {
                Name = contextName,
                File = $@"{_logPath}{DateTime.Today:yyyy-MM-dd}_{contextName}.log",
                AppendToFile = true,
                MaxSizeRollBackups = 1000,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaximumFileSize = "10MB",
                CountDirection = 1,
                Layout = patternLayout,
                LockingModel = new FileAppender.MinimalLock(),
                StaticLogFileName = true
            };
            appender.ActivateOptions();

            return appender;
        }

        public void Info(string message, string logContext = null)
        {
            logContext ??= DEFAULT_LOG_CONTEXT;

            if (!_logInstances.ContainsKey(logContext))
            {
                AddContext(logContext);
            }

            _logInstances[logContext].Info(message);
        }

        public void Debug(string message, string logContext = null)
        {
            logContext ??= DEFAULT_LOG_CONTEXT;

            if (!_logInstances.ContainsKey(logContext))
            {
                AddContext(logContext);
            }

            _logInstances[logContext].Debug(message);
        }

        public void Warn(string message, string logContext = null)
        {
            logContext ??= DEFAULT_LOG_CONTEXT;

            if (!_logInstances.ContainsKey(logContext))
            {
                AddContext(logContext);
            }

            _logInstances[logContext].Warn(message);
        }

        public void Error(string message, string logContext = null)
        {
            logContext ??= DEFAULT_LOG_CONTEXT;

            if (!_logInstances.ContainsKey(logContext))
            {
                AddContext(logContext);
            }

            _logInstances[logContext].Error(message);
        }

        public void Fatal(string message, string logContext = null)
        {
            logContext ??= DEFAULT_LOG_CONTEXT;

            if (!_logInstances.ContainsKey(logContext))
            {
                AddContext(logContext);
            }

            _logInstances[logContext].Fatal(message);
        }

        public void ForMongo(string message)
        {
            if (!_logInstances.ContainsKey(DEFAULT_MONGO_CONTEXT))
            {
                AddContext(DEFAULT_MONGO_CONTEXT);
            }

            _logInstances[DEFAULT_MONGO_CONTEXT].Info(message);
        }

        public void ToConsole(string message)
        {
            Console.WriteLine(message);
        }
    }
}
