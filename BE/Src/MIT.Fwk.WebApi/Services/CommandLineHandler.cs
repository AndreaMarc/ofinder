using DasMulli.Win32.ServiceUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Licensing;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace MIT.Fwk.WebApi.Services
{
    # nullable enable
    /// <summary>
    /// Handles command line argument parsing and execution.
    /// Modernizes the command-line handling previously in Program.Main.
    /// </summary>
    public class CommandLineHandler
    {
        private readonly ILicenseService _licenseService;
        private readonly ILogger<CommandLineHandler>? _logger;

        // Command line flags
        private const string HelpFlag = "--help";
        private const string RunAsServiceFlag = "--run-as-service";
        private const string RegisterServiceFlag = "--register-service";
        private const string UnregisterServiceFlag = "--unregister-service";
        private const string ServiceNameFlag = "--service-name";
        private const string InteractiveFlag = "--interactive";
        private const string KeyFlag = "-key";
        private const string LicenseFlag = "-lic";
        private const string ValidityFlag = "-v";

        // Service configuration
        private const string DefaultServiceName = "Maestrale WebApi Server";
        private const string DefaultServiceDescription = "Maestrale WebApi Service";

        public CommandLineHandler(ILicenseService licenseService, ILogger<CommandLineHandler>? logger = null)
        {
            _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
            _logger = logger;
        }

        /// <summary>
        /// Determines the execution mode based on command line arguments.
        /// </summary>
        public CommandLineAction DetermineAction(string[] args)
        {
            if (args.Contains(KeyFlag))
                return CommandLineAction.GenerateKey;

            if (args.Contains(LicenseFlag))
                return CommandLineAction.ActivateLicense;

            if (args.Contains(RegisterServiceFlag))
                return CommandLineAction.RegisterService;

            if (args.Contains(UnregisterServiceFlag))
                return CommandLineAction.UnregisterService;

            if (args.Contains(HelpFlag))
                return CommandLineAction.ShowHelp;

            if (args.Contains(RunAsServiceFlag))
                return CommandLineAction.RunAsService;

            return CommandLineAction.RunInteractive;
        }

        /// <summary>
        /// Handles license key generation.
        /// </summary>
        public int HandleGenerateKey(string[] args)
        {
            int keyIndex = Array.IndexOf(args, KeyFlag);
            if (keyIndex >= 0 && keyIndex + 1 < args.Length)
            {
                if (args[keyIndex + 1].Equals("Mae2019!", StringComparison.Ordinal))
                {
                    long key = _licenseService.GenerateKey();
                    Console.WriteLine($"{key}");
                    return 0;
                }
            }

            Console.WriteLine("Error: Invalid key generation command");
            Console.WriteLine("Usage: MIT.Fwk.WebApi.exe -key Mae2019!");
            return 1;
        }

        /// <summary>
        /// Handles license activation.
        /// </summary>
        public int HandleActivateLicense(string[] args)
        {
            int licIndex = Array.IndexOf(args, LicenseFlag);
            if (licIndex < 0 || licIndex + 1 >= args.Length)
            {
                Console.WriteLine("Error: License key not provided");
                Console.WriteLine("Usage: MIT.Fwk.WebApi.exe -lic <key> -v <months>");
                return 1;
            }

            if (!long.TryParse(args[licIndex + 1], out long key))
            {
                Console.WriteLine("Error: Invalid license key format");
                return 1;
            }

            int validityIndex = Array.IndexOf(args, ValidityFlag);
            if (validityIndex < 0 || validityIndex + 1 >= args.Length)
            {
                Console.WriteLine("Error: Validity period not provided");
                Console.WriteLine("Usage: MIT.Fwk.WebApi.exe -lic <key> -v <months>");
                return 1;
            }

            if (!int.TryParse(args[validityIndex + 1], out int months) || months <= 0)
            {
                Console.WriteLine("Error: Invalid validity period");
                return 1;
            }

            if (key != _licenseService.GenerateKey())
            {
                Console.WriteLine("Error: Invalid license key");
                return 1;
            }

            DateTime expirationDate = DateTime.Today.AddMonths(months);
            _licenseService.ActivateLicense(key, months);
            Console.WriteLine($"License activated successfully until {expirationDate:dd/MM/yyyy}");

            return 0;
        }

        /// <summary>
        /// Registers the application as a Windows Service.
        /// </summary>
        public int HandleRegisterService(string[] args)
        {
            (string serviceName, string displayName, string description) = GetServiceInfo(args);

            string[] filteredArgs = args
                .Where(arg => arg != RegisterServiceFlag && arg != ServiceNameFlag)
                .Select(EscapeCommandLineArgument)
                .Append(RunAsServiceFlag)
                .ToArray();

            string hostPath = Process.GetCurrentProcess().MainModule?.FileName
                              ?? throw new InvalidOperationException("Cannot determine executable path");

            // For self-contained apps, skip the dll path argument
            if (!hostPath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                filteredArgs = filteredArgs.Skip(1).ToArray();
            }

            string fullCommand = $"{hostPath} {string.Join(" ", filteredArgs)}";

            ServiceDefinition serviceDefinition = new ServiceDefinitionBuilder(serviceName)
                .WithDisplayName(displayName)
                .WithDescription(description)
                .WithBinaryPath(fullCommand)
                .WithCredentials(Win32ServiceCredentials.LocalSystem)
                .WithAutoStart(true)
                .Build();

            new Win32ServiceManager().CreateOrUpdateService(serviceDefinition, startImmediately: true);

            Console.WriteLine($"Successfully registered and started service '{displayName}' ('{description}')");
            return 0;
        }

        /// <summary>
        /// Unregisters the Windows Service.
        /// </summary>
        public int HandleUnregisterService(string[] args)
        {
            (string serviceName, string displayName, string description) = GetServiceInfo(args);

            new Win32ServiceManager().DeleteService(serviceName);

            Console.WriteLine($"Successfully unregistered service '{displayName}' ('{description}')");
            return 0;
        }

        /// <summary>
        /// Displays help information.
        /// </summary>
        public int HandleShowHelp()
        {
            Console.WriteLine(DefaultServiceDescription);
            Console.WriteLine();
            Console.WriteLine("Maestrale WebApi Service can run as a windows service or interactively.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help                    Display this help message");
            Console.WriteLine("  --interactive             Run interactively (default)");
            Console.WriteLine("  --run-as-service          Run as Windows Service");
            Console.WriteLine("  --register-service        Register and start as Windows Service");
            Console.WriteLine("      --service-name <name> Custom service name (optional)");
            Console.WriteLine("  --unregister-service      Unregister Windows Service");
            Console.WriteLine();
            Console.WriteLine("License management:");
            Console.WriteLine("  -key Mae2019!             Generate license key");
            Console.WriteLine("  -lic <key> -v <months>    Activate license for specified months");
            Console.WriteLine();

            return 0;
        }

        /// <summary>
        /// Gets service name information from args or defaults.
        /// </summary>
        private (string serviceName, string displayName, string description) GetServiceInfo(string[] args)
        {
            string serviceName = DefaultServiceName;
            string displayName = DefaultServiceName;
            string description = DefaultServiceDescription;

            int serviceNameIndex = Array.IndexOf(args, ServiceNameFlag);
            if (serviceNameIndex >= 0 && serviceNameIndex + 1 < args.Length)
            {
                string customName = args[serviceNameIndex + 1];
                serviceName = $"{DefaultServiceName} {customName}";
                displayName = $"{DefaultServiceName} {customName}";
                description = $"{DefaultServiceDescription} {customName}";
            }

            return (serviceName, displayName, description);
        }

        /// <summary>
        /// Escapes command line arguments for proper shell execution.
        /// </summary>
        private static string EscapeCommandLineArgument(string arg)
        {
            arg = Regex.Replace(arg, @"(\\*)""", @"$1$1\""");
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
    }

    /// <summary>
    /// Represents the action to take based on command line arguments.
    /// </summary>
    public enum CommandLineAction
    {
        RunInteractive,
        RunAsService,
        RegisterService,
        UnregisterService,
        GenerateKey,
        ActivateLicense,
        ShowHelp
    }
}
