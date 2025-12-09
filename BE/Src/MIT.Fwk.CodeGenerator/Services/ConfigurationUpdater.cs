using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Updates configuration files (appsettings.json and dbconnections.json).
    /// </summary>
    public class ConfigurationUpdater
    {
        private const string AppSettingsPath = @"..\..\..\..\MIT.Fwk.WebApi\appsettings.json";
        private const string DbConnectionsPath = @"..\..\..\..\MIT.Fwk.WebApi\dbconnections.json";

        /// <summary>
        /// Updates appsettings.json with the new DbContext provider configuration.
        /// </summary>
        public bool UpdateAppSettings(string dbName, DatabaseEngine engine)
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    Console.WriteLine($"Warning: {AppSettingsPath} not found. Skipping appsettings update.");
                    return false;
                }

                // Read and parse JSON
                string json = File.ReadAllText(AppSettingsPath);
                JObject settings = JObject.Parse(json);

                // Add or update provider setting
                string providerValue = engine == DatabaseEngine.SqlServer ? "Sql" : "MySql";
                string contextKey = $"{dbName}DbContext";

                settings[contextKey] = providerValue;

                // Write back with pretty formatting
                string updatedJson = settings.ToString(Formatting.Indented);
                File.WriteAllText(AppSettingsPath, updatedJson);

                Console.WriteLine($"  ✓ Added to appsettings.json: \"{contextKey}\": \"{providerValue}\"");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appsettings.json: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates dbconnections.json with the new connection string.
        /// </summary>
        public bool UpdateDbConnections(string dbName, string connectionString)
        {
            try
            {
                if (!File.Exists(DbConnectionsPath))
                {
                    Console.WriteLine($"Warning: {DbConnectionsPath} not found. Creating new file.");

                    // Create new dbconnections.json structure
                    var newConfig = new JObject
                    {
                        ["ConnectionStrings"] = new JObject
                        {
                            [$"{dbName}DbContext"] = connectionString
                        }
                    };

                    string newJson = newConfig.ToString(Formatting.Indented);
                    File.WriteAllText(DbConnectionsPath, newJson);
                    Console.WriteLine($"  ✓ Created {DbConnectionsPath} with connection string");
                    return true;
                }

                // Read and parse JSON
                string json = File.ReadAllText(DbConnectionsPath);
                JObject config = JObject.Parse(json);

                // Ensure ConnectionStrings section exists
                if (config["ConnectionStrings"] == null)
                {
                    config["ConnectionStrings"] = new JObject();
                }

                // Add or update connection string
                string contextKey = $"{dbName}DbContext";
                ((JObject)config["ConnectionStrings"])[contextKey] = connectionString;

                // Write back with pretty formatting
                string updatedJson = config.ToString(Formatting.Indented);
                File.WriteAllText(DbConnectionsPath, updatedJson);

                Console.WriteLine($"  ✓ Added to dbconnections.json: \"{contextKey}\"");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating dbconnections.json: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates both configuration files.
        /// </summary>
        public bool UpdateConfigurations(string dbName, DatabaseEngine engine, string connectionString)
        {
            Console.WriteLine("\n📝 Updating configuration files...");

            bool appSettingsUpdated = UpdateAppSettings(dbName, engine);
            bool dbConnectionsUpdated = UpdateDbConnections(dbName, connectionString);

            if (appSettingsUpdated && dbConnectionsUpdated)
            {
                Console.WriteLine("  ✅ Configuration files updated successfully");
                return true;
            }
            else
            {
                Console.WriteLine("  ⚠️  Some configuration updates failed");
                return false;
            }
        }

        /// <summary>
        /// Verifies that configuration files exist.
        /// </summary>
        public bool VerifyConfigurationFiles()
        {
            bool appSettingsExists = File.Exists(AppSettingsPath);
            bool dbConnectionsExists = File.Exists(DbConnectionsPath);

            if (!appSettingsExists)
                Console.WriteLine($"⚠️  Warning: {AppSettingsPath} not found");

            if (!dbConnectionsExists)
                Console.WriteLine($"⚠️  Warning: {DbConnectionsPath} not found");

            return appSettingsExists && dbConnectionsExists;
        }
    }
}
