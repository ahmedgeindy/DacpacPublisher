using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DacpacPublisher.Services
{
	public class DataAnalysisService : IDataAnalysisService
	{
		private readonly IConnectionService _connectionService;
		private readonly ILogService _logService;

		public DataAnalysisService(IConnectionService connectionService, ILogService logService)
		{
			_connectionService = connectionService;
			_logService = logService;
		}

		public async Task<List<string>> GetTablesAsync(ConnectionInfo connectionInfo)
		{
			var tables = new List<string>();

			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					const string query = @"
                        SELECT 
                            SCHEMA_NAME(t.schema_id) + '.' + t.name as TableName
                        FROM sys.tables t
                        WHERE t.is_ms_shipped = 0
                        ORDER BY SCHEMA_NAME(t.schema_id), t.name";

					using (var command = new SqlCommand(query, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync()) tables.Add(reader["TableName"].ToString());
					}
				}

				_logService.LogInfo($"📊 Retrieved {tables.Count} tables from database");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to retrieve tables", ex);
				throw;
			}

			return tables;
		}

		public async Task<DataTable> QueryTableAsync(ConnectionInfo connectionInfo, string tableName,
			int maxRows = 1000)
		{
			var dataTable = new DataTable();

			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					var query = $"SELECT TOP {maxRows} * FROM [{tableName}]";

					using (var adapter = new SqlDataAdapter(query, connection))
					{
						adapter.Fill(dataTable);
					}
				}

				_logService.LogInfo($"📋 Queried table {tableName}: {dataTable.Rows.Count} rows returned");
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to query table {tableName}", ex);
				throw;
			}

			return dataTable;
		}

		public async Task<DataTable> ExecuteCustomQueryAsync(ConnectionInfo connectionInfo, string query)
		{
			var dataTable = new DataTable();

			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					using (var adapter = new SqlDataAdapter(query, connection))
					{
						adapter.Fill(dataTable);
					}
				}

				_logService.LogInfo($"🔍 Custom query executed: {dataTable.Rows.Count} rows returned");
			}
			catch (Exception ex)
			{
				_logService.LogError("Custom query execution failed", ex);
				throw;
			}

			return dataTable;
		}

		public async Task<List<DataRecommendation>> AnalyzeTableDataAsync(ConnectionInfo connectionInfo,
			string tableName)
		{
			var recommendations = new List<DataRecommendation>();

			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					// Analyze table structure and data
					await AnalyzeTableStructure(connection, tableName, recommendations);
					await AnalyzeDataQuality(connection, tableName, recommendations);
					await AnalyzePerformance(connection, tableName, recommendations);
					await AnalyzeDataDistribution(connection, tableName, recommendations);
				}

				_logService.LogInfo($"🎯 Generated {recommendations.Count} recommendations for {tableName}");
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to analyze table {tableName}", ex);
				recommendations.Add(new DataRecommendation
				{
					Type = "Error",
					Title = "Analysis Failed",
					Description = $"Could not analyze table: {ex.Message}",
					Severity = "High",
					Action = "Check table permissions and structure",
					Icon = "❌"
				});
			}

			return recommendations;
		}

		public async Task<DatabaseSummary> GetDatabaseSummaryAsync(ConnectionInfo connectionInfo)
		{
			var summary = new DatabaseSummary
			{
				TableRowCounts = new Dictionary<string, int>(),
				LargestTables = new List<string>(),
				EmptyTables = new List<string>(),
				LastAnalyzed = DateTime.Now
			};

			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					var summaryQuery = @"
                        SELECT 
                            t.name as TableName,
                            p.rows as RowCount,
                            (SUM(a.total_pages) * 8) / 1024 as TableSizeMB
                        FROM sys.tables t
                        INNER JOIN sys.partitions p ON t.object_id = p.object_id
                        INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                        WHERE t.is_ms_shipped = 0
                        AND p.index_id IN (0,1)
                        GROUP BY t.name, p.rows
                        ORDER BY p.rows DESC";

					using (var command = new SqlCommand(summaryQuery, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var tableName = reader["TableName"].ToString();
							var rowCount = Convert.ToInt32(reader["RowCount"]);

							summary.TableRowCounts[tableName] = rowCount;
							summary.TotalRows += rowCount;

							if (rowCount == 0)
								summary.EmptyTables.Add(tableName);
							else if (summary.LargestTables.Count < 5)
								summary.LargestTables.Add($"{tableName} ({rowCount:N0} rows)");
						}
					}

					summary.TotalTables = summary.TableRowCounts.Count;
				}

				_logService.LogInfo(
					$"📊 Database summary: {summary.TotalTables} tables, {summary.TotalRows:N0} total rows");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to generate database summary", ex);
				throw;
			}

			return summary;
		}

		private async Task AnalyzeTableStructure(SqlConnection connection, string tableName,
			List<DataRecommendation> recommendations)
		{
			// Check for primary key
			var pkQuery = $@"
                SELECT COUNT(*) as PKCount
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                WHERE TABLE_NAME = '{tableName.Split('.').Last()}' 
                AND CONSTRAINT_TYPE = 'PRIMARY KEY'";

			using (var command = new SqlCommand(pkQuery, connection))
			{
				var pkCount = (int)await command.ExecuteScalarAsync();

				if (pkCount == 0)
					recommendations.Add(new DataRecommendation
					{
						Type = "Structure",
						Title = "Missing Primary Key",
						Description = "This table doesn't have a primary key defined.",
						Severity = "Medium",
						Action = "Consider adding a primary key for better performance and data integrity",
						Icon = "🔑"
					});
			}

			// Check for indexes
			var indexQuery = $@"
                SELECT COUNT(*) as IndexCount
                FROM sys.indexes i
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = '{tableName.Split('.').Last()}' 
                AND i.type > 0"; // Exclude heap

			using (var command = new SqlCommand(indexQuery, connection))
			{
				var indexCount = (int)await command.ExecuteScalarAsync();

				if (indexCount <= 1) // Only clustered index or no indexes
					recommendations.Add(new DataRecommendation
					{
						Type = "Performance",
						Title = "Limited Indexes",
						Description = "This table has few or no indexes.",
						Severity = "Low",
						Action = "Consider adding indexes on frequently queried columns",
						Icon = "🏃"
					});
			}
		}

		private async Task AnalyzeDataQuality(SqlConnection connection, string tableName,
			List<DataRecommendation> recommendations)
		{
			// Check for NULL values in important columns
			var nullCheckQuery = $@"
                SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = '{tableName.Split('.').Last()}'";

			using (var command = new SqlCommand(nullCheckQuery, connection))
			using (var reader = await command.ExecuteReaderAsync())
			{
				var nullableColumns = new List<string>();
				while (await reader.ReadAsync())
					if (reader["IS_NULLABLE"].ToString() == "YES")
						nullableColumns.Add(reader["COLUMN_NAME"].ToString());

				if (nullableColumns.Count > 0)
					recommendations.Add(new DataRecommendation
					{
						Type = "Data Quality",
						Title = "Nullable Columns Detected",
						Description = $"Found {nullableColumns.Count} nullable columns.",
						Severity = "Low",
						Action = "Review if these columns should allow NULL values",
						Icon = "🔍"
					});
			}
		}

		private async Task AnalyzePerformance(SqlConnection connection, string tableName,
			List<DataRecommendation> recommendations)
		{
			// Check table size and row count
			var sizeQuery = $@"
                SELECT 
                    p.rows as RowCount,
                    (SUM(a.total_pages) * 8) / 1024 as TableSizeMB
                FROM sys.tables t
                INNER JOIN sys.partitions p ON t.object_id = p.object_id
                INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                WHERE t.name = '{tableName.Split('.').Last()}'
                AND p.index_id IN (0,1)
                GROUP BY p.rows";

			using (var command = new SqlCommand(sizeQuery, connection))
			using (var reader = await command.ExecuteReaderAsync())
			{
				if (await reader.ReadAsync())
				{
					var rowCount = Convert.ToInt64(reader["RowCount"]);
					var sizeMB = Convert.ToDecimal(reader["TableSizeMB"]);

					if (rowCount > 100000)
						recommendations.Add(new DataRecommendation
						{
							Type = "Performance",
							Title = "Large Table Detected",
							Description = $"Table has {rowCount:N0} rows ({sizeMB:N1} MB).",
							Severity = "Medium",
							Action = "Consider partitioning or archiving strategies",
							Icon = "📈"
						});

					if (rowCount == 0)
						recommendations.Add(new DataRecommendation
						{
							Type = "Data Quality",
							Title = "Empty Table",
							Description = "This table contains no data.",
							Severity = "Low",
							Action = "Verify if this table should contain data",
							Icon = "📭"
						});
				}
			}
		}

		private async Task AnalyzeDataDistribution(SqlConnection connection, string tableName,
			List<DataRecommendation> recommendations)
		{
			try
			{
				// Analyze data distribution for potential insights
				var statsQuery = $@"
                    SELECT 
                        COUNT(*) as TotalRows,
                        COUNT(DISTINCT *) as UniqueRows
                    FROM (SELECT TOP 10000 * FROM [{tableName}]) t";

				using (var command = new SqlCommand(statsQuery, connection))
				using (var reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						var totalRows = Convert.ToInt32(reader["TotalRows"]);
						var uniqueRows = Convert.ToInt32(reader["UniqueRows"]);

						if (totalRows > 0 && uniqueRows < totalRows * 0.9)
							recommendations.Add(new DataRecommendation
							{
								Type = "Data Quality",
								Title = "Potential Duplicate Data",
								Description = $"Only {uniqueRows * 100.0 / totalRows:F1}% of rows are unique.",
								Severity = "Medium",
								Action = "Review data for potential duplicates",
								Icon = "🔄"
							});
					}
				}
			}
			catch
			{
				// Skip this analysis if it fails
			}
		}
	}
}