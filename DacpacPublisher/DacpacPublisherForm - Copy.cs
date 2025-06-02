using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DacpacPublisher.Application_Constants;
using DacpacPublisher.Data_Models;
using DacpacPublisher.Helper;
using DacpacPublisher.Services;
using DacpacPublisher.Interfaces;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;


namespace DacpacPublisher
{

	public partial class DacpacPublisherForm : Form
	{
		// Services
		private readonly ILogService _logService;
		private readonly IConnectionService _connectionService;
		private readonly IDeploymentService _deploymentService;
		private readonly IConfigurationService _configurationService;
		private readonly IBackupService _backupService;
		private Button btnSmartProcedures;
		private Label lblSmartProcedureStatus;
		private CheckBox chkUseSmartProcedures;

		// Private fields for multiple database support
		private string _secondDacpacPath = string.Empty;
		private string _secondDatabase = string.Empty;

		// Configuration
		private PublisherConfiguration _currentConfig;

		// Job script validation timer
		private System.Windows.Forms.Timer _jobScriptValidationTimer;

		public DacpacPublisherForm()
		{
			InitializeComponent();
			InitializeCustomHeader();

			// CORRECT service initialization (NO Services.Services prefix!)
			_logService = new LogService();
			_connectionService = new ConnectionService(_logService);
			_deploymentService = new DeploymentService(_connectionService, _logService);
			_configurationService = new ConfigurationService(_logService);
			_backupService = new BackupService(_logService);

			// Initialize configuration
			_currentConfig = new PublisherConfiguration();
			_currentConfig.SmartProcedures = new List<SmartStoredProcedureInfo>();
			_currentConfig.StoredProcedures = new List<StoredProcedureInfo>();
			_currentConfig.DeploymentTargets = new List<DatabaseDeploymentTarget>();

			// Subscribe to events
			_logService.MessageLogged += OnLogMessageReceived;
			_deploymentService.ProgressChanged += OnProgressChanged;

			InitializeFileDialogs();

			// Initialize smart procedure features
			InitializeSmartProcedureFeatures();

			_logService.LogInfo("✅ Application started with smart procedure support");
		}

		private void InitializeFileDialogs()
		{
			// Configure the file dialogs that are already declared in the designer
			openDacpacDialog.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			openDacpacDialog.Title = "Select DACPAC File";
			openDacpacDialog.DefaultExt = "dacpac";

			folderBrowserDialog.Description = "Select SQL Agent Job Scripts Folder";

			saveConfigDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
			saveConfigDialog.Title = "Save Configuration";
			saveConfigDialog.DefaultExt = "json";

			openConfigDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
			openConfigDialog.Title = "Open Configuration";
			openConfigDialog.DefaultExt = "json";

			saveLogDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
			saveLogDialog.Title = "Export Log";
			saveLogDialog.DefaultExt = "txt";

			openDacpacDialog2.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			openDacpacDialog2.Title = "Select Second DACPAC File";
			openDacpacDialog2.DefaultExt = "dacpac";
		}

		private void InitializeSmartProcedureControls()
		{
			// Smart procedures checkbox (add this after chkExecuteProcedures)
			chkUseSmartProcedures = new CheckBox
			{
				Text = "Use Smart Procedure Management",
				Location = new Point(163, 55),
				Size = new Size(220, 21),
				Checked = true,
				ForeColor = UIHelper.ISTDarkBlue,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			// Smart procedure configuration button
			btnSmartProcedures = new Button
			{
				Text = "Configure Smart Procedures",
				Location = new Point(665, 95),
				Size = new Size(157, 35),
				BackColor = UIHelper.ISTBlue,
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Enabled = false
			};

			// Smart procedure status label
			lblSmartProcedureStatus = new Label
			{
				Location = new Point(163, 138),
				Size = new Size(450, 20),
				Text = "Smart procedures not configured - click Configure to get started",
				ForeColor = UIHelper.ISTGray,
				Font = new Font("Segoe UI", 8.5F)
			};

			// Add to the stored procedures group
			grpStoredProcedures.Controls.Add(chkUseSmartProcedures);
			grpStoredProcedures.Controls.Add(btnSmartProcedures);
			grpStoredProcedures.Controls.Add(lblSmartProcedureStatus);

			// Wire up events
			chkUseSmartProcedures.CheckedChanged += ChkUseSmartProcedures_CheckedChanged;
			btnSmartProcedures.Click += BtnSmartProcedures_Click;
		}
		private async void BtnSmartProcedures_Click(object sender, EventArgs e)
		{
			try
			{
				using (var smartDialog = new SmartProcedureDialog(_currentConfig))
				{
					if (smartDialog.ShowDialog() == DialogResult.OK)
					{
						_currentConfig = smartDialog.UpdatedConfiguration;
						_currentConfig.UseSmartProcedures = true;

						UpdateSmartProcedureStatus();

						var procedureCount = _currentConfig.SmartProcedures?.Count ?? 0;
						var db1Count = _currentConfig.SmartProcedures?.Count(p => p.ExecuteOnDatabase1) ?? 0;
						var db2Count = _currentConfig.SmartProcedures?.Count(p => p.ExecuteOnDatabase2) ?? 0;

						_logService.LogInfo($"✅ Smart procedures updated: {procedureCount} total, Primary: {db1Count}, Secondary: {db2Count}");

						MessageBox.Show(
							$"🎉 Smart procedures configured!\n\n" +
							$"📊 Total: {procedureCount}\n" +
							$"🗄️ Primary DB: {db1Count}\n" +
							$"🗄️ Secondary DB: {db2Count}",
							"Success",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error opening smart procedure configuration", ex);
				MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ChkUseSmartProcedures_CheckedChanged(object sender, EventArgs e)
		{
			bool useSmartProcedures = chkUseSmartProcedures.Checked;

			// Enable/disable smart procedure button
			btnSmartProcedures.Enabled = useSmartProcedures && chkExecuteProcedures.Checked;

			// Update UI visibility
			if (useSmartProcedures)
			{
				// Hide legacy procedure controls
				listStoredProcedures.Visible = false;
				btnAddProcedure.Visible = false;

				// Show smart procedure controls
				btnSmartProcedures.Visible = true;
				lblSmartProcedureStatus.Visible = true;

				// Update group box text
				grpStoredProcedures.Text = "Smart Stored Procedures";

				// Update tooltip
				toolTip.SetToolTip(grpStoredProcedures,
					"Smart procedures allow you to specify which databases each procedure runs on, " +
					"set execution order, and organize by categories for better deployment control.");
			}
			else
			{
				// Show legacy procedure controls
				listStoredProcedures.Visible = true;
				btnAddProcedure.Visible = true;

				// Hide smart procedure controls
				btnSmartProcedures.Visible = false;
				lblSmartProcedureStatus.Visible = false;

				// Reset group box text
				grpStoredProcedures.Text = "Stored Procedures";
				toolTip.SetToolTip(grpStoredProcedures, null);
			}

			UpdateSmartProcedureStatus();
		}


		// Form Load Event Handler
		private void DacpacPublisherForm_Load(object sender, EventArgs e)
		{
			// Set default values
			txtServerName.Text = "(local)";
			chkWindowsAuth.Checked = true;

			// Add tooltips for better UX
			toolTip.SetToolTip(chkEnableMultipleDatabases, "Deploy the same or different DACPAC files to multiple databases");
			toolTip.SetToolTip(txtJobScriptsFolder, "Folder containing SQL Agent job scripts (.sql files)");

			// Initialize UI state - FIXED: Check if controls exist before calling events
			if (chkWindowsAuth != null)
				chkWindowsAuth_CheckedChanged(chkWindowsAuth, EventArgs.Empty);

			if (chkCreateSynonyms != null)
				chkCreateSynonyms_CheckedChanged(chkCreateSynonyms, EventArgs.Empty);

			if (chkCreateSqlAgentJobs != null)
				chkCreateSqlAgentJobs_CheckedChanged(chkCreateSqlAgentJobs, EventArgs.Empty);

			if (chkExecuteProcedures != null)
				chkExecuteProcedures_CheckedChanged(chkExecuteProcedures, EventArgs.Empty);

			if (chkEnableMultipleDatabases != null)
				chkEnableMultipleDatabases_CheckedChanged(chkEnableMultipleDatabases, EventArgs.Empty);
		}

		// Windows Authentication Checkbox Event Handler
		private void chkWindowsAuth_CheckedChanged(object sender, EventArgs e)
		{
			bool useWindowsAuth = chkWindowsAuth.Checked;

			lblUsername.Enabled = !useWindowsAuth;
			txtUsername.Enabled = !useWindowsAuth;
			lblPassword.Enabled = !useWindowsAuth;
			txtPassword.Enabled = !useWindowsAuth;

			if (useWindowsAuth)
			{
				txtUsername.Text = string.Empty;
				txtPassword.Text = string.Empty;
			}
		}
		private void InitializeSmartProcedureFeatures()
		{
			InitializeSmartProcedureControls();

			// Set default to use smart procedures for new configurations
			if (_currentConfig.SmartProcedures?.Any() != true && _currentConfig.StoredProcedures?.Any() != true)
			{
				_currentConfig.UseSmartProcedures = true;
				if (chkUseSmartProcedures != null)
				{
					chkUseSmartProcedures.Checked = true;
				}
			}

			UpdateSmartProcedureStatus();
		}
		private void UpdateSmartProcedureStatus()
		{
			if (lblSmartProcedureStatus == null) return;

			try
			{
				if (_currentConfig.SmartProcedures?.Any() == true)
				{
					var count = _currentConfig.SmartProcedures.Count;
					lblSmartProcedureStatus.Text = $"✅ {count} smart procedures configured";
					lblSmartProcedureStatus.ForeColor = Color.Green;
				}
				else
				{
					lblSmartProcedureStatus.Text = "⚙️ Click 'Configure Smart Procedures' to get started";
					lblSmartProcedureStatus.ForeColor = Color.Gray;
				}
			}
			catch (Exception ex)
			{
				lblSmartProcedureStatus.Text = $"Error: {ex.Message}";
				lblSmartProcedureStatus.ForeColor = Color.Red;
			}
		}
		// Test Connection Button Event Handler
		private async void btnTestConnection_Click(object sender, EventArgs e)
		{
			try
			{
				UpdateConfigurationFromUI();
				var connectionInfo = CreateConnectionInfo(_currentConfig);

				toolStripStatusLabel.Text = "Testing connection...";
				toolStripProgressBar.Visible = true;

				bool success = await _connectionService.TestConnectionAsync(connectionInfo);

				if (success)
				{
					MessageBox.Show("Connection successful!", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
					await RefreshDatabasesAsync();
				}
				else
				{
					MessageBox.Show("Connection failed. Check the log for details.", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Connection test error", ex);
				MessageBox.Show($"Connection test failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
			}
		}

		// Refresh Databases Button Event Handler
		private async void btnRefreshDatabases_Click(object sender, EventArgs e)
		{
			await RefreshDatabasesAsync();
		}

		// Browse DACPAC Button Event Handler
		private void btnBrowseDacpac_Click(object sender, EventArgs e)
		{
			if (openDacpacDialog.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath.Text = openDacpacDialog.FileName;
			}
		}

		// Enhanced Browse Job Scripts Button Event Handler
		private async void btnBrowseJobScripts_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				txtJobScriptsFolder.Text = folderBrowserDialog.SelectedPath;

				// Automatically validate the selected folder
				await ValidateJobScriptsFolderAsync();
			}
		}

		// Job Scripts Folder TextBox validation
		private async void txtJobScriptsFolder_TextChanged(object sender, EventArgs e)
		{
			// Add a small delay to avoid validating on every keystroke
			if (_jobScriptValidationTimer != null)
			{
				_jobScriptValidationTimer.Stop();
				_jobScriptValidationTimer.Dispose();
			}

			_jobScriptValidationTimer = new System.Windows.Forms.Timer();
			_jobScriptValidationTimer.Interval = 1000; // 1 second delay
			_jobScriptValidationTimer.Tick += async (s, args) =>
			{
				_jobScriptValidationTimer.Stop();
				_jobScriptValidationTimer.Dispose();
				_jobScriptValidationTimer = null;
				await ValidateJobScriptsFolderAsync();
			};
			_jobScriptValidationTimer.Start();
		}

		/// <summary>
		/// Validates the job scripts folder and updates UI accordingly
		/// </summary>
		private async Task ValidateJobScriptsFolderAsync()
		{
			try
			{
				string folderPath = txtJobScriptsFolder.Text.Trim();

				if (string.IsNullOrEmpty(folderPath))
				{
					UpdateJobScriptValidationUI(null, "No folder specified");
					return;
				}

				if (!Directory.Exists(folderPath))
				{
					UpdateJobScriptValidationUI(null, "Folder does not exist");
					return;
				}

				toolStripStatusLabel.Text = "Validating job scripts...";

				// Use the validation method from deployment service
				var validationResult = await _deploymentService.ValidateJobScriptsAsync(folderPath);

				UpdateJobScriptValidationUI(validationResult, null);
			}
			catch (Exception ex)
			{
				_logService.LogError("Error validating job scripts folder", ex);
				UpdateJobScriptValidationUI(null, $"Validation error: {ex.Message}");
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
			}
		}

		/// <summary>
		/// Updates the UI based on job script validation results
		/// </summary>
		private void UpdateJobScriptValidationUI(JobScriptValidationResult validationResult, string errorMessage)
		{
			if (validationResult != null && validationResult.IsValid)
			{
				// Update job descriptions label with found jobs
				var jobNames = validationResult.JobScripts.Select(js => js.JobName).ToList();
				var descriptions = GetDefaultJobTemplates()
					.Where(template => jobNames.Any(name => name.Contains(template.JobName)))
					.Select(template => $"- {template.JobName}: {template.Description}")
					.ToList();

				if (descriptions.Any())
				{
					lblJobDescriptions.Text = $"Found {validationResult.JobCount} job script(s):\n" +
											string.Join("\n", descriptions);
					lblJobDescriptions.ForeColor = Color.Green;
				}
				else
				{
					lblJobDescriptions.Text = $"Found {validationResult.JobCount} job script(s):\n" +
											string.Join("\n", jobNames.Select(name => $"- {name}"));
					lblJobDescriptions.ForeColor = Color.Blue;
				}

				// Enable job creation if scripts are valid
				chkCreateSqlAgentJobs.Enabled = true;
			}
			else
			{
				// Show error or no scripts found
				string message = errorMessage ?? validationResult?.ErrorMessage ?? "Unknown error";

				if (validationResult != null && !validationResult.IsValid && validationResult.JobScripts.Any())
				{
					// Some scripts found but with errors
					var errorDetails = validationResult.JobScripts
						.Where(js => !js.IsValid)
						.SelectMany(js => js.ValidationErrors)
						.Take(3) // Limit to first 3 errors
						.ToList();

					lblJobDescriptions.Text = $"Found {validationResult.JobCount} script(s) with errors:\n" +
											string.Join("\n", errorDetails.Select(e => $"- {e}"));
					lblJobDescriptions.ForeColor = Color.Red;
				}
				else
				{
					lblJobDescriptions.Text = message;
					lblJobDescriptions.ForeColor = Color.Red;
				}

				// Keep checkbox enabled but warn about issues
				chkCreateSqlAgentJobs.Enabled = true;
			}
		}

		/// <summary>
		/// Gets default job templates for validation and display
		/// </summary>
		private List<JobScriptTemplate> GetDefaultJobTemplates()
		{
			return new List<JobScriptTemplate>
			{
				new JobScriptTemplate
				{
					JobName = "HiveCFMAutoArchiveSurveys",
					Description = "Archives surveys according to expiration date"
				},
				new JobScriptTemplate
				{
					JobName = "HiveCFMAutoPublishSurveys",
					Description = "Publishes surveys according to published date"
				},
				new JobScriptTemplate
				{
					JobName = "HiveCFMStopStartQuarantineMode",
					Description = "Manages channel quarantine mode"
				},
				new JobScriptTemplate
				{
					JobName = "HiveCFMSendFollowup",
					Description = "Processes followup queue every 30 minutes"
				},
				new JobScriptTemplate
				{
					JobName = "HiveCFMDeleteSurveysPermanently",
					Description = "Permanently deletes soft-deleted surveys"
				}
			};
		}

		// Create Synonyms Checkbox Event Handler
		private void chkCreateSynonyms_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkCreateSynonyms.Checked;
			lblSynonymSourceDb.Enabled = enabled;
			txtSynonymSourceDb.Enabled = enabled;

			if (!enabled)
			{
				txtSynonymSourceDb.Text = string.Empty;
			}
		}

		// Create SQL Agent Jobs Checkbox Event Handler
		private void chkCreateSqlAgentJobs_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkCreateSqlAgentJobs.Checked;
			lblJobOwnerLoginName.Enabled = enabled;
			txtJobOwnerLoginName.Enabled = enabled;
			lblJobDescriptions.Enabled = enabled;

			if (!enabled)
			{
				txtJobOwnerLoginName.Text = string.Empty;
			}
		}

		// Execute Procedures Checkbox Event Handler
		private void chkExecuteProcedures_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkExecuteProcedures.Checked;

			// Enable/disable controls based on smart procedure mode
			if (chkUseSmartProcedures?.Checked == true)
			{
				btnSmartProcedures.Enabled = enabled;
				lblSmartProcedureStatus.Enabled = enabled;
			}
			else
			{
				// Legacy mode
				listStoredProcedures.Enabled = enabled;
				btnAddProcedure.Enabled = enabled;
			}

			if (!enabled)
			{
				if (chkUseSmartProcedures?.Checked != true)
				{
					listStoredProcedures.Items.Clear();
				}
			}

			UpdateSmartProcedureStatus();
		}
		// Multiple Databases Checkbox Event Handler
		private void chkEnableMultipleDatabases_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkEnableMultipleDatabases.Checked;

			// Enable/disable second database controls
			lblDatabase2.Enabled = enabled;
			cboDatabases2.Enabled = enabled;
			btnRefreshDatabases2.Enabled = enabled;

			// Enable/disable second DACPAC controls
			lblDacpacPath2.Enabled = enabled;
			txtDacpacPath2.Enabled = enabled;
			btnBrowseDacpac2.Enabled = enabled;

			if (enabled)
			{
				InitializeProcedureManagementUI();

				// Update the stored procedures group title to show it affects strategy
				grpStoredProcedures.Text = "Stored Procedures (Smart Strategy)";

				// Add tooltip explaining the strategy
				toolTip.SetToolTip(grpStoredProcedures,
					"Procedures will be deployed based on the selected strategy:\n" +
					"• All Procedures: Deploy to primary database\n" +
					"• No Procedures: Skip procedures for secondary\n" +
					"• Minimal Setup: Deploy only setup procedures");
			}
			else
			{
				// Hide procedure strategy controls
				if (lblProcedureStrategy != null && cboProcedureStrategy != null)
				{
					lblProcedureStrategy.Visible = false;
					cboProcedureStrategy.Visible = false;
				}

				// Reset the stored procedures group title
				grpStoredProcedures.Text = "Stored Procedures";
				toolTip.SetToolTip(grpStoredProcedures, null);
			}
		}

		// Event handler for second database refresh
		private async void btnRefreshDatabases2_Click(object sender, EventArgs e)
		{
			await RefreshDatabases2Async();
		}

		// Event handler for second DACPAC browse
		private void btnBrowseDacpac2_Click(object sender, EventArgs e)
		{
			if (openDacpacDialog2.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath2.Text = openDacpacDialog2.FileName;
				_secondDacpacPath = openDacpacDialog2.FileName;
			}
		}

		// Method to refresh second database dropdown
		private async Task RefreshDatabases2Async()
		{
			try
			{
				UpdateConfigurationFromUI();
				var connectionInfo = CreateConnectionInfo(_currentConfig);

				toolStripStatusLabel.Text = "Refreshing databases for target 2...";
				toolStripProgressBar.Visible = true;

				var databases = await _connectionService.GetDatabasesAsync(connectionInfo);

				string currentSelection = cboDatabases2.SelectedItem?.ToString();
				cboDatabases2.Items.Clear();

				foreach (string db in databases)
				{
					cboDatabases2.Items.Add(db);
				}

				if (!string.IsNullOrEmpty(currentSelection) && cboDatabases2.Items.Contains(currentSelection))
				{
					cboDatabases2.SelectedItem = currentSelection;
				}
				else if (cboDatabases2.Items.Count > 0)
				{
					cboDatabases2.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh databases for target 2", ex);
				MessageBox.Show($"Failed to refresh databases: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
			}
		}

		// Add Procedure Button Event Handler
		private void btnAddProcedure_Click(object sender, EventArgs e)
		{
			using (var inputDialog = new InputDialog("Add Stored Procedure", "Enter stored procedure name:", ""))
			{
				if (inputDialog.ShowDialog(this) == DialogResult.OK)
				{
					var procedureName = inputDialog.InputValue;
					if (!string.IsNullOrWhiteSpace(procedureName))
					{
						listStoredProcedures.Items.Add(procedureName);
					}
				}
			}
		}

		// Context Menu Event Handlers
		private void addToolStripMenuItem_Click(object sender, EventArgs e)
		{
			btnAddProcedure_Click(sender, e);
		}

		private void editToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (listStoredProcedures.SelectedItem != null)
			{
				var currentName = listStoredProcedures.SelectedItem.ToString();
				using (var inputDialog = new InputDialog("Edit Stored Procedure", "Edit stored procedure name:", currentName))
				{
					if (inputDialog.ShowDialog(this) == DialogResult.OK)
					{
						var newName = inputDialog.InputValue;
						if (!string.IsNullOrWhiteSpace(newName))
						{
							int selectedIndex = listStoredProcedures.SelectedIndex;
							listStoredProcedures.Items[selectedIndex] = newName;
						}
					}
				}
			}
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (listStoredProcedures.SelectedItem != null)
			{
				listStoredProcedures.Items.RemoveAt(listStoredProcedures.SelectedIndex);
			}
		}

		// Configuration Management Event Handlers
		private async void btnSaveConfig_Click(object sender, EventArgs e)
		{
			if (saveConfigDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					UpdateConfigurationFromUISafely();
					await _configurationService.SaveConfigurationAsync(_currentConfig, saveConfigDialog.FileName);
					MessageBox.Show("Configuration saved successfully!", "Save Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					_logService.LogError("Failed to save configuration", ex);
					MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private async void btnLoadConfig_Click(object sender, EventArgs e)
		{
			if (openConfigDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// FIXED: Add await here
					_currentConfig = _configurationService.LoadConfigurationAsync(openConfigDialog.FileName);

					UpdateUIFromConfiguration();
					MessageBox.Show("Configuration loaded successfully!", "Load Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					_logService.LogError("Failed to load configuration", ex);
					MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		// Add this improved validation method to your form
		private async Task<bool> ValidateConfigurationWithDetailedErrors()
		{
			var errors = new List<string>();

			// Check required fields
			if (string.IsNullOrWhiteSpace(txtServerName.Text))
				errors.Add("Server Name is required");

			if (string.IsNullOrWhiteSpace(cboDatabases.Text))
				errors.Add("Target Database must be selected");

			if (string.IsNullOrWhiteSpace(txtDacpacPath.Text))
				errors.Add("DACPAC file path is required");
			else if (!File.Exists(txtDacpacPath.Text.Trim()))
				errors.Add($"DACPAC file not found: {txtDacpacPath.Text}");

			// Check authentication
			if (!chkWindowsAuth.Checked)
			{
				if (string.IsNullOrWhiteSpace(txtUsername.Text))
					errors.Add("Username is required for SQL authentication");
				if (string.IsNullOrWhiteSpace(txtPassword.Text))
					errors.Add("Password is required for SQL authentication");
			}

			// Check SQL Agent Jobs
			if (chkCreateSqlAgentJobs.Checked)
			{
				if (string.IsNullOrWhiteSpace(txtJobOwnerLoginName.Text))
					errors.Add("Job Owner Login Name is required when creating SQL Agent Jobs");

				if (string.IsNullOrWhiteSpace(txtJobScriptsFolder.Text))
					errors.Add("Job Scripts Folder is required when creating SQL Agent Jobs");
				else if (!Directory.Exists(txtJobScriptsFolder.Text.Trim()))
					errors.Add($"Job Scripts Folder not found: {txtJobScriptsFolder.Text}");
			}

			// Check Multiple Databases
			if (chkEnableMultipleDatabases.Checked)
			{
				if (string.IsNullOrWhiteSpace(cboDatabases2.Text))
					errors.Add("Second Database must be selected when using Multiple Databases");

				if (string.IsNullOrWhiteSpace(txtDacpacPath2.Text))
					errors.Add("Second DACPAC file is required when using Multiple Databases");
				else if (!File.Exists(txtDacpacPath2.Text.Trim()))
					errors.Add($"Second DACPAC file not found: {txtDacpacPath2.Text}");

				if (cboDatabases.Text?.Trim().Equals(cboDatabases2.Text?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
					errors.Add("Target databases must be different");
			}

			// Check Synonyms
			if (chkCreateSynonyms.Checked && string.IsNullOrWhiteSpace(txtSynonymSourceDb.Text))
				errors.Add("Source Database is required when creating Synonyms");

			// NEW: Check Smart Procedures
			if (chkExecuteProcedures.Checked && chkUseSmartProcedures?.Checked == true)
			{
				if (!_currentConfig.SmartProcedures?.Any() == true)
				{
					errors.Add("No smart procedures configured - click 'Configure Smart Procedures' to add procedures");
				}
				else
				{
					// Validate smart procedure configuration
					var smartErrors = await ValidateSmartProceduresAsync();
					errors.AddRange(smartErrors);
				}
			}

			// Show all errors at once
			if (errors.Any())
			{
				string errorMessage = "Please fix the following issues:\n\n" +
									string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
				MessageBox.Show(errorMessage, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}

		private async Task<List<string>> ValidateSmartProceduresAsync()
		{
			var errors = new List<string>();

			if (_currentConfig.SmartProcedures?.Any() != true)
				return errors;

			try
			{
				// Check for procedures with no database assignment
				var orphanedProcs = _currentConfig.SmartProcedures.Where(p => !p.ExecuteOnDatabase1 && !p.ExecuteOnDatabase2);
				if (orphanedProcs.Any())
				{
					errors.Add($"Procedures with no database assignment: {string.Join(", ", orphanedProcs.Select(p => p.Name))}");
				}

				// Check for duplicate names
				var duplicateNames = _currentConfig.SmartProcedures.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key);
				if (duplicateNames.Any())
				{
					errors.Add($"Duplicate procedure names: {string.Join(", ", duplicateNames)}");
				}

				// Check execution order conflicts
				var orderConflicts = _currentConfig.SmartProcedures
					.GroupBy(p => p.ExecutionOrder)
					.Where(g => g.Count() > 1)
					.Select(g => $"Order {g.Key}: {string.Join(", ", g.Select(p => p.Name))}");

				if (orderConflicts.Any())
				{
					errors.Add($"Execution order conflicts: {string.Join("; ", orderConflicts)}");
				}

				// Validate against database if possible
				if (_currentConfig.ValidateProceduresBeforeDeployment)
				{
					await ValidateProceduresExistInDatabaseAsync(errors);
				}
			}
			catch (Exception ex)
			{
				errors.Add($"Error validating smart procedures: {ex.Message}");
			}

			return errors;
		}
		private async Task ValidateProceduresExistInDatabaseAsync(List<string> errors)
		{
			try
			{
				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var connectionString = _connectionService.BuildConnectionString(connectionInfo);

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();

					var procedureNames = _currentConfig.SmartProcedures.Select(p => p.Name).ToList();
					var existingProcedures = new List<string>();

					using (var command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = @"
                    SELECT ROUTINE_NAME 
                    FROM INFORMATION_SCHEMA.ROUTINES 
                    WHERE ROUTINE_TYPE = 'PROCEDURE'
                    AND ROUTINE_SCHEMA = 'dbo'";

						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								existingProcedures.Add(reader["ROUTINE_NAME"].ToString());
							}
						}
					}

					var missingProcedures = procedureNames.Where(p => !existingProcedures.Contains(p, StringComparer.OrdinalIgnoreCase));
					if (missingProcedures.Any())
					{
						errors.Add($"Procedures not found in database: {string.Join(", ", missingProcedures)}");
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogWarning($"Could not validate procedures against database: {ex.Message}");
			}
		}
		// Improved UpdateConfigurationFromUI with null safety
		private void UpdateConfigurationFromUISafely()
		{
			try
			{
				_currentConfig.ServerName = txtServerName.Text?.Trim() ?? "";
				_currentConfig.WindowsAuth = chkWindowsAuth.Checked;
				_currentConfig.Username = txtUsername.Text?.Trim() ?? "";
				_currentConfig.Password = txtPassword.Text ?? "";
				_currentConfig.Database = cboDatabases.SelectedItem?.ToString()?.Trim() ?? "";
				_currentConfig.DacpacPath = txtDacpacPath.Text?.Trim() ?? "";

				_currentConfig.CreateSynonyms = chkCreateSynonyms.Checked;
				_currentConfig.SynonymSourceDb = chkCreateSynonyms.Checked ? (txtSynonymSourceDb.Text?.Trim() ?? "") : "";

				_currentConfig.CreateSqlAgentJobs = chkCreateSqlAgentJobs.Checked;
				_currentConfig.JobOwnerLoginName = chkCreateSqlAgentJobs.Checked ? (txtJobOwnerLoginName.Text?.Trim() ?? "") : "";
				_currentConfig.JobScriptsFolder = chkCreateSqlAgentJobs.Checked ? (txtJobScriptsFolder.Text?.Trim() ?? "") : "";

				_currentConfig.ExecuteProcedures = chkExecuteProcedures.Checked;
				_currentConfig.EnableMultipleDatabases = chkEnableMultipleDatabases.Checked;

				// NEW: Smart procedure configuration
				_currentConfig.UseSmartProcedures = chkUseSmartProcedures?.Checked ?? false;

				// Handle multiple databases
				_currentConfig.DeploymentTargets.Clear();

				if (_currentConfig.EnableMultipleDatabases && chkEnableMultipleDatabases.Checked)
				{
					// Add first target
					var primaryTarget = new DatabaseDeploymentTarget
					{
						Name = "Primary Database",
						ServerName = _currentConfig.ServerName,
						Database = _currentConfig.Database,
						DacpacPath = _currentConfig.DacpacPath,
						IsEnabled = true,
						ProcedureStrategy = GetSelectedProcedureStrategy()
					};

					// Add smart procedures for primary database
					if (_currentConfig.UseSmartProcedures && _currentConfig.SmartProcedures.Any())
					{
						primaryTarget.SmartProcedures = _currentConfig.SmartProcedures
							.Where(p => p.ExecuteOnDatabase1)
							.ToList();
					}

					_currentConfig.DeploymentTargets.Add(primaryTarget);

					// Add second target if configured
					var secondDb = cboDatabases2.SelectedItem?.ToString()?.Trim() ?? "";
					var secondPath = txtDacpacPath2.Text?.Trim() ?? "";

					if (!string.IsNullOrEmpty(secondDb) && !string.IsNullOrEmpty(secondPath))
					{
						var secondaryTarget = new DatabaseDeploymentTarget
						{
							Name = "Secondary Database",
							ServerName = _currentConfig.ServerName,
							Database = secondDb,
							DacpacPath = secondPath,
							IsEnabled = true,
							ProcedureStrategy = "Secondary" // Avoid conflicts
						};

						// Add smart procedures for secondary database
						if (_currentConfig.UseSmartProcedures && _currentConfig.SmartProcedures.Any())
						{
							secondaryTarget.SmartProcedures = _currentConfig.SmartProcedures
								.Where(p => p.ExecuteOnDatabase2)
								.ToList();
						}

						_currentConfig.DeploymentTargets.Add(secondaryTarget);
					}
				}

				// Update stored procedures (both legacy and smart)
				_currentConfig.StoredProcedures.Clear();

				if (chkExecuteProcedures.Checked)
				{
					if (_currentConfig.UseSmartProcedures && _currentConfig.SmartProcedures?.Any() == true)
					{
						_currentConfig.StoredProcedures = _currentConfig.SmartProcedures
							.Select(sp => new StoredProcedureInfo
							{
								Name = sp.Name,
								Parameters = sp.Parameters,
								ExecutionOrder = sp.ExecutionOrder,
								Description = sp.Description
							})
							.ToList();
					}
					else
					{
						// Legacy mode - get from list box
						foreach (var item in listStoredProcedures.Items.Cast<string>())
						{
							if (!string.IsNullOrWhiteSpace(item))
							{
								_currentConfig.StoredProcedures.Add(new StoredProcedureInfo { Name = item.Trim() });
							}
						}
					}
				}

				_logService.LogInfo("Configuration updated successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error updating configuration", ex);
				throw new InvalidOperationException($"Failed to update configuration: {ex.Message}", ex);
			}
		}
		// Replace your btnPublish_Click method with this safer version
		private async void btnPublish_Click(object sender, EventArgs e)
		{
			try
			{
				// Use the safer configuration update
				UpdateConfigurationFromUISafely();

				// Use the detailed validation
				if (!await ValidateConfigurationWithDetailedErrors())
					return;

				btnPublish.Enabled = false;
				toolStripStatusLabel.Text = "Publishing...";
				toolStripProgressBar.Visible = true;
				toolStripProgressBar.Style = ProgressBarStyle.Marquee;

				if (_currentConfig.EnableMultipleDatabases && _currentConfig.DeploymentTargets.Count > 1)
				{
					await DeployMultipleDatabasesAsync();
				}
				else
				{
					await DeploySingleDatabaseAsync();
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Deployment failed", ex);
				MessageBox.Show($"Deployment failed: {ex.Message}\n\nCheck the log for more details.",
					"Deployment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				btnPublish.Enabled = true;
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
				toolStripProgressBar.Style = ProgressBarStyle.Blocks;
			}
		}

		// Log Management Event Handlers
		private void btnClearLog_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
		}

		private void btnExportLog_Click(object sender, EventArgs e)
		{
			if (saveLogDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					File.WriteAllText(saveLogDialog.FileName, txtLog.Text);
					MessageBox.Show("Log exported successfully!", "Export Log",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Failed to export log: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		// Helper Methods
		private async Task RefreshDatabasesAsync()
		{
			try
			{
				UpdateConfigurationFromUI();
				var connectionInfo = CreateConnectionInfo(_currentConfig);

				toolStripStatusLabel.Text = "Refreshing databases...";
				toolStripProgressBar.Visible = true;

				var databases = await _connectionService.GetDatabasesAsync(connectionInfo);

				string currentSelection = cboDatabases.SelectedItem?.ToString();
				cboDatabases.Items.Clear();

				foreach (string db in databases)
				{
					cboDatabases.Items.Add(db);
				}

				if (!string.IsNullOrEmpty(currentSelection) && cboDatabases.Items.Contains(currentSelection))
				{
					cboDatabases.SelectedItem = currentSelection;
				}
				else if (cboDatabases.Items.Count > 0)
				{
					cboDatabases.SelectedIndex = 0;
				}

				// Also refresh second database dropdown if multiple databases are enabled
				if (chkEnableMultipleDatabases.Checked)
				{
					string currentSelection2 = cboDatabases2.SelectedItem?.ToString();
					cboDatabases2.Items.Clear();

					foreach (string db in databases)
					{
						cboDatabases2.Items.Add(db);
					}

					if (!string.IsNullOrEmpty(currentSelection2) && cboDatabases2.Items.Contains(currentSelection2))
					{
						cboDatabases2.SelectedItem = currentSelection2;
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh databases", ex);
				MessageBox.Show($"Failed to refresh databases: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
			}
		}

		private void UpdateConfigurationFromUI()
		{
			_currentConfig.ServerName = txtServerName.Text;
			_currentConfig.WindowsAuth = chkWindowsAuth.Checked;
			_currentConfig.Username = txtUsername.Text;
			_currentConfig.Password = txtPassword.Text;
			_currentConfig.Database = cboDatabases.SelectedItem?.ToString() ?? string.Empty;
			_currentConfig.DacpacPath = txtDacpacPath.Text;
			_currentConfig.CreateSynonyms = chkCreateSynonyms.Checked;
			_currentConfig.SynonymSourceDb = txtSynonymSourceDb.Text;
			_currentConfig.CreateSqlAgentJobs = chkCreateSqlAgentJobs.Checked;
			_currentConfig.JobOwnerLoginName = txtJobOwnerLoginName.Text;
			_currentConfig.JobScriptsFolder = txtJobScriptsFolder.Text;
			_currentConfig.ExecuteProcedures = chkExecuteProcedures.Checked;

			// Add multiple database support
			_currentConfig.EnableMultipleDatabases = chkEnableMultipleDatabases.Checked;

			if (_currentConfig.EnableMultipleDatabases)
			{
				_secondDatabase = cboDatabases2.SelectedItem?.ToString() ?? string.Empty;
				_secondDacpacPath = txtDacpacPath2.Text;

				// Clear and rebuild deployment targets
				_currentConfig.DeploymentTargets.Clear();

				// Add first target
				_currentConfig.DeploymentTargets.Add(new DatabaseDeploymentTarget
				{
					Name = "Target 1",
					ServerName = _currentConfig.ServerName,
					Database = _currentConfig.Database,
					DacpacPath = _currentConfig.DacpacPath,
					IsEnabled = true
				});

				// Add second target if configured
				if (!string.IsNullOrEmpty(_secondDatabase) && !string.IsNullOrEmpty(_secondDacpacPath))
				{
					_currentConfig.DeploymentTargets.Add(new DatabaseDeploymentTarget
					{
						Name = "Target 2",
						ServerName = _currentConfig.ServerName, // Same server for now
						Database = _secondDatabase,
						DacpacPath = _secondDacpacPath,
						IsEnabled = true
					});
				}
			}

			// Update stored procedures list
			_currentConfig.StoredProcedures.Clear();
			foreach (string procedureName in listStoredProcedures.Items)
			{
				_currentConfig.StoredProcedures.Add(new StoredProcedureInfo { Name = procedureName });
			}
		}

		private void UpdateUIFromConfiguration()
		{
			txtServerName.Text = _currentConfig.ServerName;
			chkWindowsAuth.Checked = _currentConfig.WindowsAuth;
			txtUsername.Text = _currentConfig.Username;
			txtPassword.Text = _currentConfig.Password;
			txtDacpacPath.Text = _currentConfig.DacpacPath;
			chkCreateSynonyms.Checked = _currentConfig.CreateSynonyms;
			txtSynonymSourceDb.Text = _currentConfig.SynonymSourceDb;
			chkCreateSqlAgentJobs.Checked = _currentConfig.CreateSqlAgentJobs;
			txtJobOwnerLoginName.Text = _currentConfig.JobOwnerLoginName;
			txtJobScriptsFolder.Text = _currentConfig.JobScriptsFolder;
			chkExecuteProcedures.Checked = _currentConfig.ExecuteProcedures;

			// Handle multiple database configuration
			chkEnableMultipleDatabases.Checked = _currentConfig.EnableMultipleDatabases;

			if (_currentConfig.EnableMultipleDatabases && _currentConfig.DeploymentTargets.Count > 1)
			{
				var secondTarget = _currentConfig.DeploymentTargets[1];
				txtDacpacPath2.Text = secondTarget.DacpacPath;

				// Add the second database to the dropdown if not already there
				if (!cboDatabases2.Items.Contains(secondTarget.Database))
				{
					cboDatabases2.Items.Add(secondTarget.Database);
				}
				cboDatabases2.SelectedItem = secondTarget.Database;
			}

			// NEW: Smart procedure configuration
			if (chkUseSmartProcedures != null)
			{
				chkUseSmartProcedures.Checked = _currentConfig.UseSmartProcedures;
			}

			// Update stored procedures list (legacy mode)
			listStoredProcedures.Items.Clear();

			if (_currentConfig.UseSmartProcedures && _currentConfig.SmartProcedures.Any())
			{
				// Show smart procedures in legacy list for display purposes
				foreach (var procedure in _currentConfig.SmartProcedures.OrderBy(p => p.ExecutionOrder))
				{
					var displayText = $"{procedure.Name}";
					if (chkEnableMultipleDatabases.Checked)
					{
						var targets = new List<string>();
						if (procedure.ExecuteOnDatabase1) targets.Add("DB1");
						if (procedure.ExecuteOnDatabase2) targets.Add("DB2");
						displayText += $" [{string.Join(",", targets)}]";
					}
					listStoredProcedures.Items.Add(displayText);
				}
			}
			else
			{
				// Legacy procedures
				foreach (var procedure in _currentConfig.StoredProcedures)
				{
					listStoredProcedures.Items.Add(procedure.Name ?? procedure.ToString());
				}
			}

			// Update database dropdown if possible
			if (!string.IsNullOrEmpty(_currentConfig.Database))
			{
				if (!cboDatabases.Items.Contains(_currentConfig.Database))
				{
					cboDatabases.Items.Add(_currentConfig.Database);
				}
				cboDatabases.SelectedItem = _currentConfig.Database;
			}

			// Update smart procedure status
			UpdateSmartProcedureStatus();

			// Trigger job script validation if folder is specified
			if (!string.IsNullOrEmpty(_currentConfig.JobScriptsFolder))
			{
				Task.Run(async () => await ValidateJobScriptsFolderAsync());
			}
		}

		private ConnectionInfo CreateConnectionInfo(PublisherConfiguration config)
		{
			return new ConnectionInfo
			{
				ServerName = config.ServerName,
				WindowsAuth = config.WindowsAuth,
				Username = config.Username,
				Password = config.Password,
				Database = config.Database
			};
		}

		private void OnLogMessageReceived(string message)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string>(OnLogMessageReceived), message);
				return;
			}

			txtLog.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
			txtLog.ScrollToCaret();
		}

		private void OnProgressChanged(int progress)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<int>(OnProgressChanged), progress);
				return;
			}

			if (toolStripProgressBar.Style == ProgressBarStyle.Blocks)
			{
				toolStripProgressBar.Value = Math.Min(progress, 100);
			}
		}

		private async Task DeployMultipleDatabasesAsync()
		{
			try
			{
				_logService.LogInfo("=== Starting Smart Multiple Database Deployment ===");

				// Prepare deployment configuration
				PrepareMultiDatabaseConfiguration();

				// Create smart deployment service
				var multiDbService = new Services.SmartMultipleDatabaseDeployment(_deploymentService, _connectionService, _logService);

				// Execute deployment
				var result = await multiDbService.DeployToMultipleDatabasesAsync(_currentConfig);

				// Show results
				ShowMultiDatabaseResults(result);
			}
			catch (Exception ex)
			{
				_logService.LogError("Multiple database deployment failed", ex);
				MessageBox.Show($"❌ Multiple database deployment failed:\n\n{ex.Message}\n\nCheck the log for details.",
					"Deployment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void PrepareMultiDatabaseConfiguration()
		{
			// Clear existing targets
			_currentConfig.DeploymentTargets.Clear();

			// Primary database target
			var primaryTarget = new DatabaseDeploymentTarget
			{
				Name = "Primary Database",
				ServerName = _currentConfig.ServerName,
				Database = _currentConfig.Database,
				DacpacPath = _currentConfig.DacpacPath,
				IsEnabled = true,
				ProcedureStrategy = GetSelectedProcedureStrategy()
			};

			_currentConfig.DeploymentTargets.Add(primaryTarget);

			// Secondary database target (if enabled)
			if (chkEnableMultipleDatabases.Checked &&
				!string.IsNullOrEmpty(cboDatabases2.Text) &&
				!string.IsNullOrEmpty(txtDacpacPath2.Text))
			{
				var secondaryTarget = new DatabaseDeploymentTarget
				{
					Name = "Secondary Database",
					ServerName = _currentConfig.ServerName,
					Database = cboDatabases2.Text,
					DacpacPath = txtDacpacPath2.Text,
					IsEnabled = true,
					ProcedureStrategy = "None" // No procedures for secondary to avoid conflicts
				};

				_currentConfig.DeploymentTargets.Add(secondaryTarget);
			}
		}

		private string GetSelectedProcedureStrategy()
		{
			if (cboProcedureStrategy?.SelectedItem != null)
			{
				var selected = cboProcedureStrategy.SelectedItem.ToString();
				switch (selected)
				{
					case "All Procedures": return "All";
					case "No Procedures": return "None";
					case "Minimal Setup": return "Minimal";
					default: return "All";
				}
			}
			return "All";
		}

		private void ShowMultiDatabaseResults(DeploymentResult.MultiDatabaseDeploymentResult result)
		{
			var message = new StringBuilder();
			message.AppendLine("🎯 **Multiple Database Deployment Results**\n");
			message.AppendLine($"📊 **Summary:** {result.SummaryMessage}");
			message.AppendLine($"⏱️ **Total Duration:** {result.TotalDuration.TotalMinutes:F1} minutes\n");

			message.AppendLine("📋 **Individual Results:**");
			foreach (var targetResult in result.Results)
			{
				var icon = targetResult.Success ? "✅" : "❌";
				message.AppendLine($"{icon} **{targetResult.TargetName}** ({targetResult.DatabaseName})");
				message.AppendLine($"   Duration: {targetResult.Duration.TotalSeconds:F1}s");

				if (!targetResult.Success)
				{
					message.AppendLine($"   Error: {targetResult.ErrorMessage}");
				}
				message.AppendLine();
			}

			if (result.OverallSuccess)
			{
				message.AppendLine("🎉 **All deployments completed successfully!**");
				MessageBox.Show(message.ToString(), "Deployment Successful",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				message.AppendLine("⚠️ **Some deployments failed. Check the log for details.**");
				MessageBox.Show(message.ToString(), "Deployment Partially Failed",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void InitializeProcedureManagementUI()
		{
			// Only initialize if controls exist (they should be created in designer now)
			if (cboProcedureStrategy == null || lblProcedureStrategy == null)
			{
				_logService.LogWarning("Procedure strategy controls not found in designer");
				return;
			}

			// Show the procedure strategy controls
			lblProcedureStrategy.Visible = true;
			cboProcedureStrategy.Visible = true;

			// Ensure they have the right items
			if (cboProcedureStrategy.Items.Count == 0)
			{
				cboProcedureStrategy.Items.AddRange(new[] {
					"All Procedures",
					"No Procedures",
					"Minimal Setup"
				});
				cboProcedureStrategy.SelectedIndex = 0; // Default to "All Procedures"
			}

			_logService.LogInfo("Procedure management UI initialized");
		}

		private async Task DeploySingleDatabaseAsync()
		{
			if (!await ValidateConfigurationWithDetailedErrors())
			{
				return;
			}

			btnPublish.Enabled = false;
			toolStripStatusLabel.Text = "Publishing...";
			toolStripProgressBar.Visible = true;
			toolStripProgressBar.Style = ProgressBarStyle.Marquee;

			try
			{
				// Create backup if enabled - FIXED VERSION
				string backupPath = null;
				if (_currentConfig.CreateBackupBeforeDeployment)
				{
					// Ask user for backup location
					using (var saveBackupDialog = new SaveFileDialog())
					{
						saveBackupDialog.Title = "Save Database Backup";
						saveBackupDialog.Filter = "SQL Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
						saveBackupDialog.DefaultExt = "bak";

						string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
						saveBackupDialog.FileName = $"{_currentConfig.Database}_Backup_{timestamp}.bak";

						if (saveBackupDialog.ShowDialog() == DialogResult.OK)
						{
							var connectionInfo = CreateConnectionInfo(_currentConfig);
							backupPath = await _backupService.CreateBackupAsync(connectionInfo, saveBackupDialog.FileName);
							_logService.LogInfo($"Backup created: {backupPath}");
						}
						else
						{
							// User cancelled backup
							var resultb = MessageBox.Show("No backup location selected. Continue without backup?",
								"Backup Cancelled", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
							if (resultb != DialogResult.Yes)
							{
								return; // Cancel deployment
							}
						}
					}
				}

				// Continue with deployment
				var result = await _deploymentService.DeployDacpacAsync(_currentConfig);

				// Record history
				var historyEntry = new DeploymentHistory
				{
					Timestamp = DateTime.Now,
					ServerName = _currentConfig.ServerName,
					Database = _currentConfig.Database,
					DacpacFile = _currentConfig.DacpacPath,
					Success = result.Success,
					BackupPath = backupPath,
					Duration = result.Duration,
					ErrorMessage = result.Success ? string.Empty : result.Message
				};

				_currentConfig.History.Add(historyEntry);

				if (result.Success)
				{
					string message = "Deployment completed successfully!";
					if (!string.IsNullOrEmpty(backupPath))
					{
						message += $"\n\nBackup saved to: {backupPath}";
					}
					MessageBox.Show(message, "Deployment", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					MessageBox.Show($"Deployment failed: {result.Message}", "Deployment Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Deployment failed", ex);
				MessageBox.Show($"Deployment failed: {ex.Message}", "Deployment Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				btnPublish.Enabled = true;
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
				toolStripProgressBar.Style = ProgressBarStyle.Blocks;
			}
		}
	}

}