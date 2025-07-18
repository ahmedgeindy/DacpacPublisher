using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Handles all configuration-related business logic
	/// Extracted from DacpacPublisherForm to reduce class size and improve maintainability
	/// </summary>
	public class ConfigurationController
	{
		private readonly DacpacPublisherForm _form;
		private readonly ILogService _logService;

		public ConfigurationController(DacpacPublisherForm form, ILogService logService)
		{
			_form = form ?? throw new ArgumentNullException(nameof(form));
			_logService = logService ?? throw new ArgumentNullException(nameof(logService));
		}

		/// <summary>
		/// Updates configuration object from UI controls
		/// MOVED FROM: DacpacPublisherForm.UpdateConfigurationFromUI()
		/// </summary>
		public void UpdateConfigurationFromUI(PublisherConfiguration config)
		{
			try
			{
				if (config == null)
					throw new ArgumentNullException(nameof(config));

				// Basic connection settings
				config.ServerName = _form.txtServerName?.Text?.Trim() ?? string.Empty;
				config.WindowsAuth = _form.chkWindowsAuth?.Checked ?? false;
				config.Username = _form.txtUsername?.Text?.Trim() ?? string.Empty;
				config.Password = _form.txtPassword?.Text ?? string.Empty;
				config.Database = _form.cboDatabases?.SelectedItem?.ToString()?.Trim() ?? string.Empty;
				config.DacpacPath = _form.txtDacpacPath?.Text?.Trim() ?? string.Empty;

				// Feature settings
				config.CreateSynonyms = _form.chkCreateSynonyms?.Checked ?? false;
				config.CreateSqlAgentJobs = _form.chkCreateSqlAgentJobs?.Checked ?? false;
				config.ExecuteProcedures = _form.chkExecuteProcedures?.Checked ?? false;
				config.CreateBackupBeforeDeployment = _form.chkCreateBackup?.Checked ?? false;

				// Multiple databases setting
				config.EnableMultipleDatabases = _form.chkEnableMultipleDatabases?.Checked ?? false;

				// Job settings
				if (config.CreateSqlAgentJobs)
				{
					config.JobOwnerLoginName = _form.txtJobOwnerLoginName?.Text?.Trim() ?? string.Empty;
					config.JobScriptsFolder = _form.txtJobScriptsFolder?.Text?.Trim() ?? string.Empty;
				}

				// Synonym settings
				if (config.CreateSynonyms)
				{
					//config.SynonymSourceDb = _form.txtSynonymSourceDb?.Text?.Trim() ?? string.Empty;
					config.SynonymTargetDatabase = _form.txtSynonymTargetDatabase?.Text?.Trim() ?? "";

					// Get selected synonym target databases if available
					//if (_form.clbSynonymTargets != null)
					//{
					//	config.SynonymTargetDatabases = GetSelectedSynonymTargetDatabases();
					//}
				}

				// Secondary database settings (if multiple databases enabled)
				if (config.EnableMultipleDatabases)
				{
					var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
					var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;

					if (cboDatabases2 != null && txtDacpacPath2 != null)
					{
						// Initialize deployment targets if null
						if (config.DeploymentTargets == null)
							config.DeploymentTargets = new System.Collections.Generic.List<DatabaseDeploymentTarget>();

						// Add or update secondary database target
						var secondaryTarget = config.DeploymentTargets.FirstOrDefault(t => t.Name == "Secondary");
						if (secondaryTarget == null)
						{
							secondaryTarget = new DatabaseDeploymentTarget { Name = "Secondary" };
							config.DeploymentTargets.Add(secondaryTarget);
						}

						secondaryTarget.Database = cboDatabases2.SelectedItem?.ToString()?.Trim() ?? string.Empty;
						secondaryTarget.DacpacPath = txtDacpacPath2.Text?.Trim() ?? string.Empty;
						secondaryTarget.ServerName = config.ServerName; // Use same server
						secondaryTarget.IsEnabled = !string.IsNullOrEmpty(secondaryTarget.Database);
					}
				}

				_logService?.LogInfo("✅ Configuration updated from UI");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to update configuration from UI", ex);
				throw;
			}
		}

		/// <summary>
		/// Updates UI controls from configuration object
		/// MOVED FROM: DacpacPublisherForm.UpdateUIFromConfiguration()
		/// </summary>
		public void UpdateUIFromConfiguration(PublisherConfiguration config)
		{
			try
			{
				if (config == null)
				{
					_logService?.LogWarning("Configuration is null, using defaults");
					config = new PublisherConfiguration();
				}

				// Set initializing flag to prevent event handlers from firing
				_form._isInitializing = true;

				// Connection settings
				if (_form.txtServerName != null) _form.txtServerName.Text = config.ServerName ?? string.Empty;
				if (_form.chkWindowsAuth != null) _form.chkWindowsAuth.Checked = config.WindowsAuth;
				if (_form.txtUsername != null) _form.txtUsername.Text = config.Username ?? string.Empty;
				if (_form.txtPassword != null) _form.txtPassword.Text = config.Password ?? string.Empty;
				if (_form.txtDacpacPath != null) _form.txtDacpacPath.Text = config.DacpacPath ?? string.Empty;

				// Feature settings
				if (_form.chkCreateSynonyms != null) _form.chkCreateSynonyms.Checked = config.CreateSynonyms;
				if (_form.chkCreateSqlAgentJobs != null) _form.chkCreateSqlAgentJobs.Checked = config.CreateSqlAgentJobs;
				if (_form.chkExecuteProcedures != null) _form.chkExecuteProcedures.Checked = config.ExecuteProcedures;
				if (_form.chkCreateBackup != null) _form.chkCreateBackup.Checked = config.CreateBackupBeforeDeployment;
				if (_form.chkEnableMultipleDatabases != null) _form.chkEnableMultipleDatabases.Checked = config.EnableMultipleDatabases;

				// Job settings
				if (_form.txtJobOwnerLoginName != null) _form.txtJobOwnerLoginName.Text = config.JobOwnerLoginName ?? string.Empty;
				if (_form.txtJobScriptsFolder != null) _form.txtJobScriptsFolder.Text = config.JobScriptsFolder ?? string.Empty;

				// Synonym settings
				//if (_form.txtSynonymSourceDb != null) _form.txtSynonymSourceDb.Text = config.SynonymSourceDb ?? string.Empty;
				if (_form.txtSynonymTargetDatabase != null)
				{
					_form.txtSynonymTargetDatabase.Text = config.SynonymTargetDatabase ?? "";
				}
				// Update database dropdown
				if (_form.cboDatabases != null && !string.IsNullOrEmpty(config.Database))
				{
					if (!_form.cboDatabases.Items.Contains(config.Database))
						_form.cboDatabases.Items.Add(config.Database);
					_form.cboDatabases.SelectedItem = config.Database;
				}

				// Update secondary database controls if multiple databases enabled
				if (config.EnableMultipleDatabases && config.DeploymentTargets?.Any() == true)
				{
					var secondaryTarget = config.DeploymentTargets.FirstOrDefault(t => t.Name == "Secondary");
					if (secondaryTarget != null)
					{
						var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
						var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;

						if (cboDatabases2 != null && !string.IsNullOrEmpty(secondaryTarget.Database))
						{
							if (!cboDatabases2.Items.Contains(secondaryTarget.Database))
								cboDatabases2.Items.Add(secondaryTarget.Database);
							cboDatabases2.SelectedItem = secondaryTarget.Database;
						}

						if (txtDacpacPath2 != null)
							txtDacpacPath2.Text = secondaryTarget.DacpacPath ?? string.Empty;
					}
				}

				// Update control states
				UpdateAuthenticationControls();
				UpdateSmartProcedureStatus();

				_logService?.LogInfo("✅ UI updated from configuration");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to update UI from configuration", ex);
				ShowWarningMessage("⚠️ Some settings could not be restored: " + ex.Message,
					"Configuration Load Warning");
			}
			finally
			{
				_form._isInitializing = false;
			}
		}

		/// <summary>
		/// Updates smart procedure status display
		/// MOVED FROM: DacpacPublisherForm.UpdateSmartProcedureStatus()
		/// </summary>
		public void UpdateSmartProcedureStatus()
		{
			if (_form.lblSmartProcedureStatus == null) return;

			try
			{
				if (_form._currentConfig?.SmartProcedures?.Any() == true)
				{
					var count = _form._currentConfig.SmartProcedures.Count;
					var db1Count = _form._currentConfig.SmartProcedures.Count(p => p.ExecuteOnDatabase1);
					var db2Count = _form._currentConfig.SmartProcedures.Count(p => p.ExecuteOnDatabase2);

					_form.lblSmartProcedureStatus.Text = $"✅ {count} procedures configured (Primary: {db1Count}, Secondary: {db2Count})";
					_form.lblSmartProcedureStatus.ForeColor = System.Drawing.Color.Green;
				}
				else
				{
					_form.lblSmartProcedureStatus.Text = "⚙️ Click 'Configure Smart Procedures' to get started";
					_form.lblSmartProcedureStatus.ForeColor = System.Drawing.Color.Gray;
				}
			}
			catch (Exception ex)
			{
				_form.lblSmartProcedureStatus.Text = $"❌ Error: {ex.Message}";
				_form.lblSmartProcedureStatus.ForeColor = System.Drawing.Color.Red;
				_logService?.LogError("Error updating smart procedure status", ex);
			}
		}

		/// <summary>
		/// Creates a connection info object from current configuration
		/// </summary>
		public ConnectionInfo CreateConnectionInfo(PublisherConfiguration config)
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

		/// <summary>
		/// Validates that configuration is properly filled out
		/// </summary>
		public bool IsConfigurationValid(PublisherConfiguration config)
		{
			if (config == null) return false;

			var errors = config.ValidateConfiguration();
			if (errors.Any())
			{
				string errorMessage = "Configuration errors:\n\n" + string.Join("\n", errors);
				MessageBox.Show(errorMessage, "Configuration Validation",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Resets configuration to default values
		/// </summary>
		public void ResetToDefaults()
		{
			try
			{
				var defaultConfig = new PublisherConfiguration
				{
					ServerName = "(local)",
					WindowsAuth = true,
					CreateBackupBeforeDeployment = true
				};

				UpdateUIFromConfiguration(defaultConfig);
				_form._currentConfig = defaultConfig;

				_logService?.LogInfo("Configuration reset to defaults");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error resetting configuration to defaults", ex);
				MessageBox.Show($"Error resetting configuration: {ex.Message}", "Reset Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Gets a summary of current configuration for display
		/// </summary>
		public string GetConfigurationSummary(PublisherConfiguration config)
		{
			if (config == null) return "No configuration loaded";

			try
			{
				return config.GetDeploymentSummary();
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error getting configuration summary", ex);
				return "Error generating configuration summary";
			}
		}

		/// <summary>
		/// Applies configuration changes with validation
		/// </summary>
		public bool ApplyConfiguration(PublisherConfiguration config)
		{
			try
			{
				if (!IsConfigurationValid(config))
					return false;

				UpdateUIFromConfiguration(config);
				_form._currentConfig = config;

				_logService?.LogInfo("Configuration applied successfully");
				return true;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error applying configuration", ex);
				MessageBox.Show($"Error applying configuration: {ex.Message}", "Configuration Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		#region Private Helper Methods

		private void UpdateAuthenticationControls()
		{
			try
			{
				var useWindowsAuth = _form.chkWindowsAuth?.Checked ?? true;

				if (_form.lblUsername != null) _form.lblUsername.Enabled = !useWindowsAuth;
				if (_form.txtUsername != null) _form.txtUsername.Enabled = !useWindowsAuth;
				if (_form.lblPassword != null) _form.lblPassword.Enabled = !useWindowsAuth;
				if (_form.txtPassword != null) _form.txtPassword.Enabled = !useWindowsAuth;

				if (useWindowsAuth)
				{
					if (_form.txtUsername != null) _form.txtUsername.Clear();
					if (_form.txtPassword != null) _form.txtPassword.Clear();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Auth controls update error: {ex.Message}");
			}
		}

		private Control FindControlByName(string name)
		{
			try
			{
				if (string.IsNullOrEmpty(name)) return null;

				return FindControlByNameRecursive(_form, name);
			}
			catch (Exception ex)
			{
				_logService?.LogError($"Error finding control '{name}'", ex);
				return null;
			}
		}

		private Control FindControlByNameRecursive(Control parent, string name)
		{
			try
			{
				if (parent?.Name == name)
					return parent;

				if (parent?.Controls != null)
				{
					foreach (Control child in parent.Controls)
					{
						var found = FindControlByNameRecursive(child, name);
						if (found != null)
							return found;
					}
				}
			}
			catch (Exception ex)
			{
				// Log but don't throw - continue searching
				System.Diagnostics.Debug.WriteLine($"Error searching in control {parent?.Name}: {ex.Message}");
			}

			return null;
		}

		private void ShowWarningMessage(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		#endregion
	}
}