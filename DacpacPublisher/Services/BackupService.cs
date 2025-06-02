using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace DacpacPublisher.Services
{
	public class BackupService : IBackupService
	{
		private readonly ILogService _logService;

		public BackupService(ILogService logService)
		{
			_logService = logService;
		}

		public async Task<string> CreateBackupAsync(ConnectionInfo connectionInfo, string backupPath = null)
		{
			try
			{
				backupPath = backupPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					"DatabaseBackups");
				Directory.CreateDirectory(backupPath);

				var fileName = $"{connectionInfo.Database}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
				var fullBackupPath = Path.Combine(backupPath, fileName);

				var connectionString = BuildMasterConnectionString(connectionInfo);

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					var backupQuery = $@"
                        BACKUP DATABASE [{connectionInfo.Database}] 
                        TO DISK = '{fullBackupPath}'
                        WITH FORMAT, INIT, COMPRESSION";

					using (var command = new SqlCommand(backupQuery, connection))
					{
						command.CommandTimeout = 300; // 5 minutes
						await command.ExecuteNonQueryAsync();
					}
				}

				_logService.LogInfo($"Database backup created: {fullBackupPath}");
				return fullBackupPath;
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to create database backup", ex);
				throw;
			}
		}

		public async Task<bool> RestoreBackupAsync(ConnectionInfo connectionInfo, string backupFilePath)
		{
			try
			{
				if (!File.Exists(backupFilePath))
					throw new FileNotFoundException($"Backup file not found: {backupFilePath}");

				var connectionString = BuildMasterConnectionString(connectionInfo);

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					// Set database to single user mode
					var setSingleUserQuery =
						$"ALTER DATABASE [{connectionInfo.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
					using (var command = new SqlCommand(setSingleUserQuery, connection))
					{
						await command.ExecuteNonQueryAsync();
					}

					// Restore database
					var restoreQuery = $@"
                RESTORE DATABASE [{connectionInfo.Database}] 
                FROM DISK = '{backupFilePath}'
                WITH REPLACE";

					using (var command = new SqlCommand(restoreQuery, connection))
					{
						command.CommandTimeout = 600; // 10 minutes
						await command.ExecuteNonQueryAsync();
					}

					// Set database back to multi user mode
					var setMultiUserQuery = $"ALTER DATABASE [{connectionInfo.Database}] SET MULTI_USER";
					using (var command = new SqlCommand(setMultiUserQuery, connection))
					{
						await command.ExecuteNonQueryAsync();
					}
				}

				_logService.LogInfo($"Database restored from: {backupFilePath}");
				return true;
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to restore database backup", ex);
				throw;
			}
		}

		public async Task<List<string>> GetBackupHistoryAsync(ConnectionInfo connectionInfo)
		{
			var backupHistory = new List<string>();

			try
			{
				var connectionString = BuildMasterConnectionString(connectionInfo);

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					var query = $@"
                SELECT TOP 10
                    backup_start_date,
                    physical_device_name,
                    backup_size
                FROM msdb.dbo.backupset bs
                INNER JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
                WHERE database_name = '{connectionInfo.Database}'
                    AND type = 'D' -- Full backup
                ORDER BY backup_start_date DESC";

					using (var command = new SqlCommand(query, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var backupDate = reader.GetDateTime(Convert.ToInt32("backup_start_date"));
							var deviceName = reader.GetString(Convert.ToInt32("physical_device_name"));
							var backupSize = reader.GetInt64(Convert.ToInt32("backup_size"));

							backupHistory.Add(
								$"{backupDate:yyyy-MM-dd HH:mm:ss} - {deviceName} ({backupSize / (1024 * 1024)} MB)");
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to get backup history", ex);
			}

			return backupHistory;
		}

		private string BuildMasterConnectionString(ConnectionInfo connectionInfo)
		{
			var builder = new SqlConnectionStringBuilder
			{
				DataSource = connectionInfo.ServerName,
				IntegratedSecurity = connectionInfo.WindowsAuth,
				InitialCatalog = "master",
				TrustServerCertificate = true
			};

			if (!connectionInfo.WindowsAuth)
			{
				builder.UserID = connectionInfo.Username;
				builder.Password = connectionInfo.Password;
			}

			return builder.ConnectionString;
		}
	}
}