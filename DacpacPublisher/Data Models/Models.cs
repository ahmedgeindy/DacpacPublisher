using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DacpacPublisher.Data_Models
{
	#region Core Configuration Models

	/// <summary>
	/// Enhanced Publisher Configuration with smart deployment features
	/// </summary>
	public class PublisherConfiguration
	{
		// Connection Settings
		public string ServerName { get; set; } = string.Empty;
		public bool WindowsAuth { get; set; } = true;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;

		// DACPAC Settings
		public string DacpacPath { get; set; } = string.Empty;

		// Feature Flags
		public bool CreateSynonyms { get; set; } = false;
		public bool CreateSqlAgentJobs { get; set; } = false;
		public bool ExecuteProcedures { get; set; } = false;
		public bool EnableMultipleDatabases { get; set; } = false;
		public bool CreateBackupBeforeDeployment { get; set; } = true;
		public bool UseSmartProcedures { get; set; } = true;
		public bool ValidateProceduresBeforeDeployment { get; set; } = true;

		// Advanced Settings
		public string SynonymSourceDb { get; set; } = string.Empty;
		public List<string> SynonymTargetDatabases { get; set; } = new List<string>();
		public string JobScriptsFolder { get; set; } = string.Empty;
		public string JobOwnerLoginName { get; set; } = string.Empty;
		public string BackupPath { get; set; } = string.Empty;
		public string SmartDeploymentStrategy { get; set; } = "Full Deployment";
		public int MaxConcurrentProcedures { get; set; } = 1;
		public int ProcedureTimeoutSeconds { get; set; } = 300;

		// Collections
		public List<StoredProcedureInfo> StoredProcedures { get; set; } = new List<StoredProcedureInfo>();
		public List<SmartStoredProcedureInfo> SmartProcedures { get; set; } = new List<SmartStoredProcedureInfo>();
		public List<DatabaseDeploymentTarget> DeploymentTargets { get; set; } = new List<DatabaseDeploymentTarget>();
		public List<DeploymentHistory> History { get; set; } = new List<DeploymentHistory>();

		// Computed Properties
		public bool HasMultipleTargets => EnableMultipleDatabases && DeploymentTargets?.Count > 1;
		public bool HasSmartProcedures => UseSmartProcedures && SmartProcedures?.Any() == true;
		public bool IsValid => !string.IsNullOrWhiteSpace(ServerName) &&
							  !string.IsNullOrWhiteSpace(Database) &&
							  !string.IsNullOrWhiteSpace(DacpacPath) &&
							  File.Exists(DacpacPath);

		// Utility Methods
		public string GetDeploymentSummary()
		{
			var summary = new StringBuilder();
			summary.AppendLine("Deployment Configuration Summary:");
			summary.AppendLine($"├─ Server: {ServerName}");
			summary.AppendLine($"├─ Primary Database: {Database}");
			summary.AppendLine($"├─ Authentication: {(WindowsAuth ? "Windows" : "SQL Server")}");
			summary.AppendLine($"├─ DACPAC: {Path.GetFileName(DacpacPath)}");
			summary.AppendLine($"├─ Multiple Databases: {EnableMultipleDatabases}");

			if (HasMultipleTargets)
				summary.AppendLine($"├─ Deployment Targets: {DeploymentTargets.Count}");

			if (CreateSynonyms)
				summary.AppendLine($"├─ Synonyms: {SynonymTargetDatabases.Count} target(s)");

			if (HasSmartProcedures)
			{
				summary.AppendLine($"├─ Smart Procedures: {SmartProcedures.Count}");
				var categories = SmartProcedures.GroupBy(p => p.Category)
					.Select(g => $"{g.Key}: {g.Count()}");
				summary.AppendLine($"└─ Categories: {string.Join(", ", categories)}");
			}
			else
			{
				summary.AppendLine($"└─ Features: Synonyms={CreateSynonyms}, Jobs={CreateSqlAgentJobs}, Backup={CreateBackupBeforeDeployment}");
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

			if (string.IsNullOrWhiteSpace(DacpacPath))
				errors.Add("DACPAC file path is required");
			else if (!File.Exists(DacpacPath))
				errors.Add($"DACPAC file not found: {DacpacPath}");

			if (!WindowsAuth && (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)))
				errors.Add("Username and password are required for SQL Server authentication");

			if (EnableMultipleDatabases && DeploymentTargets.Count < 2)
				errors.Add("Multiple databases enabled but insufficient targets configured");

			if (CreateSqlAgentJobs)
			{
				if (string.IsNullOrWhiteSpace(JobOwnerLoginName))
					errors.Add("Job owner login name is required for SQL Agent Jobs");
				if (string.IsNullOrWhiteSpace(JobScriptsFolder) || !Directory.Exists(JobScriptsFolder))
					errors.Add("Valid job scripts folder is required for SQL Agent Jobs");
			}

			if (HasSmartProcedures)
			{
				var duplicates = SmartProcedures.GroupBy(p => p.Name)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key);
				if (duplicates.Any())
					errors.Add($"Duplicate procedure names: {string.Join(", ", duplicates)}");

				var invalidOrders = SmartProcedures.Where(p => p.ExecutionOrder <= 0);
				if (invalidOrders.Any())
					errors.Add("All procedures must have execution order greater than 0");
			}

			return errors;
		}

		public PublisherConfiguration Clone()
		{
			return new PublisherConfiguration
			{
				ServerName = ServerName,
				WindowsAuth = WindowsAuth,
				Username = Username,
				Password = Password,
				Database = Database,
				DacpacPath = DacpacPath,
				CreateSynonyms = CreateSynonyms,
				CreateSqlAgentJobs = CreateSqlAgentJobs,
				ExecuteProcedures = ExecuteProcedures,
				EnableMultipleDatabases = EnableMultipleDatabases,
				CreateBackupBeforeDeployment = CreateBackupBeforeDeployment,
				UseSmartProcedures = UseSmartProcedures,
				ValidateProceduresBeforeDeployment = ValidateProceduresBeforeDeployment,
				SynonymSourceDb = SynonymSourceDb,
				SynonymTargetDatabases = new List<string>(SynonymTargetDatabases),
				JobScriptsFolder = JobScriptsFolder,
				JobOwnerLoginName = JobOwnerLoginName,
				BackupPath = BackupPath,
				SmartDeploymentStrategy = SmartDeploymentStrategy,
				MaxConcurrentProcedures = MaxConcurrentProcedures,
				ProcedureTimeoutSeconds = ProcedureTimeoutSeconds,
				StoredProcedures = StoredProcedures.Select(sp => sp.Clone()).ToList(),
				SmartProcedures = SmartProcedures.Select(sp => sp.Clone()).ToList(),
				DeploymentTargets = DeploymentTargets.Select(dt => dt.Clone()).ToList(),
				History = new List<DeploymentHistory>(History)
			};
		}
	}

	#endregion

	#region Procedure Models

	/// <summary>
	/// Enhanced stored procedure information with smart features
	/// </summary>
	public class StoredProcedureInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Parameters { get; set; } = string.Empty;
		public int ExecutionOrder { get; set; } = 100;
		public string Description { get; set; } = string.Empty;

		public bool IsValid => !string.IsNullOrWhiteSpace(Name);

		public override string ToString()
		{
			return string.IsNullOrEmpty(Parameters) ? Name : $"{Name} ({Parameters})";
		}

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

		public StoredProcedureInfo Clone()
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

	/// <summary>
	/// Smart stored procedure with enhanced deployment capabilities
	/// </summary>
	public class SmartStoredProcedureInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Parameters { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool ExecuteOnDatabase1 { get; set; } = true;
		public bool ExecuteOnDatabase2 { get; set; } = true;
		public ProcedureCategory Category { get; set; } = ProcedureCategory.General;
		public int ExecutionOrder { get; set; } = 100;

		public bool IsValid => !string.IsNullOrWhiteSpace(Name);
		public bool HasTargets => ExecuteOnDatabase1 || ExecuteOnDatabase2;
		public string TargetsDisplay
		{
			get
			{
				var targets = new List<string>();
				if (ExecuteOnDatabase1) targets.Add("Primary");
				if (ExecuteOnDatabase2) targets.Add("Secondary");
				return string.Join(", ", targets);
			}
		}

		public override string ToString()
		{
			return $"{Name} → [{TargetsDisplay}] ({Category})";
		}

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

		public SmartStoredProcedureInfo Clone()
		{
			return new SmartStoredProcedureInfo
			{
				Name = Name,
				Parameters = Parameters,
				Description = Description,
				ExecuteOnDatabase1 = ExecuteOnDatabase1,
				ExecuteOnDatabase2 = ExecuteOnDatabase2,
				Category = Category,
				ExecutionOrder = ExecutionOrder
			};
		}
	}

	public enum ProcedureCategory
	{
		[Description("Setup & Initialization")]
		Setup,
		[Description("Data Processing")]
		DataProcessing,
		[Description("Maintenance")]
		Maintenance,
		[Description("Reporting")]
		Reporting,
		[Description("General")]
		General
	}

	#endregion

	#region Deployment Models

	/// <summary>
	/// Enhanced database deployment target with smart features
	/// </summary>
	public class DatabaseDeploymentTarget
	{
		public string Name { get; set; } = string.Empty;
		public string ServerName { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;
		public string DacpacPath { get; set; } = string.Empty;
		public bool IsEnabled { get; set; } = true;
		public string ProcedureStrategy { get; set; } = "All";
		public bool CreateBackup { get; set; } = false;
		public string BackupPath { get; set; } = string.Empty;
		public bool RunPostDeploymentValidation { get; set; } = true;

		public List<StoredProcedureInfo> SpecificProcedures { get; set; } = new List<StoredProcedureInfo>();
		public List<SmartStoredProcedureInfo> SmartProcedures { get; set; } = new List<SmartStoredProcedureInfo>();

		public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
							  !string.IsNullOrWhiteSpace(Database) &&
							  !string.IsNullOrWhiteSpace(DacpacPath);

		public List<SmartStoredProcedureInfo> GetOrderedProcedures()
		{
			return SmartProcedures?.OrderBy(p => p.ExecutionOrder).ToList() ?? new List<SmartStoredProcedureInfo>();
		}

		public List<SmartStoredProcedureInfo> GetProceduresByCategory(ProcedureCategory category)
		{
			return SmartProcedures?.Where(p => p.Category == category)
				.OrderBy(p => p.ExecutionOrder).ToList() ?? new List<SmartStoredProcedureInfo>();
		}

		public string GetSummary()
		{
			var procedureCount = SmartProcedures?.Count ?? SpecificProcedures?.Count ?? 0;
			var status = IsEnabled ? "✓" : "✗";

			return $"{status} {Name} ({Database}) - {procedureCount} procedure(s)";
		}

		public List<StoredProcedureInfo> GetExecutableProcedures()
		{
			if (SmartProcedures?.Any() == true)
				return SmartProcedures.OrderBy(p => p.ExecutionOrder)
					.Select(sp => sp.ToLegacyProcedure()).ToList();

			return SpecificProcedures ?? new List<StoredProcedureInfo>();
		}

		public DatabaseDeploymentTarget Clone()
		{
			return new DatabaseDeploymentTarget
			{
				Name = Name,
				ServerName = ServerName,
				Database = Database,
				DacpacPath = DacpacPath,
				IsEnabled = IsEnabled,
				ProcedureStrategy = ProcedureStrategy,
				CreateBackup = CreateBackup,
				BackupPath = BackupPath,
				RunPostDeploymentValidation = RunPostDeploymentValidation,
				SpecificProcedures = SpecificProcedures.Select(sp => sp.Clone()).ToList(),
				SmartProcedures = SmartProcedures.Select(sp => sp.Clone()).ToList()
			};
		}
	}

	/// <summary>
	/// Enhanced deployment result with comprehensive tracking
	/// </summary>
	public class DeploymentResult
	{
		public DeploymentResult()
		{
			Warnings = new List<string>();
			Errors = new List<string>();
			Timestamp = DateTime.Now;
		}

		// Core Properties
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public TimeSpan Duration { get; set; }
		public DateTime Timestamp { get; set; }
		public Exception Exception { get; set; }

		// Deployment Details
		public string DatabaseName { get; set; } = string.Empty;
		public string DacpacPath { get; set; } = string.Empty;
		public string BackupPath { get; set; } = string.Empty;

		// Metrics
		public int ProceduresExecuted { get; set; }
		public int JobsCreated { get; set; }
		public int SynonymsCreated { get; set; }

		// Issue Tracking
		public List<string> Warnings { get; set; }
		public List<string> Errors { get; set; }

		// Helper Properties
		public bool HasWarnings => Warnings?.Count > 0;
		public bool HasErrors => Errors?.Count > 0;
		public bool HasCriticalErrors => HasErrors && Errors.Any(e =>
			DeploymentConfiguration.IsCriticalError(e));

		public string GetSummary()
		{
			var summary = $"Deployment {(Success ? "✓" : "✗")} in {Duration:mm\\:ss}";

			var details = new List<string>();
			if (ProceduresExecuted > 0) details.Add($"Procedures: {ProceduresExecuted}");
			if (JobsCreated > 0) details.Add($"Jobs: {JobsCreated}");
			if (SynonymsCreated > 0) details.Add($"Synonyms: {SynonymsCreated}");
			if (HasErrors) details.Add($"Errors: {Errors.Count}");
			if (HasWarnings) details.Add($"Warnings: {Warnings.Count}");

			if (details.Any())
				summary += $" | {string.Join(", ", details)}";

			return summary;
		}

		public List<string> GetTopErrors(int count = 5) => Errors?.Take(count).ToList() ?? new List<string>();
		public List<string> GetTopWarnings(int count = 5) => Warnings?.Take(count).ToList() ?? new List<string>();
	}

	#endregion

	#region Support Models

	public class ConnectionInfo
	{
		public string ServerName { get; set; } = string.Empty;
		public bool WindowsAuth { get; set; } = true;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Database { get; set; } = string.Empty;

		public bool IsValid => !string.IsNullOrWhiteSpace(ServerName) &&
							  !string.IsNullOrWhiteSpace(Database) &&
							  (WindowsAuth || (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password)));
	}

	public class JobScriptInfo
	{
		public string FilePath { get; set; } = string.Empty;
		public string FileName { get; set; } = string.Empty;
		public string JobName { get; set; } = string.Empty;
		public bool IsValid { get; set; }
		public List<string> ValidationErrors { get; set; } = new List<string>();

		// Analysis Properties
		public bool HasServerNameParameter { get; set; }
		public bool HasDatabaseNameParameter { get; set; }
		public bool HasOwnerLoginParameter { get; set; }
		public bool HasTransactionLogic { get; set; }
		public bool HasGotoStatements { get; set; }
		public bool HasUseStatement { get; set; }
		public int GoStatementCount { get; set; }
		public string RecommendedExecutionStrategy { get; set; } = string.Empty;
	}

	public class JobScriptValidationResult
	{
		public bool IsValid { get; set; }
		public string ErrorMessage { get; set; } = string.Empty;
		public int JobCount { get; set; }
		public List<JobScriptInfo> JobScripts { get; set; } = new List<JobScriptInfo>();
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

		public string GetDisplayText()
		{
			var status = Success ? "✓" : "✗";
			var file = Path.GetFileName(DacpacFile);
			return $"{status} {Timestamp:yyyy-MM-dd HH:mm} | {Database} | {file} ({Duration:mm\\:ss})";
		}
	}

	#endregion

	#region Data Analysis Models

	public class DataRecommendation
	{
		public string Type { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Severity { get; set; } = "Medium";
		public string Action { get; set; } = string.Empty;
		public string Icon { get; set; } = "💡";
	}

	public class DatabaseSummary
	{
		public int TotalTables { get; set; }
		public int TotalRows { get; set; }
		public long DatabaseSize { get; set; }
		public Dictionary<string, int> TableRowCounts { get; set; } = new Dictionary<string, int>();
		public List<string> LargestTables { get; set; } = new List<string>();
		public List<string> EmptyTables { get; set; } = new List<string>();
		public DateTime LastAnalyzed { get; set; } = DateTime.Now;
	}

	#endregion

	#region Enhanced Deployment Configuration

	/// <summary>
	/// Enhanced deployment configuration with Service Broker support
	/// </summary>
	public static class DeploymentConfiguration
	{
		#region Auto-Generated Procedure Patterns

		private static readonly Dictionary<string, string> AutoGeneratedPatterns = new Dictionary<string, string>
		{
			["ServiceBroker_GUID"] = @".*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}.*",
			["QueueActionSender"] = @".*QueueActionSender.*",
			["BulkSurveyQueue"] = @".*BulkSurveyQueue.*",
			["ServiceBrokerGeneric"] = @".*_Queue_.*",
			["MigrationHistory"] = @".*__MigrationHistory.*",
			["SystemProcedures"] = @"sp_MS.*",
			["EntityFramework"] = @".*_(Insert|Update|Delete)_.*_[0-9a-fA-F]+",
			["Temporal"] = @".*_[0-9]{8}_[0-9]{6}_.*"
		};

		private static readonly List<string> IgnorableErrorPatterns = new List<string>
		{
			"Could not find stored procedure.*QueueActionSender",
			"Could not find stored procedure.*[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
			"Could not find stored procedure.*BulkSurveyQueue",
			"Invalid object name.*QueueActionSender",
			"auto-generated procedures not found"
		};

		private static readonly List<string> CriticalErrorPatterns = new List<string>
		{
			"Login failed",
			"Access denied",
			"Cannot open database",
			"Database.*does not exist",
			"Permission denied",
			"Timeout expired",
			"Cannot connect to",
			"Network-related or instance-specific error"
		};

		#endregion

		#region Enhanced Analysis Methods

		public static bool IsAutoGeneratedProcedure(string procedureName)
		{
			if (string.IsNullOrEmpty(procedureName))
				return false;

			return AutoGeneratedPatterns.Values.Any(pattern =>
				Regex.IsMatch(procedureName, pattern, RegexOptions.IgnoreCase));
		}

		public static bool IsIgnorableError(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage) || IsCriticalError(errorMessage))
				return false;

			return IgnorableErrorPatterns.Any(pattern =>
				Regex.IsMatch(errorMessage, pattern, RegexOptions.IgnoreCase));
		}

		public static bool IsCriticalError(string errorMessage)
		{
			if (string.IsNullOrEmpty(errorMessage))
				return false;

			return CriticalErrorPatterns.Any(pattern =>
				Regex.IsMatch(errorMessage, pattern, RegexOptions.IgnoreCase));
		}

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
						"ServiceBroker_GUID" => "Service Broker auto-generated procedure with GUID",
						"QueueActionSender" => "Service Broker queue action sender procedure",
						"BulkSurveyQueue" => "Service Broker bulk survey queue procedure",
						"ServiceBrokerGeneric" => "Service Broker queue procedure",
						"MigrationHistory" => "Entity Framework migration history procedure",
						"SystemProcedures" => "Microsoft system procedure",
						"EntityFramework" => "Entity Framework auto-generated procedure",
						"Temporal" => "Temporal table auto-generated procedure",
						_ => "Auto-generated procedure"
					};
				}
			}

			return "Procedure appears to be auto-generated";
		}

		public static DeploymentResultCategory CategorizeResult(List<string> errors, List<string> warnings)
		{
			errors ??= new List<string>();
			warnings ??= new List<string>();

			if (errors.Any(IsCriticalError))
				return DeploymentResultCategory.CriticalFailure;

			if (errors.Any() && errors.All(IsIgnorableError))
				return DeploymentResultCategory.SuccessWithIgnorableErrors;

			if (errors.Any())
				return DeploymentResultCategory.PartialFailure;

			if (warnings.Any())
				return DeploymentResultCategory.SuccessWithWarnings;

			return DeploymentResultCategory.Success;
		}

		#endregion

		#region SQL Package Arguments

		public static string GetServiceBrokerSafeArguments(string dacpacPath, string connectionString, string synonymSourceDb = null)
		{
			var args = $"/a:Publish /SourceFile:\"{dacpacPath}\" /TargetConnectionString:\"{connectionString}\" " +
					  "/p:BlockOnPossibleDataLoss=false " +
					  "/p:DropObjectsNotInSource=true " +
					  "/p:VerifyDeployment=true " +
					  "/p:ExcludeObjectTypes=Logins;Users;RoleMembership;Synonyms;Queues;Services;Contracts;MessageTypes;BrokerPriorities;RemoteServiceBindings " +
					  "/p:TreatVerificationErrorsAsWarnings=true " +
					  "/p:AllowIncompatiblePlatform=true " +
					  "/p:IgnorePermissions=true " +
					  "/p:IgnoreUserSettingsObjects=true " +
					  "/p:IgnoreExtendedProperties=true " +
					  "/p:DoNotDropObjectTypes=Queues;Services;Contracts;MessageTypes;BrokerPriorities;RemoteServiceBindings;StoredProcedures " +
					  "/p:DropPermissionsNotInSource=false " +
					  "/p:DropRoleMembersNotInSource=false " +
					  "/p:IgnoreNotForReplication=true " +
					  "/p:IgnoreAuthorization=true " +
					  "/p:IgnoreQuotedIdentifiers=true ";

			if (!string.IsNullOrEmpty(synonymSourceDb))
				args += $" /v:SynonymSourceDb=\"{synonymSourceDb}\"";

			return args;
		}

		#endregion
	}

	public enum DeploymentResultCategory
	{
		Success,
		SuccessWithWarnings,
		SuccessWithIgnorableErrors,
		PartialFailure,
		CriticalFailure
	}

	public enum DeploymentMode
	{
		Conservative,  // Preserve all data
		Moderate,      // Allow constraint changes
		Aggressive     // Allow drops (dev only)
	}

	#endregion
}