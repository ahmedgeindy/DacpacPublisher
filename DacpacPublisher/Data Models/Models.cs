using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DacpacPublisher.Data_Models
{
	// ENHANCED: Your existing PublisherConfiguration with smart features added
	public class PublisherConfiguration
	{
		// Existing properties (unchanged)
		public List<string> SynonymTargetDatabases { get; set; } = new List<string>();

		public string ServerName { get; set; } = string.Empty;
		public bool WindowsAuth { get; set; } = true;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;
		public string DacpacPath { get; set; } = string.Empty;
		public bool CreateSynonyms { get; set; } = false;
		public string SynonymSourceDb { get; set; } = string.Empty;
		public string JobScriptsFolder { get; set; } = string.Empty;
		public bool CreateSqlAgentJobs { get; set; } = false;
		public string JobOwnerLoginName { get; set; } = string.Empty;
		public bool ExecuteProcedures { get; set; } = false;
		public List<StoredProcedureInfo> StoredProcedures { get; set; } = new List<StoredProcedureInfo>();
		public bool EnableMultipleDatabases { get; set; } = false;
		public bool CreateBackupBeforeDeployment { get; set; } = true;
		public string BackupPath { get; set; } = string.Empty;
		public List<DatabaseDeploymentTarget> DeploymentTargets { get; set; } = new List<DatabaseDeploymentTarget>();
		public List<DeploymentHistory> History { get; set; } = new List<DeploymentHistory>();

		// NEW: Smart procedure features
		public List<SmartStoredProcedureInfo> SmartProcedures { get; set; } = new List<SmartStoredProcedureInfo>();
		public string SmartDeploymentStrategy { get; set; } = "Full Deployment";
		public bool UseSmartProcedures { get; set; } = true;
		public bool ValidateProceduresBeforeDeployment { get; set; } = true;
		public int MaxConcurrentProcedures { get; set; } = 1;
		public int ProcedureTimeoutSeconds { get; set; } = 300;

		// ENHANCED: Utility methods
		public string GetDeploymentSummary()
		{
			var summary = new StringBuilder();
			summary.AppendLine("Deployment Configuration Summary:");
			summary.AppendLine($"- Server: {ServerName}");
			summary.AppendLine($"- Primary Database: {Database}");
			summary.AppendLine($"- Multiple Databases: {EnableMultipleDatabases}");
			summary.AppendLine($"- DACPAC: {Path.GetFileName(DacpacPath)}");
			summary.AppendLine($"- Smart Procedures: {UseSmartProcedures}");

			if (UseSmartProcedures && SmartProcedures.Any())
			{
				summary.AppendLine($"- Total Procedures: {SmartProcedures.Count}");
				var categories = SmartProcedures.GroupBy(p => p.Category).Select(g => $"  {g.Key}: {g.Count()}");
				summary.AppendLine($"- By Category: {string.Join(", ", categories)}");
			}

			if (EnableMultipleDatabases && DeploymentTargets.Count > 1)
			{
				summary.AppendLine($"- Deployment Targets: {DeploymentTargets.Count}");
				foreach (var target in DeploymentTargets) summary.AppendLine($"  * {target.GetSummary()}");
			}

			return summary.ToString();
		}

		public List<string> ValidateConfiguration()
		{
			var errors = new List<string>();

			if (string.IsNullOrWhiteSpace(ServerName))
				errors.Add("Server name is required");

			if (string.IsNullOrWhiteSpace(Database))
				errors.Add("Database name is required");

			if (string.IsNullOrWhiteSpace(DacpacPath) || !File.Exists(DacpacPath))
				errors.Add("Valid DACPAC file path is required");

			if (EnableMultipleDatabases && DeploymentTargets.Count < 2)
				errors.Add("Multiple databases enabled but less than 2 targets configured");

			if (UseSmartProcedures && SmartProcedures.Any())
			{
				var duplicateNames = SmartProcedures.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key);
				if (duplicateNames.Any())
					errors.Add($"Duplicate procedure names found: {string.Join(", ", duplicateNames)}");

				var invalidOrders = SmartProcedures.Where(p => p.ExecutionOrder <= 0);
				if (invalidOrders.Any())
					errors.Add("All procedures must have execution order greater than 0");
			}

			return errors;
		}
	}

	// ENHANCED: Your existing StoredProcedureInfo with smart features
	public class StoredProcedureInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Parameters { get; set; } = string.Empty;

		// NEW: Smart features (backward compatible)
		public int ExecutionOrder { get; set; } = 100;
		public string Description { get; set; } = string.Empty;

		public override string ToString()
		{
			return string.IsNullOrEmpty(Parameters) ? Name : $"{Name} ({Parameters})";
		}

		// NEW: Convert to smart procedure
		public SmartStoredProcedureInfo ToSmartProcedure()
		{
			return new SmartStoredProcedureInfo
			{
				Name = Name,
				Parameters = Parameters,
				ExecutionOrder = ExecutionOrder,
				Description = Description,
				ExecuteOnDatabase1 = true,
				ExecuteOnDatabase2 = false,
				Category = ProcedureCategory.General
			};
		}
	}

	// ENHANCED: Your existing DatabaseDeploymentTarget with smart features
	public class DatabaseDeploymentTarget
	{
		// Existing properties (unchanged)
		public string Name { get; set; }
		public string ServerName { get; set; }
		public string Database { get; set; }
		public string DacpacPath { get; set; }
		public bool IsEnabled { get; set; } = true;
		public string ProcedureStrategy { get; set; } = "All"; // "All", "None", "Minimal"
		public List<StoredProcedureInfo> SpecificProcedures { get; set; } = new List<StoredProcedureInfo>();

		// NEW: Smart procedure support
		public List<SmartStoredProcedureInfo> SmartProcedures { get; set; } = new List<SmartStoredProcedureInfo>();
		public bool CreateBackup { get; set; } = false;
		public string BackupPath { get; set; }
		public bool RunPostDeploymentValidation { get; set; } = true;

		// NEW: Utility methods
		public List<SmartStoredProcedureInfo> GetOrderedProcedures()
		{
			return SmartProcedures?.OrderBy(p => p.ExecutionOrder).ToList() ?? new List<SmartStoredProcedureInfo>();
		}

		public List<SmartStoredProcedureInfo> GetProceduresByCategory(ProcedureCategory category)
		{
			return SmartProcedures?.Where(p => p.Category == category).OrderBy(p => p.ExecutionOrder).ToList() ??
				   new List<SmartStoredProcedureInfo>();
		}

		public string GetSummary()
		{
			var procedureCount = SmartProcedures?.Count ?? SpecificProcedures?.Count ?? 0;
			var categories = SmartProcedures?.GroupBy(p => p.Category).Select(g => $"{g.Key}: {g.Count()}").ToList() ??
							 new List<string>();

			return $"{Name} ({Database}) - DACPAC: {Path.GetFileName(DacpacPath)}, Procedures: {procedureCount}" +
				   (categories.Any() ? $" [{string.Join(", ", categories)}]" : "");
		}

		// ENHANCED: Get procedures for execution (supports both legacy and smart)
		public List<StoredProcedureInfo> GetExecutableProcedures()
		{
			// Prefer smart procedures if available
			if (SmartProcedures?.Any() == true)
				return SmartProcedures.OrderBy(p => p.ExecutionOrder)
					.Select(sp => new StoredProcedureInfo
					{
						Name = sp.Name,
						Parameters = sp.Parameters,
						ExecutionOrder = sp.ExecutionOrder,
						Description = sp.Description
					}).ToList();

			// Fallback to specific procedures
			return SpecificProcedures ?? new List<StoredProcedureInfo>();
		}
	}

	// NEW: Smart stored procedure info (enhanced version)
	public class SmartStoredProcedureInfo
	{
		public string Name { get; set; }
		public bool ExecuteOnDatabase1 { get; set; } = true;
		public bool ExecuteOnDatabase2 { get; set; } = true;
		public string Description { get; set; }
		public ProcedureCategory Category { get; set; } = ProcedureCategory.General;
		public int ExecutionOrder { get; set; } = 100;
		public string Parameters { get; set; }

		public override string ToString()
		{
			var targets = new List<string>();
			if (ExecuteOnDatabase1) targets.Add("DB1");
			if (ExecuteOnDatabase2) targets.Add("DB2");

			return $"{Name} → [{string.Join(", ", targets)}]";
		}

		// Convert to legacy StoredProcedureInfo for backward compatibility
		public StoredProcedureInfo ToLegacyProcedure()
		{
			return new StoredProcedureInfo
			{
				Name = Name,
				Parameters = Parameters,
				ExecutionOrder = ExecutionOrder,
				Description = Description
			};
		}
	}

	public enum ProcedureCategory
	{
		[Description("Setup & Initialization")]
		Setup,
		[Description("Data Processing")] DataProcessing,
		[Description("Maintenance")] Maintenance,
		[Description("Reporting")] Reporting,
		[Description("General")] General
	}

	// NEW: Smart deployment strategy templates
	public class SmartDeploymentStrategy
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public List<SmartProcedureTemplate> ProcedureTemplates { get; set; } = new List<SmartProcedureTemplate>();

		public static List<SmartDeploymentStrategy> GetPredefinedStrategies()
		{
			return new List<SmartDeploymentStrategy>
			{
				new SmartDeploymentStrategy
				{
					Name = "Full Deployment",
					Description = "Deploy all procedures to both databases",
					ProcedureTemplates = new List<SmartProcedureTemplate>
					{
						new SmartProcedureTemplate
						{
							Name = "prInitializeDatabaseTables", Category = ProcedureCategory.Setup,
							RecommendedOrder = 10, DefaultDatabase1 = true, DefaultDatabase2 = true
						},
						new SmartProcedureTemplate
						{
							Name = "prSetupDefaultData", Category = ProcedureCategory.Setup, RecommendedOrder = 20,
							DefaultDatabase1 = true, DefaultDatabase2 = true
						},
						new SmartProcedureTemplate
						{
							Name = "prConfigureSystem", Category = ProcedureCategory.Setup, RecommendedOrder = 30,
							DefaultDatabase1 = true, DefaultDatabase2 = true
						}
					}
				},
				new SmartDeploymentStrategy
				{
					Name = "Primary-Secondary Split",
					Description = "Deploy setup procedures to primary, processing to secondary",
					ProcedureTemplates = new List<SmartProcedureTemplate>
					{
						new SmartProcedureTemplate
						{
							Name = "prInitializeDatabaseTables", Category = ProcedureCategory.Setup,
							RecommendedOrder = 10, DefaultDatabase1 = true, DefaultDatabase2 = false
						},
						new SmartProcedureTemplate
						{
							Name = "prProcessData", Category = ProcedureCategory.DataProcessing, RecommendedOrder = 50,
							DefaultDatabase1 = false, DefaultDatabase2 = true
						},
						new SmartProcedureTemplate
						{
							Name = "prGenerateReports", Category = ProcedureCategory.Reporting, RecommendedOrder = 80,
							DefaultDatabase1 = false, DefaultDatabase2 = true
						}
					}
				},
				new SmartDeploymentStrategy
				{
					Name = "Setup Only",
					Description = "Deploy only essential setup procedures",
					ProcedureTemplates = new List<SmartProcedureTemplate>
					{
						new SmartProcedureTemplate
						{
							Name = "prInitializeDatabaseTables", Category = ProcedureCategory.Setup,
							RecommendedOrder = 10, DefaultDatabase1 = true, DefaultDatabase2 = true
						}
					}
				}
			};
		}
	}

	public class SmartProcedureTemplate
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool DefaultDatabase1 { get; set; } = true;
		public bool DefaultDatabase2 { get; set; } = true;
		public ProcedureCategory Category { get; set; }
		public int RecommendedOrder { get; set; }
		public string DefaultParameters { get; set; }
	}

	// Your existing classes (unchanged)
	public class DeploymentResult
	{
		public DeploymentResult()
		{
			Warnings = new List<string>();
			Errors = new List<string>();
			Timestamp = DateTime.Now;
		}

		// Core result properties
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public TimeSpan Duration { get; set; }
		public DateTime Timestamp { get; set; }

		// Enhanced error and warning tracking
		public List<string> Warnings { get; set; }
		public List<string> Errors { get; set; }
		public Exception Exception { get; set; }

		// Deployment details
		public string DatabaseName { get; set; } = string.Empty;
		public string DacpacPath { get; set; } = string.Empty;
		public string BackupPath { get; set; } = string.Empty;
		public int ProceduresExecuted { get; set; }
		public int JobsCreated { get; set; }
		public int SynonymsCreated { get; set; }

		// Helper properties for summary
		public bool HasWarnings => Warnings?.Count > 0;
		public bool HasErrors => Errors?.Count > 0;
		public bool HasCriticalErrors => HasErrors && Errors.Exists(e =>
			e.Contains("Could not find stored procedure") ||
			e.Contains("Login failed") ||
			e.Contains("Access denied") ||
			e.Contains("Invalid object name"));

		// Summary methods
		public string GetSummary()
		{
			var summary = $"Deployment {(Success ? "Successful" : "Failed")} in {Duration:mm\\:ss}";

			if (HasErrors)
				summary += $" | Errors: {Errors.Count}";

			if (HasWarnings)
				summary += $" | Warnings: {Warnings.Count}";

			if (ProceduresExecuted > 0)
				summary += $" | Procedures: {ProceduresExecuted}";

			if (JobsCreated > 0)
				summary += $" | Jobs: {JobsCreated}";

			if (SynonymsCreated > 0)
				summary += $" | Synonyms: {SynonymsCreated}";

			return summary;
		}

		public List<string> GetTopErrors(int count = 5)
		{
			return Errors?.Take(count).ToList() ?? new List<string>();
		}

		public List<string> GetTopWarnings(int count = 5)
		{
			return Warnings?.Take(count).ToList() ?? new List<string>();
		}
	}
	public class ConnectionInfo
	{
		public string ServerName { get; set; }
		public bool WindowsAuth { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Database { get; set; }
	}

	public class JobScriptValidationResult
	{
		public bool IsValid { get; set; }
		public string ErrorMessage { get; set; } = string.Empty;
		public int JobCount { get; set; }
		public List<JobScriptInfo> JobScripts { get; set; } = new List<JobScriptInfo>();
	}

	public class JobScriptInfo
	{
		public string FilePath { get; set; } = string.Empty;
		public string FileName { get; set; } = string.Empty;
		public string JobName { get; set; } = string.Empty;
		public bool IsValid { get; set; }
		public List<string> ValidationErrors { get; set; } = new List<string>();
		public bool HasServerNameParameter { get; set; }
		public bool HasDatabaseNameParameter { get; set; }
		public bool HasOwnerLoginParameter { get; set; }
		public bool HasTransactionLogic { get; set; }
		public bool HasGotoStatements { get; set; }
		public bool HasUseStatement { get; set; }
		public int GoStatementCount { get; set; }
		public string RecommendedExecutionStrategy { get; set; } = string.Empty;
	}

	public class JobScriptTemplate
	{
		public string JobName { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Schedule { get; set; } = string.Empty;
		public string StoredProcedure { get; set; } = string.Empty;
	}

	public class DeploymentHistory
	{
		public DateTime Timestamp { get; set; }
		public string ServerName { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;
		public string DacpacFile { get; set; } = string.Empty;
		public bool Success { get; set; }
		public string BackupPath { get; set; } = string.Empty;
		public TimeSpan Duration { get; set; }
		public string ErrorMessage { get; set; } = string.Empty;
	}

	public enum DeploymentMode
	{
		Conservative, // Safest - preserve all data
		Moderate, // Some risk - allow constraint changes
		Aggressive // High risk - allow drops (dev only)
	}

	// NEW: Smart deployment results
	public class SmartDeploymentResult
	{
		public string TargetName { get; set; }
		public string DatabaseName { get; set; }
		public bool Success { get; set; }
		public TimeSpan Duration { get; set; }
		public string ErrorMessage { get; set; }
		public int ProceduresExecuted { get; set; }
	}

	public class SmartProcedureExecutionResult
	{
		public string ProcedureName { get; set; }
		public string DatabaseName { get; set; }
		public bool Success { get; set; }
		public TimeSpan Duration { get; set; }
		public string ErrorMessage { get; set; }
		public int ExecutionOrder { get; set; }
		public ProcedureCategory Category { get; set; }
		public DateTime ExecutedAt { get; set; } = DateTime.Now;

		public override string ToString()
		{
			var status = Success ? "✅" : "❌";
			return $"{status} {ProcedureName} on {DatabaseName} ({Duration.TotalSeconds:F1}s)";
		}
	}

	public class DataRecommendation
	{
		public string Type { get; set; } // "Performance", "Data Quality", "Security", etc.
		public string Title { get; set; }
		public string Description { get; set; }
		public string Severity { get; set; } // "High", "Medium", "Low"
		public string Action { get; set; }
		public string Icon { get; set; }
	}

	public class DatabaseSummary
	{
		public int TotalTables { get; set; }
		public int TotalRows { get; set; }
		public long DatabaseSize { get; set; }
		public Dictionary<string, int> TableRowCounts { get; set; }
		public List<string> LargestTables { get; set; }
		public List<string> EmptyTables { get; set; }
		public DateTime LastAnalyzed { get; set; }
	}
	public static class DeploymentConfiguration
	{



		// ADD this method to your DeploymentConfiguration class in Models.cs
		/// <summary>
		/// Enhanced SQL Package arguments specifically designed to handle Service Broker auto-generated procedures
		/// </summary>
		public static string GetServiceBrokerSafeArguments(string dacpacPath, string connectionString, string synonymSourceDb = null)
		{
			var args = $"/a:Publish /SourceFile:\"{dacpacPath}\" /TargetConnectionString:\"{connectionString}\" " +
					  "/p:BlockOnPossibleDataLoss=false " +
					  "/p:DropObjectsNotInSource=true " +
					  "/p:VerifyDeployment=true " +

					  // COMPREHENSIVE Service Broker and auto-generated object exclusions
					  "/p:ExcludeObjectTypes=Logins;Users;RoleMembership;Synonyms;Queues;Services;Contracts;MessageTypes;BrokerPriorities;RemoteServiceBindings " +

					  // Enhanced error handling for auto-generated procedures
					  "/p:TreatVerificationErrorsAsWarnings=true " +
					  "/p:AllowIncompatiblePlatform=true " +
					  "/p:IgnorePermissions=true " +
					  "/p:IgnoreUserSettingsObjects=true " +
					  "/p:IgnoreExtendedProperties=true " +

					  // CRITICAL: Don't drop Service Broker objects that may have auto-generated dependencies
					  "/p:DoNotDropObjectTypes=Queues;Services;Contracts;MessageTypes;BrokerPriorities;RemoteServiceBindings;StoredProcedures " +

					  // Additional safety settings
					  "/p:DropPermissionsNotInSource=false " +
					  "/p:DropRoleMembersNotInSource=false " +
					  "/p:IgnoreNotForReplication=true " +              // Ignore replication settings
					  "/p:IgnoreAuthorization=true " +                  // Ignore authorization differences
					  "/p:IgnoreQuotedIdentifiers=true ";               // Ignore quoted identifier settings

			// Add synonym source variable if needed
			if (!string.IsNullOrEmpty(synonymSourceDb))
			{
				args += $" /v:SynonymSourceDb=\"{synonymSourceDb}\"";
			}

			return args;
		}

		/// <summary>
		/// Enhanced auto-generated procedure patterns including Service Broker GUID procedures
		/// </summary>
		public static readonly Dictionary<string, string> EnhancedAutoGeneratedPatterns = new Dictionary<string, string>
		{
			// Service Broker auto-generated procedures (YOUR SPECIFIC ISSUE)
			["ServiceBroker_GUID"] = @".*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}.*",
			["QueueActionSender"] = @".*QueueActionSender_.*",
			["QueueActivationSender"] = @".*QueueActivationSender.*",
			["ServiceBroker_Numbered"] = @".*_[0-9a-fA-F]{8}_.*",
			["BulkSurveyQueue"] = @".*BulkSurveyQueue.*",              // Your specific case
			["ServiceBrokerGeneric"] = @".*_Queue_.*",

			// SQL Server system auto-generated
			["MigrationHistory"] = @".*__MigrationHistory.*",
			["ValidationProcedure"] = @".*_ValidationProcedure_.*",
			["GeneratedProcedure"] = @".*_GeneratedProcedure_.*",
			["SystemProcedures"] = @"sp_MS.*",

			// Entity Framework auto-generated
			["EF_Insert"] = @".*_Insert_.*_[0-9a-fA-F]+",
			["EF_Update"] = @".*_Update_.*_[0-9a-fA-F]+",
			["EF_Delete"] = @".*_Delete_.*_[0-9a-fA-F]+",

			// Temporal and versioning patterns
			["Temporal"] = @".*_[0-9]{8}_[0-9]{6}_.*",
			["Versioning"] = @".*_v[0-9]+_[0-9a-fA-F]+.*"
		};

		/// <summary>
		/// Enhanced ignorable error patterns for Service Broker procedures
		/// </summary>
		public static readonly List<string> EnhancedIgnorableErrorPatterns = new List<string>
{
    // Service Broker specific errors (YOUR ISSUE)
    "Could not find stored procedure 'dbo.QueueActionSender_",
	"Could not find stored procedure 'QueueActionSender_",
	"Could not find stored procedure.*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
	"The object.*QueueActionSender.*does not exist",
	"Invalid object name.*QueueActionSender",
	"Could not find stored procedure.*_[0-9a-fA-F]{8}_",
	"Could not find stored procedure.*BulkSurveyQueue.*",      // Your specific pattern
    "Could not find stored procedure.*dbo_BULKSurveyQueue.*",  // Your specific pattern
    
    // Generic Service Broker patterns
    "Could not find stored procedure.*Queue_[0-9a-fA-F\\-]+",
	"Could not find stored procedure.*_BULK.*Queue_",
	"Invalid object name.*Queue_[0-9a-fA-F\\-]+",
    
    // Auto-generated procedure warnings
    "auto-generated procedures not found",
	"These are typically created by SQL Server Service Broker",
	"Consider this deployment successful if no other critical errors exist"
};
		/// <summary>
		/// Patterns to identify auto-generated procedures that should be skipped
		/// </summary>
		public static readonly Dictionary<string, string> AutoGeneratedPatterns = new Dictionary<string, string>
		{
			// Service Broker auto-generated procedures
			["ServiceBroker_GUID"] = @".*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}.*",
			["QueueActionSender"] = @".*QueueActionSender.*",
			["QueueActivationSender"] = @".*QueueActivationSender.*",
			["ServiceBroker_Numbered"] = @".*_[0-9a-fA-F]{8}_.*",

			// SQL Server system auto-generated
			["MigrationHistory"] = @".*__MigrationHistory.*",
			["ValidationProcedure"] = @".*_ValidationProcedure_.*",
			["GeneratedProcedure"] = @".*_GeneratedProcedure_.*",
			["SystemProcedures"] = @"sp_MS.*",

			// Entity Framework auto-generated
			["EF_Insert"] = @".*_Insert_.*_[0-9a-fA-F]+",
			["EF_Update"] = @".*_Update_.*_[0-9a-fA-F]+",
			["EF_Delete"] = @".*_Delete_.*_[0-9a-fA-F]+",

			// Temporal and versioning patterns
			["Temporal"] = @".*_[0-9]{8}_[0-9]{6}_.*",
			["Versioning"] = @".*_v[0-9]+_[0-9a-fA-F]+.*"
		};

		/// <summary>
		/// Error messages that indicate auto-generated procedure issues (can usually be ignored)
		/// </summary>
		public static readonly List<string> IgnorableErrorPatterns = new List<string>
		{
			"Could not find stored procedure 'dbo.QueueActionSender_",
			"Could not find stored procedure 'QueueActionSender_",
			"Could not find stored procedure.*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
			"The object.*QueueActionSender.*does not exist",
			"Invalid object name.*QueueActionSender",
			"Could not find stored procedure.*_[0-9a-fA-F]{8}_"
		};

		/// <summary>
		/// Critical error patterns that should always be treated as failures
		/// </summary>
		public static readonly List<string> CriticalErrorPatterns = new List<string>
		{
			"Login failed",
			"Access denied",
			"Cannot open database",
			"Database.*does not exist",
			"The server principal.*does not exist",
			"Permission denied",
			"Timeout expired",
			"Cannot connect to",
			"Network-related or instance-specific error"
		};

		/// <summary>
		/// Enhanced method to determine if a procedure name indicates it's auto-generated
		/// </summary>
		/// <param name="procedureName">The procedure name to check</param>
		/// <returns>True if the procedure appears to be auto-generated</returns>
		public static bool IsAutoGeneratedProcedure(string procedureName)
		{
			if (string.IsNullOrEmpty(procedureName))
				return false;

			// Check against all auto-generated patterns
			return AutoGeneratedPatterns.Values.Any(pattern =>
				Regex.IsMatch(procedureName, pattern, RegexOptions.IgnoreCase));
		}

		/// <summary>
		/// Determine if an error message is related to auto-generated procedures and can be ignored
		/// </summary>
		/// <param name="errorMessage">The error message to check</param>
		/// <returns>True if the error can be safely ignored</returns>
		public static bool IsIgnorableError(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage))
				return false;

			// First check if it's a critical error that should never be ignored
			if (IsCriticalError(errorMessage))
				return false;

			// Check against ignorable error patterns
			return IgnorableErrorPatterns.Any(pattern =>
				Regex.IsMatch(errorMessage, pattern, RegexOptions.IgnoreCase));
		}

		/// <summary>
		/// Determine if an error message indicates a critical failure
		/// </summary>
		/// <param name="errorMessage">The error message to check</param>
		/// <returns>True if the error is critical and deployment should fail</returns>
		public static bool IsCriticalError(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage))
				return false;

			return CriticalErrorPatterns.Any(pattern =>
				Regex.IsMatch(errorMessage, pattern, RegexOptions.IgnoreCase));
		}

		/// <summary>
		/// Get a user-friendly explanation for why a procedure was skipped
		/// </summary>
		/// <param name="procedureName">The procedure name</param>
		/// <returns>Explanation text</returns>
		public static string GetSkipReason(string procedureName)
		{
			if (string.IsNullOrEmpty(procedureName))
				return "Unknown procedure name";

			foreach (var pattern in AutoGeneratedPatterns)
			{
				if (Regex.IsMatch(procedureName, pattern.Value, RegexOptions.IgnoreCase))
				{
					return pattern.Key switch
					{
						"ServiceBroker_GUID" => "Service Broker auto-generated procedure with GUID identifier",
						"QueueActionSender" => "Service Broker queue action sender procedure",
						"QueueActivationSender" => "Service Broker queue activation procedure",
						"ServiceBroker_Numbered" => "Service Broker numbered procedure",
						"MigrationHistory" => "Entity Framework migration history procedure",
						"ValidationProcedure" => "Auto-generated validation procedure",
						"GeneratedProcedure" => "System-generated procedure",
						"SystemProcedures" => "Microsoft system procedure",
						"EF_Insert" => "Entity Framework auto-generated insert procedure",
						"EF_Update" => "Entity Framework auto-generated update procedure",
						"EF_Delete" => "Entity Framework auto-generated delete procedure",
						"Temporal" => "Temporal table auto-generated procedure",
						"Versioning" => "Version control auto-generated procedure",
						_ => "Auto-generated procedure"
					};
				}
			}

			return "Procedure appears to be auto-generated";
		}

		/// <summary>
		/// Enhanced deployment settings with auto-generated procedure handling
		/// </summary>
		public static class Settings
		{
			/// <summary>
			/// Whether to skip auto-generated procedures during deployment
			/// </summary>
			public static bool SkipAutoGeneratedProcedures { get; set; } = true;

			/// <summary>
			/// Whether to treat auto-generated procedure errors as warnings instead of failures
			/// </summary>
			public static bool TreatAutoGeneratedErrorsAsWarnings { get; set; } = true;

			/// <summary>
			/// Whether to show detailed information about skipped procedures
			/// </summary>
			public static bool ShowSkippedProcedureDetails { get; set; } = true;

			/// <summary>
			/// Maximum number of retries for procedure execution
			/// </summary>
			public static int MaxProcedureRetries { get; set; } = 2;

			/// <summary>
			/// Timeout for procedure execution (in seconds)
			/// </summary>
			public static int ProcedureTimeoutSeconds { get; set; } = 300;

			/// <summary>
			/// Whether to continue deployment if non-critical procedures fail
			/// </summary>
			public static bool ContinueOnNonCriticalProcedureFailures { get; set; } = true;
		}

		/// <summary>
		/// Enhanced SQL Package arguments with better error handling
		/// </summary>
		public static string GetEnhancedSqlPackageArguments(string dacpacPath, string connectionString, string synonymSourceDb = null)
		{
			var args = $"/a:Publish /SourceFile:\"{dacpacPath}\" /TargetConnectionString:\"{connectionString}\" " +
					  "/p:BlockOnPossibleDataLoss=false " +
					  "/p:DropObjectsNotInSource=true " +
					  "/p:VerifyDeployment=true " +
					  "/p:ExcludeObjectTypes=Logins;Users;RoleMembership;Synonyms " +
					  "/p:TreatVerificationErrorsAsWarnings=true " +        // NEW: Treat verification errors as warnings
					  "/p:AllowIncompatiblePlatform=true " +                // NEW: Allow platform differences  
					  "/p:IgnorePermissions=true " +                       // NEW: Ignore permission differences
					  "/p:IgnoreUserSettingsObjects=true " +               // NEW: Ignore user settings
					  "/p:DoNotDropObjectTypes=Aggregates;Assemblies;AsymmetricKeys;BrokerPriorities;Certificates;ColumnEncryptionKeys;ColumnMasterKeys;Contracts;DatabaseRoles;DatabaseTriggers;Defaults;ExtendedProperties;ExternalDataSources;ExternalFileFormats;ExternalTables;Filegroups;FileTables;FullTextCatalogs;FullTextStoplists;MessageTypes;PartitionFunctions;PartitionSchemes;Permissions;Queues;RemoteServiceBindings;RoleMembership;Rules;Schemas;SearchPropertyLists;SecurityPolicies;Sequences;Services;Signatures;StoredProcedures;SymmetricKeys;Synonyms;Tables;TableValuedFunctions;UserDefinedDataTypes;UserDefinedTableTypes;ClrUserDefinedTypes;Users;Views;XmlSchemaCollections " +
					  "/p:DropPermissionsNotInSource=false " +             // NEW: Don't drop permissions
					  "/p:DropRoleMembersNotInSource=false ";              // NEW: Don't drop role members

			// Add synonym source variable if needed
			if (!string.IsNullOrEmpty(synonymSourceDb))
			{
				args += $"/v:SynonymSourceDb=\"{synonymSourceDb}\"";
			}

			return args;
		}

		/// <summary>
		/// Categorize deployment result based on error types
		/// </summary>
		/// <param name="errors">List of errors</param>
		/// <param name="warnings">List of warnings</param>
		/// <returns>Deployment result category</returns>
		public static DeploymentResultCategory CategorizeResult(List<string> errors, List<string> warnings)
		{
			if (errors == null) errors = new List<string>();
			if (warnings == null) warnings = new List<string>();

			// Check for critical errors
			if (errors.Any(IsCriticalError))
				return DeploymentResultCategory.CriticalFailure;

			// Check if all errors are ignorable (auto-generated procedure issues)
			if (errors.Any() && errors.All(IsIgnorableError))
				return DeploymentResultCategory.SuccessWithIgnorableErrors;

			// Check for mixed errors
			if (errors.Any() && !errors.All(IsIgnorableError))
				return DeploymentResultCategory.PartialFailure;

			// Check for warnings only
			if (warnings.Any())
				return DeploymentResultCategory.SuccessWithWarnings;

			// Complete success
			return DeploymentResultCategory.Success;
		}
	}

	/// <summary>
	/// Categories for deployment results
	/// </summary>
	public enum DeploymentResultCategory
	{
		Success,
		SuccessWithWarnings,
		SuccessWithIgnorableErrors,
		PartialFailure,
		CriticalFailure
	}
	public class SmartSynonymPlan
	{
		public string SourceDatabase { get; set; }  // Contains the actual CFMSurveyUser table
		public List<string> TargetDatabases { get; set; } = new List<string>(); // Will get synonyms
		public string DetectionReason { get; set; }
		public List<string> SkippedDatabases { get; set; } = new List<string>();
		public List<string> Warnings { get; set; } = new List<string>();
	}
	public class SynonymStatus
	{
		public string DatabaseName { get; set; }
		public bool HasCFMSurveyUserSynonym { get; set; }
		public bool HasCFMSurveyUserTable { get; set; }
		public bool HasCFMUserTable { get; set; }
		public string SynonymTarget { get; set; }
		public bool HasSelfReferencingSynonym { get; set; }
		public string CurrentDatabase { get; set; }
		public string RecommendedAction { get; set; }
		public bool HasError { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class FixSynonymResult
	{
		public List<string> FixedDatabases { get; set; } = new List<string>();
		public List<string> ErrorDatabases { get; set; } = new List<string>();
	}
}