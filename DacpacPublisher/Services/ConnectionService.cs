using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DacpacPublisher.Services
{
	public class ConnectionService : IConnectionService
	{
		private readonly ILogService _logService;

		public ConnectionService(ILogService logService)
		{
			_logService = logService;
		}

		public async Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo)
		{
			try
			{
				var connectionString = BuildConnectionString(connectionInfo);
				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();
					_logService.LogInfo($"Successfully connected to {connectionInfo.ServerName}");
					return true;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError($"Connection failed to {connectionInfo.ServerName}", ex);
				return false;
			}
		}

		public async Task<List<string>> GetDatabasesAsync(ConnectionInfo connectionInfo)
		{
			var databases = new List<string>();

			try
			{
				var connectionString = BuildConnectionString(connectionInfo);
				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					const string query = "SELECT name FROM sys.databases WHERE state_desc = 'ONLINE' ORDER BY name";
					using (var command = new SqlCommand(query, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync()) databases.Add(reader["name"].ToString());
					}
				}

				_logService.LogInfo($"Retrieved {databases.Count} databases from {connectionInfo.ServerName}");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to retrieve databases", ex);
			}

			return databases;
		}

		public string BuildConnectionString(ConnectionInfo connectionInfo)
		{
			var builder = new SqlConnectionStringBuilder
			{
				DataSource = connectionInfo.ServerName,
				IntegratedSecurity = connectionInfo.WindowsAuth,
				TrustServerCertificate = true // Add this like your console version
			};

			if (!connectionInfo.WindowsAuth)
			{
				builder.UserID = connectionInfo.Username;
				builder.Password = connectionInfo.Password;
			}

			if (!string.IsNullOrEmpty(connectionInfo.Database)) builder.InitialCatalog = connectionInfo.Database;

			return builder.ConnectionString;
		}

		public async Task<IDbConnection> CreateConnectionAsync(ConnectionInfo connectionInfo)
		{
			try
			{
				var connectionString = BuildConnectionString(connectionInfo);
				var connection = new SqlConnection(connectionString);

				await connection.OpenAsync();

				_logService.LogInfo($"Successfully connected to {connectionInfo.ServerName}/{connectionInfo.Database}");
				return connection;
			}
			catch (Exception ex)
			{
				_logService.LogError(
					$"Failed to create connection to {connectionInfo.ServerName}/{connectionInfo.Database}", ex);
				throw;
			}
		}
	}
}