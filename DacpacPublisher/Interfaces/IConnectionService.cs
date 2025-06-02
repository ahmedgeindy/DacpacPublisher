using DacpacPublisher.Data_Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DacpacPublisher.Interfaces
{
	public interface IConnectionService
	{
		Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo);
		Task<List<string>> GetDatabasesAsync(ConnectionInfo connectionInfo);
		string BuildConnectionString(ConnectionInfo connectionInfo);
		Task<IDbConnection> CreateConnectionAsync(ConnectionInfo connectionInfo);
	}

	public interface IDeploymentService
	{
		Task<DeploymentResult> DeployDacpacAsync(PublisherConfiguration config);
		Task<JobScriptValidationResult> ValidateJobScriptsAsync(string jobScriptsFolder);
		Task ExecuteProcedureAsync(ConnectionInfo connectionInfo, string procedureName, string parameters);
		event Action<string> LogMessageReceived;
		event Action<int> ProgressChanged;
	}

	public interface IConfigurationService
	{
		Task<bool> SaveConfigurationAsync(PublisherConfiguration config, string filePath);
		Task<PublisherConfiguration> LoadConfigurationAsync(string filePath);

	}

	public interface ILogService
	{
		void LogMessage(string message);
		void LogError(string message, Exception exception = null);
		void LogWarning(string message);
		void LogInfo(string message);
		event Action<string> MessageLogged;
		string GetAllLogs();
		void ClearLogs();
	}

	// Add to existing interfaces
	public interface IBackupService
	{
		Task<bool> RestoreBackupAsync(ConnectionInfo connectionInfo, string backupFilePath);

		Task<List<string>> GetBackupHistoryAsync(ConnectionInfo connectionInfo);

		// Add these missing methods that are being called:
		Task<string> CreateBackupAsync(ConnectionInfo connectionInfo, string backupPath);
	}

	public interface IDataAnalysisService
	{
		Task<List<string>> GetTablesAsync(ConnectionInfo connectionInfo);
		Task<DataTable> QueryTableAsync(ConnectionInfo connectionInfo, string tableName, int maxRows = 1000);
		Task<DataTable> ExecuteCustomQueryAsync(ConnectionInfo connectionInfo, string query);
		Task<List<DataRecommendation>> AnalyzeTableDataAsync(ConnectionInfo connectionInfo, string tableName);
		Task<DatabaseSummary> GetDatabaseSummaryAsync(ConnectionInfo connectionInfo);
	}
}