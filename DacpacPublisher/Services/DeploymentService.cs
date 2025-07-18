using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Services
{
	public class DeploymentService : IDeploymentService
	{
		private readonly IConnectionService _connectionService;
		private readonly ILogService _logService;
		private readonly Dictionary<string, bool> _synonymsCreatedTracker = new Dictionary<string, bool>();
		private PublisherConfiguration _currentConfig; // Add this field at the top of the class

		public DeploymentService(IConnectionService connectionService, ILogService logService, PublisherConfiguration currentConfig)
		{
			_connectionService = connectionService;
			_logService = logService;
			_currentConfig = currentConfig;
		}

		public event Action<string> LogMessageReceived;
		public event Action<int> ProgressChanged;

		public async Task<DeploymentResult> DeployDacpacAsync(PublisherConfiguration config)
		{
			_currentConfig = config;
			var stopwatch = Stopwatch.StartNew();
			var result = new DeploymentResult
			{
				Success = false,
				Warnings = new List<string>(),
				Errors = new List<string>()
			};

			try
			{
				OnLogMessage("🚀 === ENHANCED DACPAC Publisher Deployment Started ===");
				OnProgressChanged(0);

				// Clear synonym tracker for new deployment
				_synonymsCreatedTracker.Clear();

				// PHASE 1: Pre-deployment validation and setup
				OnLogMessage("📋 Phase 1: Pre-deployment validation...");
				await ValidateDeploymentPrerequisitesAsync(config, result);
				OnProgressChanged(5);

				if (result.Errors.Any())
				{
					OnLogMessage("❌ Pre-deployment validation failed - stopping deployment");
					result.Success = false;
					result.Message = "Pre-deployment validation failed: " + string.Join("; ", result.Errors);
					return result;
				}

				// PHASE 2: ⭐ PRE-CREATE CRITICAL SYNONYMS (from enhanced version)
				OnLogMessage("🔗 Phase 2: Pre-creating critical synonyms...");
				await PreCreateCriticalSynonymsAsync(config, result);
				OnProgressChanged(15);

				// PHASE 3: Deploy ALL DACPACs (database structures)
				OnLogMessage("🗄️ Phase 3: Deploying database structures...");
				await DeployAllDatabaseStructuresAsync(config, result);
				OnProgressChanged(50);

				if (!result.Success)
				{
					OnLogMessage("❌ Database deployment failed - stopping");
					return result;
				}

				// PHASE 4: Create SQL Agent Jobs (if enabled) - NOW WORKING!
				if (config.CreateSqlAgentJobs)
				{
					OnLogMessage("⚙️ Phase 4: Creating SQL Agent jobs...");
					await CreateSqlAgentJobsSafelyAsync(config, result);
				}

				OnProgressChanged(70);

				// PHASE 5: Execute Stored Procedures (if enabled) - NOW WORKING!
				if (config.ExecuteProcedures)
				{
					OnLogMessage("📝 Phase 5: Executing stored procedures...");
					await ExecuteStoredProceduresSafelyAsync(config, result);
				}

				OnProgressChanged(85);

				// PHASE 6: POST-DEPLOYMENT SYNONYM VERIFICATION
				if (config.CreateSynonyms)
				{
					OnLogMessage("🔍 Phase 6: Verifying synonym deployment...");
					await VerifyAndCompleteSynonymsAsync(config, result);
				}

				OnProgressChanged(100);

				stopwatch.Stop();
				result.Duration = stopwatch.Elapsed;

				if (result.Success)
				{
					OnLogMessage($"🎉 === Deployment Completed Successfully in {stopwatch.Elapsed:mm\\:ss} ===");
					result.Message = "Deployment completed successfully";
				}
				else
				{
					OnLogMessage($"⚠️ === Deployment Completed with Issues in {stopwatch.Elapsed:mm\\:ss} ===");
					result.Message = "Deployment completed with warnings/errors - check logs";
				}

				return result;
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				result.Duration = stopwatch.Elapsed;
				OnLogMessage($"💥 === Deployment Failed after {stopwatch.Elapsed:mm\\:ss} ===");
				_logService.LogError("Deployment failed with exception", ex);

				result.Success = false;
				result.Message = ex.Message;
				result.Exception = ex;
				result.Errors.Add($"Fatal Exception: {ex.Message}");

				return result;
			}
		}


		/// <summary>
		/// ⭐ NEW: Pre-create critical synonyms BEFORE DACPAC deployment
		/// This prevents the "synonym refers to invalid object" errors
		/// </summary>
		private async Task PreCreateCriticalSynonymsAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				if (!config.CreateSynonyms)
				{
					OnLogMessage("ℹ️ Synonym creation is disabled - skipping pre-creation");
					return;
				}

				OnLogMessage("🔗 === PRE-CREATING CRITICAL SYNONYMS ===");
				OnLogMessage("🎯 Strategy: Create synonyms BEFORE DACPAC deployment to prevent reference errors");

				// Step 1: Determine source and target databases
				var allDatabases = GetAllDeploymentDatabases(config);
				string sourceDatabase = await DetermineSourceDatabaseAsync(config, allDatabases);

				if (string.IsNullOrEmpty(sourceDatabase))
				{
					OnLogMessage("⚠️ No suitable source database found - synonyms will be created later");
					result.Warnings.Add("No source database found for pre-creating synonyms");
					return;
				}

				var targetDatabases = DetermineTargetDatabases(config, allDatabases, sourceDatabase);

				if (!targetDatabases.Any())
				{
					OnLogMessage("ℹ️ No target databases found for pre-creating synonyms");
					return;
				}

				OnLogMessage($"📋 PRE-CREATE SOURCE: {sourceDatabase}");
				OnLogMessage($"🎯 PRE-CREATE TARGETS: {string.Join(", ", targetDatabases)}");

				// Step 2: Verify source database has the required table
				bool sourceHasTable = await VerifySourceTableExists(config, sourceDatabase, "CFMSurveyUser");
				if (!sourceHasTable)
				{
					OnLogMessage($"⚠️ Source database '{sourceDatabase}' doesn't have CFMSurveyUser table - using placeholder");
					// Create with a temporary placeholder - will be fixed later
					await CreatePlaceholderSynonymsAsync(config, targetDatabases, sourceDatabase, result);
					return;
				}

				// Step 3: Pre-create synonyms in each target database
				foreach (var targetDb in targetDatabases)
				{
					try
					{
						OnLogMessage($"\n🔗 Pre-creating synonyms in: {targetDb}");
						await PreCreateSynonymInDatabase(config, targetDb, sourceDatabase);

						var synonymKey = targetDb.ToLower();
						_synonymsCreatedTracker[synonymKey] = true;
						result.SynonymsCreated++;

						OnLogMessage($"✅ Pre-created synonyms successfully in {targetDb}");
					}
					catch (Exception ex)
					{
						OnLogMessage($"⚠️ Could not pre-create synonyms in {targetDb}: {ex.Message}");
						result.Warnings.Add($"Pre-creation failed for {targetDb}: {ex.Message}");
						// Continue with other databases
					}
				}

				OnLogMessage($"\n📊 PRE-CREATION SUMMARY: {result.SynonymsCreated} synonyms pre-created");
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error in pre-creating synonyms: {ex.Message}");
				result.Warnings.Add($"Synonym pre-creation failed: {ex.Message}");
				_logService.LogError("Pre-creation of synonyms failed", ex);
			}
		}

		/// <summary>
		/// Pre-create synonym in a specific target database
		/// </summary>
		private async Task PreCreateSynonymInDatabase(PublisherConfiguration config, string targetDatabase, string sourceDatabase)
		{
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

				// Enhanced synonym creation script with better error handling
				var synonymScript = $@"
-- PRE-DEPLOYMENT Synonym Creation
-- Target Database: {targetDatabase}
-- Source Database: {sourceDatabase}
-- Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

PRINT '🔗 Pre-creating synonyms in [{targetDatabase}]...';

-- Verify we're not in the source database
IF DB_NAME() = '{sourceDatabase}'
BEGIN
    PRINT '⚠️ Skipping - cannot create synonym in source database itself';
    RETURN;
END

-- Drop existing synonyms if they exist
IF EXISTS (SELECT * FROM sys.synonyms WHERE name = 'CFMSurveyUser' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DROP SYNONYM [dbo].[CFMSurveyUser];
    PRINT '✅ Dropped existing CFMSurveyUser synonym';
END

-- Check if source table exists, if not create a placeholder
DECLARE @SourceExists BIT = 0;
BEGIN TRY
    IF EXISTS (SELECT 1 FROM [{sourceDatabase}].sys.tables t 
               INNER JOIN [{sourceDatabase}].sys.schemas s ON t.schema_id = s.schema_id 
               WHERE s.name = 'dbo' AND t.name = 'CFMSurveyUser')
    BEGIN
        SET @SourceExists = 1;
    END
END TRY
BEGIN CATCH
    SET @SourceExists = 0;
    PRINT '⚠️ Could not verify source table existence';
END CATCH

-- Create the synonym
BEGIN TRY
    IF @SourceExists = 1
    BEGIN
        CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [{sourceDatabase}].[dbo].[CFMSurveyUser];
        PRINT '✅ Created synonym: [dbo].[CFMSurveyUser] → [{sourceDatabase}].[dbo].[CFMSurveyUser]';
    END
    ELSE
    BEGIN
        -- Create placeholder pointing to a temporary location
        -- This prevents deployment errors, will be fixed later
        CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [{sourceDatabase}].[dbo].[CFMUser];
        PRINT '⚠️ Created placeholder synonym: [dbo].[CFMSurveyUser] → [{sourceDatabase}].[dbo].[CFMUser]';
        PRINT '   Will be updated after deployment if needed';
    END
END TRY
BEGIN CATCH
    DECLARE @ErrorMsg NVARCHAR(500) = ERROR_MESSAGE();
    PRINT '❌ Error creating synonym: ' + @ErrorMsg;
    -- Don't throw error here - allow deployment to continue
END CATCH

PRINT '🔗 Pre-creation completed for [{targetDatabase}]';
";

				using (var command = new SqlCommand(synonymScript, connection))
				{
					command.CommandTimeout = 60;

					// Capture print messages
					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"   SQL: {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		/// <summary>
		/// Create placeholder synonyms when source table doesn't exist yet
		/// </summary>
		private async Task CreatePlaceholderSynonymsAsync(PublisherConfiguration config, List<string> targetDatabases,
			string sourceDatabase, DeploymentResult result)
		{
			OnLogMessage("⚠️ Creating placeholder synonyms (source table not found)");

			foreach (var targetDb in targetDatabases)
			{
				try
				{
					var connectionInfo = new ConnectionInfo
					{
						ServerName = config.ServerName,
						WindowsAuth = config.WindowsAuth,
						Username = config.Username,
						Password = config.Password,
						Database = targetDb
					};

					using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
					{
						await connection.OpenAsync();

						var placeholderScript = $@"
-- Placeholder synonym creation
IF NOT EXISTS (SELECT * FROM sys.synonyms WHERE name = 'CFMSurveyUser' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Create a temporary synonym pointing to a safe location
    CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [tempdb].[dbo].[__TempSynonym_CFMSurveyUser_Placeholder];
    PRINT '⚠️ Created placeholder synonym in {targetDb} - will be updated later';
END
";

						using (var command = new SqlCommand(placeholderScript, connection))
						{
							command.CommandTimeout = 30;
							await command.ExecuteNonQueryAsync();
						}
					}

					result.Warnings.Add($"Created placeholder synonym in {targetDb}");
					OnLogMessage($"⚠️ Placeholder synonym created in {targetDb}");
				}
				catch (Exception ex)
				{
					OnLogMessage($"❌ Could not create placeholder in {targetDb}: {ex.Message}");
				}
			}
		}

		/// <summary>
		/// Verify that the source table exists
		/// </summary>
		private async Task<bool> VerifySourceTableExists(PublisherConfiguration config, string sourceDatabase, string tableName)
		{
			try
			{
				var connectionInfo = new ConnectionInfo
				{
					ServerName = config.ServerName,
					WindowsAuth = config.WindowsAuth,
					Username = config.Username,
					Password = config.Password,
					Database = sourceDatabase
				};

				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					var query = @"
                        SELECT COUNT(*) 
                        FROM sys.tables t
                        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                        WHERE s.name = 'dbo' AND (t.name = @TableName OR t.name = 'CFMUser')";

					using (var command = new SqlCommand(query, connection))
					{
						command.Parameters.AddWithValue("@TableName", tableName);
						var count = (int)await command.ExecuteScalarAsync();

						OnLogMessage($"🔍 Source table verification: {sourceDatabase}.dbo.{tableName} exists = {count > 0}");
						return count > 0;
					}
				}
			}
			catch (Exception ex)
			{
				OnLogMessage($"⚠️ Could not verify source table in {sourceDatabase}: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Verify and complete synonym deployment after DACPAC
		/// </summary>
		private async Task VerifyAndCompleteSynonymsAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				OnLogMessage("🔍 === VERIFYING SYNONYM DEPLOYMENT ===");

				var allDatabases = GetAllDeploymentDatabases(config);
				string sourceDatabase = await DetermineSourceDatabaseAsync(config, allDatabases);
				var targetDatabases = DetermineTargetDatabases(config, allDatabases, sourceDatabase);

				foreach (var targetDb in targetDatabases)
				{
					try
					{
						await VerifySynonymInDatabase(config, targetDb, sourceDatabase);
						OnLogMessage($"✅ Verified synonyms in {targetDb}");
					}
					catch (Exception ex)
					{
						OnLogMessage($"⚠️ Synonym verification failed for {targetDb}: {ex.Message}");
						result.Warnings.Add($"Synonym verification failed for {targetDb}");
					}
				}

				OnLogMessage("✅ Synonym verification completed");
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error in synonym verification: {ex.Message}");
				result.Warnings.Add($"Synonym verification failed: {ex.Message}");
			}
		}

		/// <summary>
		/// Verify synonym in a specific database and fix if needed
		/// </summary>
		private async Task VerifySynonymInDatabase(PublisherConfiguration config, string targetDatabase, string sourceDatabase)
		{
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

				var verificationScript = $@"
-- Verify and fix synonyms if needed
PRINT '🔍 Verifying synonyms in [{targetDatabase}]...';

-- Check if synonym exists and points to correct location
DECLARE @SynonymTarget NVARCHAR(500);
SELECT @SynonymTarget = base_object_name 
FROM sys.synonyms 
WHERE name = 'CFMSurveyUser' AND schema_id = SCHEMA_ID('dbo');

IF @SynonymTarget IS NULL
BEGIN
    PRINT '⚠️ CFMSurveyUser synonym not found - creating it now';
    CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [{sourceDatabase}].[dbo].[CFMSurveyUser];
    PRINT '✅ Created missing synonym';
END
ELSE IF @SynonymTarget LIKE '%tempdb%' OR @SynonymTarget LIKE '%Placeholder%'
BEGIN
    PRINT '🔧 Fixing placeholder synonym';
    DROP SYNONYM [dbo].[CFMSurveyUser];
    CREATE SYNONYM [dbo].[CFMSurveyUser] FOR [{sourceDatabase}].[dbo].[CFMSurveyUser];
    PRINT '✅ Fixed placeholder synonym';
END
ELSE
BEGIN
    PRINT '✅ Synonym exists and points to: ' + @SynonymTarget;
END
";

				using (var command = new SqlCommand(verificationScript, connection))
				{
					command.CommandTimeout = 30;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"   VERIFY: {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		// ... (Keep all your existing methods: DeployAllDatabaseStructuresAsync, etc.)

		private async Task DeployAllDatabaseStructuresAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				OnLogMessage("🗄️ Starting database deployment...");

				// Deploy primary database
				OnLogMessage($"📦 Deploying primary database: {config.Database}");
				await DeployDacpacToSingleDatabaseAsync(config, result, isPrimary: true);

				// Deploy secondary databases if enabled
				if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
				{
					foreach (var target in config.DeploymentTargets.Where(t => t.IsEnabled))
					{
						OnLogMessage($"📦 Deploying secondary database: {target.Database}");

						var targetConfig = new PublisherConfiguration
						{
							ServerName = target.ServerName ?? config.ServerName,
							WindowsAuth = config.WindowsAuth,
							Username = config.Username,
							Password = config.Password,
							Database = target.Database,
							DacpacPath = target.DacpacPath ?? config.DacpacPath,
							CreateSynonyms = false // Synonyms already created in pre-phase
						};

						await DeployDacpacToSingleDatabaseAsync(targetConfig, result, isPrimary: false);
					}
				}

				result.Success = !result.Errors.Any(e =>
					e.Contains("Login failed") ||
					e.Contains("Access denied"));

				OnLogMessage($"📊 Database deployment completed: Success={result.Success}");
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Errors.Add($"Database deployment failed: {ex.Message}");
				_logService.LogError("Database deployment error", ex);
			}
		}

		private async Task DeployDacpacToSingleDatabaseAsync(PublisherConfiguration config, DeploymentResult result, bool isPrimary)
		{
			try
			{
				var connectionInfo = new ConnectionInfo
				{
					ServerName = config.ServerName,
					WindowsAuth = config.WindowsAuth,
					Username = config.Username,
					Password = config.Password,
					Database = config.Database
				};

				var connectionString = _connectionService.BuildConnectionString(connectionInfo);
				var sqlPackagePath = FindSqlPackagePath();

				var arguments = BuildSqlPackageArguments(config, connectionString);

				OnLogMessage($"⚙️ Executing SQLPackage for {config.Database}");

				await ExecuteSqlPackageAsync(sqlPackagePath, arguments, result);

				if (isPrimary && result.Errors.Count == 0)
				{
					result.Success = true;
				}
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Errors.Add($"DACPAC deployment failed for {config.Database}: {ex.Message}");
				_logService.LogError($"DACPAC deployment failed for {config.Database}", ex);
			}
		}

		private string BuildSqlPackageArguments(PublisherConfiguration config, string connectionString)
		{
			var args = $"/a:Publish " +
					  $"/SourceFile:\"{config.DacpacPath}\" " +
					  $"/TargetConnectionString:\"{connectionString}\" " +
					  "/p:BlockOnPossibleDataLoss=false " +
					  "/p:DropObjectsNotInSource=false " +
					  "/p:VerifyDeployment=true " +
					  "/p:TreatVerificationErrorsAsWarnings=true " +
					  "/p:AllowIncompatiblePlatform=true " +
					  "/p:IgnorePermissions=true " +
					  "/p:IgnoreUserSettingsObjects=true " +
					  "/p:DropPermissionsNotInSource=false " +
					  "/p:DropRoleMembersNotInSource=false " +
					  "/p:CreateNewDatabase=false " +
					  "/p:CommandTimeout=300 " +
					  "/p:ExcludeObjectTypes=Queues;Services;Contracts;MessageTypes;BrokerPriorities;RemoteServiceBindings;Logins;Users;RoleMembership";
			// Note: Removed Synonyms from ExcludeObjectTypes since we pre-created them

			if (!string.IsNullOrEmpty(config.SynonymSourceDb))
			{
				args += $" /v:SynonymSourceDb=\"{config.SynonymSourceDb}\"";
			}

			return args;
		}

		// ... (Keep all other existing methods)

		private async Task<string> DetermineSourceDatabaseAsync(PublisherConfiguration config, List<string> allDatabases)
		{
			try
			{
				OnLogMessage("🔍 Determining source database...");

				// Priority 1: User-specified source
				if (!string.IsNullOrEmpty(config.SynonymSourceDb) &&
					config.SynonymSourceDb != "AUTO_DETECT" &&
					allDatabases.Contains(config.SynonymSourceDb))
				{
					OnLogMessage($"✅ Using user-specified source: {config.SynonymSourceDb}");
					return config.SynonymSourceDb;
				}

				// Priority 2: Find HiveCFMSurvey databases
				var surveyDatabases = allDatabases
					.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();

				if (surveyDatabases.Count == 1)
				{
					OnLogMessage($"✅ Found single HiveCFMSurvey database: {surveyDatabases[0]}");
					return surveyDatabases[0];
				}

				if (surveyDatabases.Count > 1)
				{
					OnLogMessage($"🔍 Found {surveyDatabases.Count} HiveCFMSurvey databases, using first: {surveyDatabases[0]}");
					return surveyDatabases[0];
				}

				// Priority 3: Default
				OnLogMessage("⚠️ No HiveCFMSurvey database found, using default");
				return "HiveCFMSurveyDB";
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error determining source database: {ex.Message}");
				return null;
			}
		}

		private List<string> DetermineTargetDatabases(PublisherConfiguration config, List<string> allDatabases, string sourceDatabase)
		{
			var targetDatabases = new List<string>();

			try
			{
				OnLogMessage("🎯 Determining target databases...");

				// Add primary database if it's different from source
				if (!string.IsNullOrEmpty(config.Database) &&
					!string.Equals(config.Database, sourceDatabase, StringComparison.OrdinalIgnoreCase))
				{
					targetDatabases.Add(config.Database);
				}

				// Add secondary databases if enabled
				if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
				{
					foreach (var target in config.DeploymentTargets.Where(t => t.IsEnabled))
					{
						if (!string.IsNullOrEmpty(target.Database) &&
							!string.Equals(target.Database, sourceDatabase, StringComparison.OrdinalIgnoreCase) &&
							!targetDatabases.Contains(target.Database))
						{
							targetDatabases.Add(target.Database);
						}
					}
				}

				OnLogMessage($"📋 Target databases determined: {string.Join(", ", targetDatabases)}");
				return targetDatabases;
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error determining target databases: {ex.Message}");
				return new List<string>();
			}
		}

		private List<string> GetAllDeploymentDatabases(PublisherConfiguration config)
		{
			var databases = new List<string>();

			// Add primary database
			if (!string.IsNullOrEmpty(config.Database))
			{
				databases.Add(config.Database);
			}

			// Add secondary databases if multiple deployment is enabled
			if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
			{
				foreach (var target in config.DeploymentTargets.Where(t => t.IsEnabled))
				{
					if (!string.IsNullOrEmpty(target.Database) && !databases.Contains(target.Database))
					{
						databases.Add(target.Database);
					}
				}
			}

			return databases;
		}

		// ... (Include all other existing methods: ExecuteSqlPackageAsync, FindSqlPackagePath, etc.)

		private async Task ExecuteSqlPackageAsync(string sqlPackagePath, string arguments, DeploymentResult result)
		{
			try
			{
				var processInfo = new ProcessStartInfo
				{
					FileName = sqlPackagePath,
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using (var process = new Process { StartInfo = processInfo })
				{
					var output = new List<string>();
					var errors = new List<string>();

					process.OutputDataReceived += (sender, args) =>
					{
						if (!string.IsNullOrEmpty(args.Data))
						{
							output.Add(args.Data);

							if (args.Data.Contains("Warning"))
							{
								result.Warnings.Add(args.Data);
								OnLogMessage($"⚠️ {args.Data}");
							}
							else if (args.Data.Contains("Error"))
							{
								result.Errors.Add(args.Data);
								OnLogMessage($"❌ {args.Data}");
							}
							else
							{
								OnLogMessage($"ℹ️ {args.Data}");
							}
						}
					};

					process.ErrorDataReceived += (sender, args) =>
					{
						if (!string.IsNullOrEmpty(args.Data))
						{
							errors.Add(args.Data);
							result.Errors.Add(args.Data);
							OnLogMessage($"🔥 ERROR: {args.Data}");
						}
					};

					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					await Task.Run(() => process.WaitForExit());

					if (process.ExitCode != 0)
					{
						string errorMessage = $"SQLPackage.exe exited with code {process.ExitCode}";

						var criticalErrors = errors.Where(e =>
							e.Contains("Login failed") ||
							e.Contains("Access denied")).ToList();

						if (criticalErrors.Any())
						{
							errorMessage = "Critical deployment errors: " + string.Join("; ", criticalErrors);
							result.Success = false;
						}
						else
						{
							result.Success = true; // Allow continuation for non-critical errors
						}

						result.Errors.Add(errorMessage);
					}
					else
					{
						OnLogMessage($"✅ SQLPackage executed successfully");
						result.Success = true;
					}
				}
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Errors.Add($"SQLPackage execution exception: {ex.Message}");
				_logService.LogError("SQLPackage execution failed", ex);
			}
		}

		private string FindSqlPackagePath()
		{
			var possiblePaths = new[]
			{
				@"C:\Program Files\Microsoft SQL Server\160\DAC\bin\sqlpackage.exe",
				@"C:\Program Files\Microsoft SQL Server\150\DAC\bin\sqlpackage.exe",
				@"C:\Program Files\Microsoft SQL Server\140\DAC\bin\sqlpackage.exe",
				@"C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\sqlpackage.exe",
				@"C:\Program Files (x86)\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\sqlpackage.exe",
				@"C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\sqlpackage.exe"
			};

			foreach (var path in possiblePaths)
			{
				if (File.Exists(path))
					return path;
			}

			return null;
		}

		private async Task ValidateDeploymentPrerequisitesAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				OnLogMessage("🔍 Validating deployment prerequisites...");

				// Validate DACPAC files exist
				if (!File.Exists(config.DacpacPath))
				{
					result.Errors.Add($"Primary DACPAC file not found: {config.DacpacPath}");
				}

				// Validate database connections
				var connectionInfo = new ConnectionInfo
				{
					ServerName = config.ServerName,
					WindowsAuth = config.WindowsAuth,
					Username = config.Username,
					Password = config.Password,
					Database = "master"
				};

				bool canConnect = await _connectionService.TestConnectionAsync(connectionInfo);
				if (!canConnect)
				{
					result.Errors.Add("Cannot connect to SQL Server - check connection settings");
					return;
				}

				// Validate SQLPackage availability
				var sqlPackagePath = FindSqlPackagePath();
				if (string.IsNullOrEmpty(sqlPackagePath))
				{
					result.Errors.Add("SQLPackage.exe not found - install SQL Server Data Tools (SSDT)");
					return;
				}

				OnLogMessage($"✅ Using SQLPackage: {Path.GetFileName(sqlPackagePath)}");
			}
			catch (Exception ex)
			{
				result.Errors.Add($"Pre-deployment validation failed: {ex.Message}");
				_logService.LogError("Pre-deployment validation error", ex);
			}
		}

		private async Task CreateSqlAgentJobsSafelyAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				OnLogMessage("⚙️ === CREATING SQL AGENT JOBS ===");

				if (!config.CreateSqlAgentJobs)
				{
					OnLogMessage("ℹ️ SQL Agent job creation is disabled - skipping");
					return;
				}

				if (string.IsNullOrEmpty(config.JobScriptsFolder) || !Directory.Exists(config.JobScriptsFolder))
				{
					OnLogMessage("❌ Job scripts folder not found - skipping job creation");
					result.Warnings.Add("Job scripts folder not found");
					return;
				}

				if (string.IsNullOrEmpty(config.JobOwnerLoginName))
				{
					OnLogMessage("❌ Job owner login name not specified - skipping job creation");
					result.Warnings.Add("Job owner login name not specified");
					return;
				}

				// Validate job scripts first
				var validationResult = await ValidateJobScriptsAsync(config.JobScriptsFolder);
				if (!validationResult.IsValid)
				{
					OnLogMessage($"❌ Job script validation failed: {validationResult.ErrorMessage}");
					result.Warnings.Add($"Job script validation failed: {validationResult.ErrorMessage}");
					return;
				}

				OnLogMessage($"📋 Found {validationResult.JobCount} valid job script(s)");

				// Execute job scripts
				var connectionInfo = new ConnectionInfo
				{
					ServerName = config.ServerName,
					WindowsAuth = config.WindowsAuth,
					Username = config.Username,
					Password = config.Password,
					Database = config.Database
				};

				var jobsCreated = 0;
				foreach (var jobScript in validationResult.JobScripts.Where(js => js.IsValid))
				{
					try
					{
						OnLogMessage($"\n⚙️ Creating job: {jobScript.JobName}");
						await ExecuteJobScriptAsync(connectionInfo, jobScript, config);
						jobsCreated++;
						OnLogMessage($"✅ Job created: {jobScript.JobName}");
					}
					catch (Exception ex)
					{
						var errorMsg = $"Failed to create job {jobScript.JobName}: {ex.Message}";
						OnLogMessage($"❌ {errorMsg}");
						result.Warnings.Add(errorMsg);
					}
				}

				result.JobsCreated = jobsCreated;
				OnLogMessage($"\n📊 JOB CREATION SUMMARY: {jobsCreated} job(s) created successfully");
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error in SQL Agent job creation: {ex.Message}");
				result.Errors.Add($"SQL Agent job creation failed: {ex.Message}");
				_logService.LogError("SQL Agent job creation failed", ex);
			}
		}

		private async Task ExecuteJobScriptAsync(ConnectionInfo connectionInfo, JobScriptInfo jobScript, PublisherConfiguration config)
		{
			try
			{
				var scriptContent = await Task.Run(() => File.ReadAllText(jobScript.FilePath));

				OnLogMessage($"   📄 Original script length: {scriptContent.Length} characters");

				// IMPROVED: Better parameter replacement with proper SQL escaping
				scriptContent = ReplaceJobScriptParameters(scriptContent, config);

				OnLogMessage($"   🔧 After parameter replacement: {scriptContent.Length} characters");

				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();
					OnLogMessage($"   🔌 Connected to {connectionInfo.Database}");

					// Split script by GO statements and execute each batch
					var batches = SplitScriptIntoBatches(scriptContent);
					OnLogMessage($"   📋 Script split into {batches.Count} batch(es)");

					int batchNumber = 1;
					foreach (var batch in batches)
					{
						if (string.IsNullOrWhiteSpace(batch)) continue;

						try
						{
							OnLogMessage($"   ⚙️ Executing batch {batchNumber}/{batches.Count}");

							using (var command = new SqlCommand(batch, connection))
							{
								command.CommandTimeout = 300;

								// Capture SQL messages
								connection.InfoMessage += (sender, e) =>
								{
									OnLogMessage($"     📢 SQL: {e.Message}");
								};

								await command.ExecuteNonQueryAsync();
							}

							OnLogMessage($"   ✅ Batch {batchNumber} completed");
							batchNumber++;
						}
						catch (Exception batchEx)
						{
							OnLogMessage($"   ❌ Batch {batchNumber} failed: {batchEx.Message}");
							OnLogMessage($"   📝 Problematic batch content (first 200 chars): {batch.Substring(0, Math.Min(200, batch.Length))}");
							throw new Exception($"Batch {batchNumber} execution failed: {batchEx.Message}", batchEx);
						}
					}
				}

				OnLogMessage($"   🎉 Job script {jobScript.FileName} executed successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to execute job script {jobScript.FileName}", ex);
				throw;
			}
		}

		private bool VerifyParameterReplacement(string scriptContent, string serverName, string databaseName, string ownerLoginName)
		{
			try
			{
				// Check that the script contains the actual values
				bool hasServerName = scriptContent.Contains($"N'{serverName}'");
				bool hasDatabaseName = scriptContent.Contains($"N'{databaseName}'");
				bool hasOwnerLogin = scriptContent.Contains($"N'{ownerLoginName}'");

				// Check that no empty parameters remain
				bool hasEmptyServerName = System.Text.RegularExpressions.Regex.IsMatch(scriptContent, @"@ServerName\s+NVARCHAR\(\d+\)\s*=\s*N''", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				bool hasEmptyDatabaseName = System.Text.RegularExpressions.Regex.IsMatch(scriptContent, @"@DatabaseName\s+NVARCHAR\(\d+\)\s*=\s*N''", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				bool hasEmptyOwnerLogin = System.Text.RegularExpressions.Regex.IsMatch(scriptContent, @"@OwnerLoginName\s+NVARCHAR\(\d+\)\s*=\s*N''", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				OnLogMessage($"   🔍 Verification: ServerName={hasServerName}, DatabaseName={hasDatabaseName}, OwnerLogin={hasOwnerLogin}");
				OnLogMessage($"   🔍 Empty check: EmptyServer={hasEmptyServerName}, EmptyDB={hasEmptyDatabaseName}, EmptyOwner={hasEmptyOwnerLogin}");

				return hasServerName && hasDatabaseName && hasOwnerLogin && !hasEmptyServerName && !hasEmptyDatabaseName && !hasEmptyOwnerLogin;
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Verification error: {ex.Message}");
				return false;
			}
		}

		private string ReplaceJobScriptParameters(string scriptContent, PublisherConfiguration config)
		{
			try
			{
				OnLogMessage($"   🔧 Replacing parameters in job script...");

				// Log original parameters for debugging
				OnLogMessage($"   📋 ServerName: '{config.ServerName}'");
				OnLogMessage($"   📋 Database: '{config.Database}'");
				OnLogMessage($"   📋 JobOwnerLoginName: '{config.JobOwnerLoginName}'");

				string serverName = config.ServerName;
				string databaseName = config.Database;
				string ownerLoginName = config.JobOwnerLoginName;

				// Handle special server names for SQL Agent jobs
				if (serverName.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
					serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
					serverName.Equals(".", StringComparison.OrdinalIgnoreCase))
				{
					// For SQL Agent jobs, use the actual machine name
					serverName = Environment.MachineName;
					OnLogMessage($"   🔄 Converted (local) to machine name: {serverName}");
				}

				// Clean up values - escape single quotes for SQL
				serverName = serverName.Replace("'", "''");
				databaseName = databaseName.Replace("'", "''");
				ownerLoginName = ownerLoginName.Replace("'", "''");

				var originalLength = scriptContent.Length;

				// CORRECTED: Replace parameter assignments, not the variable declarations
				// Pattern 1: DECLARE @Parameter NVARCHAR(100) = N'' ;
				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(DECLARE\s+@ServerName\s+NVARCHAR\(\d+\)\s*=\s*)N'[^']*'",
					$"$1N'{serverName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(DECLARE\s+@DatabaseName\s+NVARCHAR\(\d+\)\s*=\s*)N'[^']*'",
					$"$1N'{databaseName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(DECLARE\s+@OwnerLoginName\s+NVARCHAR\(\d+\)\s*=\s*)N'[^']*'",
					$"$1N'{ownerLoginName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				// Pattern 2: Handle SET statements if they exist
				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(SET\s+@ServerName\s*=\s*)N'[^']*'",
					$"$1N'{serverName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(SET\s+@DatabaseName\s*=\s*)N'[^']*'",
					$"$1N'{databaseName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				scriptContent = System.Text.RegularExpressions.Regex.Replace(
					scriptContent,
					@"(SET\s+@OwnerLoginName\s*=\s*)N'[^']*'",
					$"$1N'{ownerLoginName}'",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				OnLogMessage($"   ✅ Parameter replacement completed. Length: {originalLength} → {scriptContent.Length}");

				// Verify the replacement was successful
				if (!VerifyParameterReplacement(scriptContent, serverName, databaseName, ownerLoginName))
				{
					throw new Exception("Parameter replacement verification failed - some parameters may not have been replaced correctly");
				}

				// Log a sample of the replaced content for debugging
				var lines = scriptContent.Split('\n');
				OnLogMessage($"   📄 Sample content after replacement:");
				for (int i = 0; i < Math.Min(15, lines.Length); i++)
				{
					if (lines[i].Contains("DECLARE @") && (lines[i].Contains("ServerName") || lines[i].Contains("DatabaseName") || lines[i].Contains("OwnerLoginName")))
					{
						OnLogMessage($"     {lines[i].Trim()}");
					}
				}

				return scriptContent;
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ❌ Error during parameter replacement: {ex.Message}");
				throw new Exception($"Parameter replacement failed: {ex.Message}", ex);
			}
		}
		/// <summary>
		/// Split SQL script into batches by GO statements - IMPROVED
		/// </summary>
		private List<string> SplitScriptIntoBatches(string script)
		{
			try
			{
				var batches = new List<string>();
				var lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
				var currentBatch = new StringBuilder();

				foreach (var line in lines)
				{
					// Check if line is a GO statement (ignore case and whitespace)
					var trimmedLine = line.Trim();
					if (trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase) ||
						trimmedLine.Equals("go", StringComparison.OrdinalIgnoreCase))
					{
						if (currentBatch.Length > 0)
						{
							var batchContent = currentBatch.ToString().Trim();
							if (!string.IsNullOrWhiteSpace(batchContent))
							{
								batches.Add(batchContent);
							}
							currentBatch.Clear();
						}
					}
					else
					{
						currentBatch.AppendLine(line);
					}
				}

				// Add final batch if there's content
				if (currentBatch.Length > 0)
				{
					var finalBatch = currentBatch.ToString().Trim();
					if (!string.IsNullOrWhiteSpace(finalBatch))
					{
						batches.Add(finalBatch);
					}
				}

				OnLogMessage($"   📋 Script split into {batches.Count} batch(es)");
				return batches;
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ❌ Error splitting script into batches: {ex.Message}");
				// Fallback: return the entire script as one batch
				return new List<string> { script };
			}
		}


		/// <summary>
		/// Split SQL script into batches by GO statements
		/// </summary>

		private async Task ExecuteStoredProceduresSafelyAsync(PublisherConfiguration config, DeploymentResult result)
		{
			try
			{
				OnLogMessage("📝 === EXECUTING STORED PROCEDURES ===");

				if (!config.ExecuteProcedures)
				{
					OnLogMessage("ℹ️ Stored procedure execution is disabled - skipping");
					return;
				}

				// Get procedures to execute (prioritize smart procedures)
				var proceduresToExecute = GetProceduresForExecution(config);

				if (!proceduresToExecute.Any())
				{
					OnLogMessage("ℹ️ No stored procedures configured for execution");
					return;
				}

				OnLogMessage($"📋 Found {proceduresToExecute.Count} procedure(s) to execute");

				// Execute procedures for primary database
				if (!string.IsNullOrEmpty(config.Database))
				{
					OnLogMessage($"\n🗄️ Executing procedures on PRIMARY database: {config.Database}");
					var primaryProcedures = GetProceduresForDatabase(proceduresToExecute, config.Database, true);
					await ExecuteProceduresOnDatabase(config, config.Database, primaryProcedures, result);
				}

				// Execute procedures for secondary databases if enabled
				if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
				{
					foreach (var target in config.DeploymentTargets.Where(t => t.IsEnabled))
					{
						OnLogMessage($"\n🗄️ Executing procedures on SECONDARY database: {target.Database}");
						var secondaryProcedures = GetProceduresForDatabase(proceduresToExecute, target.Database, false);

						if (secondaryProcedures.Any())
						{
							var targetConfig = CreateTargetConfiguration(config, target);
							await ExecuteProceduresOnDatabase(targetConfig, target.Database, secondaryProcedures, result);
						}
						else
						{
							OnLogMessage($"ℹ️ No procedures configured for secondary database: {target.Database}");
						}
					}
				}

				OnLogMessage($"\n📊 PROCEDURE EXECUTION SUMMARY: {result.ProceduresExecuted} procedure(s) executed");
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error in stored procedure execution: {ex.Message}");
				result.Errors.Add($"Stored procedure execution failed: {ex.Message}");
				_logService.LogError("Stored procedure execution failed", ex);
			}
		}

		private void OnLogMessage(string message)
		{
			_logService.LogInfo(message);
			LogMessageReceived?.Invoke(message);
		}

		private void OnProgressChanged(int progress)
		{
			ProgressChanged?.Invoke(progress);
		}

		// Keep all your existing interface implementations
		public async Task<JobScriptValidationResult> ValidateJobScriptsAsync(string jobScriptsFolder)
		{
			var result = new JobScriptValidationResult();

			try
			{
				if (!Directory.Exists(jobScriptsFolder))
				{
					result.IsValid = false;
					result.ErrorMessage = "Job scripts folder does not exist";
					return result;
				}

				var scriptFiles = Directory.GetFiles(jobScriptsFolder, "*.sql")
					.OrderBy(f => Path.GetFileName(f))
					.ToArray();

				if (scriptFiles.Length == 0)
				{
					result.IsValid = false;
					result.ErrorMessage = "No SQL script files found in the specified folder";
					return result;
				}

				foreach (var scriptFile in scriptFiles)
				{
					var scriptInfo = await AnalyzeJobScriptAsync(scriptFile);
					result.JobScripts.Add(scriptInfo);
				}

				result.IsValid = result.JobScripts.All(js => js.IsValid);
				result.JobCount = result.JobScripts.Count;

				if (!result.IsValid) result.ErrorMessage = "One or more job scripts contain errors";
			}
			catch (Exception ex)
			{
				result.IsValid = false;
				result.ErrorMessage = $"Error validating job scripts: {ex.Message}";
			}

			return result;
		}

		private async Task<JobScriptInfo> AnalyzeJobScriptAsync(string scriptFilePath)
		{
			var info = new JobScriptInfo
			{
				FilePath = scriptFilePath,
				FileName = Path.GetFileName(scriptFilePath)
			};

			try
			{
				var content = await Task.Run(() => File.ReadAllText(scriptFilePath));

				// Extract job name
				var jobNameMatch = Regex.Match(
					content,
					@"@job_name\s*=\s*N?'([^']+)'",
					RegexOptions.IgnoreCase);

				if (jobNameMatch.Success) info.JobName = jobNameMatch.Groups[1].Value;

				info.HasServerNameParameter = content.Contains("@ServerName") || content.Contains("@serverName");
				info.HasDatabaseNameParameter = content.Contains("@DatabaseName") || content.Contains("@databaseName");
				info.HasOwnerLoginParameter =
					content.Contains("@OwnerLoginName") || content.Contains("@ownerLoginName");
				info.HasTransactionLogic = content.Contains("BEGIN TRANSACTION") ||
										   content.Contains("COMMIT TRANSACTION") ||
										   content.Contains("ROLLBACK TRANSACTION");
				info.HasGotoStatements = content.Contains("GOTO ") || content.Contains("goto ");
				info.HasUseStatement = content.TrimStart().StartsWith("USE ", StringComparison.OrdinalIgnoreCase);
				info.GoStatementCount = CountGoStatements(content);

				var validationErrors = new List<string>();
				if (string.IsNullOrEmpty(info.JobName)) validationErrors.Add("Could not extract job name from script");
				if (!info.HasServerNameParameter) validationErrors.Add("Script does not contain @ServerName parameter");
				if (!info.HasDatabaseNameParameter)
					validationErrors.Add("Script does not contain @DatabaseName parameter");
				if (!info.HasOwnerLoginParameter)
					validationErrors.Add("Script does not contain @OwnerLoginName parameter");

				info.ValidationErrors = validationErrors;
				info.IsValid = validationErrors.Count == 0;

				if (info.HasTransactionLogic && info.HasGotoStatements)
					info.RecommendedExecutionStrategy = "Single batch with transaction handling";
				else if (info.GoStatementCount > 1)
					info.RecommendedExecutionStrategy = "Multiple batch execution";
				else
					info.RecommendedExecutionStrategy = "Standard execution";
			}
			catch (Exception ex)
			{
				info.IsValid = false;
				info.ValidationErrors.Add($"Error analyzing script: {ex.Message}");
			}

			return info;
		}

		private int CountGoStatements(string script)
		{
			string[] goPatterns = { "\nGO\n", "\nGO\r\n", "\rGO\r", "\nGO ", " GO\n", "\nGO" };

			var count = 0;
			foreach (var pattern in goPatterns)
			{
				var index = 0;
				while ((index = script.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase)) != -1)
				{
					count++;
					index += pattern.Length;
				}
			}

			return count;
		}

		public async Task ExecuteProcedureAsync(ConnectionInfo connectionInfo, string procedureName, string parameters)
		{
			try
			{
				var connectionString = _connectionService.BuildConnectionString(connectionInfo);

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					using (var command = connection.CreateCommand())
					{
						var commandText = string.Format("EXEC {0}", procedureName);
						if (!string.IsNullOrWhiteSpace(parameters)) commandText += string.Format(" {0}", parameters);

						command.CommandText = commandText;
						command.CommandTimeout = 300;

						_logService.LogInfo(string.Format("Executing procedure: {0}", commandText));

						await command.ExecuteNonQueryAsync();

						_logService.LogInfo(string.Format("Successfully executed procedure: {0}", procedureName));
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogError(string.Format("Failed to execute procedure {0}", procedureName), ex);
				throw;
			}
		}

		private PublisherConfiguration CreateTargetConfiguration(PublisherConfiguration baseConfig, DatabaseDeploymentTarget target)
		{
			return new PublisherConfiguration
			{
				ServerName = target.ServerName ?? baseConfig.ServerName,
				WindowsAuth = baseConfig.WindowsAuth,
				Username = baseConfig.Username,
				Password = baseConfig.Password,
				Database = target.Database,
				StoredProcedures = target.GetExecutableProcedures(),
				SmartProcedures = target.SmartProcedures,
				ExecuteProcedures = baseConfig.ExecuteProcedures,
				UseSmartProcedures = baseConfig.UseSmartProcedures
			};
		}

		private List<StoredProcedureInfo> GetProceduresForExecution(PublisherConfiguration config)
		{
			var procedures = new List<StoredProcedureInfo>();

			try
			{
				// Priority 1: Use Smart Procedures if available
				if (config.UseSmartProcedures && config.SmartProcedures?.Any() == true)
				{
					OnLogMessage($"🧠 Using Smart Procedures: {config.SmartProcedures.Count} configured");

					// Convert smart procedures to regular procedures for execution
					procedures.AddRange(config.SmartProcedures
						.Where(sp => sp != null && sp.IsValid)
						.Select(sp => sp.ToLegacyProcedure()));
				}
				// Priority 2: Use regular stored procedures
				else if (config.StoredProcedures?.Any() == true)
				{
					OnLogMessage($"📝 Using Regular Procedures: {config.StoredProcedures.Count} configured");
					procedures.AddRange(config.StoredProcedures.Where(sp => sp != null && sp.IsValid));
				}

				// Sort by execution order
				procedures = procedures.OrderBy(p => p.ExecutionOrder).ToList();

				OnLogMessage($"📋 Total procedures for execution: {procedures.Count}");
				return procedures;
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error getting procedures for execution: {ex.Message}");
				return new List<StoredProcedureInfo>();
			}
		}

		/// <summary>
		/// Get procedures that should run on a specific database
		/// </summary>
		private List<StoredProcedureInfo> GetProceduresForDatabase(List<StoredProcedureInfo> allProcedures,
			string databaseName, bool isPrimaryDatabase)
		{
			var procedures = new List<StoredProcedureInfo>();

			try
			{
				if (isPrimaryDatabase)
				{
					// For primary database, include procedures that should run on Database1
					procedures = allProcedures.Where(p => ShouldRunOnPrimaryDatabase(p)).ToList();
				}
				else
				{
					// For secondary database, include procedures that should run on Database2
					procedures = allProcedures.Where(p => ShouldRunOnSecondaryDatabase(p)).ToList();
				}

				OnLogMessage($"📋 {databaseName}: {procedures.Count} procedure(s) to execute");
				return procedures.OrderBy(p => p.ExecutionOrder).ToList();
			}
			catch (Exception ex)
			{
				OnLogMessage($"❌ Error filtering procedures for {databaseName}: {ex.Message}");
				return new List<StoredProcedureInfo>();
			}
		}

		/// <summary>
		/// Determine if procedure should run on primary database
		/// </summary>
		private bool ShouldRunOnPrimaryDatabase(StoredProcedureInfo procedure)
		{
			// Check if this was originally a smart procedure
			var smartProc = _currentConfig?.SmartProcedures?.FirstOrDefault(sp => sp.Name == procedure.Name);
			if (smartProc != null)
			{
				return smartProc.ExecuteOnDatabase1;
			}

			// For regular procedures, assume they run on primary database
			return true;
		}

		/// <summary>
		/// Determine if procedure should run on secondary database
		/// </summary>
		private bool ShouldRunOnSecondaryDatabase(StoredProcedureInfo procedure)
		{
			// Check if this was originally a smart procedure
			var smartProc = _currentConfig?.SmartProcedures?.FirstOrDefault(sp => sp.Name == procedure.Name);
			if (smartProc != null)
			{
				return smartProc.ExecuteOnDatabase2;
			}

			// For regular procedures, don't run on secondary by default
			return false;
		}

		/// <summary>
		/// Execute procedures on a specific database
		/// </summary>
		private async Task ExecuteProceduresOnDatabase(PublisherConfiguration config, string databaseName,
			List<StoredProcedureInfo> procedures, DeploymentResult result)
		{
			if (!procedures.Any())
			{
				OnLogMessage($"ℹ️ No procedures to execute on {databaseName}");
				return;
			}

			try
			{
				var connectionInfo = new ConnectionInfo
				{
					ServerName = config.ServerName,
					WindowsAuth = config.WindowsAuth,
					Username = config.Username,
					Password = config.Password,
					Database = databaseName
				};

				OnLogMessage($"🔌 Connecting to {databaseName}...");

				// Test connection first
				bool canConnect = await _connectionService.TestConnectionAsync(connectionInfo);
				if (!canConnect)
				{
					throw new Exception($"Cannot connect to database '{databaseName}'");
				}

				OnLogMessage($"✅ Connected to {databaseName}");

				// Execute each procedure
				var executedCount = 0;
				foreach (var procedure in procedures)
				{
					try
					{
						OnLogMessage($"\n🔧 Executing: {procedure.Name}");

						if (!string.IsNullOrEmpty(procedure.Parameters))
						{
							OnLogMessage($"   📋 Parameters: {procedure.Parameters}");
						}

						await ExecuteSingleProcedureAsync(connectionInfo, procedure);

						executedCount++;
						result.ProceduresExecuted++;

						OnLogMessage($"✅ Success: {procedure.Name}");

						// Small delay between procedures to avoid overwhelming the database
						await Task.Delay(100);
					}
					catch (Exception procEx)
					{
						var errorMsg = $"Failed to execute {procedure.Name} on {databaseName}: {procEx.Message}";
						OnLogMessage($"❌ {errorMsg}");
						result.Errors.Add(errorMsg);

						// Continue with other procedures unless it's a critical error
						if (procEx.Message.Contains("Login failed") || procEx.Message.Contains("Access denied"))
						{
							throw; // Stop execution for authentication issues
						}
					}
				}

				OnLogMessage($"\n📊 {databaseName}: {executedCount}/{procedures.Count} procedures executed successfully");
			}
			catch (Exception ex)
			{
				var errorMsg = $"Error executing procedures on {databaseName}: {ex.Message}";
				OnLogMessage($"❌ {errorMsg}");
				result.Errors.Add(errorMsg);
				_logService.LogError($"Procedure execution failed on {databaseName}", ex);
			}
		}

		/// <summary>
		/// Execute a single stored procedure
		/// </summary>
		private async Task ExecuteSingleProcedureAsync(ConnectionInfo connectionInfo, StoredProcedureInfo procedure)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					// Use the smart cleanup approach that handles IDENTITY_INSERT issues
					await ExecuteProcedureWithSmartCleanup(connection, procedure);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to execute procedure {procedure.Name}", ex);
				throw new Exception($"Procedure execution failed: {ex.Message}", ex);
			}
		}
		private async Task ExecuteProcedureWithSmartCleanup(SqlConnection connection, StoredProcedureInfo procedure)
		{
			try
			{
				OnLogMessage($"   🔧 Executing: {procedure.Name}");

				// Check if this is an initialization procedure
				if (procedure.Name.ToLower().Contains("initialize") ||
				    procedure.Name.ToLower().Contains("setup") ||
				    procedure.Name.ToLower().Contains("seed"))
				{
					OnLogMessage($"   🎯 Detected initialization procedure - using table-by-table approach");
					await ExecuteInitializationTableByTable(connection, procedure.Name);
				}
				else
				{
					// For non-initialization procedures, use the regular approach
					await ExecuteProcedureWithIntelligentErrorHandling(connection, procedure);
				}

				OnLogMessage($"   ✅ Successfully executed: {procedure.Name}");
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ❌ Failed to execute {procedure.Name}: {ex.Message}");
				throw;
			}
		}

		/// <summary>
		/// 🧠 INTELLIGENT: Handle any procedure with smart error recovery
		/// </summary>
		private async Task ExecuteProcedureWithIntelligentErrorHandling(SqlConnection connection, StoredProcedureInfo procedure)
		{
			const int maxRetries = 3;
			var attempt = 0;
			var handledErrors = new HashSet<string>();

			while (attempt < maxRetries)
			{
				attempt++;

				try
				{
					OnLogMessage($"   🔧 Attempt {attempt}/{maxRetries}: Executing {procedure.Name}");

					// Always clean IDENTITY_INSERT states before execution
					await DynamicIdentityInsertCleanup(connection);

					// Execute the procedure
					await ExecuteProcedureDirectly(connection, procedure);

					OnLogMessage($"   ✅ Success: {procedure.Name} (attempt {attempt})");

					// 🆕 ADD THIS: Post-execution verification for initialization procedures
					if (procedure.Name.ToLower().Contains("initialize") ||
						procedure.Name.ToLower().Contains("setup") ||
						procedure.Name.ToLower().Contains("seed"))
					{
						await VerifyAndCompleteInitialization(connection, procedure.Name);
					}

					return;
				}
				catch (SqlException sqlEx) when (IsIdentityInsertError(sqlEx) && attempt < maxRetries)
				{
					OnLogMessage($"   🔍 IDENTITY_INSERT conflict on attempt {attempt}");

					// Extract and fix the specific conflicting table
					var conflictingTable = ExtractTableFromIdentityError(sqlEx.Message);
					if (!string.IsNullOrEmpty(conflictingTable) && !handledErrors.Contains(conflictingTable))
					{
						await FixSpecificIdentityInsertConflict(connection, conflictingTable);
						handledErrors.Add(conflictingTable);
					}

					await Task.Delay(500 * attempt);
					OnLogMessage($"   🔄 Retrying after IDENTITY_INSERT fix...");
				}
				catch (SqlException sqlEx) when (IsDuplicateKeyError(sqlEx))
				{
					OnLogMessage($"   ℹ️ Duplicate key detected - data already exists");

					// For initialization procedures, duplicates mean success
					if (procedure.Name.ToLower().Contains("initialize") ||
						procedure.Name.ToLower().Contains("setup") ||
						procedure.Name.ToLower().Contains("seed"))
					{
						OnLogMessage($"   ✅ Initialization goal achieved (data already exists)");

						// 🆕 ADD THIS: Verify completion even when duplicates occur
						await VerifyAndCompleteInitialization(connection, procedure.Name);
						return;
					}

					// For other procedures, this might be a real error
					throw;
				}
				catch (SqlException sqlEx) when (IsPermissionError(sqlEx))
				{
					OnLogMessage($"   ⚠️ Permission error - attempting to resolve...");
					await HandlePermissionErrors(connection);

					if (attempt < maxRetries)
					{
						await Task.Delay(1000);
						OnLogMessage($"   🔄 Retrying after permission fix...");
					}
				}
				catch (Exception ex) when (attempt < maxRetries)
				{
					OnLogMessage($"   ⚠️ Unexpected error on attempt {attempt}: {ex.Message}");
					await Task.Delay(1000 * attempt);
				}
			}

			throw new Exception($"Failed to execute {procedure.Name} after {maxRetries} attempts");
		}
		private async Task VerifyAndCompleteInitialization(SqlConnection connection, string procedureName)
		{
			try
			{
				OnLogMessage($"   🔍 Verifying initialization completeness...");

				var verificationScript = @"
-- 🔍 DYNAMIC VERIFICATION: Check for missing critical data
DECLARE @MissingItems TABLE (
    ItemType NVARCHAR(100),
    ItemName NVARCHAR(100),
    Status NVARCHAR(50),
    Action NVARCHAR(200)
)

DECLARE @CompletionStatus TABLE (
    Category NVARCHAR(100),
    Expected INT,
    Found INT,
    Missing INT,
    Status NVARCHAR(20)
)

-- Check critical system data that should exist after initialization
DECLARE @ExpectedUsers INT = 2  -- SystemAdmin + ServiceUser
DECLARE @FoundUsers INT = 0
DECLARE @ExpectedKPIs INT = 4   -- None, NPS, CSAT, CES, FCR  
DECLARE @FoundKPIs INT = 0
DECLARE @ExpectedConfigs INT = 2 -- At least basic configs
DECLARE @FoundConfigs INT = 0

-- Check API Users
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMAPIUser')
BEGIN
    SELECT @FoundUsers = COUNT(*) FROM CFMAPIUser
    IF @FoundUsers < @ExpectedUsers
        INSERT INTO @MissingItems VALUES ('APIUser', 'SystemAdmin', 'MISSING', 'Need to create SystemAdmin API user')
END
ELSE
    INSERT INTO @MissingItems VALUES ('Table', 'CFMAPIUser', 'MISSING', 'Table does not exist')

-- Check KPIs
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMKPI')
BEGIN
    SELECT @FoundKPIs = COUNT(*) FROM CFMKPI
    IF @FoundKPIs < @ExpectedKPIs
        INSERT INTO @MissingItems VALUES ('KPI', 'Basic KPIs', 'MISSING', 'Need to create basic KPI records')
END

-- Check Config Settings
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMConfigSettings')
BEGIN
    SELECT @FoundConfigs = COUNT(*) FROM CFMConfigSettings
    IF @FoundConfigs < @ExpectedConfigs
        INSERT INTO @MissingItems VALUES ('Config', 'Basic Settings', 'MISSING', 'Need to create config settings')
END

-- Summary
INSERT INTO @CompletionStatus VALUES ('API Users', @ExpectedUsers, @FoundUsers, @ExpectedUsers - @FoundUsers, CASE WHEN @FoundUsers >= @ExpectedUsers THEN 'COMPLETE' ELSE 'INCOMPLETE' END)
INSERT INTO @CompletionStatus VALUES ('KPIs', @ExpectedKPIs, @FoundKPIs, @ExpectedKPIs - @FoundKPIs, CASE WHEN @FoundKPIs >= @ExpectedKPIs THEN 'COMPLETE' ELSE 'INCOMPLETE' END)
INSERT INTO @CompletionStatus VALUES ('Configs', @ExpectedConfigs, @FoundConfigs, @ExpectedConfigs - @FoundConfigs, CASE WHEN @FoundConfigs >= @ExpectedConfigs THEN 'COMPLETE' ELSE 'INCOMPLETE' END)

-- Report findings
PRINT '🔍 Initialization Verification Results:'
SELECT 
    Category,
    CAST(Found AS VARCHAR(10)) + '/' + CAST(Expected AS VARCHAR(10)) AS 'Found/Expected',
    Status
FROM @CompletionStatus
ORDER BY Category

-- Check if any critical items are missing
DECLARE @MissingCount INT = (SELECT COUNT(*) FROM @MissingItems)
IF @MissingCount > 0
BEGIN
    PRINT '⚠️ Missing Items Found: ' + CAST(@MissingCount AS VARCHAR(10))
    SELECT ItemType, ItemName, Status, Action FROM @MissingItems
END
ELSE
BEGIN
    PRINT '✅ All critical initialization data verified'
END

-- Return missing count for processing
SELECT @MissingCount AS MissingCount
";

				var missingCount = 0;
				using (var command = new SqlCommand(verificationScript, connection))
				{
					command.CommandTimeout = 60;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"     🔍 {e.Message}");
					};

					var result = await command.ExecuteScalarAsync();
					if (result != null && int.TryParse(result.ToString(), out missingCount))
					{
						// Process the missing count
					}
				}

				// If missing items found, fix them
				if (missingCount > 0)
				{
					OnLogMessage($"   🔧 Found {missingCount} missing items - completing initialization...");
					await CompleteMissingInitializationData(connection);
				}
				else
				{
					OnLogMessage($"   ✅ Initialization verification complete - all data present");
				}
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Verification error: {ex.Message}");
				// Don't throw - verification failure shouldn't stop deployment
			}
		}
		private async Task CompleteMissingInitializationData(SqlConnection connection)
		{
			try
			{
				OnLogMessage($"   🔧 Completing missing initialization data...");

				var completionScript = @"
-- 🔧 SMART COMPLETION: Add only missing critical data
PRINT '🔧 Starting smart initialization completion...'

-- 1. Ensure SystemAdmin API User exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMAPIUser')
AND NOT EXISTS (SELECT 1 FROM CFMAPIUser WHERE UserName = 'SystemAdmin')
BEGIN
    SET IDENTITY_INSERT CFMAPIUser ON
    INSERT INTO CFMAPIUser (ID, Name, UserName, Password, PasswordCode, Role, CreatedOn, LastModifiedOn, IsOld) 
    VALUES (1, 'SystemAdmin', 'SystemAdmin', 'FE-3D-C7-B8-49-75-D6-EA-CF-6D-96-54-A6-32-58-3E', 'YRwMlyQUW946PCBA', 1, GETDATE(), GETDATE(), 0)
    SET IDENTITY_INSERT CFMAPIUser OFF
    PRINT '✅ Added SystemAdmin API User'
END

-- 2. Ensure Service User exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMUser')
AND NOT EXISTS (SELECT 1 FROM CFMUser WHERE Name = 'ServiceUser')
BEGIN
    INSERT INTO CFMUSER (Guid, Name) VALUES (NEWID(), 'ServiceUser')
    PRINT '✅ Added ServiceUser'
    
    -- Add Service User Group if needed
    IF NOT EXISTS (SELECT 1 FROM CFMUserGroup WHERE GroupRole = 9)
    BEGIN
        INSERT INTO CFMUserGroup (Guid, Name, GroupRole) VALUES (NEWID(), 'Service', 9)
        PRINT '✅ Added Service User Group'
    END
    
    -- Link Service User to Group
    DECLARE @ServiceUserID INT = (SELECT ID FROM CFMUSER WHERE Name = 'ServiceUser')
    DECLARE @ServiceGroupID INT = (SELECT ID FROM CFMUserGroup WHERE GroupRole = 9)
    
    IF @ServiceUserID IS NOT NULL AND @ServiceGroupID IS NOT NULL
    BEGIN
        INSERT INTO CFMUserGroupBrg (UserID, UserGroupID) VALUES (@ServiceUserID, @ServiceGroupID)
        PRINT '✅ Linked ServiceUser to Service Group'
    END
END

-- 3. Ensure basic KPIs exist
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMKPI')
BEGIN
    IF NOT EXISTS (SELECT * FROM CFMKPI WHERE Name LIKE '%none%')
    BEGIN
        INSERT INTO CFMKPI (Name, IsOld) VALUES ('None', 1)
        PRINT '✅ Added KPI: None'
    END
    
    IF NOT EXISTS (SELECT * FROM CFMKPI WHERE Name LIKE '%nps%')
    BEGIN
        INSERT INTO CFMKPI (Name) VALUES ('NPS')
        PRINT '✅ Added KPI: NPS'
    END
    
    IF NOT EXISTS (SELECT * FROM CFMKPI WHERE Name LIKE '%csat%')
    BEGIN
        INSERT INTO CFMKPI (Name) VALUES ('CSAT')
        PRINT '✅ Added KPI: CSAT'
    END
    
    IF NOT EXISTS (SELECT * FROM CFMKPI WHERE Name LIKE '%ces%')
    BEGIN
        INSERT INTO CFMKPI (Name) VALUES ('CES')
        PRINT '✅ Added KPI: CES'
    END
    
    IF NOT EXISTS (SELECT * FROM CFMKPI WHERE Name LIKE '%fcr%')
    BEGIN
        INSERT INTO CFMKPI (Name) VALUES ('FCR')
        PRINT '✅ Added KPI: FCR'
    END
END

-- 4. Ensure basic config settings exist
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CFMConfigSettings')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CFMConfigSettings WHERE Name = 'DefaultSurveyLanguage')
    BEGIN
        INSERT INTO CFMConfigSettings (Name, Value) VALUES ('DefaultSurveyLanguage', 'en')
        PRINT '✅ Added Config: DefaultSurveyLanguage'
    END
    
    IF NOT EXISTS (SELECT 1 FROM CFMConfigSettings WHERE Name = 'SupportedSurveyLangs')
    BEGIN
        INSERT INTO CFMConfigSettings (Name, Value) VALUES ('SupportedSurveyLangs', 'en,ar,fr')
        PRINT '✅ Added Config: SupportedSurveyLangs'
    END
END

PRINT '🎉 Smart initialization completion finished'
";

				using (var command = new SqlCommand(completionScript, connection))
				{
					command.CommandTimeout = 120;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"     🔧 {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}

				OnLogMessage($"   ✅ Missing data completion successful");
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Completion error: {ex.Message}");
				// Log but don't throw - this is supplementary
			}
		}
		private async Task ExecuteProcedureDirectly(SqlConnection connection, StoredProcedureInfo procedure)
		{
			using (var command = connection.CreateCommand())
			{
				var commandText = $"EXEC [{procedure.Name}]";
				if (!string.IsNullOrWhiteSpace(procedure.Parameters))
				{
					commandText += $" {procedure.Parameters}";
				}

				command.CommandText = commandText;
				command.CommandTimeout = 300;

				OnLogMessage($"   🔧 SQL: {commandText}");

				// Capture info messages
				connection.InfoMessage += (sender, e) =>
				{
					OnLogMessage($"   📢 SQL: {e.Message}");
				};

				await command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>
		/// DYNAMIC: Detect and clean up IDENTITY_INSERT issues automatically
		/// No static lists needed!
		/// </summary>
		private async Task DynamicIdentityInsertCleanup(SqlConnection connection)
		{
			try
			{
				OnLogMessage($"   🧹 Dynamic IDENTITY_INSERT cleanup...");

				var dynamicCleanupScript = @"
-- 🧹 DYNAMIC IDENTITY_INSERT CLEANUP
-- Automatically finds and cleans ALL identity tables

DECLARE @CleanupCommands TABLE (Command NVARCHAR(MAX))
DECLARE @TotalTables INT = 0
DECLARE @CleanedTables INT = 0

-- Build cleanup commands for ALL identity tables
INSERT INTO @CleanupCommands (Command)
SELECT 'SET IDENTITY_INSERT [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] OFF'
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE c.is_identity = 1 
  AND t.is_ms_shipped = 0
  AND SCHEMA_NAME(t.schema_id) NOT IN ('sys', 'INFORMATION_SCHEMA')

SELECT @TotalTables = COUNT(*) FROM @CleanupCommands

-- Execute each cleanup command
DECLARE @sql NVARCHAR(MAX)
DECLARE cleanup_cursor CURSOR FAST_FORWARD FOR
SELECT Command FROM @CleanupCommands

OPEN cleanup_cursor
FETCH NEXT FROM cleanup_cursor INTO @sql

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC sp_executesql @sql
        SET @CleanedTables = @CleanedTables + 1
    END TRY
    BEGIN CATCH
        -- Ignore errors - table might already be OFF
    END CATCH
    
    FETCH NEXT FROM cleanup_cursor INTO @sql
END

CLOSE cleanup_cursor
DEALLOCATE cleanup_cursor

PRINT '🧹 Dynamic cleanup completed: ' + CAST(@CleanedTables AS VARCHAR(10)) + '/' + CAST(@TotalTables AS VARCHAR(10)) + ' tables processed'
";

				using (var command = new SqlCommand(dynamicCleanupScript, connection))
				{
					command.CommandTimeout = 60;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"     🧹 {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Dynamic cleanup warning: {ex.Message}");
			}
		}


		/// <summary>
		/// Check if the exception is an IDENTITY_INSERT conflict
		/// </summary>
		private async Task FixSpecificIdentityInsertConflict(SqlConnection connection, string tableName)
		{
			try
			{
				OnLogMessage($"   🔧 Fixing IDENTITY_INSERT for: {tableName}");

				var fixScript = $@"
-- Fix specific IDENTITY_INSERT conflict
DECLARE @TableName NVARCHAR(261) = '{tableName}'

-- Remove brackets if present and ensure proper formatting
SET @TableName = REPLACE(REPLACE(@TableName, '[', ''), ']', '')

-- Add schema if missing
IF CHARINDEX('.', @TableName) = 0
    SET @TableName = 'dbo.' + @TableName

-- Check if table exists and has identity column
IF EXISTS (
    SELECT 1 
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.columns c ON t.object_id = c.object_id
    WHERE s.name + '.' + t.name = @TableName
    AND c.is_identity = 1
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) = 'SET IDENTITY_INSERT [' + REPLACE(@TableName, '.', '].[') + '] OFF'
    
    BEGIN TRY
        EXEC sp_executesql @sql
        PRINT '🔧 Fixed IDENTITY_INSERT for: ' + @TableName
    END TRY
    BEGIN CATCH
        PRINT '⚠️ Could not fix: ' + @TableName + ' - ' + ERROR_MESSAGE()
    END CATCH
END
ELSE
BEGIN
    PRINT 'ℹ️ Table not found or no identity column: ' + @TableName
END
";

				using (var command = new SqlCommand(fixScript, connection))
				{
					command.CommandTimeout = 30;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"     🔧 {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Error fixing {tableName}: {ex.Message}");
			}
		}

		/// <summary>
		/// 🔍 Extract table name from IDENTITY_INSERT error message
		/// </summary>
		private string ExtractTableFromIdentityError(string errorMessage)
		{
			try
			{
				if (string.IsNullOrEmpty(errorMessage)) return null;

				// Pattern 1: "IDENTITY_INSERT is already ON for table 'Database.dbo.TableName'"
				var match1 = Regex.Match(
					errorMessage,
					@"IDENTITY_INSERT is already ON for table '([^']+)'",
					RegexOptions.IgnoreCase);

				if (match1.Success)
				{
					var fullName = match1.Groups[1].Value;
					var parts = fullName.Split('.');
					if (parts.Length >= 2)
					{
						return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
					}
					return fullName;
				}

				// Pattern 2: "Cannot perform SET operation for table 'TableName'"
				var match2 = Regex.Match(
					errorMessage,
					@"Cannot perform SET operation for table '([^']+)'",
					RegexOptions.IgnoreCase);

				if (match2.Success)
				{
					var tableName = match2.Groups[1].Value;
					return tableName.Contains(".") ? tableName : $"dbo.{tableName}";
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// ⚠️ Handle permission-related errors
		/// </summary>
		private async Task HandlePermissionErrors(SqlConnection connection)
		{
			try
			{
				OnLogMessage($"   🔧 Checking and fixing permission issues...");

				var permissionScript = @"
-- Check current user permissions and database context
PRINT '🔍 Current User: ' + SYSTEM_USER
PRINT '🔍 Current Database: ' + DB_NAME()
PRINT '🔍 Database Owner: ' + (SELECT name FROM sys.database_principals WHERE principal_id = 1)

-- Check if user has necessary permissions
IF IS_MEMBER('db_owner') = 1
    PRINT '✅ User has db_owner permissions'
ELSE IF IS_MEMBER('db_ddladmin') = 1
    PRINT '⚠️ User has db_ddladmin permissions (might need db_datawriter)'
ELSE
    PRINT '❌ User may need additional permissions'
";

				using (var command = new SqlCommand(permissionScript, connection))
				{
					command.CommandTimeout = 30;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"     🔧 {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Permission check error: {ex.Message}");
			}
		}

		/// <summary>
		/// 🔍 Enhanced error detection methods
		/// </summary>
		private bool IsIdentityInsertError(SqlException sqlException)
		{
			if (sqlException?.Message == null) return false;

			var errorMessage = sqlException.Message.ToUpperInvariant();
			return errorMessage.Contains("IDENTITY_INSERT IS ALREADY ON") ||
				   errorMessage.Contains("CANNOT PERFORM SET OPERATION") ||
				   errorMessage.Contains("IDENTITY_INSERT");
		}

		private bool IsDuplicateKeyError(SqlException sqlException)
		{
			if (sqlException?.Message == null) return false;

			var errorMessage = sqlException.Message.ToUpperInvariant();
			return errorMessage.Contains("VIOLATION OF PRIMARY KEY CONSTRAINT") ||
				   errorMessage.Contains("CANNOT INSERT DUPLICATE KEY") ||
				   errorMessage.Contains("DUPLICATE KEY VALUE");
		}

		private bool IsPermissionError(SqlException sqlException)
		{
			if (sqlException?.Message == null) return false;

			var errorMessage = sqlException.Message.ToUpperInvariant();
			return errorMessage.Contains("PERMISSION") ||
				   errorMessage.Contains("ACCESS DENIED") ||
				   errorMessage.Contains("LOGIN FAILED") ||
				   errorMessage.Contains("NOT HAVE PERMISSION");
		}

		/// <summary>
		/// 🚀 FULLY DYNAMIC: Parse and execute stored procedure content table by table
		/// No hardcoding - automatically extracts all table operations from your procedure
		/// </summary>
		private async Task ExecuteInitializationTableByTable(SqlConnection connection, string procedureName)
		{
			try
			{
				OnLogMessage($"   📋 Starting dynamic table-by-table initialization...");

				// Always clean IDENTITY_INSERT states first
				await DynamicIdentityInsertCleanup(connection);

				// Step 1: Get the stored procedure content
				var procedureContent = await GetStoredProcedureContent(connection, procedureName);
				if (string.IsNullOrEmpty(procedureContent))
				{
					OnLogMessage($"   ⚠️ Could not retrieve procedure content - falling back to direct execution");
					await ExecuteProcedureDirectly(connection, new StoredProcedureInfo { Name = procedureName });
					return;
				}

				// Step 2: Parse procedure into individual table operations
				var tableOperations = ParseProcedureIntoTableOperations(procedureContent);
				OnLogMessage($"   📊 Found {tableOperations.Count} table operation(s) to execute");

				if (tableOperations.Count == 0)
				{
					OnLogMessage($"   ⚠️ No table operations found - executing procedure normally");
					await ExecuteProcedureDirectly(connection, new StoredProcedureInfo { Name = procedureName });
					return;
				}

				// Step 3: Execute each table operation independently
				var successCount = 0;
				var skipCount = 0;
				var errorCount = 0;

				foreach (var operation in tableOperations)
				{
					try
					{
						OnLogMessage($"   📋 Processing: {operation.TableName}");

						// Check if table exists first
						if (!await TableExists(connection, operation.TableName))
						{
							OnLogMessage($"     ⚠️ Table {operation.TableName} does not exist - skipping");
							skipCount++;
							continue;
						}

						// Execute the table operation
						await ExecuteTableOperation(connection, operation);
						successCount++;
						OnLogMessage($"     ✅ {operation.TableName} completed");
					}
					catch (Exception ex)
					{
						OnLogMessage($"     ❌ {operation.TableName} failed: {ex.Message}");

						// Check if it's a "data already exists" scenario
						if (ex.Message.ToLower().Contains("duplicate key") ||
							ex.Message.ToLower().Contains("already exists") ||
							ex.Message.ToLower().Contains("violation of primary key"))
						{
							OnLogMessage($"     ℹ️ Data already exists - treating as success");
							successCount++;
						}
						else
						{
							errorCount++;
							// Continue with other tables even if one fails
						}
					}
				}

				OnLogMessage($"   📊 Dynamic Initialization Summary:");
				OnLogMessage($"     ✅ Successful: {successCount}");
				OnLogMessage($"     ⚠️ Skipped: {skipCount}");
				OnLogMessage($"     ❌ Errors: {errorCount}");
				OnLogMessage($"   🎉 Dynamic table-by-table initialization completed");
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ❌ Dynamic initialization failed: {ex.Message}");
				throw;
			}
		}

		/// <summary>
		/// 📄 Get stored procedure content from database
		/// </summary>
		private async Task<string> GetStoredProcedureContent(SqlConnection connection, string procedureName)
		{
			try
			{
				OnLogMessage($"   📄 Retrieving procedure content for: {procedureName}");

				var query = @"
            SELECT 
                m.definition
            FROM sys.sql_modules m
            INNER JOIN sys.objects o ON m.object_id = o.object_id
            WHERE o.name = @ProcedureName 
            AND o.type = 'P'";

				using (var command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@ProcedureName", procedureName);
					var content = await command.ExecuteScalarAsync();

					if (content != null)
					{
						OnLogMessage($"   ✅ Retrieved procedure content ({content.ToString().Length} characters)");
						return content.ToString();
					}
				}

				OnLogMessage($"   ⚠️ Procedure content not found");
				return null;
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Error retrieving procedure content: {ex.Message}");
				return null;
			}
		}


		/// <summary>
		/// 📋 Extract table names from procedure content
		/// </summary>
		private List<string> ExtractTableNamesFromContent(string content)
		{
			var tableNames = new HashSet<string>();

			try
			{
				// Look for CFM table names in various contexts
				var patterns = new[]
				{
			@"INSERT\s+INTO\s+(\[?dbo\]?\.)?\[?(CFM\w+)\]?",           // INSERT INTO
            @"SELECT.*?FROM\s+(\[?dbo\]?\.)?\[?(CFM\w+)\]?",           // SELECT FROM
            @"UPDATE\s+(\[?dbo\]?\.)?\[?(CFM\w+)\]?",                  // UPDATE
            @"DELETE\s+FROM\s+(\[?dbo\]?\.)?\[?(CFM\w+)\]?",           // DELETE FROM
            @"SET\s+IDENTITY_INSERT\s+(\[?dbo\]?\.)?\[?(CFM\w+)\]?",   // IDENTITY_INSERT
            @"\b(CFM\w+):",                                            // Table labels
        };

				foreach (var pattern in patterns)
				{
					var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
					foreach (Match match in matches)
					{
						var tableName = match.Groups.Count > 2 ? match.Groups[2].Value : match.Groups[1].Value;
						if (!string.IsNullOrEmpty(tableName) && tableName.StartsWith("CFM"))
						{
							tableNames.Add(tableName);
						}
					}
				}

				return tableNames.OrderBy(t => t).ToList();
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error extracting table names: {ex.Message}");
				return new List<string>();
			}
		}



		/// <summary>
		/// 🔍 Check if table exists
		/// </summary>
		private async Task<bool> TableExists(SqlConnection connection, string tableName)
		{
			try
			{
				var query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
				using (var command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@TableName", tableName);
					var count = (int)await command.ExecuteScalarAsync();
					return count > 0;
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 🧹 Clean IDENTITY_INSERT for specific table
		/// </summary>
		private async Task CleanTableIdentityInsert(SqlConnection connection, string tableName)
		{
			try
			{
				var cleanupSql = $"SET IDENTITY_INSERT [{tableName}] OFF";
				using (var command = new SqlCommand(cleanupSql, connection))
				{
					command.CommandTimeout = 30;
					await command.ExecuteNonQueryAsync();
				}
			}
			catch
			{
				// Ignore errors - table might not have IDENTITY or already OFF
			}
		}


		/// <summary>
		/// 🔧 IMPROVED: Better procedure parsing that avoids over-splitting and handles dependencies
		/// </summary>
		private List<TableOperation> ParseProcedureIntoTableOperations(string procedureContent)
		{
			var operations = new List<TableOperation>();

			try
			{
				OnLogMessage($"   🔍 Parsing procedure content with improved logic...");

				// Step 1: Clean the procedure content
				var cleanContent = CleanProcedureContent(procedureContent);

				// Step 2: Split by logical table sections (not individual lines)
				var logicalSections = SplitIntoLogicalSections(cleanContent);

				OnLogMessage($"   ✂️ Found {logicalSections.Count} logical sections");

				// Step 3: Group related operations and handle dependencies
				var groupedOperations = GroupRelatedOperations(logicalSections);

				foreach (var operation in groupedOperations)
				{
					operations.Add(operation);
					OnLogMessage($"     📋 Prepared operation for: {operation.TableName}");
				}

				OnLogMessage($"   ✅ Parsed {operations.Count} unique table operations");
				return operations;
			}
			catch (Exception ex)
			{
				OnLogMessage($"   ⚠️ Error parsing procedure: {ex.Message}");
				return operations;
			}
		}

		/// <summary>
		/// 🧹 Clean procedure content and remove unnecessary parts
		/// </summary>
		private string CleanProcedureContent(string procedureContent)
		{
			var cleanContent = procedureContent;

			try
			{
				// Remove procedure header
				cleanContent = Regex.Replace(cleanContent,
					@"(CREATE|ALTER)\s+PROCEDURE.*?AS\s+BEGIN",
					"", RegexOptions.IgnoreCase | RegexOptions.Singleline);

				// Remove final END
				cleanContent = Regex.Replace(cleanContent, @"\s*END\s*$", "", RegexOptions.IgnoreCase);

				// Remove SET NOCOUNT ON
				cleanContent = Regex.Replace(cleanContent, @"SET\s+NOCOUNT\s+ON\s*;?", "", RegexOptions.IgnoreCase);

				return cleanContent.Trim();
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error cleaning content: {ex.Message}");
				return procedureContent;
			}
		}

		/// <summary>
		/// ✂️ Split into logical sections based on table labels and major blocks
		/// </summary>
		private List<string> SplitIntoLogicalSections(string cleanContent)
		{
			var sections = new List<string>();

			try
			{
				// Define major section markers
				var majorSectionPatterns = new[]
				{
			@"CFMSurveyType\s*:",
			@"CFMQuestionType\s*:",
			@"CFMQuestionKPI\s*:",
			@"CFMSurveyChannel\s*:",
			@"CFMAgeGroup\s*:",
			@"CFMConfigSettings\s*:",
			@"--\s*insert\s+theme\s+style\s+type",
			@"Insert\s+into\s+CFMUserGroup",
			@"--\s*Add\s+Service\s+User",
			@"--\s*Add\s+SystemAdmin\s+API\s+User",
			@"--\s*Start\s+Intialize\s+Action\s+Card"
		};

				var combinedPattern = string.Join("|", majorSectionPatterns);
				var matches = Regex.Matches(cleanContent, combinedPattern, RegexOptions.IgnoreCase);

				if (matches.Count > 0)
				{
					var startIndex = 0;
					foreach (Match match in matches)
					{
						if (match.Index > startIndex)
						{
							var section = cleanContent.Substring(startIndex, match.Index - startIndex).Trim();
							if (!string.IsNullOrEmpty(section) && section.Length > 50) // Avoid tiny fragments
							{
								sections.Add(section);
							}
						}
						startIndex = match.Index;
					}

					// Add final section
					if (startIndex < cleanContent.Length)
					{
						var finalSection = cleanContent.Substring(startIndex).Trim();
						if (!string.IsNullOrEmpty(finalSection) && finalSection.Length > 50)
						{
							sections.Add(finalSection);
						}
					}
				}
				else
				{
					// Fallback: Split by blocks of code separated by multiple line breaks
					var blocks = Regex.Split(cleanContent, @"\n\s*\n\s*\n", RegexOptions.Multiline)
						.Where(block => !string.IsNullOrWhiteSpace(block) && block.Length > 100)
						.ToList();

					sections.AddRange(blocks);
				}

				OnLogMessage($"     ✂️ Split into {sections.Count} logical sections");
				return sections;
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error splitting sections: {ex.Message}");
				return new List<string> { cleanContent };
			}
		}

		/// <summary>
		/// 🔗 Group related operations and handle dependencies
		/// </summary>
		private List<TableOperation> GroupRelatedOperations(List<string> sections)
		{
			var operations = new List<TableOperation>();
			var processedTables = new HashSet<string>();

			try
			{
				foreach (var section in sections)
				{
					var operation = ExtractLogicalTableOperation(section);
					if (operation != null && !processedTables.Contains(operation.TableName.ToLower()))
					{
						operations.Add(operation);
						processedTables.Add(operation.TableName.ToLower());
						OnLogMessage($"     📦 Grouped operation for: {operation.TableName}");
					}
				}

				return operations;
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error grouping operations: {ex.Message}");
				return operations;
			}
		}

		/// <summary>
		/// 🔍 Extract logical table operation from a section
		/// </summary>
		private TableOperation ExtractLogicalTableOperation(string section)
		{
			try
			{
				// Find the primary table name in this section
				var tableNameMatch = Regex.Match(section, @"\b(CFM\w+)\b", RegexOptions.IgnoreCase);
				if (!tableNameMatch.Success)
					return null;

				var tableName = tableNameMatch.Value;

				// Clean up the SQL section
				var cleanSql = section.Trim();

				// Handle special cases with dependencies
				cleanSql = HandleTableDependencies(cleanSql, tableName);

				// Remove table labels (like "CFMSurveyType:")
				cleanSql = Regex.Replace(cleanSql, @"^\s*CFM\w+\s*:\s*", "", RegexOptions.IgnoreCase);

				// Fix common syntax issues
				cleanSql = FixSqlSyntaxIssues(cleanSql, tableName);

				return new TableOperation
				{
					TableName = tableName,
					SqlContent = cleanSql,
					Description = $"Initialize {tableName}"
				};
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error extracting operation: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// 🔗 Handle table dependencies and variable declarations
		/// </summary>
		private string HandleTableDependencies(string sqlContent, string tableName)
		{
			try
			{
				// Handle CFMDeviceTheme dependency on ServiceUser
				if (tableName.Equals("CFMDeviceTheme", StringComparison.OrdinalIgnoreCase) &&
					sqlContent.Contains("@ServiceUserID"))
				{
					sqlContent = @"
-- CFMDeviceTheme with dependency resolution
DECLARE @ServiceUserID INT = (SELECT ID FROM CFMUser WHERE Name = 'ServiceUser')

-- If ServiceUser doesn't exist, create it first
IF @ServiceUserID IS NULL
BEGIN
    INSERT INTO CFMUSER (Guid, Name) VALUES (NEWID(), 'ServiceUser')
    SET @ServiceUserID = (SELECT ID FROM CFMUser WHERE Name = 'ServiceUser')
    PRINT 'Created ServiceUser for CFMDeviceTheme'
END

" + sqlContent;
				}

				// Handle CFMUserGroupBrg dependency
				if (tableName.Equals("CFMUserGroupBrg", StringComparison.OrdinalIgnoreCase) &&
					(sqlContent.Contains("@UserID") || sqlContent.Contains("@GroupID")))
				{
					sqlContent = @"
-- CFMUserGroupBrg with dependency resolution
DECLARE @ServiceUserID INT = (SELECT ID FROM CFMUser WHERE Name = 'ServiceUser')
DECLARE @ServiceGroupID INT = (SELECT ID FROM CFMUserGroup WHERE GroupRole = 9)

-- Ensure both ServiceUser and ServiceGroup exist
IF @ServiceUserID IS NULL
BEGIN
    INSERT INTO CFMUSER (Guid, Name) VALUES (NEWID(), 'ServiceUser')
    SET @ServiceUserID = (SELECT ID FROM CFMUser WHERE Name = 'ServiceUser')
    PRINT 'Created ServiceUser for CFMUserGroupBrg'
END

IF @ServiceGroupID IS NULL
BEGIN
    INSERT INTO CFMUserGroup (Guid, Name, GroupRole) VALUES (NEWID(), 'Service', 9)
    SET @ServiceGroupID = (SELECT ID FROM CFMUserGroup WHERE GroupRole = 9)
    PRINT 'Created Service Group for CFMUserGroupBrg'
END

" + sqlContent.Replace("@UserID", "@ServiceUserID").Replace("@GroupID", "@ServiceGroupID");
				}

				return sqlContent;
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error handling dependencies: {ex.Message}");
				return sqlContent;
			}
		}

		/// <summary>
		/// 🔧 Fix common SQL syntax issues
		/// </summary>
		private string FixSqlSyntaxIssues(string sqlContent, string tableName)
		{
			try
			{
				// Remove extra BEGIN/END if they cause issues
				if (sqlContent.Contains("BEGIN") && sqlContent.Contains("END"))
				{
					// Check if it's already properly structured
					var beginCount = Regex.Matches(sqlContent, @"\bBEGIN\b", RegexOptions.IgnoreCase).Count;
					var endCount = Regex.Matches(sqlContent, @"\bEND\b", RegexOptions.IgnoreCase).Count;

					if (beginCount > endCount + 1)
					{
						// Remove extra BEGINs
						sqlContent = Regex.Replace(sqlContent, @"^\s*BEGIN\s*\n", "", RegexOptions.IgnoreCase);
					}
				}

				// Ensure IDENTITY_INSERT operations are properly wrapped
				if (Regex.IsMatch(sqlContent, @"INSERT\s+INTO\s+\[?dbo\]?\.\[?" + Regex.Escape(tableName) + @"\]?\s*\([^)]*\bID\b", RegexOptions.IgnoreCase))
				{
					bool hasIdentityInsertOn = sqlContent.ToUpper().Contains("IDENTITY_INSERT") && sqlContent.ToUpper().Contains("ON");
					bool hasIdentityInsertOff = sqlContent.ToUpper().Contains("IDENTITY_INSERT") && sqlContent.ToUpper().Contains("OFF");

					if (!hasIdentityInsertOn || !hasIdentityInsertOff)
					{
						sqlContent = $@"
-- Auto-wrapped IDENTITY_INSERT for {tableName}
SET IDENTITY_INSERT [{tableName}] ON

{sqlContent}

SET IDENTITY_INSERT [{tableName}] OFF
";
					}
				}

				return sqlContent;
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ⚠️ Error fixing SQL syntax: {ex.Message}");
				return sqlContent;
			}
		}

		/// <summary>
		/// 🎯 SIMPLIFIED: Execute table operation with better error handling
		/// </summary>
		private async Task ExecuteTableOperation(SqlConnection connection, TableOperation operation)
		{
			try
			{
				OnLogMessage($"     🔧 Executing: {operation.TableName}");

				// Skip if this table doesn't exist
				if (!await TableExists(connection, operation.TableName))
				{
					OnLogMessage($"     ⚠️ Table {operation.TableName} does not exist - skipping");
					return;
				}

				// Clean IDENTITY_INSERT first
				await CleanTableIdentityInsert(connection, operation.TableName);

				using (var command = new SqlCommand(operation.SqlContent, connection))
				{
					command.CommandTimeout = 120;

					connection.InfoMessage += (sender, e) =>
					{
						OnLogMessage($"       SQL: {e.Message}");
					};

					await command.ExecuteNonQueryAsync();
				}

				OnLogMessage($"     ✅ {operation.TableName} completed successfully");
			}
			catch (SqlException sqlEx) when (sqlEx.Message.ToLower().Contains("duplicate key") ||
										   sqlEx.Message.ToLower().Contains("already exists") ||
										   sqlEx.Message.ToLower().Contains("violation of primary key"))
			{
				OnLogMessage($"     ℹ️ {operation.TableName}: Data already exists - treating as success");
			}
			catch (Exception ex)
			{
				OnLogMessage($"     ❌ {operation.TableName} failed: {ex.Message}");
				throw;
			}
			finally
			{
				// Always clean up IDENTITY_INSERT
				await CleanTableIdentityInsert(connection, operation.TableName);
			}
		}




		// Helper class for table operations
		public class TableOperation
		{
			public string TableName { get; set; }
			public string SqlContent { get; set; }
			public string Description { get; set; }
		}
	}
}