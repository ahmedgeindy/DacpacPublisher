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
using System.Data;
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
		private readonly IDataAnalysisService _dataAnalysisService;

		// Configuration
		private PublisherConfiguration _currentConfig;

		// Job script validation timer
		private System.Windows.Forms.Timer _jobScriptValidationTimer;

		private ListBox lstSynonymTargetDatabases;
		private Button btnRefreshSynonymTargets;

		private CheckBox chkShowSynonymTargets;
		private CheckedListBox clbSynonymTargets;
		private Label lblSynonymTargets;
		private Button btnAutoDetectTargets;
		private Label lblSynonymSourceInfo;
		// Private fields for multiple database support
		private string _secondDacpacPath = string.Empty;
		private string _secondDatabase = string.Empty;

		public DacpacPublisherForm()
		{
			InitializeComponent();
			InitializeCustomHeader();

			try
			{
				// Initialize core services
				_logService = new LogService();
				_connectionService = new ConnectionService(_logService);
				_deploymentService = new DeploymentService(_connectionService, _logService);
				_configurationService = new ConfigurationService(_logService);
				_backupService = new BackupService(_logService);

				// Initialize data analysis service SAFELY
				try
				{
					_dataAnalysisService = new DataAnalysisService(_connectionService, _logService);
					_logService.LogInfo("✅ Data analysis service initialized");
				}
				catch (Exception ex)
				{
					_logService.LogWarning($"⚠️ Data analysis service initialization failed: {ex.Message}");
					_dataAnalysisService = null; // Set to null so we know it's not available
				}

				// Initialize configuration with null safety
				_currentConfig = new PublisherConfiguration();
				_currentConfig.SmartProcedures = new List<SmartStoredProcedureInfo>();
				_currentConfig.StoredProcedures = new List<StoredProcedureInfo>();
				_currentConfig.DeploymentTargets = new List<DatabaseDeploymentTarget>();
				_currentConfig.History = new List<DeploymentHistory>(); // ADD THIS

				// Subscribe to events
				_logService.MessageLogged += OnLogMessageReceived;
				_deploymentService.ProgressChanged += OnProgressChanged;

				InitializeFileDialogs();
				InitializeTooltips();
				InitializeDataViewerTab(); // ADD THIS LINE
				_logService.LogInfo("✅ DACPAC Publisher started successfully");
			}
			catch (Exception ex)
			{
				// Fallback error handling
				MessageBox.Show($"❌ Error initializing application: {ex.Message}", "Initialization Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void InitializeFileDialogs()
		{
			// File dialogs are already configured in the designer
			toolTip.SetToolTip(txtJobScriptsFolder, "Folder containing SQL Agent job scripts (.sql files)");
			toolTip.SetToolTip(btnConfigureSmartProcedures, "Configure procedures for smart deployment across multiple databases");
			toolTip.SetToolTip(chkCreateBackup, "Create a backup before deployment for safety");
		}

		private void InitializeTooltips()
		{
			toolTip.SetToolTip(grpSmartProcedures,
				"Smart procedures allow you to specify which databases each procedure runs on, " +
				"set execution order, and organize by categories for better deployment control.");

			toolTip.SetToolTip(btnTestConnection, "Test the database connection with current settings");
			toolTip.SetToolTip(btnRefreshDatabases, "Refresh the list of available databases");
			toolTip.SetToolTip(btnPublish, "Start the deployment process");
		}

		private void InitializeSynonymControls()
		{
			try
			{
				// Find the synonyms group box
				var synonymGroup = this.Controls.Find("grpSynonyms", true).FirstOrDefault() as GroupBox;
				if (synonymGroup == null) return;

				// Increase the height of the synonyms group box
				synonymGroup.Height = 280; // Increased from default

				// Find existing controls
				var chkCreateSynonyms = synonymGroup.Controls.Find("chkCreateSynonyms", true).FirstOrDefault() as CheckBox;
				var txtSynonymSourceDb = synonymGroup.Controls.Find("txtSynonymSourceDb", true).FirstOrDefault() as TextBox;
				var lblSynonymSourceDb = synonymGroup.Controls.Find("lblSynonymSourceDb", true).FirstOrDefault() as Label;

				// Reposition existing controls
				int yPosition = 25;

				if (chkCreateSynonyms != null)
				{
					chkCreateSynonyms.Location = new Point(15, yPosition);
					yPosition += 30;
				}

				// Hide the source database textbox and label (we'll auto-detect)
				if (lblSynonymSourceDb != null) lblSynonymSourceDb.Visible = false;
				if (txtSynonymSourceDb != null) txtSynonymSourceDb.Visible = false;

				// Create info label about automatic detection
				lblSynonymSourceInfo = new Label
				{
					Name = "lblSynonymSourceInfo",
					Text = "🤖 Source database will be auto-detected (HiveCFMSurvey pattern)",
					AutoSize = true,
					Location = new Point(15, yPosition),
					Size = new Size(400, 20),
					ForeColor = Color.DarkGreen,
					Font = new Font(this.Font.FontFamily, 8, FontStyle.Italic)
				};
				synonymGroup.Controls.Add(lblSynonymSourceInfo);
				yPosition += 25;

				// Create show targets checkbox
				chkShowSynonymTargets = new CheckBox
				{
					Name = "chkShowSynonymTargets",
					Text = "📋 Configure target databases (auto-selects HiveCFMApp databases)",
					AutoSize = true,
					Location = new Point(15, yPosition),
					Size = new Size(400, 20),
					Checked = false
				};
				synonymGroup.Controls.Add(chkShowSynonymTargets);
				yPosition += 30;

				// Create target selection label
				lblSynonymTargets = new Label
				{
					Name = "lblSynonymTargets",
					Text = "🎯 Select target databases (where synonyms will be created):",
					AutoSize = true,
					Location = new Point(15, yPosition),
					Size = new Size(400, 20),
					ForeColor = Color.DarkBlue,
					Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold),
					Visible = false
				};
				synonymGroup.Controls.Add(lblSynonymTargets);
				yPosition += 25;

				// Create target databases checklist
				clbSynonymTargets = new CheckedListBox
				{
					Name = "clbSynonymTargets",
					Location = new Point(15, yPosition),
					Size = new Size(400, 120),
					CheckOnClick = true,
					BackColor = Color.White,
					BorderStyle = BorderStyle.FixedSingle,
					Visible = false,
					ScrollAlwaysVisible = true,
					IntegralHeight = false
				};
				synonymGroup.Controls.Add(clbSynonymTargets);

				// Create auto-detect button (positioned to the right of the checklist)
				btnAutoDetectTargets = new Button
				{
					Name = "btnAutoDetectTargets",
					Text = "🔄 Auto Detect",
					Location = new Point(425, yPosition),
					Size = new Size(100, 30),
					BackColor = Color.LightBlue,
					FlatStyle = FlatStyle.Flat,
					Visible = false,
					Font = new Font(this.Font.FontFamily, 8, FontStyle.Regular)
				};
				synonymGroup.Controls.Add(btnAutoDetectTargets);

				// Add event handlers
				chkShowSynonymTargets.CheckedChanged += ChkShowSynonymTargets_CheckedChanged;
				btnAutoDetectTargets.Click += BtnAutoDetectTargets_Click;

				// Wire up the create synonyms checkbox to show/hide everything
				if (chkCreateSynonyms != null)
				{
					chkCreateSynonyms.CheckedChanged += (s, e) => {
						bool isChecked = chkCreateSynonyms.Checked;
						lblSynonymSourceInfo.Visible = isChecked;
						chkShowSynonymTargets.Visible = isChecked;

						// Hide the target selection if unchecked
						if (!isChecked)
						{
							chkShowSynonymTargets.Checked = false;
						}
					};
				}

				// Add tooltips
				if (toolTip != null)
				{
					toolTip.SetToolTip(lblSynonymSourceInfo,
						"The system will automatically find databases containing 'HiveCFMSurvey' as the source.\n" +
						"These databases contain the actual CFMSurveyUser table.");

					toolTip.SetToolTip(clbSynonymTargets,
						"Select which databases should receive CFMSurveyUser synonyms.\n" +
						"Typically: HiveCFMApp databases get synonyms, HiveCFMSurvey databases don't.\n" +
						"Check/uncheck items to customize your selection.");

					toolTip.SetToolTip(btnAutoDetectTargets,
						"Automatically detect and select HiveCFMApp databases as synonym targets");
				}

				_logService.LogInfo("✅ Synonym controls initialized successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error initializing synonym controls", ex);
				MessageBox.Show($"Error initializing synonym controls: {ex.Message}", "UI Error",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
		private void DacpacPublisherForm_Load(object sender, EventArgs e)
		{
			// Set default values
			txtServerName.Text = "(local)";
			chkWindowsAuth.Checked = true;

			// Initialize UI state
			chkWindowsAuth_CheckedChanged(chkWindowsAuth, EventArgs.Empty);
			chkCreateSynonyms_CheckedChanged(chkCreateSynonyms, EventArgs.Empty);
			chkCreateSqlAgentJobs_CheckedChanged(chkCreateSqlAgentJobs, EventArgs.Empty);
			chkExecuteProcedures_CheckedChanged(chkExecuteProcedures, EventArgs.Empty);

			UpdateSmartProcedureStatus();
			InitializeSynonymControls();

			if (cboDatabases != null)
			{
				cboDatabases.SelectedIndexChanged += cboDatabases_SelectedIndexChanged;
			}

			var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
			if (cboDatabases2 != null)
			{
				cboDatabases2.SelectedIndexChanged += cboDatabases2_SelectedIndexChanged;
			}
		}

		#region Event Handlers


		private void ChkShowSynonymTargets_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				bool showTargets = chkShowSynonymTargets.Checked;

				// Show/hide the target selection controls
				if (lblSynonymTargets != null) lblSynonymTargets.Visible = showTargets;
				if (clbSynonymTargets != null) clbSynonymTargets.Visible = showTargets;
				if (btnAutoDetectTargets != null) btnAutoDetectTargets.Visible = showTargets;

				if (showTargets)
				{
					// Populate the list when first shown
					if (clbSynonymTargets.Items.Count == 0)
					{
						// Show loading message
						clbSynonymTargets.Items.Clear();
						clbSynonymTargets.Items.Add("Loading databases...");

						// Populate asynchronously
						Task.Run(async () => {
							try
							{
								await PopulateSynonymTargetDatabasesSafe();
							}
							catch (Exception ex)
							{
								_logService.LogError("Error populating databases", ex);
								Invoke(new Action(() => {
									clbSynonymTargets.Items.Clear();
									clbSynonymTargets.Items.Add($"Error: {ex.Message}");
								}));
							}
						});
					}
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in show targets checkbox handler", ex);
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		// Replace the existing chkCreateSynonyms_CheckedChanged method
		private async void chkCreateSynonyms_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkCreateSynonyms.Checked;

			// Show/hide synonym configuration controls
			if (lblSynonymSourceInfo != null) lblSynonymSourceInfo.Visible = enabled;
			if (chkShowSynonymTargets != null) chkShowSynonymTargets.Visible = enabled;

			// Hide advanced controls by default
			if (lblSynonymTargets != null) lblSynonymTargets.Visible = enabled && chkShowSynonymTargets.Checked;
			if (clbSynonymTargets != null) clbSynonymTargets.Visible = enabled && chkShowSynonymTargets.Checked;
			if (btnAutoDetectTargets != null) btnAutoDetectTargets.Visible = enabled && chkShowSynonymTargets.Checked;

			if (enabled)
			{
				_logService.LogInfo("🔗 Synonym creation enabled");

				// Auto-populate and detect if showing targets
				if (chkShowSynonymTargets?.Checked == true)
				{
					await PopulateSynonymTargetDatabases();
				}
			}
			else
			{
				// Clear selections when disabled
				clbSynonymTargets?.Items.Clear();
			}
		}


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
					MessageBox.Show("✅ Connection successful!", "Connection Test",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					await RefreshDatabasesAsync();
				}
				else
				{
					MessageBox.Show("❌ Connection failed. Check the log for details.", "Connection Test",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Connection test error", ex);
				MessageBox.Show($"❌ Connection test failed: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
			}
		}

		private async void btnRefreshDatabases_Click(object sender, EventArgs e)
		{
			await RefreshDatabasesAsync();
		}

		private void btnBrowseDacpac_Click(object sender, EventArgs e)
		{
			if (openDacpacDialog.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath.Text = openDacpacDialog.FileName;
			}
		}

		private async void btnBrowseJobScripts_Click(object sender, EventArgs e)
		{
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				txtJobScriptsFolder.Text = folderBrowserDialog.SelectedPath;
				await ValidateJobScriptsFolderAsync();
			}
		}

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

		private void chkExecuteProcedures_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = chkExecuteProcedures.Checked;
			btnConfigureSmartProcedures.Enabled = enabled;
			lblSmartProcedureStatus.Enabled = enabled;

			UpdateSmartProcedureStatus();
		}

		private async void btnConfigureSmartProcedures_Click(object sender, EventArgs e)
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

		private async void btnSaveConfig_Click(object sender, EventArgs e)
		{
			if (saveConfigDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					UpdateConfigurationFromUI();
					await _configurationService.SaveConfigurationAsync(_currentConfig, saveConfigDialog.FileName);
					MessageBox.Show("💾 Configuration saved successfully!", "Save Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					_logService.LogError("Failed to save configuration", ex);
					MessageBox.Show($"❌ Failed to save configuration: {ex.Message}", "Error",
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
					// FIXED: Remove await since LoadConfigurationAsync is not actually async
					_currentConfig = await _configurationService.LoadConfigurationAsync(openConfigDialog.FileName);
					UpdateUIFromConfiguration();
					MessageBox.Show("📂 Configuration loaded successfully!", "Load Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					_logService.LogError("Failed to load configuration", ex);
					MessageBox.Show($"❌ Failed to load configuration: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		private async void btnPublish_Click(object sender, EventArgs e)
		{
			try
			{
				UpdateConfigurationFromUI();

				if (!await ValidateConfigurationAsync())
					return;

				btnPublish.Enabled = false;
				toolStripStatusLabel.Text = "Publishing...";
				toolStripProgressBar.Visible = true;
				toolStripProgressBar.Style = ProgressBarStyle.Marquee;

				var result = await DeployDatabaseAsync();

				// ENHANCED: Use the smart categorization
				var category = DeploymentConfiguration.CategorizeResult(result.Errors, result.Warnings);

				if (category == DeploymentResultCategory.Success ||
					category == DeploymentResultCategory.SuccessWithWarnings ||
					category == DeploymentResultCategory.SuccessWithIgnorableErrors)
				{
					string successMessage = BuildSuccessMessage(result);

					// Show appropriate icon based on category
					var icon = category == DeploymentResultCategory.Success ?
							  MessageBoxIcon.Information : MessageBoxIcon.Information;
					var title = category == DeploymentResultCategory.SuccessWithIgnorableErrors ?
							   "🚀 Deployment Successful (Auto-Generated Procedures Handled)" :
							   "🚀 Deployment Successful";

					MessageBox.Show(successMessage, title, MessageBoxButtons.OK, icon);

					_logService.LogInfo($"✅ {result.GetSummary()}");

					// Offer data viewer for successful deployments
					if (category != DeploymentResultCategory.CriticalFailure)
					{
						await SafeHandlePostDeploymentSuccess(result);
					}
				}
				else
				{
					string failureMessage = BuildFailureMessage(result);

					// Determine icon based on actual severity
					var icon = category == DeploymentResultCategory.CriticalFailure ?
							  MessageBoxIcon.Error : MessageBoxIcon.Warning;
					var title = category == DeploymentResultCategory.CriticalFailure ?
							   "💥 Deployment Failed" : "⚠️ Deployment Issues";

					MessageBox.Show(failureMessage, title, MessageBoxButtons.OK, icon);

					_logService.LogError($"❌ {result.GetSummary()}");
				}

				// Show deployment analytics in log
				LogDeploymentAnalytics(result);
			}
			catch (Exception ex)
			{
				_logService.LogError("Deployment failed with unhandled exception", ex);
				MessageBox.Show(
					$"💥 Unexpected deployment failure:\n\n{ex.Message}\n\n" +
					"This may indicate a configuration or system issue. Check the log for details.",
					"Deployment Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				btnPublish.Enabled = true;
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
				toolStripProgressBar.Style = ProgressBarStyle.Blocks;
			}
		}
		// NEW: Log deployment analytics for troubleshooting
		private void LogDeploymentAnalytics(DeploymentResult result)
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

				// Deployment recommendations
				if (result.HasCriticalErrors)
				{
					_logService.LogInfo("🔧 TROUBLESHOOTING RECOMMENDATIONS:");
					_logService.LogInfo("  • Check database permissions and connectivity");
					_logService.LogInfo("  • Verify all required stored procedures exist");
					_logService.LogInfo("  • Ensure DACPAC file is not corrupted");
					_logService.LogInfo("  • Review SQL Server error logs");
				}
				else if (result.HasWarnings)
				{
					_logService.LogInfo("💡 OPTIMIZATION SUGGESTIONS:");
					_logService.LogInfo("  • Review warnings for potential improvements");
					_logService.LogInfo("  • Consider updating DACPAC deployment options");
					_logService.LogInfo("  • Validate synonym source database accessibility");
				}

				_logService.LogInfo("📊 === End Analytics ===");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to log deployment analytics", ex);
			}
		}
		private void btnClearLog_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
			_logService.LogInfo("Log cleared by user");
		}

		private void btnExportLog_Click(object sender, EventArgs e)
		{
			if (saveLogDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					File.WriteAllText(saveLogDialog.FileName, txtLog.Text);
					MessageBox.Show("📤 Log exported successfully!", "Export Log",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show($"❌ Failed to export log: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void cboDatabases_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Update synonym info when database selection changes
			if (chkCreateSynonyms?.Checked == true)
			{
				ShowSynonymAutomaticInfo(true);
			}
		}

		private void cboDatabases2_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Update synonym info when secondary database selection changes
			if (chkCreateSynonyms?.Checked == true)
			{
				ShowSynonymAutomaticInfo(true);
			}
		}
		private async void BtnAutoDetectTargets_Click(object sender, EventArgs e)
		{
			try
			{
				btnAutoDetectTargets.Enabled = false;
				btnAutoDetectTargets.Text = "🔄 Detecting...";

				// Refresh the list
				await PopulateSynonymTargetDatabasesSafe();

				// Auto-check HiveCFMApp databases
				for (int i = 0; i < clbSynonymTargets.Items.Count; i++)
				{
					string item = clbSynonymTargets.Items[i].ToString();

					// Check items that start with ✅ (HiveCFMApp databases)
					bool shouldCheck = item.StartsWith("✅");
					clbSynonymTargets.SetItemChecked(i, shouldCheck);
				}

				MessageBox.Show(
					"✅ Auto-detection complete!\n\n" +
					"HiveCFMApp databases have been selected as targets.\n" +
					"HiveCFMSurvey databases are marked as sources.\n\n" +
					"You can manually adjust the selection if needed.",
					"Auto Detection Complete",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in auto-detect", ex);
				MessageBox.Show($"Error during auto-detection: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				btnAutoDetectTargets.Enabled = true;
				btnAutoDetectTargets.Text = "🔄 Auto Detect";
			}
		}
		#endregion

		#region Helper Methods
		private async Task PopulateSynonymTargetDatabasesSafe()
		{
			try
			{
				// Get databases on background thread
				var databases = await GetDatabasesForSynonymTargets();

				// Update UI on main thread
				Invoke(new Action(() => {
					PopulateSynonymTargetsList(databases);
				}));
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in safe populate", ex);
				throw;
			}
		}

		private async Task<List<string>> GetDatabasesForSynonymTargets()
		{
			var databases = new List<string>();

			try
			{
				// Get current configuration
				var config = new PublisherConfiguration
				{
					ServerName = txtServerName?.Text ?? "(local)",
					WindowsAuth = chkWindowsAuth?.Checked ?? true,
					Username = txtUsername?.Text ?? "",
					Password = txtPassword?.Text ?? "",
					Database = "master"
				};

				var connectionInfo = CreateConnectionInfo(config);

				// Test connection first
				if (await _connectionService.TestConnectionAsync(connectionInfo))
				{
					// Get all databases from server
					var allDatabases = await _connectionService.GetDatabasesAsync(connectionInfo);

					// Filter for CFM-related databases
					// FIX: Use IndexOf instead of Contains for StringComparison
					databases = allDatabases
						.Where(db => db.IndexOf("CFM", StringComparison.OrdinalIgnoreCase) >= 0)
						.OrderBy(db => db)
						.ToList();
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error getting databases", ex);
				// Return empty list on error
			}

			return databases;
		}
		private void PopulateSynonymTargetsList(List<string> databases)
		{
			try
			{
				clbSynonymTargets.Items.Clear();

				if (databases == null || databases.Count == 0)
				{
					clbSynonymTargets.Items.Add("No CFM databases found");
					return;
				}

				// Categorize databases
				var hiveCFMAppDatabases = databases
					.Where(db => db.IndexOf("HiveCFMApp", StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();

				var hiveCFMSurveyDatabases = databases
					.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();

				var otherCFMDatabases = databases
					.Where(db => !hiveCFMAppDatabases.Contains(db) && !hiveCFMSurveyDatabases.Contains(db))
					.ToList();

				// Add categorized databases to the list
				if (hiveCFMAppDatabases.Any())
				{
					foreach (var db in hiveCFMAppDatabases)
					{
						int index = clbSynonymTargets.Items.Add($"✅ {db}");
						clbSynonymTargets.SetItemChecked(index, true); // Auto-check
					}
				}

				if (hiveCFMSurveyDatabases.Any())
				{
					foreach (var db in hiveCFMSurveyDatabases)
					{
						int index = clbSynonymTargets.Items.Add($"📋 {db}");
						clbSynonymTargets.SetItemChecked(index, false); // Don't check
					}
				}

				if (otherCFMDatabases.Any())
				{
					foreach (var db in otherCFMDatabases)
					{
						int index = clbSynonymTargets.Items.Add($"📁 {db}");
						clbSynonymTargets.SetItemChecked(index, false); // Don't check
					}
				}

				_logService.LogInfo($"✅ Populated {databases.Count} databases in synonym targets list");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error populating synonym list", ex);
				clbSynonymTargets.Items.Clear();
				clbSynonymTargets.Items.Add($"Error: {ex.Message}");
			}
		}
		private void UpdateSmartProcedureStatus()
		{
			if (lblSmartProcedureStatus == null) return;

			try
			{
				if (_currentConfig.SmartProcedures?.Any() == true)
				{
					var count = _currentConfig.SmartProcedures.Count;
					var db1Count = _currentConfig.SmartProcedures.Count(p => p.ExecuteOnDatabase1);
					var db2Count = _currentConfig.SmartProcedures.Count(p => p.ExecuteOnDatabase2);

					lblSmartProcedureStatus.Text = $"✅ {count} procedures configured (Primary: {db1Count}, Secondary: {db2Count})";
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
				lblSmartProcedureStatus.Text = $"❌ Error: {ex.Message}";
				lblSmartProcedureStatus.ForeColor = Color.Red;
			}
		}

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

				_logService.LogInfo($"Refreshed {databases.Count} databases");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh databases", ex);
				MessageBox.Show($"❌ Failed to refresh databases: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				toolStripStatusLabel.Text = "Ready";
				toolStripProgressBar.Visible = false;
			}
		}

		// Replace your ValidateJobScriptsFolderAsync method with this safe version:

		private async Task ValidateJobScriptsFolderAsync()
		{
			try
			{
				string folderPath = "";

				// Safely get the folder path from UI thread
				if (InvokeRequired)
				{
					folderPath = (string)Invoke(new Func<string>(() => txtJobScriptsFolder.Text.Trim()));
				}
				else
				{
					folderPath = txtJobScriptsFolder.Text.Trim();
				}

				if (string.IsNullOrEmpty(folderPath))
				{
					UpdateJobValidationUI("No folder specified", Color.Gray);
					return;
				}

				if (!Directory.Exists(folderPath))
				{
					UpdateJobValidationUI("❌ Folder does not exist", Color.Red);
					return;
				}

				UpdateStatusUI("Validating job scripts...");

				var validationResult = await _deploymentService.ValidateJobScriptsAsync(folderPath);

				if (validationResult?.IsValid == true)
				{
					var jobNames = validationResult.JobScripts.Select(js => js.JobName).ToList();
					var message = $"✅ Found {validationResult.JobCount} valid job script(s):\n" +
								 string.Join("\n", jobNames.Select(name => $"• {name}"));

					UpdateJobValidationUI(message, Color.Green);
					EnableSqlAgentJobs(true);
				}
				else
				{
					string message = validationResult?.ErrorMessage ?? "Unknown validation error";
					UpdateJobValidationUI($"⚠️ {message}", Color.Orange);
					EnableSqlAgentJobs(true);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error validating job scripts folder", ex);
				UpdateJobValidationUI($"❌ Validation error: {ex.Message}", Color.Red);
			}
			finally
			{
				UpdateStatusUI("Ready");
			}
		}

		// Helper methods for safe UI updates
		private void UpdateJobValidationUI(string message, Color color)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => UpdateJobValidationUI(message, color)));
				return;
			}

			lblJobDescriptions.Text = message;
			lblJobDescriptions.ForeColor = color;
		}

		private void UpdateStatusUI(string status)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => UpdateStatusUI(status)));
				return;
			}

			toolStripStatusLabel.Text = status;
		}

		private void EnableSqlAgentJobs(bool enabled)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => EnableSqlAgentJobs(enabled)));
				return;
			}

			chkCreateSqlAgentJobs.Enabled = enabled;
		}


		// CORRECTED VERSION: ValidateSmartProceduresAsync method
		private async Task<List<string>> ValidateSmartProceduresAsync()
		{
			var errors = new List<string>();

			try
			{
				if (_currentConfig?.SmartProcedures?.Any() != true)
					return errors;

				// Check for procedures with no database assignment
				var orphanedProcs = _currentConfig.SmartProcedures
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

		private void UpdateConfigurationFromUI()
		{
			try
			{
				// Basic connection settings
				_currentConfig.ServerName = txtServerName?.Text?.Trim() ?? "";
				_currentConfig.WindowsAuth = chkWindowsAuth?.Checked ?? false;
				_currentConfig.Username = txtUsername?.Text?.Trim() ?? "";
				_currentConfig.Password = txtPassword?.Text ?? "";
				_currentConfig.Database = cboDatabases?.SelectedItem?.ToString()?.Trim() ?? "";
				_currentConfig.DacpacPath = txtDacpacPath?.Text?.Trim() ?? "";

				// IMPROVED: Smart Synonym settings with intelligent defaults
				_currentConfig.CreateSynonyms = chkCreateSynonyms?.Checked ?? false;

				if (_currentConfig.CreateSynonyms)
				{
					// Clear previous selections
					_currentConfig.SynonymTargetDatabases = new List<string>();

					// Get selected target databases from the checklist
					if (clbSynonymTargets != null && chkShowSynonymTargets?.Checked == true)
					{
						for (int i = 0; i < clbSynonymTargets.Items.Count; i++)
						{
							if (clbSynonymTargets.GetItemChecked(i))
							{
								string item = clbSynonymTargets.Items[i].ToString();

								// Extract database name (remove prefixes)
								if (item.StartsWith("✅ ") || item.StartsWith("📋 ") || item.StartsWith("📁 "))
								{
									string dbName = item.Substring(2).Trim();
									_currentConfig.SynonymTargetDatabases.Add(dbName);
								}
							}
						}

						_logService?.LogInfo($"🎯 Selected {_currentConfig.SynonymTargetDatabases.Count} target database(s) for synonyms: {string.Join(", ", _currentConfig.SynonymTargetDatabases)}");
					}

					// Set source to auto-detect
					_currentConfig.SynonymSourceDb = "AUTO_DETECT";
				}
				else
				{
					_currentConfig.SynonymTargetDatabases = new List<string>();
				}

				// SQL Agent Jobs settings
				_currentConfig.CreateSqlAgentJobs = chkCreateSqlAgentJobs?.Checked ?? false;
				_currentConfig.JobOwnerLoginName = (_currentConfig.CreateSqlAgentJobs) ? (txtJobOwnerLoginName?.Text?.Trim() ?? "") : "";
				_currentConfig.JobScriptsFolder = (_currentConfig.CreateSqlAgentJobs) ? (txtJobScriptsFolder?.Text?.Trim() ?? "") : "";

				// Smart Stored Procedures settings
				_currentConfig.ExecuteProcedures = chkExecuteProcedures?.Checked ?? false;
				_currentConfig.UseSmartProcedures = true; // Always use smart procedures in this version

				// Backup settings
				_currentConfig.CreateBackupBeforeDeployment = chkCreateBackup?.Checked ?? false;

				// FIXED: Multiple databases settings - Check if controls exist first
				var multiDbCheckbox = FindControlByName("chkEnableMultipleDatabases") as CheckBox;
				_currentConfig.EnableMultipleDatabases = multiDbCheckbox?.Checked ?? false;

				// IMPROVED: Enhanced multiple database configuration with synonym intelligence
				if (_currentConfig.EnableMultipleDatabases)
				{
					var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
					var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;

					if (cboDatabases2?.SelectedItem != null)
					{
						// Initialize DeploymentTargets if null
						if (_currentConfig.DeploymentTargets == null)
							_currentConfig.DeploymentTargets = new List<DatabaseDeploymentTarget>();

						// Update or add secondary database target
						var secondaryTarget = _currentConfig.DeploymentTargets.FirstOrDefault(t => t.Name == "Secondary");
						if (secondaryTarget == null)
						{
							secondaryTarget = new DatabaseDeploymentTarget
							{
								Name = "Secondary",
								ServerName = _currentConfig.ServerName,
								IsEnabled = true
							};
							_currentConfig.DeploymentTargets.Add(secondaryTarget);
						}

						// Update secondary database settings
						secondaryTarget.Database = cboDatabases2.SelectedItem?.ToString()?.Trim() ?? "";
						secondaryTarget.DacpacPath = txtDacpacPath2?.Text?.Trim() ?? _currentConfig.DacpacPath;

						_logService?.LogInfo($"📊 Multiple databases configured: Primary='{_currentConfig.Database}', Secondary='{secondaryTarget.Database}'");
					}
				}
				else
				{
					// Clear deployment targets if multiple databases is disabled
					_currentConfig.DeploymentTargets?.Clear();
				}

				// Update stored procedures from smart procedures
				if (_currentConfig.StoredProcedures == null)
					_currentConfig.StoredProcedures = new List<StoredProcedureInfo>();

				_currentConfig.StoredProcedures.Clear();

				if (_currentConfig.ExecuteProcedures && _currentConfig.SmartProcedures?.Any() == true)
				{
					_currentConfig.StoredProcedures = _currentConfig.SmartProcedures
						.Select(sp => new StoredProcedureInfo
						{
							Name = sp?.Name ?? "",
							Parameters = sp?.Parameters ?? "",
							ExecutionOrder = sp?.ExecutionOrder ?? 0,
							Description = sp?.Description ?? ""
						})
						.ToList();

					_logService?.LogInfo($"📋 Updated {_currentConfig.StoredProcedures.Count} stored procedures from smart configuration");
				}

				// ENHANCED: Validation and logging for synonym configuration
				if (_currentConfig.CreateSynonyms)
				{
					ValidateAndLogSynonymConfiguration();
				}

				_logService?.LogInfo("✅ Configuration updated from UI successfully");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to update configuration from UI", ex);
				throw new InvalidOperationException($"Configuration update failed: {ex.Message}", ex);
			}
		}
		private void ValidateAndLogSynonymConfiguration()
		{
			try
			{
				var synonymConfig = new List<string>();

				synonymConfig.Add($"🎯 Synonym Configuration:");
				synonymConfig.Add($"   • Source Database (contains data): {_currentConfig.SynonymSourceDb}");

				// Determine target databases
				var targetDatabases = new List<string>();

				if (!string.IsNullOrEmpty(_currentConfig.Database) &&
					_currentConfig.Database != _currentConfig.SynonymSourceDb)
				{
					targetDatabases.Add(_currentConfig.Database);
				}

				if (_currentConfig.EnableMultipleDatabases && _currentConfig.DeploymentTargets?.Any() == true)
				{
					foreach (var target in _currentConfig.DeploymentTargets.Where(t => t.IsEnabled))
					{
						if (!string.IsNullOrEmpty(target.Database) &&
							target.Database != _currentConfig.SynonymSourceDb &&
							!targetDatabases.Contains(target.Database))
						{
							targetDatabases.Add(target.Database);
						}
					}
				}

				if (targetDatabases.Any())
				{
					synonymConfig.Add($"   • Target Database(s) (will get synonyms):");
					foreach (var db in targetDatabases)
					{
						synonymConfig.Add($"     - {db}");
					}

					synonymConfig.Add($"   • Synonym Creation Strategy:");
					synonymConfig.Add($"     - CFMSurveyUser synonym will be created in target databases");
					synonymConfig.Add($"     - Synonyms will point to [{_currentConfig.SynonymSourceDb}].[dbo].[CFMUser]");
					synonymConfig.Add($"     - Only databases that need synonyms will be processed");
					synonymConfig.Add($"     - Source database '{_currentConfig.SynonymSourceDb}' will NOT get synonyms");
				}
				else
				{
					synonymConfig.Add($"   • Target Databases: None (all deployment targets are same as source)");
					synonymConfig.Add($"   • Action: No synonyms will be created (not needed)");
				}

				_logService?.LogInfo(string.Join(Environment.NewLine, synonymConfig));

				// Enhanced validation
				if (string.IsNullOrEmpty(_currentConfig.SynonymSourceDb))
				{
					_logService?.LogWarning("⚠️ No synonym source database specified - synonyms will be skipped");
				}
				else if (targetDatabases.Count == 0)
				{
					_logService?.LogInfo($"ℹ️ No synonym targets found - all databases are same as source '{_currentConfig.SynonymSourceDb}'");
				}
				else
				{
					_logService?.LogInfo($"✅ Synonym configuration validated: {targetDatabases.Count} target database(s) identified");

					// Show practical example
					var exampleTarget = targetDatabases.First();
					_logService?.LogInfo($"💡 Example: [{exampleTarget}].[dbo].[CFMSurveyUser] → [{_currentConfig.SynonymSourceDb}].[dbo].[CFMUser]");
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error validating synonym configuration", ex);
			}
		}
		private void UpdateSynonymSourceUI1()
		{
			try
			{
				if (txtSynonymSourceDb == null) return;

				// If synonyms are enabled but no source is specified, show the smart default
				if (chkCreateSynonyms?.Checked == true && string.IsNullOrWhiteSpace(txtSynonymSourceDb.Text))
				{
					txtSynonymSourceDb.Text = "HiveCFMSurvey";
					txtSynonymSourceDb.ForeColor = System.Drawing.Color.Gray;

					// Add tooltip to explain this is the standard source
					if (toolTip != null)
					{
						toolTip.SetToolTip(txtSynonymSourceDb,
							"HiveCFMSurvey is the standard source database for CFMSurveyUser synonyms.\n" +
							"You can change this if your environment uses a different source database.");
					}
				}
				else if (chkCreateSynonyms?.Checked == false)
				{
					// Clear the field when synonyms are disabled
					txtSynonymSourceDb.Text = "";
					txtSynonymSourceDb.ForeColor = System.Drawing.Color.Black;
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error updating synonym source UI", ex);
			}
		}
		private void UpdateSynonymSourceUI()
		{
			try
			{
				if (txtSynonymSourceDb == null) return;

				// If synonyms are enabled but no source is specified, show the smart default
				if (chkCreateSynonyms?.Checked == true && string.IsNullOrWhiteSpace(txtSynonymSourceDb.Text))
				{
					txtSynonymSourceDb.Text = "HiveCFMSurvey";
					txtSynonymSourceDb.ForeColor = System.Drawing.Color.Gray;

					// Enhanced tooltip with clear explanation
					if (toolTip != null)
					{
						toolTip.SetToolTip(txtSynonymSourceDb,
							"SYNONYM SOURCE DATABASE:\n" +
							"• This is the database that CONTAINS the actual CFMUser table\n" +
							"• Other databases will get CFMSurveyUser synonyms pointing to this database\n" +
							"• Example: If you enter 'HiveCFMSurvey', then other databases will have:\n" +
							"  [OtherDB].[dbo].[CFMSurveyUser] → [HiveCFMSurvey].[dbo].[CFMUser]\n" +
							"• The source database itself will NOT get synonyms (it has the real table)");
					}
				}
				else if (chkCreateSynonyms?.Checked == false)
				{
					// Clear the field when synonyms are disabled
					txtSynonymSourceDb.Text = "";
					txtSynonymSourceDb.ForeColor = System.Drawing.Color.Black;

					if (toolTip != null)
					{
						toolTip.SetToolTip(txtSynonymSourceDb, null);
					}
				}
				else if (chkCreateSynonyms?.Checked == true && !string.IsNullOrWhiteSpace(txtSynonymSourceDb.Text))
				{
					// User has entered a custom source database
					txtSynonymSourceDb.ForeColor = System.Drawing.Color.Black;

					if (toolTip != null)
					{
						var sourceDb = txtSynonymSourceDb.Text.Trim();
						toolTip.SetToolTip(txtSynonymSourceDb,
							$"CUSTOM SYNONYM SOURCE: {sourceDb}\n" +
							$"• CFMSurveyUser synonyms will point to [{sourceDb}].[dbo].[CFMUser]\n" +
							$"• Make sure this database exists and contains the CFMUser table\n" +
							$"• This database will NOT receive synonyms (it has the real table)");
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error updating synonym source UI", ex);
			}
		}

		private async Task<bool> ValidateSynonymConfigurationAsync()
		{
			try
			{
				if (!(_currentConfig?.CreateSynonyms ?? false))
				{
					return true; // Synonyms disabled, nothing to validate
				}

				if (string.IsNullOrEmpty(_currentConfig.SynonymSourceDb))
				{
					MessageBox.Show(
						"❌ Synonym source database is required when 'Create Synonyms' is enabled.\n\n" +
						"Please specify which database contains the actual CFMUser table.",
						"Synonym Configuration Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
					return false;
				}

				// Check if source database exists
				try
				{
					var connectionInfo = new ConnectionInfo
					{
						ServerName = _currentConfig.ServerName,
						WindowsAuth = _currentConfig.WindowsAuth,
						Username = _currentConfig.Username,
						Password = _currentConfig.Password,
						Database = "master"
					};

					using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
					{
						await connection.OpenAsync();
						var query = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
						using (var command = new SqlCommand(query, connection))
						{
							command.Parameters.AddWithValue("@DatabaseName", _currentConfig.SynonymSourceDb);
							var count = (int)await command.ExecuteScalarAsync();

							if (count == 0)
							{
								var continueChoice = MessageBox.Show(
									$"⚠️ Warning: Synonym source database '{_currentConfig.SynonymSourceDb}' was not found.\n\n" +
									"This might be because:\n" +
									"• The database name is misspelled\n" +
									"• The database doesn't exist yet\n" +
									"• You don't have permission to see it\n\n" +
									"Continue deployment anyway?",
									"Source Database Not Found",
									MessageBoxButtons.YesNo,
									MessageBoxIcon.Warning);

								if (continueChoice == DialogResult.No)
								{
									return false;
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logService?.LogWarning($"Could not validate synonym source database: {ex.Message}");
					// Continue anyway - might be a permission issue
				}

				// Determine target databases and show summary
				var targetDatabases = new List<string>();

				if (!string.IsNullOrEmpty(_currentConfig.Database) &&
					_currentConfig.Database != _currentConfig.SynonymSourceDb)
				{
					targetDatabases.Add(_currentConfig.Database);
				}

				if (_currentConfig.EnableMultipleDatabases && _currentConfig.DeploymentTargets?.Any() == true)
				{
					foreach (var target in _currentConfig.DeploymentTargets.Where(t => t.IsEnabled))
					{
						if (!string.IsNullOrEmpty(target.Database) &&
							target.Database != _currentConfig.SynonymSourceDb &&
							!targetDatabases.Contains(target.Database))
						{
							targetDatabases.Add(target.Database);
						}
					}
				}

				if (targetDatabases.Count == 0)
				{
					var noTargetsChoice = MessageBox.Show(
						$"ℹ️ Information: No target databases need synonyms.\n\n" +
						$"All deployment targets are the same as the source database '{_currentConfig.SynonymSourceDb}'.\n\n" +
						"This is normal if you're deploying to the database that already contains the CFMUser table.\n\n" +
						"Continue deployment without creating synonyms?",
						"No Synonym Targets",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Information);

					return noTargetsChoice == DialogResult.Yes;
				}

				// Show summary of what will happen
				var summaryMessage = $"✅ Synonym Creation Summary:\n\n" +
								   $"Source Database: {_currentConfig.SynonymSourceDb}\n" +
								   $"Target Database(s): {string.Join(", ", targetDatabases)}\n\n" +
								   $"Synonyms will be created in target databases pointing to the source.\n" +
								   $"Example: [TargetDB].[dbo].[CFMSurveyUser] → [{_currentConfig.SynonymSourceDb}].[dbo].[CFMUser]\n\n" +
								   $"Continue with deployment?";

				var confirmChoice = MessageBox.Show(summaryMessage, "Confirm Synonym Configuration",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				return confirmChoice == DialogResult.Yes;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error validating synonym configuration", ex);
				MessageBox.Show($"❌ Error validating synonym configuration: {ex.Message}",
					"Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		private void UpdateUIFromConfiguration()
		{
			try
			{
				// Basic settings with null checks
				if (txtServerName != null) txtServerName.Text = _currentConfig.ServerName ?? "";
				if (chkWindowsAuth != null) chkWindowsAuth.Checked = _currentConfig.WindowsAuth;
				if (txtUsername != null) txtUsername.Text = _currentConfig.Username ?? "";
				if (txtPassword != null) txtPassword.Text = _currentConfig.Password ?? "";
				if (txtDacpacPath != null) txtDacpacPath.Text = _currentConfig.DacpacPath ?? "";

				// Feature settings
				if (chkCreateSynonyms != null) chkCreateSynonyms.Checked = _currentConfig.CreateSynonyms;
				if (txtSynonymSourceDb != null) txtSynonymSourceDb.Text = _currentConfig.SynonymSourceDb ?? "";
				if (chkCreateSqlAgentJobs != null) chkCreateSqlAgentJobs.Checked = _currentConfig.CreateSqlAgentJobs;
				if (txtJobOwnerLoginName != null) txtJobOwnerLoginName.Text = _currentConfig.JobOwnerLoginName ?? "";
				if (txtJobScriptsFolder != null) txtJobScriptsFolder.Text = _currentConfig.JobScriptsFolder ?? "";
				if (chkExecuteProcedures != null) chkExecuteProcedures.Checked = _currentConfig.ExecuteProcedures;
				if (chkCreateBackup != null) chkCreateBackup.Checked = _currentConfig.CreateBackupBeforeDeployment;

				// FIXED: Multiple databases settings - Find controls dynamically
				var multiDbCheckbox = FindControlByName("chkEnableMultipleDatabases") as CheckBox;
				if (multiDbCheckbox != null)
				{
					multiDbCheckbox.Checked = _currentConfig.EnableMultipleDatabases;
				}

				// FIXED: Load secondary database configuration
				if (_currentConfig.EnableMultipleDatabases && _currentConfig.DeploymentTargets?.Any() == true)
				{
					var secondaryTarget = _currentConfig.DeploymentTargets.FirstOrDefault(t => t.Name == "Secondary");
					if (secondaryTarget != null)
					{
						var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
						var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;

						// Update secondary database if control exists
						if (cboDatabases2 != null && !string.IsNullOrEmpty(secondaryTarget.Database))
						{
							if (!cboDatabases2.Items.Contains(secondaryTarget.Database))
								cboDatabases2.Items.Add(secondaryTarget.Database);
							cboDatabases2.SelectedItem = secondaryTarget.Database;
						}

						// Update secondary DACPAC path if control exists
						if (txtDacpacPath2 != null)
							txtDacpacPath2.Text = secondaryTarget.DacpacPath ?? "";
					}
				}

				// Enable/disable multiple database controls
				ToggleMultipleDatabaseControls(_currentConfig.EnableMultipleDatabases);

				// Update primary database dropdown if possible
				if (cboDatabases != null && !string.IsNullOrEmpty(_currentConfig.Database))
				{
					if (!cboDatabases.Items.Contains(_currentConfig.Database))
					{
						cboDatabases.Items.Add(_currentConfig.Database);
					}
					cboDatabases.SelectedItem = _currentConfig.Database;
				}

				UpdateSmartProcedureStatus();

				// Trigger job script validation if folder is specified
				if (!string.IsNullOrEmpty(_currentConfig.JobScriptsFolder))
				{
					Task.Run(async () => await ValidateJobScriptsFolderAsync());
				}

				_logService?.LogInfo("✅ UI updated from configuration successfully");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to update UI from configuration", ex);
				MessageBox.Show($"⚠️ Warning: Some settings could not be restored: {ex.Message}",
					"Configuration Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
		private void ToggleMultipleDatabaseControls(bool enabled)
		{
			try
			{
				// Find controls dynamically to avoid null reference errors
				var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
				var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;
				var btnBrowseDacpac2 = FindControlByName("btnBrowseDacpac2") as Button;
				var btnRefreshDatabases2 = FindControlByName("btnRefreshDatabases2") as Button;
				var lblDatabase2 = FindControlByName("lblDatabase2") as Label;
				var lblDacpacPath2 = FindControlByName("lblDacpacPath2") as Label;

				// Enable/disable controls if they exist
				if (cboDatabases2 != null) cboDatabases2.Enabled = enabled;
				if (txtDacpacPath2 != null) txtDacpacPath2.Enabled = enabled;
				if (btnBrowseDacpac2 != null) btnBrowseDacpac2.Enabled = enabled;
				if (btnRefreshDatabases2 != null) btnRefreshDatabases2.Enabled = enabled;
				if (lblDatabase2 != null) lblDatabase2.Enabled = enabled;
				if (lblDacpacPath2 != null) lblDacpacPath2.Enabled = enabled;
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error toggling multiple database controls", ex);
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

		// SOLUTION 1: Safe DeployDatabaseAsync method with null checks
		private async Task<DeploymentResult> DeployDatabaseAsync()
		{
			var result = new DeploymentResult
			{
				Timestamp = DateTime.Now,
				DatabaseName = _currentConfig.Database,
				DacpacPath = _currentConfig.DacpacPath
			};

			try
			{
				string backupPath = null;

				// Step 1: Create backup if enabled
				if (_currentConfig.CreateBackupBeforeDeployment)
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

				// Step 2: Deploy using the new robust service
				_logService.LogInfo("🚀 Starting robust DACPAC deployment...");
				var deployResult = await _deploymentService.DeployDacpacAsync(_currentConfig);

				// Copy all properties from deployResult to result with null safety
				result.Success = deployResult.Success;
				result.Message = deployResult.Message ?? string.Empty;
				result.Duration = deployResult.Duration;
				result.Warnings = deployResult.Warnings ?? new List<string>();
				result.Errors = deployResult.Errors ?? new List<string>();
				result.BackupPath = backupPath;
				result.ProceduresExecuted = deployResult.ProceduresExecuted;
				result.JobsCreated = deployResult.JobsCreated;
				result.SynonymsCreated = deployResult.SynonymsCreated;
				result.Exception = deployResult.Exception;

				// Step 3: Record deployment history
				var historyEntry = new DeploymentHistory
				{
					Timestamp = DateTime.Now,
					ServerName = _currentConfig.ServerName ?? string.Empty,
					Database = _currentConfig.Database ?? string.Empty,
					DacpacFile = _currentConfig.DacpacPath ?? string.Empty,
					Success = result.Success,
					BackupPath = backupPath ?? string.Empty,
					Duration = result.Duration,
					ErrorMessage = result.Success ? string.Empty : (result.Message ?? string.Empty)
				};

				// Initialize History list if null
				if (_currentConfig.History == null)
					_currentConfig.History = new List<DeploymentHistory>();

				_currentConfig.History.Add(historyEntry);

				// Step 4: Post-deployment handling with improved messaging
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
					result.Errors = new List<string>();

				result.Errors.Add($"Exception: {ex.Message ?? "Unknown exception"}");

				if (ex.InnerException != null)
					result.Errors.Add($"Inner Exception: {ex.InnerException.Message ?? "Unknown inner exception"}");

				return result;
			}
		}
		private async Task SafeHandlePostDeploymentSuccess(DeploymentResult result)
		{
			try
			{
				// Check if we should show the data viewer option
				bool hasDataAnalysisService = _dataAnalysisService != null;
				bool hasDataViewerTab = tabControl?.TabPages?.Cast<TabPage>()?.Any(tp => tp.Name == "tabDataViewer") == true;

				string successMessage = BuildSuccessMessage(result);

				if (hasDataAnalysisService && hasDataViewerTab)
				{
					// Full data viewer integration available
					var dialogResult = MessageBox.Show(
						successMessage + "\n\n🔍 Would you like to explore the deployed data and get smart recommendations?",
						"🚀 Deployment Successful",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Information);

					if (dialogResult == DialogResult.Yes)
					{
						await SafeSwitchToDataViewer();
					}
				}
				else
				{
					// Basic success message only
					MessageBox.Show(
						successMessage + "\n\n📊 Data viewer is being prepared and will be available in the next version.",
						"🚀 Deployment Successful",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}

				_logService.LogInfo("✅ Post-deployment handling completed successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in post-deployment success handling", ex);

				// Fallback: Simple success message
				MessageBox.Show(
					"🎉 Deployment completed successfully!\n\nCheck the log for details.",
					"Deployment Successful",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		// FIXED VERSION: Replace your BuildSuccessMessage method in DacpacPublisherForm.cs
		private string BuildSuccessMessage(DeploymentResult result)
		{
			var message = new StringBuilder();
			message.AppendLine("🎉 Deployment Completed Successfully!");
			message.AppendLine();
			message.AppendLine($"✅ Database: {_currentConfig.Database ?? "Unknown"}");
			message.AppendLine($"✅ Server: {_currentConfig.ServerName ?? "Unknown"}");
			message.AppendLine($"⏱️ Duration: {result.Duration:mm\\:ss}");

			if (result.ProceduresExecuted > 0)
				message.AppendLine($"🔧 Procedures Executed: {result.ProceduresExecuted}");

			if (result.JobsCreated > 0)
				message.AppendLine($"⚙️ Jobs Created: {result.JobsCreated}");

			if (result.SynonymsCreated > 0)
				message.AppendLine($"🔗 Synonyms Created: {result.SynonymsCreated}");

			if (!string.IsNullOrEmpty(result.BackupPath))
				message.AppendLine($"💾 Backup: {Path.GetFileName(result.BackupPath)}");

			// ENHANCED: Service Broker-aware categorization
			var category = DeploymentConfiguration.CategorizeResult(result.Errors, result.Warnings);

			switch (category)
			{
				case DeploymentResultCategory.SuccessWithIgnorableErrors:
					var autoGenCount = CountServiceBrokerErrors(result.Errors, result.Warnings);
					message.AppendLine($"⏭️ Service Broker Procedures Handled: {autoGenCount}");
					message.AppendLine("💡 These are auto-generated procedures with GUID identifiers");
					message.AppendLine("🤖 Created by SQL Server Service Broker and safely excluded");
					break;

				case DeploymentResultCategory.SuccessWithWarnings:
					var warningCount = result.Warnings.Count;
					var serviceBrokerWarnings = result.Warnings.Count(w =>
						DeploymentConfiguration.IsIgnorableError(w));

					if (serviceBrokerWarnings > 0)
					{
						message.AppendLine($"⚠️ Warnings: {warningCount} total ({serviceBrokerWarnings} Service Broker related)");
						message.AppendLine("💡 Service Broker warnings are expected and can be ignored");
					}
					else
					{
						message.AppendLine($"⚠️ Minor Warnings: {warningCount} (check log for details)");
					}
					break;
			}

			// Add Service Broker explanation if relevant
			if (HasServiceBrokerIssues(result))
			{
				message.AppendLine();
				message.AppendLine("🔍 About Service Broker Auto-Generated Procedures:");
				message.AppendLine("• These procedures have GUID identifiers (e.g., QueueActionSender_5f17ce3a-41a0...)");
				message.AppendLine("• They are automatically created by SQL Server Service Broker");
				message.AppendLine("• Your deployment excluded them to prevent conflicts");
				message.AppendLine("• Your application functionality is not affected");
			}

			return message.ToString();
		}

		// ADD these helper methods to DacpacPublisherForm.cs
		private int CountServiceBrokerErrors(List<string> errors, List<string> warnings)
		{
			var allMessages = new List<string>();
			if (errors != null) allMessages.AddRange(errors);
			if (warnings != null) allMessages.AddRange(warnings);

			return allMessages.Count(msg =>
				msg.Contains("QueueActionSender_") ||
				msg.Contains("BulkSurveyQueue") ||
				System.Text.RegularExpressions.Regex.IsMatch(msg, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"));
		}

		private bool HasServiceBrokerIssues(DeploymentResult result)
		{
			var allMessages = new List<string>();
			if (result.Errors != null) allMessages.AddRange(result.Errors);
			if (result.Warnings != null) allMessages.AddRange(result.Warnings);

			return allMessages.Any(msg =>
				msg.Contains("QueueActionSender") ||
				msg.Contains("Service Broker") ||
				msg.Contains("BulkSurveyQueue") ||
				System.Text.RegularExpressions.Regex.IsMatch(msg, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"));
		}
		private string BuildFailureMessage(DeploymentResult result)
		{
			var message = new StringBuilder();

			var category = DeploymentConfiguration.CategorizeResult(result.Errors, result.Warnings);

			switch (category)
			{
				case DeploymentResultCategory.SuccessWithIgnorableErrors:
					message.AppendLine("🎉 Deployment Actually Succeeded!");
					message.AppendLine("(Despite some auto-generated procedure warnings)");
					message.AppendLine();
					var autoGenCount = CountAutoGeneratedErrors(result.Errors);
					message.AppendLine($"⏭️ Auto-Generated Procedures Safely Ignored: {autoGenCount}");
					message.AppendLine();
					message.AppendLine("🤖 What happened:");
					message.AppendLine("• SQL Server Service Broker created some auto-generated procedures");
					message.AppendLine("• These procedures have GUID identifiers and are system-generated");
					message.AppendLine("• Your DACPAC deployment succeeded despite these warnings");
					message.AppendLine("• Your application should work normally");
					break;

				default:
					message.AppendLine($"❌ Deployment Failed: {result.Message}");
					message.AppendLine();

					var categorizedErrors = CategorizeErrorsEnhanced(result.Errors);

					if (categorizedErrors.ContainsKey("Critical"))
					{
						message.AppendLine("🔥 Critical Errors (require immediate attention):");
						foreach (var error in categorizedErrors["Critical"].Take(3))
						{
							message.AppendLine($"  • {TruncateErrorMessage(error)}");
						}
						message.AppendLine();
					}

					if (categorizedErrors.ContainsKey("AutoGenerated"))
					{
						message.AppendLine("⚙️ Auto-Generated Procedure Issues (can usually be ignored):");
						message.AppendLine($"  • {categorizedErrors["AutoGenerated"].Count} auto-generated procedures not found");
						message.AppendLine("  • These are typically created by SQL Server Service Broker");
						message.AppendLine("  • Consider this deployment successful if no other critical errors exist");
						message.AppendLine();
					}

					if (categorizedErrors.ContainsKey("SqlCmd"))
					{
						message.AppendLine("📝 SqlCmd Variable Issues:");
						foreach (var error in categorizedErrors["SqlCmd"].Take(2))
						{
							message.AppendLine($"  • {TruncateErrorMessage(error)}");
						}
						message.AppendLine();
					}

					if (categorizedErrors.ContainsKey("Configuration"))
					{
						message.AppendLine("⚙️ Configuration Issues:");
						foreach (var error in categorizedErrors["Configuration"].Take(2))
						{
							message.AppendLine($"  • {TruncateErrorMessage(error)}");
						}
						message.AppendLine();
					}
					break;
			}

			// ENHANCED: Smart recommendations based on error categories
			message.AppendLine("🔧 RECOMMENDED ACTIONS:");

			switch (category)
			{
				case DeploymentResultCategory.SuccessWithIgnorableErrors:
					message.AppendLine("  ✅ No action needed - deployment was successful!");
					message.AppendLine("  💡 The auto-generated procedure warnings can be safely ignored");
					message.AppendLine("  🔍 Verify your application works as expected");
					break;

				case DeploymentResultCategory.CriticalFailure:
					message.AppendLine("  1. Fix critical errors above");
					message.AppendLine("  2. Verify database permissions");
					message.AppendLine("  3. Check DACPAC file integrity");
					break;

				default:
					var hasAutoGenErrors = result.Errors?.Any(e => DeploymentConfiguration.IsIgnorableError(e)) == true;
					if (hasAutoGenErrors)
					{
						message.AppendLine("  1. Consider this deployment successful if only auto-generated procedure errors exist");
						message.AppendLine("  2. Test your application functionality");
						message.AppendLine("  3. Review non-critical warnings in the log");
					}
					else
					{
						message.AppendLine("  1. Review detailed log for specific issues");
						message.AppendLine("  2. Verify connection settings");
						message.AppendLine("  3. Check SQL Server error logs");
					}
					break;
			}

			message.AppendLine();
			message.AppendLine("📋 Check the deployment log for complete technical details.");

			return message.ToString();
		}
		/// <summary>
		/// Filter critical warnings from the full list
		/// </summary>
		///
		private int CountAutoGeneratedErrors(List<string> errors)
		{
			if (errors == null) return 0;
			return errors.Count(e => DeploymentConfiguration.IsIgnorableError(e));
		}
		private List<string> FilterCriticalWarnings(List<string> warnings)
		{
			if (warnings == null) return new List<string>();

			var criticalKeywords = new[]
			{
		"login failed", "access denied", "permission denied",
		"cannot connect", "timeout", "deadlock", "critical"
	};

			return warnings.Where(w =>
				criticalKeywords.Any(keyword =>
					w.ToLower().Contains(keyword.ToLower()))).ToList();
		}
		private Dictionary<string, List<string>> CategorizeErrorsEnhanced(List<string> errors)
		{
			var categorized = new Dictionary<string, List<string>>
			{
				["Critical"] = new List<string>(),
				["AutoGenerated"] = new List<string>(),
				["SqlCmd"] = new List<string>(),
				["Configuration"] = new List<string>(),
				["Other"] = new List<string>()
			};

			if (errors == null) return categorized;

			foreach (var error in errors)
			{
				if (DeploymentConfiguration.IsCriticalError(error))
				{
					categorized["Critical"].Add(error);
				}
				else if (DeploymentConfiguration.IsIgnorableError(error))
				{
					categorized["AutoGenerated"].Add(error);
				}
				else if (error.ToLower().Contains("sqlcmd") || error.ToLower().Contains("variables are not defined"))
				{
					categorized["SqlCmd"].Add(error);
				}
				else if (error.ToLower().Contains("dacpac") ||
						 error.ToLower().Contains("sqlpackage") ||
						 error.ToLower().Contains("invalid object name"))
				{
					categorized["Configuration"].Add(error);
				}
				else
				{
					categorized["Other"].Add(error);
				}
			}

			return categorized;
		}
		/// <summary>
		/// Count skipped auto-generated procedures
		/// </summary>
		private int CountSkippedProcedures(List<string> warnings)
		{
			if (warnings == null) return 0;

			return warnings.Count(w =>
				w.ToLower().Contains("skipped auto-generated") ||
				w.ToLower().Contains("skipping auto-generated") ||
				w.ToLower().Contains("queueactionsender") ||
				w.ToLower().Contains("auto-generated procedure"));
		}

		/// <summary>
		/// Categorize errors for better user understanding
		/// </summary>
		private Dictionary<string, List<string>> CategorizeErrors(List<string> errors)
		{
			var categorized = new Dictionary<string, List<string>>
			{
				["Critical"] = new List<string>(),
				["AutoGenerated"] = new List<string>(),
				["Configuration"] = new List<string>(),
				["Other"] = new List<string>()
			};

			if (errors == null) return categorized;

			foreach (var error in errors)
			{
				var lowerError = error.ToLower();

				// Critical system errors
				if (lowerError.Contains("login failed") ||
					lowerError.Contains("access denied") ||
					lowerError.Contains("cannot connect") ||
					lowerError.Contains("database") && lowerError.Contains("does not exist"))
				{
					categorized["Critical"].Add(error);
				}
				// Auto-generated procedure errors (usually can be ignored)
				else if (lowerError.Contains("queueactionsender") ||
						 lowerError.Contains("could not find stored procedure") &&
						 (lowerError.Contains("-") && lowerError.Contains("_")) ||
						 IsAutoGeneratedProcedureError(error))
				{
					categorized["AutoGenerated"].Add(error);
				}
				// Configuration/setup errors
				else if (lowerError.Contains("dacpac") ||
						 lowerError.Contains("sqlpackage") ||
						 lowerError.Contains("invalid object name") ||
						 lowerError.Contains("synonym"))
				{
					categorized["Configuration"].Add(error);
				}
				// Everything else
				else
				{
					categorized["Other"].Add(error);
				}
			}

			return categorized;
		}

		/// <summary>
		/// Check if an error is related to auto-generated procedures
		/// </summary>
		private bool IsAutoGeneratedProcedureError(string error)
		{
			if (string.IsNullOrEmpty(error)) return false;

			// Look for GUID patterns in error messages
			var guidPattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
			return System.Text.RegularExpressions.Regex.IsMatch(error, guidPattern);
		}

		/// <summary>
		/// Truncate long error messages for display
		/// </summary>
		private string TruncateErrorMessage(string error, int maxLength = 120)
		{
			if (string.IsNullOrEmpty(error) || error.Length <= maxLength)
				return error;

			return error.Substring(0, maxLength) + "...";
		}




		// UPDATED: Better error message building for failed deployments
		/// <summary>
		/// SAFE version of switching to data viewer
		/// </summary>
		private async Task SafeSwitchToDataViewer()
		{
			try
			{
				_logService.LogInfo("🔄 Attempting to switch to Data Viewer...");

				// Find the data viewer tab safely
				TabPage dataViewerTab = null;
				if (tabControl?.TabPages != null)
				{
					foreach (TabPage tab in tabControl.TabPages)
					{
						if (tab.Text.Contains("Data Viewer") || tab.Name == "tabDataViewer")
						{
							dataViewerTab = tab;
							break;
						}
					}
				}

				if (dataViewerTab != null)
				{
					tabControl.SelectedTab = dataViewerTab;
					_logService.LogInfo("📊 Switched to Data Viewer tab");

					// Try to refresh tables if components are available
					await SafeRefreshDataViewer();
				}
				else
				{
					_logService.LogWarning("⚠️ Data Viewer tab not found");
					MessageBox.Show(
						"📊 Data Viewer tab is not available yet.\n\nPlease manually explore your database using SQL Server Management Studio.",
						"Data Viewer Unavailable",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to switch to data viewer", ex);
				MessageBox.Show(
					"⚠️ Could not open Data Viewer automatically.\n\nYou can manually check your deployed database.",
					"Data Viewer Warning",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		/// <summary>
		/// SAFE version of refreshing data viewer
		/// </summary>
		private async Task SafeRefreshDataViewer()
		{
			try
			{
				// Only proceed if we have the necessary components
				if (_dataAnalysisService == null)
				{
					_logService.LogInfo("📊 Data analysis service not available");
					return;
				}

				var connectionInfo = CreateConnectionInfo(_currentConfig);
				bool canConnect = await _connectionService.TestConnectionAsync(connectionInfo);

				if (!canConnect)
				{
					_logService.LogWarning("⚠️ Cannot connect to deployed database for analysis");
					return;
				}

				// Try to get tables
				var tables = await _dataAnalysisService.GetTablesAsync(connectionInfo);
				_logService.LogInfo($"📋 Found {tables.Count} tables for analysis");

				// Update UI components if they exist
				UpdateDataViewerUILater(tables);
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh data viewer", ex);
				// Don't show error message here as it's not critical
			}
		}

		/// <summary>
		/// Updates Data Viewer UI safely on the main thread
		/// </summary>
		private void UpdateDataViewerUILater(List<string> tables)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => UpdateDataViewerUILater(tables)));
					return;
				}

				// Find and update combo box if it exists
				var tableCombo = FindControlByName("cboTables") as ComboBox;
				if (tableCombo != null)
				{
					tableCombo.Items.Clear();
					foreach (var table in tables)
					{
						tableCombo.Items.Add(table);
					}
					if (tables.Count > 0)
						tableCombo.SelectedIndex = 0;
				}

				// Find and update label if it exists
				var tableCountLabel = FindControlByName("lblTableCount") as Label;
				if (tableCountLabel != null)
				{
					tableCountLabel.Text = $"Tables: {tables.Count}";
				}

				_logService.LogInfo($"✅ Data Viewer UI updated with {tables.Count} tables");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to update Data Viewer UI", ex);
			}
		}

		/// <summary>
		/// Recursively finds a control by name
		/// </summary>
		private Control FindControlByName(string name)
		{
			try
			{
				return FindControlByNameRecursive(this, name);
			}
			catch (Exception ex)
			{
				_logService?.LogError($"Error finding control '{name}'", ex);
				return null;
			}
		}

		private Control FindControlByNameRecursive(Control parent, string name)
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

			return null;
		}

		/// <summary>
		/// SAFE version of handling deployment failure with backup
		/// </summary>
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
						var connectionInfo = CreateConnectionInfo(_currentConfig);
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
								result.Warnings = new List<string>();
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






		private async Task<string> CreateBackupAsync()
		{
			using (var saveBackupDialog = new SaveFileDialog())
			{
				saveBackupDialog.Title = "Save Database Backup";
				saveBackupDialog.Filter = "SQL Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
				saveBackupDialog.DefaultExt = "bak";

				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				saveBackupDialog.FileName = $"{_currentConfig.Database}_Backup_{timestamp}.bak";

				if (saveBackupDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						var connectionInfo = CreateConnectionInfo(_currentConfig);
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
		private async Task HandlePostDeploymentDataViewer(DeploymentResult result)
		{
			try
			{
				// Check if we have the data analysis service available
				if (_dataAnalysisService == null)
				{
					_logService.LogInfo("📊 Data analysis service not available, skipping post-deployment analysis");
					return;
				}

				// Quick connection test to ensure database is accessible
				var connectionInfo = CreateConnectionInfo(_currentConfig);
				bool canConnect = await _connectionService.TestConnectionAsync(connectionInfo);

				if (!canConnect)
				{
					_logService.LogWarning("⚠️ Cannot connect to deployed database for analysis");
					return;
				}

				// Get basic database info for the prompt
				string databaseInfo = "";
				try
				{
					var tables = await _dataAnalysisService.GetTablesAsync(connectionInfo);
					databaseInfo = $"\n\n📊 Database Info:\n• {tables.Count} tables detected\n• Ready for analysis";
				}
				catch (Exception ex)
				{
					_logService.LogWarning($"Could not get table count: {ex.Message}");
					databaseInfo = "\n\n📊 Database deployed successfully";
				}

				// Show success message with data viewer option
				var dialogResult = MessageBox.Show(
					$"🎉 Deployment Completed Successfully!" +
					$"\n\n✅ Database: {_currentConfig.Database}" +
					$"\n✅ Server: {_currentConfig.ServerName}" +
					$"\n⏱️ Duration: {result.Duration:mm\\:ss}" +
					(result.ProceduresExecuted > 0 ? $"\n✅ Procedures: {result.ProceduresExecuted} executed" : "") +
					(!string.IsNullOrEmpty(result.BackupPath) ? $"\n💾 Backup: {Path.GetFileName(result.BackupPath)}" : "") +
					databaseInfo +
					$"\n\n🔍 Would you like to explore the deployed data and get smart recommendations?",
					"🚀 Deployment Successful",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);

				if (dialogResult == DialogResult.Yes)
				{
					await SwitchToDataViewerAndAnalyze();
				}
				else
				{
					// Still log the success but user chose not to view data
					_logService.LogInfo("✅ Deployment completed. User chose not to view data analysis.");
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in post-deployment data viewer handling", ex);
				// Don't propagate this error as it shouldn't fail the deployment
			}
		}
		private async Task SwitchToDataViewerAndAnalyze()
		{
			try
			{
				_logService.LogInfo("🔄 Switching to Data Viewer for post-deployment analysis...");

				// Switch to the data viewer tab
				if (tabDataViewer != null)
				{
					tabControl.SelectedTab = tabDataViewer;
					_logService.LogInfo("📊 Switched to Data Viewer tab");
				}

				// Refresh tables automatically
				await RefreshTablesInDataViewer();

				// Show initial database summary
				await LoadInitialDatabaseAnalysis();

				_logService.LogInfo("✅ Data Viewer initialized successfully");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to switch to data viewer", ex);
				MessageBox.Show(
					$"⚠️ Could not initialize Data Viewer: {ex.Message}\n\nYou can manually switch to the 'Data Viewer' tab to explore your data.",
					"Data Viewer Warning",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
		/// <summary>
		/// Refreshes tables in the data viewer (called from post-deployment)
		/// </summary>
		private async Task RefreshTablesInDataViewer()
		{
			try
			{
				if (cboTables == null) return;

				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var tables = await _dataAnalysisService.GetTablesAsync(connectionInfo);

				// Update UI on main thread
				if (InvokeRequired)
				{
					Invoke(new Action(() =>
					{
						cboTables.Items.Clear();
						foreach (var table in tables)
						{
							cboTables.Items.Add(table);
						}

						if (lblTableCount != null)
							lblTableCount.Text = $"Tables: {tables.Count}";

						if (tables.Count > 0 && cboTables.Items.Count > 0)
						{
							cboTables.SelectedIndex = 0;
						}
					}));
				}
				else
				{
					cboTables.Items.Clear();
					foreach (var table in tables)
					{
						cboTables.Items.Add(table);
					}

					if (lblTableCount != null)
						lblTableCount.Text = $"Tables: {tables.Count}";

					if (tables.Count > 0 && cboTables.Items.Count > 0)
					{
						cboTables.SelectedIndex = 0;
					}
				}

				_logService.LogInfo($"📋 Loaded {tables.Count} tables in Data Viewer");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh tables in data viewer", ex);
			}
		}

		/// <summary>
		/// Loads initial database analysis summary
		/// </summary>
		private async Task LoadInitialDatabaseAnalysis()
		{
			try
			{
				if (rtbRecommendations == null) return;

				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var summary = await _dataAnalysisService.GetDatabaseSummaryAsync(connectionInfo);

				var analysisText = $"🎯 Post-Deployment Analysis\n";
				analysisText += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";
				analysisText += $"📊 DATABASE OVERVIEW\n";
				analysisText += $"• Total Tables: {summary.TotalTables:N0}\n";
				analysisText += $"• Total Rows: {summary.TotalRows:N0}\n";
				analysisText += $"• Analysis Time: {DateTime.Now:HH:mm:ss}\n\n";

				if (summary.LargestTables.Any())
				{
					analysisText += $"🏆 LARGEST TABLES\n";
					foreach (var table in summary.LargestTables.Take(5))
					{
						analysisText += $"  • {table}\n";
					}
					analysisText += "\n";
				}

				if (summary.EmptyTables.Any())
				{
					analysisText += $"📭 EMPTY TABLES ({summary.EmptyTables.Count})\n";
					foreach (var table in summary.EmptyTables.Take(5))
					{
						analysisText += $"  • {table}\n";
					}
					if (summary.EmptyTables.Count > 5)
						analysisText += $"  ... and {summary.EmptyTables.Count - 5} more\n";
					analysisText += "\n";
				}

				analysisText += $"💡 NEXT STEPS\n";
				analysisText += $"• Select a table to view data\n";
				analysisText += $"• Review smart recommendations\n";
				analysisText += $"• Execute custom queries\n";
				analysisText += $"• Explore data patterns\n";

				// Update UI on main thread
				if (InvokeRequired)
				{
					Invoke(new Action(() =>
					{
						if (rtbRecommendations != null)
						{
							rtbRecommendations.Clear();
							rtbRecommendations.AppendText(analysisText);
						}
					}));
				}
				else
				{
					rtbRecommendations.Clear();
					rtbRecommendations.AppendText(analysisText);
				}

				_logService.LogInfo("📈 Initial database analysis loaded");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to load initial database analysis", ex);
			}
		}

		/// <summary>
		/// Handles deployment failure when backup is available
		/// </summary>
		private async Task HandleDeploymentFailureWithBackup(string backupPath, DeploymentResult result)
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
						var connectionInfo = CreateConnectionInfo(_currentConfig);
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

		private async void BtnRefreshTables_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(_currentConfig.ServerName) || string.IsNullOrEmpty(_currentConfig.Database))
				{
					MessageBox.Show("Please configure database connection first.", "Connection Required",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					tabControl.SelectedTab = tabSetup;
					return;
				}

				btnRefreshTables.Enabled = false;
				progressQuery.Visible = true;
				progressQuery.Style = ProgressBarStyle.Marquee;

				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var tables = await _dataAnalysisService.GetTablesAsync(connectionInfo);

				cboTables.Items.Clear();
				foreach (var table in tables)
				{
					cboTables.Items.Add(table);
				}

				lblTableCount.Text = $"Tables: {tables.Count}";

				if (tables.Count > 0)
				{
					cboTables.SelectedIndex = 0;
					await LoadDatabaseSummary();
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh tables", ex);
				MessageBox.Show($"Failed to refresh tables: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				btnRefreshTables.Enabled = true;
				progressQuery.Visible = false;
			}
		}

		private async void BtnQueryTable_Click(object sender, EventArgs e)
		{
			if (cboTables.SelectedItem == null)
			{
				MessageBox.Show("Please select a table first.", "No Table Selected",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			await QuerySelectedTable();
		}

		private async void BtnExecuteQuery_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtCustomQuery.Text))
			{
				MessageBox.Show("Please enter a custom query.", "No Query Entered",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			await ExecuteCustomQuery(txtCustomQuery.Text.Trim());
		}

		private async void CboTables_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (cboTables.SelectedItem != null && !string.IsNullOrEmpty(cboTables.SelectedItem.ToString()))
				{
					var selectedTable = cboTables.SelectedItem.ToString();

					// Update custom query with selected table name
					UpdateCustomQueryWithTable(selectedTable);

					// Load recommendations for this table
					await LoadTableRecommendations(selectedTable);

					// Clear previous data
					dgvTableData.DataSource = null;
					lblRowCount.Text = "Rows: Click 'Query Table' to load data";
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error in table selection", ex);
			}
		}

		private async Task QuerySelectedTable()
		{
			try
			{
				if (cboTables.SelectedItem == null)
				{
					MessageBox.Show("Please select a table first.", "No Table Selected",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				var tableName = cboTables.SelectedItem.ToString();

				btnQueryTable.Enabled = false;
				progressQuery.Visible = true;
				progressQuery.Style = ProgressBarStyle.Marquee;

				_logService.LogInfo($"🔍 Querying table: {tableName}");

				var connectionInfo = CreateConnectionInfo(_currentConfig);

				// Use improved query with proper table name handling
				var dataTable = await QueryTableSafely(connectionInfo, tableName, 1000);

				if (dataTable != null)
				{
					dgvTableData.DataSource = dataTable;
					lblRowCount.Text = $"Rows: {dataTable.Rows.Count:N0}";

					// Auto-resize columns but limit max width
					foreach (DataGridViewColumn column in dgvTableData.Columns)
					{
						column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
						if (column.Width > 200)
						{
							column.Width = 200;
							column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
						}
					}

					_logService.LogInfo($"📊 Displayed {dataTable.Rows.Count} rows from table {tableName}");

					// Load recommendations for this table
					await LoadTableRecommendations(tableName);
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to query table", ex);
				MessageBox.Show($"Failed to query table: {ex.Message}", "Query Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				btnQueryTable.Enabled = true;
				progressQuery.Visible = false;
			}
		}

		private async Task<DataTable> QueryTableSafely(ConnectionInfo connectionInfo, string tableName, int maxRows)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					// Clean and validate table name
					string cleanTableName = tableName.Trim();

					// Handle different table name formats
					string query;
					if (cleanTableName.Contains("."))
					{
						// Already has schema.table format
						query = $"SELECT  * FROM [{cleanTableName}]";
					}
					else
					{
						// Add default schema
						query = $"SELECT * FROM [dbo].[{cleanTableName}]";
					}

					_logService.LogInfo($"🔍 Executing query: {query}");

					var dataTable = new DataTable();
					using (var adapter = new SqlDataAdapter(query, connection))
					{
						adapter.Fill(dataTable);
					}

					return dataTable;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to query table {tableName}", ex);

				// Try alternative query format if first attempt fails
				try
				{
					return await TryAlternativeTableQuery(connectionInfo, tableName, maxRows);
				}
				catch (Exception retryEx)
				{
					_logService.LogError($"Alternative query also failed for {tableName}", retryEx);
					throw new Exception($"Could not query table '{tableName}'. Please check if the table exists and you have permissions.");
				}
			}
		}

		// FIX 6: Add alternative query method for difficult table names
		private async Task<DataTable> TryAlternativeTableQuery(ConnectionInfo connectionInfo, string tableName, int maxRows)
		{
			using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
			{
				await connection.OpenAsync();

				// Strip any existing brackets and schema
				string cleanName = tableName.Replace("[", "").Replace("]", "");
				if (cleanName.Contains("."))
				{
					cleanName = cleanName.Split('.').Last();
				}

				// Try with QUOTENAME for safety
				string query = $"SELECT  * FROM {SqlHelper.QuoteName(cleanName)}";

				_logService.LogInfo($"🔄 Trying alternative query: {query}");

				var dataTable = new DataTable();
				using (var adapter = new SqlDataAdapter(query, connection))
				{
					adapter.Fill(dataTable);
				}

				return dataTable;
			}
		}

		// FIX 7: Add this SQL helper class for safe table name handling
		public static class SqlHelper
		{
			public static string QuoteName(string name)
			{
				if (string.IsNullOrEmpty(name))
					return "[]";

				// Remove any existing brackets
				name = name.Replace("[", "").Replace("]", "");

				// Add brackets for safety
				return $"[{name}]";
			}

			public static string QuoteNameWithSchema(string name, string schema = "dbo")
			{
				if (string.IsNullOrEmpty(name))
					return "[dbo].[]";

				if (name.Contains("."))
				{
					var parts = name.Split('.');
					return $"[{parts[0]}].[{parts[1]}]";
				}

				return $"[{schema}].[{name}]";
			}
		}

		// FIX 8: Update the GetTablesAsync method in DataAnalysisService.cs
		// Replace the query in GetTablesAsync with this improved version:

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
                    t.name as TableName,
                    SCHEMA_NAME(t.schema_id) as SchemaName
                FROM sys.tables t
                WHERE t.is_ms_shipped = 0
                ORDER BY SCHEMA_NAME(t.schema_id), t.name";

					using (var command = new SqlCommand(query, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var schemaName = reader["SchemaName"].ToString();
							var tableName = reader["TableName"].ToString();

							// Return in format: schema.table
							tables.Add($"{schemaName}.{tableName}");
						}
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


		private async Task ExecuteCustomQuery(string query)
		{
			try
			{
				// Validate query is not empty or placeholder
				if (string.IsNullOrWhiteSpace(query) ||
					query.Contains("Select a table first") ||
					query == "SELECT * FROM [TableName] WHERE...")
				{
					MessageBox.Show("Please enter a valid SQL query or select a table first.",
						"Invalid Query", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				progressQuery.Visible = true;
				progressQuery.Style = ProgressBarStyle.Marquee;

				_logService.LogInfo($"🔍 Executing custom query: {query}");

				var connectionInfo = CreateConnectionInfo(_currentConfig);

				// Use the safe query execution method
				var dataTable = await ExecuteQuerySafely(connectionInfo, query);

				if (dataTable != null)
				{
					dgvTableData.DataSource = dataTable;
					lblRowCount.Text = $"Rows: {dataTable.Rows.Count:N0}";

					// Auto-resize columns but limit max width
					foreach (DataGridViewColumn column in dgvTableData.Columns)
					{
						column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
						if (column.Width > 200)
						{
							column.Width = 200;
							column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
						}
					}

					// Clear table-specific recommendations for custom queries
					UpdateCustomQueryRecommendations(dataTable);

					_logService.LogInfo($"🔍 Custom query executed: {dataTable.Rows.Count} rows returned");
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Custom query execution failed", ex);

				string errorMessage = ex.Message;

				// Provide helpful error messages based on common issues
				if (errorMessage.Contains("Invalid object name"))
				{
					errorMessage = "Table not found. Please check the table name and try again.\n\n" +
								  "Tips:\n" +
								  "• Make sure the table exists in the current database\n" +
								  "• Use [schema].[table] format (e.g., [dbo].[TableName])\n" +
								  "• Select a table from the dropdown to auto-populate the query";
				}
				else if (errorMessage.Contains("permission"))
				{
					errorMessage = "Permission denied. You don't have access to this table or operation.";
				}

				MessageBox.Show($"Query execution failed:\n\n{errorMessage}", "Query Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				// Show error in recommendations panel
				ShowQueryError(ex.Message);
			}
			finally
			{
				progressQuery.Visible = false;
			}
		}

		private async Task<DataTable> ExecuteQuerySafely(ConnectionInfo connectionInfo, string query)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					// Clean up the query - handle common formatting issues
					string cleanQuery = CleanQuery(query);

					_logService.LogInfo($"🔧 Cleaned query: {cleanQuery}");

					var dataTable = new DataTable();
					using (var adapter = new SqlDataAdapter(cleanQuery, connection))
					{
						// Set a reasonable timeout
						adapter.SelectCommand.CommandTimeout = 30;
						adapter.Fill(dataTable);
					}

					return dataTable;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError($"Query execution failed: {query}", ex);
				throw;
			}
		}

		// Add this helper method to clean and fix common query issues
		private string CleanQuery(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return query;

			// Remove extra whitespace
			query = query.Trim();

			// Fix common bracket issues in table names
			// Pattern: [dbo BULKSurveyQueue] should be [dbo].[BULKSurveyQueue]
			if (query.Contains("[dbo ") && !query.Contains("[dbo]."))
			{
				// Fix pattern like [dbo TableName] to [dbo].[TableName]
				query = System.Text.RegularExpressions.Regex.Replace(
					query,
					@"\[dbo\s+([^\]]+)\]",
					"[dbo].[$1]"
				);
			}

			// Fix pattern like [schema.table] to [schema].[table]
			query = System.Text.RegularExpressions.Regex.Replace(
				query,
				@"\[([^.\]]+)\.([^\]]+)\]",
				"[$1].[$2]"
			);

			return query;
		}

		// Add this method to show helpful recommendations after custom query errors
		private void ShowQueryError(string errorMessage)
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke(new Action(() => ShowQueryError(errorMessage)));
					return;
				}

				rtbRecommendations.Clear();
				rtbRecommendations.AppendText("❌ Query Execution Error\n\n");
				rtbRecommendations.AppendText($"Error: {errorMessage}\n\n");

				rtbRecommendations.AppendText("🔧 Common Solutions:\n");
				rtbRecommendations.AppendText("• Check table name spelling\n");
				rtbRecommendations.AppendText("• Use format: [schema].[table]\n");
				rtbRecommendations.AppendText("• Select table from dropdown first\n");
				rtbRecommendations.AppendText("• Verify database permissions\n");
				rtbRecommendations.AppendText("• Check SQL syntax\n\n");

				rtbRecommendations.AppendText("💡 Quick Fix:\n");
				rtbRecommendations.AppendText("1. Select a table from dropdown\n");
				rtbRecommendations.AppendText("2. Click 'Query Table' button\n");
				rtbRecommendations.AppendText("3. Modify the auto-generated query\n");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error showing query error recommendations", ex);
			}
		}

		// Add this method to show recommendations after successful custom queries
		private void UpdateCustomQueryRecommendations(DataTable dataTable)
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke(new Action(() => UpdateCustomQueryRecommendations(dataTable)));
					return;
				}

				rtbRecommendations.Clear();
				rtbRecommendations.AppendText("🔍 Custom Query Results\n\n");
				rtbRecommendations.AppendText($"✅ Query executed successfully\n");
				rtbRecommendations.AppendText($"📊 Returned {dataTable.Rows.Count:N0} rows\n");
				rtbRecommendations.AppendText($"📋 {dataTable.Columns.Count} columns\n\n");

				// Show column information
				if (dataTable.Columns.Count > 0)
				{
					rtbRecommendations.AppendText("📋 Columns:\n");
					foreach (DataColumn column in dataTable.Columns.Cast<DataColumn>().Take(10))
					{
						string columnInfo = $"• {column.ColumnName}";
						if (column.DataType != typeof(string))
						{
							columnInfo += $" ({column.DataType.Name})";
						}
						rtbRecommendations.AppendText($"{columnInfo}\n");
					}

					if (dataTable.Columns.Count > 10)
					{
						rtbRecommendations.AppendText($"... and {dataTable.Columns.Count - 10} more columns\n");
					}
					rtbRecommendations.AppendText("\n");
				}

				// Performance recommendations
				if (dataTable.Rows.Count > 10000)
				{
					rtbRecommendations.AppendText("⚠️ Performance Notice:\n");
					rtbRecommendations.AppendText("Large result set detected\n");
					rtbRecommendations.AppendText("Consider adding WHERE clause\n");
					rtbRecommendations.AppendText("or using TOP/LIMIT\n\n");
				}

				if (dataTable.Rows.Count == 0)
				{
					rtbRecommendations.AppendText("📭 No Data Found:\n");
					rtbRecommendations.AppendText("• Check WHERE conditions\n");
					rtbRecommendations.AppendText("• Verify table has data\n");
					rtbRecommendations.AppendText("• Review JOIN conditions\n\n");
				}

				rtbRecommendations.AppendText($"⏱️ Executed at: {DateTime.Now:HH:mm:ss}");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error updating custom query recommendations", ex);
			}
		}

		// ALSO: Update the UpdateCustomQueryWithTable method to fix the bracket issue
		private void UpdateCustomQueryWithTable(string tableName)
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke(new Action(() => UpdateCustomQueryWithTable(tableName)));
					return;
				}

				// Clean table name - handle both "schema.table" and "table" formats
				string cleanTableName = tableName.Trim();

				// Format the table name correctly for SQL
				string formattedTableName;
				if (cleanTableName.Contains("."))
				{
					// Split schema and table name
					var parts = cleanTableName.Split('.');
					formattedTableName = $"[{parts[0]}].[{parts[1]}]";
				}
				else
				{
					// Add default schema
					formattedTableName = $"[dbo].[{cleanTableName}]";
				}

				// Update the custom query textbox
				txtCustomQuery.ReadOnly = false;
				txtCustomQuery.Text = $"SELECT TOP 100 * FROM {formattedTableName}";
				txtCustomQuery.ForeColor = Color.Black;

				_logService.LogInfo($"📝 Updated custom query for table: {tableName} -> {formattedTableName}");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error updating custom query", ex);
			}
		}
		private async Task LoadTableRecommendations(string tableName)
		{
			try
			{
				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var recommendations = await _dataAnalysisService.AnalyzeTableDataAsync(connectionInfo, tableName);

				rtbRecommendations.Clear();
				rtbRecommendations.AppendText($"🎯 Analysis: {tableName}\n");
				rtbRecommendations.AppendText($"Generated: {DateTime.Now:HH:mm:ss}\n\n");

				if (recommendations.Count == 0)
				{
					rtbRecommendations.AppendText("✅ No issues detected!\n");
					rtbRecommendations.AppendText("This table appears to be well-configured.\n");
				}
				else
				{
					// Group recommendations by severity
					var highSeverity = recommendations.Where(r => r.Severity == "High").ToList();
					var mediumSeverity = recommendations.Where(r => r.Severity == "Medium").ToList();
					var lowSeverity = recommendations.Where(r => r.Severity == "Low").ToList();

					if (highSeverity.Any())
					{
						rtbRecommendations.AppendText("🔴 HIGH PRIORITY\n");
						foreach (var rec in highSeverity)
						{
							rtbRecommendations.AppendText($"{rec.Icon} {rec.Title}\n");
							rtbRecommendations.AppendText($"   {rec.Description}\n");
							rtbRecommendations.AppendText($"   Action: {rec.Action}\n\n");
						}
					}

					if (mediumSeverity.Any())
					{
						rtbRecommendations.AppendText("🟡 MEDIUM PRIORITY\n");
						foreach (var rec in mediumSeverity)
						{
							rtbRecommendations.AppendText($"{rec.Icon} {rec.Title}\n");
							rtbRecommendations.AppendText($"   {rec.Description}\n");
							rtbRecommendations.AppendText($"   Action: {rec.Action}\n\n");
						}
					}

					if (lowSeverity.Any())
					{
						rtbRecommendations.AppendText("🟢 LOW PRIORITY\n");
						foreach (var rec in lowSeverity)
						{
							rtbRecommendations.AppendText($"{rec.Icon} {rec.Title}\n");
							rtbRecommendations.AppendText($"   {rec.Description}\n\n");
						}
					}
				}

				_logService.LogInfo($"🎯 Loaded {recommendations.Count} recommendations for {tableName}");
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to load recommendations for {tableName}", ex);
				rtbRecommendations.Clear();
				rtbRecommendations.AppendText("❌ Analysis Failed\n\n");
				rtbRecommendations.AppendText($"Could not analyze table: {ex.Message}");
			}
		}

		private async Task LoadDatabaseSummary()
		{
			try
			{
				var connectionInfo = CreateConnectionInfo(_currentConfig);
				var summary = await _dataAnalysisService.GetDatabaseSummaryAsync(connectionInfo);

				// Add summary to recommendations panel when no table is selected
				if (cboTables.SelectedItem == null && summary.TotalTables > 0)
				{
					rtbRecommendations.Clear();
					rtbRecommendations.AppendText("📊 Database Overview\n\n");
					rtbRecommendations.AppendText($"📋 Total Tables: {summary.TotalTables:N0}\n");
					rtbRecommendations.AppendText($"📊 Total Rows: {summary.TotalRows:N0}\n\n");

					if (summary.LargestTables.Any())
					{
						rtbRecommendations.AppendText("🏆 Largest Tables:\n");
						foreach (var table in summary.LargestTables.Take(5))
						{
							rtbRecommendations.AppendText($"  • {table}\n");
						}
						rtbRecommendations.AppendText("\n");
					}

					if (summary.EmptyTables.Any())
					{
						rtbRecommendations.AppendText($"📭 Empty Tables ({summary.EmptyTables.Count}):\n");
						foreach (var table in summary.EmptyTables.Take(10))
						{
							rtbRecommendations.AppendText($"  • {table}\n");
						}
						if (summary.EmptyTables.Count > 10)
							rtbRecommendations.AppendText($"  ... and {summary.EmptyTables.Count - 10} more\n");
					}

					rtbRecommendations.AppendText($"\n🕒 Last Updated: {summary.LastAnalyzed:HH:mm:ss}");
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to load database summary", ex);
			}
		}

		#endregion


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



		// Event handler for second DACPAC browse
		private void btnBrowseDacpac2_Click(object sender, EventArgs e)
		{
			if (openDacpacDialog2.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath2.Text = openDacpacDialog2.FileName;
				_secondDacpacPath = openDacpacDialog2.FileName;
			}
		}


		// Event handler for second database refresh
		private async void btnRefreshDatabases2_Click(object sender, EventArgs e)
		{
			await RefreshDatabases2Async();
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

			//if (enabled)
			//{
			// InitializeProcedureManagementUI();

			// // Update the stored procedures group title to show it affects strategy
			// grpStoredProcedures.Text = "Stored Procedures (Smart Strategy)";

			// // Add tooltip explaining the strategy
			// toolTip.SetToolTip(grpStoredProcedures,
			//  "Procedures will be deployed based on the selected strategy:\n" +
			//  "• All Procedures: Deploy to primary database\n" +
			//  "• No Procedures: Skip procedures for secondary\n" +
			//  "• Minimal Setup: Deploy only setup procedures");
			//}
			//else
			//{
			// // Hide procedure strategy controls
			// if (lblProcedureStrategy != null && cboProcedureStrategy != null)
			// {
			//  lblProcedureStrategy.Visible = false;
			//  cboProcedureStrategy.Visible = false;
			// }

			// // Reset the stored procedures group title
			// grpStoredProcedures.Text = "Stored Procedures";
			// toolTip.SetToolTip(grpStoredProcedures, null);
			//}
		}
		// ADD THIS METHOD to DacpacPublisherForm.cs

		private bool IsSuccessWithMinorIssues(DeploymentResult result)
		{
			if (result.Success) return true;

			// Use the enhanced categorization from DeploymentConfiguration
			var category = DeploymentConfiguration.CategorizeResult(result.Errors, result.Warnings);

			return category == DeploymentResultCategory.SuccessWithIgnorableErrors ||
				   category == DeploymentResultCategory.SuccessWithWarnings;
		}

		/// <summary>
		/// Check if result has critical issues that would prevent normal operation
		/// </summary>
		private bool HasCriticalIssues(DeploymentResult result)
		{
			if (!result.HasErrors && !result.HasWarnings) return false;

			var allIssues = new List<string>();
			if (result.Errors != null) allIssues.AddRange(result.Errors);
			if (result.Warnings != null) allIssues.AddRange(result.Warnings);

			var criticalKeywords = new[]
			{
		"login failed", "access denied", "database does not exist",
		"cannot connect", "dacpac", "critical", "fatal"
	};

			return allIssues.Any(issue =>
				criticalKeywords.Any(keyword =>
					issue.ToLower().Contains(keyword.ToLower())));
		}

		/// <summary>
		/// Determine if a failure is recoverable (mostly auto-generated procedure issues)
		/// </summary>
		private bool IsRecoverableFailure(DeploymentResult result)
		{
			if (!result.HasErrors) return true;

			var categorizedErrors = CategorizeErrors(result.Errors);

			// Recoverable if most errors are auto-generated procedure issues
			var totalErrors = result.Errors.Count;
			var recoverableErrors = categorizedErrors["AutoGenerated"].Count;

			return recoverableErrors > 0 && (recoverableErrors >= totalErrors * 0.7); // 70% or more are recoverable
		}

		/// <summary>
		/// ENHANCED: Configuration validation with auto-generated procedure awareness
		/// </summary>
		private async Task<bool> ValidateConfigurationAsync()
		{
			var errors = new List<string>();

			try
			{
				// Basic validation with null checks
				if (string.IsNullOrWhiteSpace(txtServerName?.Text))
					errors.Add("Server Name is required");

				if (string.IsNullOrWhiteSpace(cboDatabases?.Text))
					errors.Add("Target Database must be selected");

				if (string.IsNullOrWhiteSpace(txtDacpacPath?.Text))
					errors.Add("DACPAC file path is required");
				else if (!File.Exists(txtDacpacPath.Text.Trim()))
					errors.Add($"DACPAC file not found: {txtDacpacPath.Text}");

				// Authentication validation
				if (!(chkWindowsAuth?.Checked ?? false))
				{
					if (string.IsNullOrWhiteSpace(txtUsername?.Text))
						errors.Add("Username is required for SQL authentication");
					if (string.IsNullOrWhiteSpace(txtPassword?.Text))
						errors.Add("Password is required for SQL authentication");
				}

				// SQL Agent Jobs validation
				if (chkCreateSqlAgentJobs?.Checked == true)
				{
					if (string.IsNullOrWhiteSpace(txtJobOwnerLoginName?.Text))
						errors.Add("Job Owner Login Name is required when creating SQL Agent Jobs");

					if (string.IsNullOrWhiteSpace(txtJobScriptsFolder?.Text))
						errors.Add("Job Scripts Folder is required when creating SQL Agent Jobs");
					else if (!Directory.Exists(txtJobScriptsFolder.Text.Trim()))
						errors.Add($"Job Scripts Folder not found: {txtJobScriptsFolder.Text}");
				}

				// Synonyms validation
				if (chkCreateSynonyms?.Checked == true)
				{
					var databases = GetAllDeploymentDatabasesForDisplay();

					var hiveCFMDatabases = databases
						.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
						.ToList();
					if (!hiveCFMDatabases.Any())
					{
						var continueChoice = MessageBox.Show(
							"⚠️ Automatic Synonym Detection Warning:\n\n" +
							"No databases containing 'HiveCFMSurvey' found in deployment targets.\n\n" +
							"Synonym creation will be skipped during deployment.\n" +
							"This is normal if your databases use different naming.\n\n" +
							"Continue deployment anyway?",
							"No HiveCFMSurvey Databases Found",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning);

						if (continueChoice == DialogResult.No)
						{
							return false;
						}
					}
					else
					{
						// Show what will happen
						var confirmMessage = $"✅ Automatic Synonym Detection Summary:\n\n" +
										   $"Found HiveCFMSurvey database(s): {string.Join(", ", hiveCFMDatabases)}\n\n" +
										   $"The system will:\n" +
										   $"• Auto-detect the source database (with CFMSurveyUser table)\n" +
										   $"• Create synonyms in databases that need them\n" +
										   $"• Skip databases that don't need synonyms\n" +
										   $"• Show detailed logs during deployment\n\n" +
										   $"Continue with automatic synonym creation?";

						var confirmChoice = MessageBox.Show(confirmMessage,
							"Confirm Automatic Synonym Creation",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question);

						if (confirmChoice == DialogResult.No)
						{
							return false;
						}
					}
				}

				// ENHANCED: Smart Procedures validation with auto-generated awareness
				if (chkExecuteProcedures?.Checked == true)
				{
					if (_currentConfig?.SmartProcedures?.Any() != true)
					{
						// Provide helpful guidance instead of hard error
						var addProceduresChoice = MessageBox.Show(
							"No procedures configured for execution.\n\n" +
							"Would you like to:\n" +
							"• Yes: Continue deployment without procedures\n" +
							"• No: Configure procedures first\n" +
							"• Cancel: Abort deployment",
							"No Procedures Configured",
							MessageBoxButtons.YesNoCancel,
							MessageBoxIcon.Question);

						switch (addProceduresChoice)
						{
							case DialogResult.Yes:
								// User wants to continue without procedures
								chkExecuteProcedures.Checked = false;
								_logService.LogInfo("User chose to continue deployment without procedures");
								break;
							case DialogResult.No:
								// User wants to configure procedures
								errors.Add("Please configure procedures using 'Configure Smart Procedures' button");
								break;
							case DialogResult.Cancel:
								// User wants to abort
								return false;
						}
					}
					else
					{
						try
						{
							var smartErrors = await ValidateSmartProceduresAsync();
							if (smartErrors?.Any() == true)
							{
								// Filter out auto-generated procedure warnings
								var criticalErrors = smartErrors.Where(e =>
									!IsAutoGeneratedProcedureError(e)).ToList();

								if (criticalErrors.Any())
								{
									errors.AddRange(criticalErrors);
								}
								else if (smartErrors.Count > criticalErrors.Count)
								{
									_logService.LogInfo($"Found {smartErrors.Count - criticalErrors.Count} auto-generated procedure references (will be handled automatically)");
								}
							}
						}
						catch (Exception ex)
						{
							errors.Add($"Smart procedures validation failed: {ex.Message}");
							_logService?.LogError("Smart procedures validation error", ex);
						}
					}
				}

				// Multiple databases validation
				var chkMultipleDatabases = FindControlByName("chkEnableMultipleDatabases") as CheckBox;
				if (chkMultipleDatabases?.Checked == true)
				{
					var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
					if (string.IsNullOrWhiteSpace(cboDatabases2?.Text))
					{
						errors.Add("Secondary database must be selected when multiple databases is enabled");
					}

					var txtDacpacPath2 = FindControlByName("txtDacpacPath2") as TextBox;
					if (!string.IsNullOrWhiteSpace(txtDacpacPath2?.Text) && !File.Exists(txtDacpacPath2.Text.Trim()))
					{
						errors.Add($"Secondary DACPAC file not found: {txtDacpacPath2.Text}");
					}
				}

				if (errors.Any())
				{
					string errorMessage = "Please fix the following issues:\n\n" +
										string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
					MessageBox.Show(errorMessage, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}

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

		private void CreateSynonymInfoLabel()
		{
			try
			{
				// Find the synonyms group box
				var synonymGroup = FindControlByName("grpSynonyms") as GroupBox;
				if (synonymGroup == null) return;

				// Create info label
				var infoLabel = new Label
				{
					Name = "lblSynonymInfo",
					Text = "🤖 Automatic Detection: Will auto-detect HiveCFMSurvey databases",
					ForeColor = Color.Green,
					AutoSize = true,
					Visible = false
				};

				// Position it where the source database field was
				if (lblSynonymSourceDb != null)
				{
					infoLabel.Location = new Point(lblSynonymSourceDb.Location.X, lblSynonymSourceDb.Location.Y);
				}

				synonymGroup.Controls.Add(infoLabel);

				// Add detailed tooltip
				if (toolTip != null)
				{
					toolTip.SetToolTip(infoLabel,
						"AUTOMATIC SYNONYM DETECTION:\n" +
						"✅ Finds databases containing 'HiveCFMSurvey' in the name\n" +
						"✅ Auto-selects the one with CFMSurveyUser table as source\n" +
						"✅ Creates synonyms in other databases that need them\n" +
						"✅ Skips databases that don't need synonyms\n" +
						"✅ Shows warnings if no suitable source found\n\n" +
						"Example: HiveCFMSurveyDB → HiveCFMAppDB\n" +
						"Result: [HiveCFMAppDB].[dbo].[CFMSurveyUser] → [HiveCFMSurveyDB].[dbo].[CFMSurveyUser]");
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error creating synonym info label", ex);
			}
		}

		// 3. NEW: Show/hide automatic synonym information
		private void ShowSynonymAutomaticInfo(bool show)
		{
			try
			{
				var infoLabel = FindControlByName("lblSynonymInfo") as Label;
				if (infoLabel != null)
				{
					infoLabel.Visible = show;

					if (show)
					{
						// Update text based on current database configuration
						var databases = GetAllDeploymentDatabasesForDisplay();

						var hiveCFMDatabases = databases
							.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
							.ToList();
						if (hiveCFMDatabases.Any())
						{
							infoLabel.Text = $"🤖 Auto-Detection: Found {hiveCFMDatabases.Count} HiveCFMSurvey database(s): {string.Join(", ", hiveCFMDatabases)}";
							infoLabel.ForeColor = Color.Green;
						}
						else
						{
							infoLabel.Text = "⚠️ Auto-Detection: No HiveCFMSurvey databases found in current targets";
							infoLabel.ForeColor = Color.Orange;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error updating synonym info display", ex);
			}
		}

		// 4. NEW: Get databases for display purposes
		private List<string> GetAllDeploymentDatabasesForDisplay()
		{
			var databases = new List<string>();

			try
			{
				if (!string.IsNullOrEmpty(cboDatabases?.Text))
				{
					databases.Add(cboDatabases.Text);
				}

				// Check for secondary database
				var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
				if (cboDatabases2?.Text != null && !string.IsNullOrEmpty(cboDatabases2.Text))
				{
					if (!databases.Contains(cboDatabases2.Text))
					{
						databases.Add(cboDatabases2.Text);
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error getting deployment databases for display", ex);
			}

			return databases;
		}

		private void ShowSynonymTargetSelection(bool show)
		{
			try
			{
				// Create controls if they don't exist
				if (lstSynonymTargetDatabases == null)
				{
					CreateSynonymTargetControls();
				}

				// Show/hide the controls
				chkShowSynonymTargets.Visible = show;
				lblSynonymTargets.Visible = show;
				lstSynonymTargetDatabases.Visible = show;
				btnRefreshSynonymTargets.Visible = show;

				if (show)
				{
					// Position controls nicely
					PositionSynonymTargetControls();
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error showing synonym target selection", ex);
			}
		}

		// NEW: Create synonym target selection controls
		private void CreateSynonymTargetControls()
		{
			try
			{
				// Find the synonyms group box
				var synonymGroup = FindControlByName("grpSynonyms") as GroupBox;
				if (synonymGroup == null) return;

				// Create target selection label
				lblSynonymTargets = new Label
				{
					Name = "lblSynonymTargets",
					Text = "🎯 Target Databases (where synonyms will be created):",
					AutoSize = true,
					ForeColor = Color.DarkBlue,
					Font = new Font(this.Font, FontStyle.Bold)
				};

				// Create target databases list
				lstSynonymTargetDatabases = new ListBox
				{
					Name = "lstSynonymTargetDatabases",
					SelectionMode = SelectionMode.MultiExtended,
					Height = 80,
					Width = 300,
					BackColor = Color.LightCyan,
					BorderStyle = BorderStyle.FixedSingle
				};

				// Create refresh button
				btnRefreshSynonymTargets = new Button
				{
					Name = "btnRefreshSynonymTargets",
					Text = "🔄 Refresh",
					Width = 80,
					Height = 25,
					BackColor = Color.LightBlue
				};

				// Create toggle for advanced selection
				chkShowSynonymTargets = new CheckBox
				{
					Name = "chkShowSynonymTargets",
					Text = "📋 Show target database selection (auto-selects HiveCFMApp databases)",
					AutoSize = true,
					Checked = true,
					ForeColor = Color.DarkGreen
				};

				// Add event handlers
				btnRefreshSynonymTargets.Click += async (s, e) => await LoadSynonymTargetDatabasesAsync();
				chkShowSynonymTargets.CheckedChanged += (s, e) =>
				{
					bool showList = chkShowSynonymTargets.Checked;
					lblSynonymTargets.Visible = showList;
					lstSynonymTargetDatabases.Visible = showList;
					btnRefreshSynonymTargets.Visible = showList;
				};

				// Add to synonyms group
				synonymGroup.Controls.Add(chkShowSynonymTargets);
				synonymGroup.Controls.Add(lblSynonymTargets);
				synonymGroup.Controls.Add(lstSynonymTargetDatabases);
				synonymGroup.Controls.Add(btnRefreshSynonymTargets);

				// Add tooltips
				if (toolTip != null)
				{
					toolTip.SetToolTip(lstSynonymTargetDatabases,
						"Select which databases should receive CFMSurveyUser synonyms.\n" +
						"Typically: HiveCFMApp databases get synonyms, HiveCFMSurvey databases don't.\n" +
						"Hold Ctrl to select multiple databases.");

					toolTip.SetToolTip(btnRefreshSynonymTargets,
						"Refresh the list of available databases for synonym creation");
				}

				_logService.LogInfo("✅ Synonym target selection controls created");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error creating synonym target controls", ex);
			}
		}

		// NEW: Position synonym target controls
		private void PositionSynonymTargetControls()
		{
			try
			{
				var synonymGroup = FindControlByName("grpSynonyms") as GroupBox;
				if (synonymGroup == null) return;

				int yOffset = 60; // Start below the "Create Synonyms" checkbox

				// Position toggle
				chkShowSynonymTargets.Location = new Point(10, yOffset);
				yOffset += 25;

				// Position label
				lblSynonymTargets.Location = new Point(10, yOffset);
				yOffset += 20;

				// Position list box and refresh button side by side
				lstSynonymTargetDatabases.Location = new Point(10, yOffset);
				btnRefreshSynonymTargets.Location = new Point(320, yOffset);

				// Expand group box height if needed
				int requiredHeight = yOffset + lstSynonymTargetDatabases.Height + 20;
				if (synonymGroup.Height < requiredHeight)
				{
					synonymGroup.Height = requiredHeight;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error positioning synonym target controls", ex);
			}
		}

		// NEW: Load available databases for synonym targeting
		private async Task LoadSynonymTargetDatabasesAsync()
		{
			try
			{
				if (lstSynonymTargetDatabases == null) return;

				_logService.LogInfo("🔄 Loading databases for synonym targeting...");

				UpdateConfigurationFromUI();
				var connectionInfo = CreateConnectionInfo(_currentConfig);

				// Test connection first
				bool canConnect = await _connectionService.TestConnectionAsync(connectionInfo);
				if (!canConnect)
				{
					lstSynonymTargetDatabases.Items.Clear();
					lstSynonymTargetDatabases.Items.Add("❌ Cannot connect to server");
					return;
				}

				// Get all databases
				var allDatabases = await _connectionService.GetDatabasesAsync(connectionInfo);

				lstSynonymTargetDatabases.Items.Clear();

				// Categorize databases for better user understanding
				var hiveCFMAppDatabases = new List<string>();
				var hiveCFMSurveyDatabases = new List<string>();
				var otherDatabases = new List<string>();

				foreach (var db in allDatabases.OrderBy(d => d))
				{
					if (db.IndexOf("HiveCFMApp", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						hiveCFMAppDatabases.Add(db);
					}
					else if (db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						hiveCFMSurveyDatabases.Add(db);
					}
					else if (db.IndexOf("CFM", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						otherDatabases.Add(db);
					}
				}

				// Add databases with visual indicators
				if (hiveCFMAppDatabases.Any())
				{
					lstSynonymTargetDatabases.Items.Add("🎯 RECOMMENDED (HiveCFMApp databases):");
					foreach (var db in hiveCFMAppDatabases)
					{
						lstSynonymTargetDatabases.Items.Add($"  ✅ {db}");
					}
				}

				if (hiveCFMSurveyDatabases.Any())
				{
					lstSynonymTargetDatabases.Items.Add("⚠️ SOURCE CANDIDATES (HiveCFMSurvey - usually don't need synonyms):");
					foreach (var db in hiveCFMSurveyDatabases)
					{
						lstSynonymTargetDatabases.Items.Add($"  📋 {db}");
					}
				}

				if (otherDatabases.Any())
				{
					lstSynonymTargetDatabases.Items.Add("📁 OTHER CFM DATABASES:");
					foreach (var db in otherDatabases)
					{
						lstSynonymTargetDatabases.Items.Add($"  📁 {db}");
					}
				}

				_logService.LogInfo($"📊 Loaded {allDatabases.Count} databases for synonym targeting");

				// Auto-select HiveCFMApp databases
				AutoSelectHiveCFMAppDatabases();
			}
			catch (Exception ex)
			{
				_logService.LogError("Error loading synonym target databases", ex);
				if (lstSynonymTargetDatabases != null)
				{
					lstSynonymTargetDatabases.Items.Clear();
					lstSynonymTargetDatabases.Items.Add($"❌ Error: {ex.Message}");
				}
			}
		}



		// NEW: Get selected synonym target databases
		private List<string> GetSelectedSynonymTargetDatabases()
		{
			var selectedDatabases = new List<string>();

			try
			{
				if (lstSynonymTargetDatabases?.SelectedItems != null)
				{
					foreach (var item in lstSynonymTargetDatabases.SelectedItems)
					{
						var itemText = item.ToString();

						// Extract database name (remove prefixes like "  ✅ ")
						if (itemText.StartsWith("  ✅") || itemText.StartsWith("  📋") || itemText.StartsWith("  📁"))
						{
							var dbName = itemText.Substring(4).Trim();
							selectedDatabases.Add(dbName);
						}
					}
				}

				_logService.LogInfo($"📋 Selected {selectedDatabases.Count} databases for synonym creation: {string.Join(", ", selectedDatabases)}");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error getting selected synonym target databases", ex);
			}

			return selectedDatabases;
		}

		// NEW: Clear synonym targets
		private void ClearSynonymTargets()
		{
			try
			{
				if (lstSynonymTargetDatabases != null)
				{
					lstSynonymTargetDatabases.Items.Clear();
					lstSynonymTargetDatabases.ClearSelected();
				}
				_logService.LogInfo("🧹 Cleared synonym target selection");
			}
			catch (Exception ex)
			{
				_logService.LogError("Error clearing synonym targets", ex);
			}
		}
		private void AutoSelectHiveCFMAppDatabases()
		{
			try
			{
				if (clbSynonymTargets == null) return;

				int selectedCount = 0;

				for (int i = 0; i < clbSynonymTargets.Items.Count; i++)
				{
					var item = clbSynonymTargets.Items[i].ToString();

					// Check items that start with ✅ (HiveCFMApp databases)
					if (item.StartsWith("✅"))
					{
						clbSynonymTargets.SetItemChecked(i, true);
						selectedCount++;
						_logService.LogInfo($"🎯 Auto-selected: {item.Substring(2)}");
					}
					// Uncheck source databases and headers
					else
					{
						clbSynonymTargets.SetItemChecked(i, false);
					}
				}

				if (selectedCount > 0)
				{
					_logService.LogInfo($"✅ Auto-selected {selectedCount} HiveCFMApp database(s) for synonym creation");
				}
				else
				{
					_logService.LogWarning("⚠️ No HiveCFMApp databases found for auto-selection");
				}
			}
			catch (Exception ex)
			{
				_logService.LogError("Error auto-selecting HiveCFMApp databases", ex);
			}
		}

		private async Task PopulateSynonymTargetDatabases()
		{
			try
			{
				if (InvokeRequired)
				{
					// Fix: Use proper delegate without async/await inside BeginInvoke
					BeginInvoke(new Action(() => PopulateSynonymTargetDatabases().Wait()));
					return;
				}

				if (clbSynonymTargets == null) return;

				_logService.LogInfo("🔄 Loading databases for synonym targeting...");

				clbSynonymTargets.Items.Clear();

				// Get all deployment databases
				var databases = new List<string>();

				// Primary database
				if (!string.IsNullOrEmpty(cboDatabases?.Text))
				{
					databases.Add(cboDatabases.Text);
				}

				// Secondary database if enabled
				var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
				if (chkEnableMultipleDatabases?.Checked == true && !string.IsNullOrEmpty(cboDatabases2?.Text))
				{
					if (!databases.Contains(cboDatabases2.Text))
					{
						databases.Add(cboDatabases2.Text);
					}
				}

				// Also try to get all databases from server
				try
				{
					UpdateConfigurationFromUI();
					var connectionInfo = CreateConnectionInfo(_currentConfig);

					if (await _connectionService.TestConnectionAsync(connectionInfo))
					{
						var allServerDatabases = await _connectionService.GetDatabasesAsync(connectionInfo);

						// Add CFM-related databases that aren't in deployment
						foreach (var db in allServerDatabases)
						{
							if (db.Contains("CFM") && !databases.Contains(db))
							{
								databases.Add(db);
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logService.LogWarning($"Could not retrieve all server databases: {ex.Message}");
				}

				// Categorize and add databases
				var hiveCFMAppDatabases = new List<string>();
				var hiveCFMSurveyDatabases = new List<string>();
				var otherCFMDatabases = new List<string>();

				foreach (var db in databases.OrderBy(d => d))
				{
					if (db.IndexOf("HiveCFMApp", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						hiveCFMAppDatabases.Add(db);
					}
					else if (db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						hiveCFMSurveyDatabases.Add(db);
					}
					else if (db.IndexOf("CFM", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						otherCFMDatabases.Add(db);
					}
				}

				// Add databases with visual indicators
				if (hiveCFMAppDatabases.Any())
				{
					clbSynonymTargets.Items.Add("=== RECOMMENDED TARGETS (HiveCFMApp) ===", false);
					foreach (var db in hiveCFMAppDatabases)
					{
						clbSynonymTargets.Items.Add($"✅ {db}", true); // Auto-check these
					}
				}

				if (hiveCFMSurveyDatabases.Any())
				{
					clbSynonymTargets.Items.Add("=== SOURCE DATABASES (HiveCFMSurvey) ===", false);
					foreach (var db in hiveCFMSurveyDatabases)
					{
						clbSynonymTargets.Items.Add($"📋 {db}", false); // Don't check these
					}
				}

				if (otherCFMDatabases.Any())
				{
					clbSynonymTargets.Items.Add("=== OTHER CFM DATABASES ===", false);
					foreach (var db in otherCFMDatabases)
					{
						clbSynonymTargets.Items.Add($"📁 {db}", false);
					}
				}

				_logService.LogInfo($"📊 Loaded {databases.Count} databases for synonym configuration");

				// Auto-select after population
				AutoSelectHiveCFMAppDatabases();
			}
			catch (Exception ex)
			{
				_logService.LogError("Error populating synonym target databases", ex);
				if (clbSynonymTargets != null)
				{
					clbSynonymTargets.Items.Clear();
					clbSynonymTargets.Items.Add($"❌ Error: {ex.Message}");
				}
			}
		}
	}
}