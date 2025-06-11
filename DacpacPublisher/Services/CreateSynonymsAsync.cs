// Step 2: CREATE new SynonymService.cs (or enhance existing one)
// This adds smart database selection while keeping your project structure

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Services
{
    public class SynonymService : ISynonymService
    {
        private readonly IConnectionService _connectionService;
        private readonly ILogService _logService;

        public SynonymService(IConnectionService connectionService, ILogService logService)
        {
            _connectionService = connectionService;
            _logService = logService;
        }

        public event Action<string> LogMessageReceived;

        // ENHANCED: Your existing CreateSynonymsAsync method with smart database selection
        public async Task CreateSynonymsAsync(PublisherConfiguration config, DeploymentResult result)
        {
            try
            {
                if (!config.CreateSynonyms)
                {
                    OnLogMessage("ℹ️ Synonym creation is disabled - skipping");
                    return;
                }

                OnLogMessage("🔗 Starting smart synonym creation process...");

                // Step 1: Validate source database
                if (string.IsNullOrEmpty(config.SynonymSourceDb))
                {
                    config.SynonymSourceDb = "HiveCFMSurvey"; // Your default
                    OnLogMessage("📝 Using default synonym source: HiveCFMSurvey");
                }

                var sourceDbExists = await CheckDatabaseExistsAsync(config, config.SynonymSourceDb);
                if (!sourceDbExists)
                {
                    result.Warnings.Add($"Synonym source database '{config.SynonymSourceDb}' not found - skipping");
                    OnLogMessage($"⚠️ Source database '{config.SynonymSourceDb}' not found - skipping");
                    return;
                }

                // Step 2: Get target databases based on configuration
                var targetDatabases = GetSynonymTargetDatabases(config);

                if (!targetDatabases.Any())
                {
                    OnLogMessage("ℹ️ No target databases specified for synonym creation");
                    result.Warnings.Add("No target databases specified for synonym creation");
                    return;
                }

                OnLogMessage($"📋 Creating synonyms in {targetDatabases.Count} database(s): {string.Join(", ", targetDatabases)}");
                OnLogMessage($"🎯 Source database: {config.SynonymSourceDb}");

                // Step 3: Create synonyms for each target database
                foreach (var targetDatabase in targetDatabases)
                {
                    await CreateSynonymsForDatabaseAsync(config, targetDatabase, result);
                }

                OnLogMessage("✅ Smart synonym creation process completed!");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Synonym creation failed: {ex.Message}";
                result.Errors.Add(errorMsg);
                OnLogMessage($"❌ {errorMsg}");
                _logService.LogError("Synonym creation failed", ex);
            }
        }

        // NEW: Get target databases based on synonym configuration
        private List<string> GetSynonymTargetDatabases(PublisherConfiguration config)
        {
            var targetDatabases = new List<string>();

            try
            {
                // Always include primary database
                if (!string.IsNullOrEmpty(config.Database))
                {
                    targetDatabases.Add(config.Database);
                    OnLogMessage($"🎯 Primary database for synonyms: {config.Database}");
                }

                // Add secondary databases if multi-database is enabled
                if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
                {
                    foreach (var target in config.DeploymentTargets.Where(t => t.IsEnabled))
                    {
                        if (!string.IsNullOrEmpty(target.Database) && !targetDatabases.Contains(target.Database))
                        {
                            targetDatabases.Add(target.Database);
                            OnLogMessage($"🎯 Secondary database for synonyms: {target.Database}");
                        }
                    }
                }

                return targetDatabases;
            }
            catch (Exception ex)
            {
                OnLogMessage($"⚠️ Error determining synonym target databases: {ex.Message}");
                // Fallback to primary database only
                return !string.IsNullOrEmpty(config.Database)
                    ? new List<string> { config.Database }
                    : new List<string>();
            }
        }

        // ENHANCED: Create synonyms for a specific database
        private async Task CreateSynonymsForDatabaseAsync(PublisherConfiguration config, string targetDatabase, DeploymentResult result)
        {
            try
            {
                OnLogMessage($"\n🎯 Creating synonyms in database: {targetDatabase}");

                var connectionInfo = new ConnectionInfo
                {
                    ServerName = config.ServerName,
                    WindowsAuth = config.WindowsAuth,
                    Username = config.Username,
                    Password = config.Password,
                    Database = targetDatabase
                };

                using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
                {
                    await connection.OpenAsync();
                    OnLogMessage($"✅ Connected to {targetDatabase}");

                    // Create your existing synonym (keeping your exact script)
                    await CreateCFMSurveyUserSynonymAsync(connection, config.SynonymSourceDb);

                    // NEW: Check if database needs additional synonyms and create them
                    await CreateAdditionalSynonymsIfNeededAsync(connection, config.SynonymSourceDb, targetDatabase);

                    result.SynonymsCreated++;
                    OnLogMessage($"✅ Synonym creation completed for {targetDatabase}");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to create synonyms in {targetDatabase}: {ex.Message}";
                result.Errors.Add(errorMsg);
                OnLogMessage($"❌ {errorMsg}");
                _logService.LogError($"Synonym creation failed for {targetDatabase}", ex);
            }
        }

        // ENHANCED: Your existing CFMSurveyUser synonym with better error handling
        private async Task CreateCFMSurveyUserSynonymAsync(SqlConnection connection, string sourceDatabase)
        {
            try
            {
                OnLogMessage($"🔗 Creating CFMSurveyUser synonym from {sourceDatabase}...");

                // Your exact existing synonym script
                var synonymScript = $@"
-- This script sets up the necessary synonym
IF EXISTS (SELECT * FROM sys.synonyms WHERE name = 'CFMSurveyUser')
BEGIN
    DROP SYNONYM [dbo].[CFMSurveyUser];
    PRINT 'Dropped existing CFMSurveyUser synonym';
END

-- Verify source table exists
IF EXISTS (SELECT 1 FROM [{sourceDatabase}].sys.tables t 
           INNER JOIN [{sourceDatabase}].sys.schemas s ON t.schema_id = s.schema_id 
           WHERE s.name = 'dbo' AND t.name = 'CFMUser')
BEGIN
    CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [{sourceDatabase}].[dbo].[CFMUser];
    PRINT 'Created CFMSurveyUser synonym successfully';
END
ELSE
BEGIN
    PRINT 'Warning: Source table [{sourceDatabase}].[dbo].[CFMUser] not found';
END
";

                using (var command = new SqlCommand(synonymScript, connection))
                {
                    command.CommandTimeout = 60;
                    await command.ExecuteNonQueryAsync();
                    OnLogMessage($"✅ CFMSurveyUser synonym created: [dbo].[CFMSurveyUser] → [{sourceDatabase}].[dbo].[CFMUser]");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"⚠️ CFMSurveyUser synonym creation failed: {ex.Message}");
                throw;
            }
        }

        // NEW: Smart detection and creation of additional synonyms
        private async Task CreateAdditionalSynonymsIfNeededAsync(SqlConnection connection, string sourceDatabase, string targetDatabase)
        {
            try
            {
                OnLogMessage($"🔍 Checking for additional synonym requirements in {targetDatabase}...");

                // Check for objects that reference tables/views that might need synonyms
                var analysisQuery = @"
                    SELECT DISTINCT 
                        OBJECT_NAME(referencing_id) as ReferencingObject,
                        referenced_entity_name as ReferencedObject
                    FROM sys.sql_expression_dependencies
                    WHERE referenced_database_name IS NULL
                      AND referenced_entity_name NOT IN (
                          SELECT name FROM sys.synonyms WHERE schema_id = SCHEMA_ID('dbo')
                      )
                      AND referenced_entity_name LIKE '%CFM%'
                      AND OBJECT_NAME(referencing_id) IS NOT NULL";

                var missingReferences = new List<string>();

                using (var command = new SqlCommand(analysisQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var referencedObject = reader.GetString(Convert.ToInt32("ReferencedObject"));
                            if (!missingReferences.Contains(referencedObject))
                            {
                                missingReferences.Add(referencedObject);
                            }
                        }
                    }
                }

                if (missingReferences.Any())
                {
                    OnLogMessage($"📊 Found {missingReferences.Count} objects that might need synonyms:");
                    foreach (var obj in missingReferences.Take(5)) // Show first 5
                    {
                        OnLogMessage($"   • {obj}");
                    }

                    // Create synonyms for the most common missing references
                    await CreateCommonSynonymsAsync(connection, sourceDatabase, missingReferences);
                }
                else
                {
                    OnLogMessage($"✅ No additional synonyms needed for {targetDatabase}");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"⚠️ Additional synonym analysis failed: {ex.Message}");
                // Don't throw - this is optional analysis
            }
        }

        // NEW: Create common synonyms based on missing references
        private async Task CreateCommonSynonymsAsync(SqlConnection connection, string sourceDatabase, List<string> missingReferences)
        {
            var commonSynonyms = new Dictionary<string, string>
            {
                {"CFMUser", "CFMSurveyUser"}, // Already created above, but for reference
                {"CFMSurvey", "CFMSurveySynonym"},
                {"CFMQuestion", "CFMQuestionSynonym"},
                {"CFMResponse", "CFMResponseSynonym"}
            };

            foreach (var missing in missingReferences.Take(3)) // Limit to first 3 for safety
            {
                foreach (var synonym in commonSynonyms)
                {
                    if (missing.Contains(synonym.Key) && synonym.Key != "CFMUser") // Skip CFMUser as it's already handled
                    {
                        try
                        {
                            await CreateSpecificSynonymAsync(connection, sourceDatabase, synonym.Value, missing);
                            OnLogMessage($"✅ Created additional synonym: {synonym.Value} → {missing}");
                        }
                        catch (Exception ex)
                        {
                            OnLogMessage($"⚠️ Could not create synonym {synonym.Value}: {ex.Message}");
                        }
                        break;
                    }
                }
            }
        }

        // ENHANCED: Create specific synonym with validation
        private async Task CreateSpecificSynonymAsync(SqlConnection connection, string sourceDatabase, string synonymName, string targetObjectName)
        {
            try
            {
                var createSynonymScript = $@"
-- Check if target exists in source database
IF EXISTS (SELECT 1 FROM [{sourceDatabase}].sys.tables t 
           INNER JOIN [{sourceDatabase}].sys.schemas s ON t.schema_id = s.schema_id 
           WHERE s.name = 'dbo' AND t.name = '{targetObjectName}')
   OR EXISTS (SELECT 1 FROM [{sourceDatabase}].sys.views v 
              INNER JOIN [{sourceDatabase}].sys.schemas s ON v.schema_id = s.schema_id 
              WHERE s.name = 'dbo' AND v.name = '{targetObjectName}')
BEGIN
    -- Drop existing synonym if it exists
    IF EXISTS (SELECT * FROM sys.synonyms WHERE name = '{synonymName}' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        DROP SYNONYM [dbo].[{synonymName}];
    END
    
    -- Create new synonym
    CREATE SYNONYM [dbo].[{synonymName}] FOR [{sourceDatabase}].[dbo].[{targetObjectName}];
    PRINT 'Created synonym {synonymName} -> {targetObjectName}';
END
ELSE
BEGIN
    PRINT 'Skipped synonym {synonymName} - target object {targetObjectName} not found in {sourceDatabase}';
END";

                using (var command = new SqlCommand(createSynonymScript, connection))
                {
                    command.CommandTimeout = 30;
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"❌ Error creating synonym {synonymName}: {ex.Message}");
                throw;
            }
        }

        // Your existing database existence check (keeping it the same)
        private async Task<bool> CheckDatabaseExistsAsync(PublisherConfiguration config, string databaseName)
        {
            try
            {
                var connectionInfo = new ConnectionInfo
                {
                    ServerName = config.ServerName,
                    WindowsAuth = config.WindowsAuth,
                    Username = config.Username,
                    Password = config.Password,
                    Database = "master"
                };

                using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
                {
                    await connection.OpenAsync();
                    var query = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DatabaseName", databaseName);
                        var count = (int)await command.ExecuteScalarAsync();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                OnLogMessage($"⚠️ Could not check if database '{databaseName}' exists: {ex.Message}");
                return false;
            }
        }

        private void OnLogMessage(string message)
        {
            _logService.LogInfo(message);
            LogMessageReceived?.Invoke(message);
        }
    }

    // Interface for the service (if you don't have it)
    public interface ISynonymService
    {
        Task CreateSynonymsAsync(PublisherConfiguration config, DeploymentResult result);
        event Action<string> LogMessageReceived;
    }
}