using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Licensing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Background service that periodically checks license validity.
    /// Replaces the old Timer-based approach with modern BackgroundService pattern.
    /// </summary>
    public class LicenseCheckService : BackgroundService
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<LicenseCheckService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(5);

        public LicenseCheckService(
            ILicenseService licenseService,
            ILogger<LicenseCheckService> logger)
        {
            _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("License Check Service is starting");

            // Initial delay before first check
            await Task.Delay(_startDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckLicenseAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not InvalidLicenseException)
                {
                    _logger.LogError(ex, "Error during license check");
                }

                // Wait for next check interval
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected when stopping
                    break;
                }
            }

            _logger.LogInformation("License Check Service is stopping");
        }

        private Task CheckLicenseAsync(CancellationToken cancellationToken)
        {
            if (!_licenseService.IsValid())
            {
                _logger.LogCritical("Invalid license detected - shutting down application");
                throw new InvalidLicenseException("License validation failed");
            }

            _logger.LogDebug("License validation successful");
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("License Check Service stop requested");
            return base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Exception thrown when license validation fails
    /// </summary>
    public class InvalidLicenseException : Exception
    {
        public InvalidLicenseException(string message) : base(message) { }
        public InvalidLicenseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
