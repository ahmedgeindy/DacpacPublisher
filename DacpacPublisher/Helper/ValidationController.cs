using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Handles all validation-related business logic
	/// Extracted from DacpacPublisherForm to reduce class size and improve maintainability
	/// </summary>
	public class ValidationController
	{
		private readonly DacpacPublisherForm _form;
		private readonly ILogService _logService;

		public ValidationController(DacpacPublisherForm form, ILogService logService)
		{
			_form = form ?? throw new ArgumentNullException(nameof(form));
			_logService = logService ?? throw new ArgumentNullException(nameof(logService));
		}

		/// <summary>
		/// Validates configuration before deployment
		/// MOVED FROM: DacpacPublisherForm.ValidateConfigurationAsync()
		/// </summary>
		public async Task<bool> ValidateConfigurationAsync()
		{
			var errors = new List<string>();

			try
			{
				_logService.LogInfo("🔍 Starting configuration validation...");

				// Basic validation with null checks
				if (string.IsNullOrWhiteSpace(_form.txtServerName?.Text))
					errors.Add("Server Name is required");

				if (string.IsNullOrWhiteSpace(_form.cboDatabases?.Text))
					errors.Add("Target Database must be selected");

				if (string.IsNullOrWhiteSpace(_form.txtDacpacPath?.Text))
					errors.Add("DACPAC file path is required");
				else if (!File.Exists(_form.txtDacpacPath.Text.Trim()))
					errors.Add($"DACPAC file not found: {_form.txtDacpacPath.Text}");

				// Authentication validation
				if (!(_form.chkWindowsAuth?.Checked ?? false))
				{
					if (string.IsNullOrWhiteSpace(_form.txtUsername?.Text))
						errors.Add("Username is required for SQL authentication");
					if (string.IsNullOrWhiteSpace(_form.txtPassword?.Text))
						errors.Add("Password is required for SQL authentication");
				}

				// SQL Agent Jobs validation
				if (_form.chkCreateSqlAgentJobs?.Checked == true)
				{
					if (string.IsNullOrWhiteSpace(_form.txtJobOwnerLoginName?.Text))
						errors.Add("Job Owner Login Name is required when creating SQL Agent Jobs");

					if (string.IsNullOrWhiteSpace(_form.txtJobScriptsFolder?.Text))
						errors.Add("Job Scripts Folder is required when creating SQL Agent Jobs");
					else if (!Directory.Exists(_form.txtJobScriptsFolder.Text.Trim()))
						errors.Add($"Job Scripts Folder not found: {_form.txtJobScriptsFolder.Text}");
				}

				// IMPROVED: Synonym validation - now handles auto-detection
				if (_form.chkCreateSynonyms?.Checked == true)
				{
					try
					{
						var synonymValidation = await ValidateSynonymConfigurationAsync();
						if (!synonymValidation)
						{
							errors.Add("Synonym configuration validation was cancelled by user");
						}
					}
					catch (Exception ex)
					{
						errors.Add($"Synonym validation failed: {ex.Message}");
					}
				}

				// Smart procedures validation
				if (_form.chkExecuteProcedures?.Checked == true && _form._currentConfig?.SmartProcedures?.Any() == true)
				{
					var procedureErrors = await ValidateSmartProceduresAsync();
					errors.AddRange(procedureErrors);
				}

				if (errors.Any())
				{
					string errorMessage = "Please fix the following issues:\n\n" +
										string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
					MessageBox.Show(errorMessage, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);

					_logService.LogWarning($"Configuration validation failed: {errors.Count} errors found");
					return false;
				}

				_logService.LogInfo("✅ Configuration validation passed");
				return true;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Configuration validation failed", ex);
				MessageBox.Show($"Validation failed: {ex.Message}", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		/// <summary>
		/// Validates job scripts folder and contents
		/// MOVED FROM: DacpacPublisherForm.ValidateJobScriptsFolderAsync()
		/// </summary>
		public async Task ValidateJobScriptsFolderAsync()
		{
			try
			{
				string folderPath = "";

				// Safely get the folder path from UI thread
				if (_form.InvokeRequired)
				{
					folderPath = (string)_form.Invoke(new Func<string>(() => _form.txtJobScriptsFolder.Text.Trim()));
				}
				else
				{
					folderPath = _form.txtJobScriptsFolder.Text.Trim();
				}

				if (string.IsNullOrEmpty(folderPath))
				{
					UpdateJobValidationUI("No folder specified", System.Drawing.Color.Gray);
					return;
				}

				if (!Directory.Exists(folderPath))
				{
					UpdateJobValidationUI("❌ Folder does not exist", System.Drawing.Color.Red);
					return;
				}

				UpdateStatusUI("Validating job scripts...");

				// Use deployment service to validate job scripts
				var deploymentService = _form._deploymentService; // Access from form
				var validationResult = await deploymentService.ValidateJobScriptsAsync(folderPath);

				if (validationResult?.IsValid == true)
				{
					var jobNames = validationResult.JobScripts.Select(js => js.JobName).ToList();
					var message = $"✅ Found {validationResult.JobCount} valid job script(s):\n" +
								 string.Join("\n", jobNames.Select(name => $"• {name}"));

					UpdateJobValidationUI(message, System.Drawing.Color.Green);
					EnableSqlAgentJobs(true);
				}
				else
				{
					string message = validationResult?.ErrorMessage ?? "Unknown validation error";
					UpdateJobValidationUI($"⚠️ {message}", System.Drawing.Color.Orange);
					EnableSqlAgentJobs(true);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error validating job scripts folder", ex);
				UpdateJobValidationUI($"❌ Validation error: {ex.Message}", System.Drawing.Color.Red);
			}
			finally
			{
				UpdateStatusUI("Ready");
			}
		}

		/// <summary>
		/// Validates synonym configuration
		/// MOVED FROM: DacpacPublisherForm.ValidateSynonymConfigurationAsync()
		/// </summary>
		public async Task<bool> ValidateSynonymConfigurationAsync()
		{
			try
			{
				if (!(_form._currentConfig?.CreateSynonyms ?? false))
				{
					return true; // Synonyms disabled, nothing to validate
				}

				_logService?.LogInfo("🔍 Validating synonym configuration...");

				// NEW: Auto-detect source database if not specified
				if (string.IsNullOrEmpty(_form._currentConfig.SynonymSourceDb))
				{
					_form._currentConfig.SynonymSourceDb = await AutoDetectSourceDatabaseAsync();
					_logService?.LogInfo($"🤖 Auto-detected source database: {_form._currentConfig.SynonymSourceDb}");
				}

				// If still no source database, use default
				if (string.IsNullOrEmpty(_form._currentConfig.SynonymSourceDb))
				{
					_form._currentConfig.SynonymSourceDb = "HiveCFMSurvey"; // Your default
					_logService?.LogInfo($"📝 Using default source database: {_form._currentConfig.SynonymSourceDb}");
				}

				// Get target databases
				var targetDatabases = GetSynonymTargetDatabases();

				if (targetDatabases.Count == 0)
				{
					_logService?.LogInfo("ℹ️ No target databases need synonyms - all targets are same as source");

					// This is valid - just show info to user
					var infoChoice = MessageBox.Show(
						$"ℹ️ Synonym Analysis:\n\n" +
						$"Source Database: {_form._currentConfig.SynonymSourceDb}\n" +
						$"Target Database(s): {_form._currentConfig.Database}\n\n" +
						$"Since source and target are the same, no synonyms are needed.\n" +
						$"This is normal when deploying to the database that contains the actual tables.\n\n" +
						$"Continue deployment?",
						"No Synonyms Needed",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Information);

					return infoChoice == DialogResult.Yes;
				}

				// Show what will happen
				var summaryMessage = $"✅ Synonym Configuration Valid:\n\n" +
								   $"📋 Source Database: {_form._currentConfig.SynonymSourceDb}\n" +
								   $"🎯 Target Database(s): {string.Join(", ", targetDatabases)}\n\n" +
								   $"Synonyms will be created in target databases.\n" +
								   $"Example: [{targetDatabases.First()}].[dbo].[CFMSurveyUser] → [{_form._currentConfig.SynonymSourceDb}].[dbo].[CFMSurveyUser]\n\n" +
								   $"Configuration is valid. Continue?";

				var confirmChoice = MessageBox.Show(summaryMessage, "Synonym Configuration Valid",
					MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (confirmChoice == DialogResult.Yes)
				{
					_logService?.LogInfo("✅ Synonym configuration validated successfully");
					return true;
				}
				else
				{
					_logService?.LogInfo("⚠️ User cancelled synonym configuration");
					return false;
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error validating synonym configuration", ex);
				MessageBox.Show($"❌ Error validating synonym configuration: {ex.Message}",
					"Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}


		/// <summary>
		/// Validates smart procedures configuration
		/// MOVED FROM: DacpacPublisherForm.ValidateSmartProceduresAsync()
		/// </summary>
		public async Task<List<string>> ValidateSmartProceduresAsync()
		{
			var errors = new List<string>();

			try
			{
				if (_form._currentConfig?.SmartProcedures?.Any() != true)
					return errors;

				// Check for procedures with no database assignment
				var orphanedProcs = _form._currentConfig.SmartProcedures
					.Where(p => p != null && !p.ExecuteOnDatabase1 && !p.ExecuteOnDatabase2);

				if (orphanedProcs.Any())
				{
					var procNames = orphanedProcs
						.Select(p => p.Name ?? "Unknown")
						.Where(name => !string.IsNullOrEmpty(name));

					if (procNames.Any())
					{
						errors.Add($"Procedures with no database assignment: {string.Join(", ", procNames)}");
					}
				}

				// Check for duplicate procedure names
				var duplicates = _form._currentConfig.SmartProcedures
					.GroupBy(p => p.Name)
					.Where(g => g.Count() > 1)
					.Select(g => g.Key);

				if (duplicates.Any())
				{
					errors.Add($"Duplicate procedure names: {string.Join(", ", duplicates)}");
				}

				// Check for invalid execution orders
				var invalidOrders = _form._currentConfig.SmartProcedures
					.Where(p => p.ExecutionOrder <= 0);

				if (invalidOrders.Any())
				{
					errors.Add("All procedures must have execution order greater than 0");
				}

				// Placeholder for any async operations
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				errors.Add($"Error validating smart procedures: {ex.Message}");
				_logService?.LogError("Smart procedure validation failed", ex);
			}

			return errors;
		}

		/// <summary>
		/// Validates that required files exist and are accessible
		/// </summary>
		public bool ValidateRequiredFiles()
		{
			try
			{
				// Check DACPAC file
				if (string.IsNullOrEmpty(_form.txtDacpacPath?.Text) || !File.Exists(_form.txtDacpacPath.Text))
				{
					MessageBox.Show("DACPAC file is required and must exist.", "Validation Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				// Check job scripts folder if jobs are enabled
				if (_form.chkCreateSqlAgentJobs?.Checked == true)
				{
					if (string.IsNullOrEmpty(_form.txtJobScriptsFolder?.Text) ||
						!Directory.Exists(_form.txtJobScriptsFolder.Text))
					{
						MessageBox.Show("Job Scripts Folder is required when creating SQL Agent Jobs.",
							"Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				_logService.LogError("Error validating required files", ex);
				MessageBox.Show($"Error validating files: {ex.Message}", "Validation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		#region Private Helper Methods

		private void UpdateJobValidationUI(string message, System.Drawing.Color color)
		{
			if (_form.InvokeRequired)
			{
				_form.Invoke(new Action(() => UpdateJobValidationUI(message, color)));
				return;
			}

			try
			{
				if (_form.lblJobDescriptions != null)
				{
					_form.lblJobDescriptions.Text = message;
					_form.lblJobDescriptions.ForeColor = color;
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error updating job validation UI", ex);
			}
		}

		private void UpdateStatusUI(string status)
		{
			if (_form.InvokeRequired)
			{
				_form.Invoke(new Action(() => UpdateStatusUI(status)));
				return;
			}

			if (_form.toolStripStatusLabel != null)
				_form.toolStripStatusLabel.Text = status;
		}

		private void EnableSqlAgentJobs(bool enabled)
		{
			if (_form.InvokeRequired)
			{
				_form.Invoke(new Action(() => EnableSqlAgentJobs(enabled)));
				return;
			}

			if (_form.chkCreateSqlAgentJobs != null)
				_form.chkCreateSqlAgentJobs.Enabled = enabled;
		}



		private async Task<string> AutoDetectSourceDatabaseAsync()
		{
			try
			{
				// Get all databases from the server
				var connectionInfo = new ConnectionInfo
				{
					ServerName = _form._currentConfig.ServerName,
					WindowsAuth = _form._currentConfig.WindowsAuth,
					Username = _form._currentConfig.Username,
					Password = _form._currentConfig.Password,
					Database = "master"
				};

				var allDatabases = await _form._connectionService.GetDatabasesAsync(connectionInfo);

				// Look for HiveCFMSurvey pattern databases
				var surveyDatabases = allDatabases
					.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();

				if (surveyDatabases.Count == 1)
				{
					return surveyDatabases[0];
				}

				if (surveyDatabases.Count > 1)
				{
					// Return the first one as default
					return surveyDatabases[0];
				}

				// No HiveCFMSurvey found, return null (will use default)
				return null;
			}
			catch (Exception ex)
			{
				_logService?.LogWarning($"Could not auto-detect source database: {ex.Message}");
				return null;
			}
		}
		private List<string> GetSynonymTargetDatabases()
		{
			var targetDatabases = new List<string>();

			try
			{
				// Primary database
				if (!string.IsNullOrEmpty(_form._currentConfig.Database) &&
				    !string.Equals(_form._currentConfig.Database, _form._currentConfig.SynonymSourceDb, StringComparison.OrdinalIgnoreCase))
				{
					targetDatabases.Add(_form._currentConfig.Database);
				}

				// Secondary databases if multiple deployment is enabled
				if (_form._currentConfig.EnableMultipleDatabases && _form._currentConfig.DeploymentTargets?.Any() == true)
				{
					foreach (var target in _form._currentConfig.DeploymentTargets.Where(t => t.IsEnabled))
					{
						if (!string.IsNullOrEmpty(target.Database) &&
						    !string.Equals(target.Database, _form._currentConfig.SynonymSourceDb, StringComparison.OrdinalIgnoreCase) &&
						    !targetDatabases.Contains(target.Database))
						{
							targetDatabases.Add(target.Database);
						}
					}
				}

				return targetDatabases;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error determining synonym target databases", ex);
				return new List<string>();
			}
		}

		#endregion
	}
}