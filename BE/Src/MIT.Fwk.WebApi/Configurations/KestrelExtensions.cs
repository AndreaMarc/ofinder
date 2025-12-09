using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MIT.Fwk.WebApi.Configurations
{
    # nullable enable
    /// <summary>
    /// Extension methods for configuring Kestrel web server with SSL support.
    /// Modernizes the Kestrel configuration previously in Configurator.cs.
    /// </summary>
    public static class KestrelExtensions
    {
        /// <summary>
        /// Configures Kestrel endpoints based on configuration.
        /// Supports both HTTP and HTTPS with flexible certificate configuration.
        /// </summary>
        public static IWebHostBuilder ConfigureKestrelEndpoints(
            this IWebHostBuilder builder,
            IConfiguration configuration)
        {
            bool enableSSL = configuration.GetValue<bool>("EnableSSL", false);

            // Try to read LaunchUrl as array first, then as single string
            string[]? launchUrls = configuration.GetSection("LaunchUrl").Get<string[]>();

            if (launchUrls == null || launchUrls.Length == 0)
            {
                // Fallback: try reading as single string
                var singleUrl = configuration.GetValue<string>("LaunchUrl");
                launchUrls = !string.IsNullOrEmpty(singleUrl)
                    ? new[] { singleUrl }
                    : new[] { "http://localhost:5000" };
            }

            Console.WriteLine($"Configuring Kestrel endpoints: {string.Join(", ", launchUrls)}");

            if (enableSSL)
            {
                // SSL enabled - configure Kestrel with HTTPS support
                builder.ConfigureKestrel(options =>
                {
                    ConfigureEndpointsWithSSL(options, launchUrls, configuration);
                });
            }
            else
            {
                // No SSL - use simple URL configuration
                builder.UseUrls(launchUrls);
            }

            return builder;
        }

        /// <summary>
        /// Configures Kestrel endpoints with SSL certificate support.
        /// </summary>
        private static void ConfigureEndpointsWithSSL(
            KestrelServerOptions options,
            string[] urls,
            IConfiguration configuration)
        {
            var certificatePath = configuration["Kestrel:Certificates:Default:Path"];
            var certificatePassword = configuration["Kestrel:Certificates:Default:Password"];

            foreach (string url in urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    Console.WriteLine($"Warning: Invalid URL '{url}' - skipping");
                    continue;
                }

                IPAddress ipAddress = ResolveIpAddress(uri.Host);
                int port = uri.Port;
                bool isHttps = uri.Scheme.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);

                if (isHttps)
                {
                    ConfigureHttpsEndpoint(options, ipAddress, port, certificatePath, certificatePassword);
                }
                else
                {
                    options.Listen(ipAddress, port);
                    Console.WriteLine($"Listening on HTTP: {ipAddress}:{port}");
                }
            }
        }

        /// <summary>
        /// Configures an HTTPS endpoint with certificate.
        /// </summary>
        private static void ConfigureHttpsEndpoint(
            KestrelServerOptions options,
            IPAddress ipAddress,
            int port,
            string? certificatePath,
            string? certificatePassword)
        {
            string maskedPassword = string.IsNullOrEmpty(certificatePassword)
                ? "(no password)"
                : Regex.Replace(certificatePassword, ".", "*");

            Console.WriteLine($"Loading SSL Certificate: {certificatePath ?? "(default)"} with password: {maskedPassword}");

            options.Listen(ipAddress, port, listenOptions =>
            {
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    // Use certificate from file
                    if (!string.IsNullOrEmpty(certificatePassword))
                    {
                        listenOptions.UseHttps(certificatePath, certificatePassword);
                    }
                    else
                    {
                        listenOptions.UseHttps(certificatePath);
                    }
                }
                else
                {
                    // Use default certificate (development)
                    listenOptions.UseHttps();
                }

                Console.WriteLine($"Listening on HTTPS: {ipAddress}:{port}");
            });
        }

        /// <summary>
        /// Resolves hostname to IP address for Kestrel binding.
        /// Supports: localhost, *, specific IPs, and hostnames
        /// </summary>
        private static IPAddress ResolveIpAddress(string host)
        {
            return host.ToLower() switch
            {
                "localhost" => IPAddress.Loopback,
                "*" => IPAddress.Any,
                _ => IPAddress.TryParse(host, out IPAddress? address)
                    ? address
                    : IPAddress.Loopback  // Fallback to localhost
            };
        }

        /// <summary>
        /// Configures Kestrel limits and options.
        /// </summary>
        public static IWebHostBuilder ConfigureKestrelLimits(this IWebHostBuilder builder)
        {
            builder.ConfigureKestrel(options =>
            {
                // Set max request body size (useful for file uploads)
                options.Limits.MaxRequestBodySize = 524288000; // 500 MB

                // Set request timeout
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
            });

            return builder;
        }
    }
}
