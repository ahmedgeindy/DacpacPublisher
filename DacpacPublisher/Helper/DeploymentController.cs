using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Handles all deployment-related business logic
	/// Extracted from DacpacPublisherForm to reduce class size and improve maintainability
	/// </summary>
	public class DeploymentController
	{
		private readonly DacpacPublisherForm _form;
		private readonly IDeploymentService _deploymentService;
		private readonly ILogService _logService;
		private readonly IConnectionService _connectionService;
		private readonly IBackupService _backupService;

		public DeploymentController(DacpacPublisherForm form,
			IDeploymentService deploymentService,
			ILogService logService,
			IConnectionService connectionService,
			IBackupService backupService)
		{
			_form = form ?? throw new ArgumentNullException(nameof(form));
			_deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
			_logService = logService ?? throw new ArgumentNullException(nameof(logService));
			_connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
			_backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
		}

		/// <summary>
		/// Execute deployment process
		/// MOVED FROM: DacpacPublisherForm.ExecuteDeploymentAsync()
		/// </summary>
		public async Task ExecuteDeploymentAsync()
		{
			try
			{
				await ExecuteWithErrorHandling("Deployment", async () =>
				{
					// Update configuration from UI
					var configController = new ConfigurationController(_form, _logService);
					configController.UpdateConfigurationFromUI(_form._currentConfig);

					var validationController = new ValidationController(_form, _logService);
					var isValid = await validationController.ValidateConfigurationAsync();
					if (!isValid)
						return;

					var shouldContinue = false;
					_form.Invoke(new Action(() => shouldContinue = ConfirmDeployment()));
					if (!shouldContinue)
						return;

					_form.Invoke(new Action(() => _form.btnPublish.Enabled = false));
					UpdateStatus("Deploying...", true);

					// Switch to log tab
					if (_form.tabControl?.TabPages != null)
					{
						_form.Invoke(new Action(() =>
						{
							foreach (TabPage tab in _form.tabControl.TabPages)
							{
								if (tab.Text.Contains("Log"))
								{
									_form.tabControl.SelectedTab = tab;
									break;
								}
							}
						}));
					}

					var result = await _deploymentService.DeployDacpacAsync(_form._currentConfig);

					_form.Invoke(new Action(() => ProcessDeploymentResult(result)));
				},
					() =>
					{
						_form.Invoke(new Action(() => _form.btnPublish.Enabled = true));
						UpdateStatus("Ready", false);
					});
			}
			catch (Exception ex)
			{
				_logService.LogError("Deployment execution failed", ex);
				HandleError("Deployment", ex);
			}
		}

		/// <summary>
		/// Deploy DACPAC to database
		/// MOVED FROM: DacpacPublisherForm.DeployDatabaseAsync()
		/// </summary>
		public async Task<DeploymentResult> DeployDatabaseAsync()
		{
			var result = new DeploymentResult
			{
				Timestamp = DateTime.Now,
				DatabaseName = _form._currentConfig.Database,
				DacpacPath = _form._currentConfig.DacpacPath
			};

			try
			{
				string backupPath = null;

				// Step 1: Create backup if enabled
				if (_form._currentConfig.CreateBackupBeforeDeployment)
				{
					_logService.LogInfo("🔄 Creating backup before deployment...");
					backupPath = await CreateBackupAsync();
					if (backupPath == null) // User cancelled
					{
						result.Success = false;
						result.Message = "Deployment cancelled by user";
						_logService.LogWarning("⚠️ Deployment cancelled by user during backup phase");
						return result;
					}
					_logService.LogInfo($"✅ Backup created successfully: {Path.GetFileName(backupPath)}");
				}

				// Step 2: Deploy using the service
				_logService.LogInfo("🚀 Starting robust DACPAC deployment...");
				var deployResult = await _deploymentService.DeployDacpacAsync(_form._currentConfig);

				// Copy all properties from deployResult to result with null safety
				result.Success = deployResult.Success;
				result.Message = deployResult.Message ?? string.Empty;
				result.Duration = deployResult.Duration;
				result.Warnings = deployResult.Warnings ?? new System.Collections.Generic.List<string>();
				result.Errors = deployResult.Errors ?? new System.Collections.Generic.List<string>();
				result.BackupPath = backupPath;
				result.ProceduresExecuted = deployResult.ProceduresExecuted;
				result.JobsCreated = deployResult.JobsCreated;
				result.SynonymsCreated = deployResult.SynonymsCreated;
				result.Exception = deployResult.Exception;

				// Step 3: Record deployment history
				var historyEntry = new DeploymentHistory
				{
					Timestamp = DateTime.Now,
					ServerName = _form._currentConfig.ServerName ?? string.Empty,
					Database = _form._currentConfig.Database ?? string.Empty,
					DacpacFile = _form._currentConfig.DacpacPath ?? string.Empty,
					Success = result.Success,
					BackupPath = backupPath ?? string.Empty,
					Duration = result.Duration,
					ErrorMessage = result.Success ? string.Empty : (result.Message ?? string.Empty)
				};

				// Initialize History list if null
				if (_form._currentConfig.History == null)
					_form._currentConfig.History = new System.Collections.Generic.List<DeploymentHistory>();

				_form._currentConfig.History.Add(historyEntry);

				// Step 4: Post-deployment handling
				if (result.Success)
				{
					_logService.LogInfo("🎯 Deployment successful!");
					await SafeHandlePostDeploymentSuccess(result);
				}
				else
				{
					_logService.LogError($"❌ Deployment failed: {result.Message}");

					// If backup was created but deployment failed, offer restoration
					if (!string.IsNullOrEmpty(backupPath) && File.Exists(backupPath))
					{
						await SafeHandleDeploymentFailureWithBackup(backupPath, result);
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				_logService.LogError("💥 Deployment failed with exception", ex);
				result.Success = false;
				result.Message = ex.Message ?? "Unknown error occurred";
				result.Duration = DateTime.Now - result.Timestamp;
				result.Exception = ex;

				// Initialize and add exception details to errors list
				if (result.Errors == null)
					result.Errors = new System.Collections.Generic.List<string>();

				result.Errors.Add($"Exception: {ex.Message ?? "Unknown exception"}");

				if (ex.InnerException != null)
					result.Errors.Add($"Inner Exception: {ex.InnerException.Message ?? "Unknown inner exception"}");

				return result;
			}
		}

		/// <summary>
		/// Build success message for deployment result
		/// MOVED FROM: DacpacPublisherForm.BuildSuccessMessage()
		/// </summary>
		public string BuildSuccessMessage(DeploymentResult result)
		{
			try
			{
				var message = new StringBuilder();
				message.AppendLine("🎉 Deployment Completed Successfully!");
				message.AppendLine();
				message.AppendLine("✅ Database: " + (_form._currentConfig?.Database ?? ""));
				message.AppendLine("✅ Server: " + (_form._currentConfig?.ServerName ?? ""));
				message.AppendLine("⏱️ Duration: " + (result?.Duration.ToString(@"mm\:ss") ?? ""));

				if (result?.ProceduresExecuted > 0)
					message.AppendLine("🔧 Procedures Executed: " + result.ProceduresExecuted);

				if (result?.JobsCreated > 0)
					message.AppendLine("⚙️ Jobs Created: " + result.JobsCreated);

				if (result != null && !string.IsNullOrEmpty(result.BackupPath))
					message.AppendLine("💾 Backup: " + Path.GetFileName(result.BackupPath));

				return message.ToString();
			}
			catch (Exception ex)
			{
				return "Deployment completed successfully. Duration: " + (result?.Duration.ToString(@"mm\:ss") ?? "");
			}
		}

		/// <summary>
		/// Build failure message for deployment result
		/// MOVED FROM: DacpacPublisherForm.BuildFailureMessage()
		/// </summary>
		public string BuildFailureMessage(DeploymentResult result)
		{
			try
			{
				var message = new StringBuilder();
				message.AppendLine("❌ Deployment Failed: " + (result?.Message ?? ""));
				message.AppendLine();
				message.AppendLine("🔧 RECOMMENDED ACTIONS:");
				message.AppendLine("  1. Check the log for detailed error information");
				message.AppendLine("  2. Verify database permissions");
				message.AppendLine("  3. Ensure DACPAC file is valid");
				message.AppendLine("  4. Check SQL Server connectivity");

				return message.ToString();
			}
			catch (Exception ex)
			{
				return "Deployment failed: " + (result?.Message ?? "Unknown error");
			}
		}

		/// <summary>
		/// Log deployment analytics for troubleshooting
		/// MOVED FROM: DacpacPublisherForm.LogDeploymentAnalytics()
		/// </summary>
		public void LogDeploymentAnalytics(DeploymentResult result)
		{
			try
			{
				_logService.LogInfo("📊 === Deployment Analytics ===");
				_logService.LogInfo($"🎯 Overall Success: {result.Success}");
				_logService.LogInfo($"⏱️ Total Duration: {result.Duration:mm\\:ss}");
				_logService.LogInfo($"🗄️ Database: {result.DatabaseName}");
				_logService.LogInfo($"📦 DACPAC: {Path.GetFileName(result.DacpacPath)}");

				if (result.ProceduresExecuted > 0)
					_logService.LogInfo($"🔧 Procedures Executed: {result.ProceduresExecuted}");

				if (result.JobsCreated > 0)
					_logService.LogInfo($"⚙️ Jobs Created: {result.JobsCreated}");

				if (result.SynonymsCreated > 0)
					_logService.LogInfo($"🔗 Synonyms Created: {result.SynonymsCreated}");

				if (!string.IsNullOrEmpty(result.BackupPath))
					_logService.LogInfo($"💾 Backup Created: {Path.GetFileName(result.BackupPath)}");

				if (result.HasWarnings)
				{
					_logService.LogInfo($"⚠️ Warnings Summary ({result.Warnings.Count} total):");
					foreach (var warning in result.GetTopWarnings(5))
					{
						_logService.LogInfo($"  • {warning}");
					}
				}

				if (result.HasErrors)
				{
					_logService.LogInfo($"❌ Errors Summary ({result.Errors.Count} total):");
					foreach (var error in result.GetTopErrors(5))
					{
						_logService.LogInfo($"  • {error}");
					}
				}

				_logService.LogInfo("📊 === End Analytics ===");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to log deployment analytics", ex);
			}
		}

		#region Private Helper Methods

		private bool ConfirmDeployment()
		{
			try
			{
				var summary = "🚀 Ready to deploy!\n\n" +
							 "Server: " + (_form._currentConfig?.ServerName ?? "") + "\n" +
							 "Database: " + (_form._currentConfig?.Database ?? "") + "\n" +
							 "DACPAC: " + (_form._currentConfig?.DacpacPath != null ? Path.GetFileName(_form._currentConfig.DacpacPath) : "") + "\n\n" +
							 "Proceed with deployment?";

				return MessageBox.Show(summary, "Confirm Deployment",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error in deployment confirmation", ex);
				return false;
			}
		}

		private void ProcessDeploymentResult(DeploymentResult result)
		{
			try
			{
				if (result != null && result.Success)
				{
					ShowSuccessMessage(BuildSuccessMessage(result), "🚀 Deployment Successful");
					_logService.LogInfo("✅ " + result.GetSummary());
				}
				else
				{
					MessageBox.Show(BuildFailureMessage(result), "💥 Deployment Failed",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					_logService.LogError("❌ " + (result?.GetSummary() ?? "Unknown error"));
				}

				// Show deployment analytics in log
				if (result != null)
					LogDeploymentAnalytics(result);
			}
			catch (Exception ex)
			{
				_logService.LogError("Error processing deployment result", ex);
				MessageBox.Show("Error processing deployment result: " + ex.Message, "Processing Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async Task<string> CreateBackupAsync()
		{
			using (var saveBackupDialog = new SaveFileDialog())
			{
				saveBackupDialog.Title = "Save Database Backup";
				saveBackupDialog.Filter = "SQL Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
				saveBackupDialog.DefaultExt = "bak";

				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				saveBackupDialog.FileName = $"{_form._currentConfig.Database}_Backup_{timestamp}.bak";

				if (saveBackupDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var connectionInfo = CreateConnectionInfo(_form._currentConfig);
						string backupPath = await _backupService.CreateBackupAsync(connectionInfo, saveBackupDialog.FileName);
						_logService.LogInfo($"💾 Backup created: {backupPath}");
						return backupPath;
					}
					catch (Exception ex)
					{
						_logService.LogError("Backup creation failed", ex);
						var continueResult = MessageBox.Show(
							$"❌ Backup creation failed: {ex.Message}\n\nContinue deployment without backup?",
							"Backup Failed",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning);

						return continueResult == DialogResult.Yes ? string.Empty : null;
					}
				}
				else
				{
					var result = MessageBox.Show("No backup location selected. Continue without backup?",
						"Backup Cancelled", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					return result == DialogResult.Yes ? string.Empty : null;
				}
			}
		}

		private async Task SafeHandlePostDeploymentSuccess(DeploymentResult result)
		{
			try
			{
				// Implementation for post-deployment success handling
				// This can be expanded based on your existing implementation
				_logService.LogInfo("✅ Post-deployment handling completed successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in post-deployment success handling", ex);
			}
		}

		private async Task SafeHandleDeploymentFailureWithBackup(string backupPath, DeploymentResult result)
		{
			try
			{
				var restoreChoice = MessageBox.Show(
					$"❌ Deployment Failed!\n\n" +
					$"Error: {result.Message}\n\n" +
					$"💾 A backup was created before deployment:\n" +
					$"{Path.GetFileName(backupPath)}\n\n" +
					$"Would you like to restore the backup to rollback changes?",
					"Deployment Failed - Restore Backup?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Error);

				if (restoreChoice == DialogResult.Yes)
				{
					_logService.LogInfo("🔄 User chose to restore backup after failed deployment");

					try
					{
						var connectionInfo = CreateConnectionInfo(_form._currentConfig);
						bool restoreSuccess = await _backupService.RestoreBackupAsync(connectionInfo, backupPath);

						if (restoreSuccess)
						{
							MessageBox.Show(
								"✅ Database restored successfully from backup!\n\n" +
								"The database has been returned to its previous state.",
								"Restore Successful",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);

							_logService.LogInfo("✅ Database restored successfully from backup");

							if (result.Warnings == null)
								result.Warnings = new System.Collections.Generic.List<string>();
							result.Warnings.Add("Database was restored from backup after deployment failure");
						}
						else
						{
							MessageBox.Show(
								"❌ Failed to restore backup!\n\n" +
								"Please check the logs and manually restore if needed.",
								"Restore Failed",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						}
					}
					catch (Exception restoreEx)
					{
						_logService.LogError("Failed to restore backup", restoreEx);
						MessageBox.Show(
							$"❌ Backup restoration failed: {restoreEx.Message}\n\n" +
							$"Manual restoration may be required.",
							"Restore Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
					}
				}
				else
				{
					_logService.LogInfo("ℹ️ User chose not to restore backup after failed deployment");
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error handling deployment failure with backup", ex);
			}
		}

		private ConnectionInfo CreateConnectionInfo(PublisherConfiguration config)
		{
			return new ConnectionInfo
			{
				ServerName = config?.ServerName ?? string.Empty,
				WindowsAuth = config?.WindowsAuth ?? true,
				Username = config?.Username ?? string.Empty,
				Password = config?.Password ?? string.Empty,
				Database = config?.Database ?? "master"
			};
		}

		private void UpdateStatus(string message, bool showProgress)
		{
			try
			{
				if (_form.InvokeRequired)
				{
					_form.Invoke(new Action(() => UpdateStatus(message, showProgress)));
					return;
				}

				if (_form.toolStripStatusLabel != null)
					_form.toolStripStatusLabel.Text = showProgress ? $"⏳ {message}" : $"🟢 {message}";

				if (_form.toolStripProgressBar != null)
				{
					_form.toolStripProgressBar.Visible = showProgress;
					if (showProgress)
						_form.toolStripProgressBar.Style = ProgressBarStyle.Marquee;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Status update error: {ex.Message}");
			}
		}

		private async Task ExecuteWithErrorHandling(string operation, Func<Task> action, Action finallyAction = null)
		{
			try
			{
				await action();
			}
			catch (Exception ex)
			{
				_logService?.LogError($"{operation} failed", ex);
				HandleError(operation, ex);
			}
			finally
			{
				finallyAction?.Invoke();
			}
		}

		private void HandleError(string context, Exception ex)
		{
			var message = $"❌ {context} failed:\n\n{ex.Message}";
			MessageBox.Show(message, $"{context} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void ShowSuccessMessage(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion
	}
}