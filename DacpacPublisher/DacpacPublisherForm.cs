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
using System.ComponentModel;

namespace DacpacPublisher
{
	public partial class DacpacPublisherForm : Form
	{
		private bool IsInDesignMode
		{
			get { return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
		}
		private DeploymentController _deploymentController;
		private ValidationController _validationController;
		private ConfigurationController _configurationController;
		private DatabaseController _databaseController;
		// Services
		public readonly ILogService _logService;
		public readonly IConnectionService _connectionService;
		public readonly IDeploymentService _deploymentService;
		public readonly IConfigurationService _configurationService;
		public readonly IBackupService _backupService;
		public readonly IDataAnalysisService _dataAnalysisService;
		public readonly ISynonymService _synonymService;
		// Configuration
		public PublisherConfiguration _currentConfig;

		// Job script validation timer
		public System.Windows.Forms.Timer _jobScriptValidationTimer;

		// Private fields for multiple database support
		public bool _isInitializing = true;
		public bool IsValidating = false;

		public DacpacPublisherForm()
		{
			InitializeComponent();
			if (IsInDesignMode) return;

			try
			{
				// Initialize services with proper dependency injection
				_logService = new LogService();
				_connectionService = new ConnectionService(_logService);
				_deploymentService = new DeploymentService(_connectionService, _logService,_currentConfig);
				_configurationService = new ConfigurationService(_logService);
				_backupService = new BackupService(_logService);

				// ADD THIS - Initialize missing services
				_synonymService = new SynonymService(_connectionService, _logService);
				_dataAnalysisService = new DataAnalysisService(_connectionService, _logService);

				// Initialize configuration
				_currentConfig = new PublisherConfiguration();

				// Subscribe to events
				_logService.MessageLogged += OnLogMessageReceived;
				_deploymentService.ProgressChanged += OnProgressChanged;
				_synonymService.LogMessageReceived += OnLogMessageReceived; 
				// ADD THIS
				_deploymentController = new DeploymentController(this, _deploymentService, _logService, _connectionService, _backupService);
				_validationController = new ValidationController(this, _logService);
				_configurationController = new ConfigurationController(this, _logService);
				_databaseController = new DatabaseController(this, _connectionService, _logService);

				InitializeTooltips();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error initializing application: {ex.Message}",
					"Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		private void InitializeTooltips()
		{
			toolTip.SetToolTip(grpSmartProcedures,
				"Smart procedures allow you to specify which databases each procedure runs on, " +
				"set execution order, and organize by categories for better deployment control.");

			toolTip.SetToolTip(btnTestConnection, "Test the database connection with current settings");
			toolTip.SetToolTip(btnRefreshDatabases, "Refresh the list of available databases");
			toolTip.SetToolTip(btnPublish, "Start the deployment process");
			toolTip.SetToolTip(txtJobScriptsFolder, "Folder containing SQL Agent job scripts (.sql files)");
			toolTip.SetToolTip(btnConfigureSmartProcedures, "Configure procedures for smart deployment across multiple databases");
			toolTip.SetToolTip(chkCreateBackup, "Create a backup before deployment for safety");
		}
		// OPTIONAL: Data Viewer Tab Initialization (comment out if not needed)

		private void DacpacPublisherForm_Load(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
				if (_logService != null)
					_logService.LogInfo("🚀 DACPAC Publisher started");

				// Set default values
				if (txtServerName != null) txtServerName.Text = "(local)";
				if (chkWindowsAuth != null) chkWindowsAuth.Checked = true;
				if (chkCreateBackup != null) chkCreateBackup.Checked = true;

				// Apply initial state
				UpdateAuthenticationControls();
				UpdateStatus("Ready", false);
				InitializeDataViewerTab();
				// Set focus to server name
				if (txtServerName != null) txtServerName.Focus();

				_isInitializing = false;
			}
			catch (Exception ex)
			{
				if (_logService != null)
					_logService.LogError("Form load error", ex);
				HandleError("Form Load Error", ex);
			}
		}
		private void InitializeDataViewerTab()
		{
			try
			{
				// Clear any existing controls from the tab
				this.tabDataViewer.Controls.Clear();

				// Create main container panel
				Panel mainPanel = new Panel
				{
					Dock = DockStyle.Fill,
					Padding = new Padding(15),
					BackColor = Color.FromArgb(250, 250, 250)
				};

				// Create toolbar panel at the top
				Panel toolbarPanel = new Panel
				{
					Height = 50,
					Dock = DockStyle.Top,
					BackColor = Color.White,
					BorderStyle = BorderStyle.FixedSingle
				};

				// Table selection label
				Label lblSelectTable = new Label
				{
					Text = "📊 Select Table:",
					Location = new Point(15, 15),
					Size = new Size(100, 20),
					Font = new Font("Segoe UI", 9F, FontStyle.Bold),
					ForeColor = Color.FromArgb(66, 66, 66)
				};

				// Reinitialize the combo box with proper settings
				this.cboTables = new ComboBox
				{
					Location = new Point(120, 12),
					Size = new Size(200, 25),
					DropDownStyle = ComboBoxStyle.DropDownList,
					Font = new Font("Segoe UI", 9F)
				};

				// Reinitialize refresh button
				this.btnRefreshTables = new Button
				{
					Text = "🔄 Refresh",
					Location = new Point(330, 10),
					Size = new Size(80, 30),
					BackColor = Color.FromArgb(46, 125, 50),
					ForeColor = Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9F)
				};
				this.btnRefreshTables.FlatAppearance.BorderSize = 0;

				// Reinitialize query button
				this.btnQueryTable = new Button
				{
					Text = "📋 Query Table",
					Location = new Point(420, 10),
					Size = new Size(100, 30),
					BackColor = Color.FromArgb(25, 118, 210),
					ForeColor = Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9F)
				};
				this.btnQueryTable.FlatAppearance.BorderSize = 0;

				// Status labels
				this.lblTableCount = new Label
				{
					Text = "Tables: 0",
					Location = new Point(540, 15),
					Size = new Size(80, 20),
					Font = new Font("Segoe UI", 8F),
					ForeColor = Color.FromArgb(97, 97, 97)
				};

				this.lblRowCount = new Label
				{
					Text = "Rows: 0",
					Location = new Point(630, 15),
					Size = new Size(80, 20),
					Font = new Font("Segoe UI", 8F),
					ForeColor = Color.FromArgb(97, 97, 97)
				};

				// Progress bar
				this.progressQuery = new ProgressBar
				{
					Location = new Point(720, 15),
					Size = new Size(150, 20),
					Visible = false
				};

				// Add controls to toolbar
				toolbarPanel.Controls.AddRange(new Control[] {
			lblSelectTable, this.cboTables, this.btnRefreshTables,
			this.btnQueryTable, this.lblTableCount, this.lblRowCount, this.progressQuery
		});

				// Create custom query panel
				Panel queryPanel = new Panel
				{
					Height = 80,
					Dock = DockStyle.Top,
					BackColor = Color.FromArgb(248, 249, 250),
					BorderStyle = BorderStyle.FixedSingle,
					Padding = new Padding(10)
				};

				Label lblCustomQuery = new Label
				{
					Text = "💻 Custom Query:",
					Location = new Point(10, 10),
					Size = new Size(120, 20),
					Font = new Font("Segoe UI", 9F, FontStyle.Bold),
					ForeColor = Color.FromArgb(66, 66, 66)
				};

				// Reinitialize custom query textbox
				this.txtCustomQuery = new TextBox
				{
					Location = new Point(10, 35),
					Size = new Size(800, 25),
					Font = new Font("Consolas", 9F),
					Text = "SELECT TOP 100 * FROM [TableName] -- Select a table first"
				};

				Button btnExecuteQuery = new Button
				{
					Text = "▶️ Execute",
					Location = new Point(820, 33),
					Size = new Size(80, 29),
					BackColor = Color.FromArgb(211, 47, 47),
					ForeColor = Color.White,
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 9F)
				};
				btnExecuteQuery.FlatAppearance.BorderSize = 0;

				queryPanel.Controls.AddRange(new Control[] { lblCustomQuery, this.txtCustomQuery, btnExecuteQuery });

				// Reinitialize data grid
				this.dgvTableData = new DataGridView
				{
					Dock = DockStyle.Fill,
					BackgroundColor = Color.White,
					BorderStyle = BorderStyle.FixedSingle,
					AllowUserToAddRows = false,
					AllowUserToDeleteRows = false,
					ReadOnly = true,
					SelectionMode = DataGridViewSelectionMode.FullRowSelect,
					MultiSelect = false,
					AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
					Font = new Font("Segoe UI", 9F)
				};

				// Configure grid appearance
				this.dgvTableData.EnableHeadersVisualStyles = false;
				this.dgvTableData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 118, 210);
				this.dgvTableData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
				this.dgvTableData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
				this.dgvTableData.ColumnHeadersHeight = 35;
				this.dgvTableData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(187, 222, 251);
				this.dgvTableData.DefaultCellStyle.SelectionForeColor = Color.Black;
				this.dgvTableData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

		

				// Add all panels to main panel in correct order
				mainPanel.Controls.Add(this.dgvTableData);           // Fill (goes first)
				mainPanel.Controls.Add(queryPanel);                  // Top (but below toolbar)
				mainPanel.Controls.Add(toolbarPanel);                // Top

				// Add main panel to tab
				this.tabDataViewer.Controls.Add(mainPanel);

				// Wire up events
				this.btnRefreshTables.Click += BtnRefreshTables_Click;
				this.btnQueryTable.Click += BtnQueryTable_Click;
				btnExecuteQuery.Click += BtnExecuteQuery_Click;
				this.cboTables.SelectedIndexChanged += CboTables_SelectedIndexChanged;

				// Custom query textbox events for placeholder behavior
				this.txtCustomQuery.Enter += (s, e) =>
				{
					if (txtCustomQuery.Text.Contains("Select a table first"))
					{
						txtCustomQuery.Text = "";
						txtCustomQuery.ForeColor = Color.Black;
					}
				};

				this.txtCustomQuery.Leave += (s, e) =>
				{
					if (string.IsNullOrWhiteSpace(txtCustomQuery.Text))
					{
						txtCustomQuery.Text = "SELECT TOP 100 * FROM [TableName] -- Select a table first";
						txtCustomQuery.ForeColor = Color.Gray;
					}
				};

				_logService?.LogInfo("✅ Data Viewer tab initialized successfully");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to initialize Data Viewer tab", ex);
				MessageBox.Show($"Warning: Could not initialize Data Viewer tab properly.\n\nError: {ex.Message}",
					"Data Viewer Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		#region Event Handlers

		// Replace the existing chkCreateSynonyms_CheckedChanged method
		private void chkCreateSynonyms_CheckedChanged(object sender, EventArgs e)
		{
			if (IsInDesignMode || _isInitializing) return;

			try
			{
				bool enabled = chkCreateSynonyms?.Checked ?? false;
				_logService?.LogInfo($"🔗 Synonym creation {(enabled ? "enabled" : "disabled")}");

				// Update checkbox text
				UpdateSynonymCheckboxText();

				// Show/hide configuration panel
				if (pnlSynonymConfig != null)
				{
					pnlSynonymConfig.Visible = enabled;

					if (enabled)
					{
						// Auto-populate target database if empty
						if (string.IsNullOrEmpty(txtSynonymTargetDatabase?.Text))
						{
							AutoDetectTargetDatabase();
						}

						AnimatePanel(pnlSynonymConfig, true);
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error in synonym checkbox handler", ex);
			}
		}
		private void txtSynonymTargetDatabase_TextChanged(object sender, EventArgs e)
		{
			if (_isInitializing) return;

			// Update checkbox text when user types
			UpdateSynonymCheckboxText();

			// Update configuration
			if (_currentConfig != null && txtSynonymTargetDatabase != null)
			{
				_currentConfig.SynonymTargetDatabase = txtSynonymTargetDatabase.Text?.Trim() ?? "";
			}
		}

		private void chkWindowsAuth_CheckedChanged(object sender, EventArgs e)
		{
			if (_isInitializing || IsInDesignMode) return;

			// The UpdateAuthenticationControls logic is now in ConfigurationController
			// We can create a temporary config to trigger the UI update
			var tempConfig = new PublisherConfiguration { WindowsAuth = chkWindowsAuth.Checked };
			_configurationController.UpdateUIFromConfiguration(tempConfig);

			_logService?.LogInfo($"Authentication mode changed: {(chkWindowsAuth.Checked ? "Windows" : "SQL Server")}");
		}

		private async void btnTestConnection_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
				// REPLACE: Complex connection test logic
				// WITH:
				await _databaseController.TestConnectionAsync();
			}
			catch (Exception ex)
			{
				_logService.LogError("Connection test failed", ex);
				MessageBox.Show($"Connection test failed: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnRefreshDatabases_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			// REPLACE: await RefreshDatabasesAsync();
			// WITH:
			await _databaseController.RefreshDatabasesAsync();
		}

		private void btnBrowseDacpac_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			if (openDacpacDialog.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath.Text = openDacpacDialog.FileName;
			}
		}

		private async void btnBrowseJobScripts_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				txtJobScriptsFolder.Text = folderBrowserDialog.SelectedPath;
				await _validationController.ValidateJobScriptsFolderAsync();
			}
		}

		private void chkCreateSqlAgentJobs_CheckedChanged(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
				if (_isInitializing) return;

				bool enabled = chkCreateSqlAgentJobs?.Checked ?? false;
				_logService?.LogInfo($"⚡ SQL Agent Jobs {(enabled ? "enabled" : "disabled")}");

				// Show/hide job configuration panel
				if (pnlJobConfig != null)
				{
					pnlJobConfig.Visible = enabled;

					if (enabled)
					{
						// Animate panel appearance
						AnimatePanel(pnlJobConfig, true);

						// Focus on job owner login
						if (txtJobOwnerLoginName != null)
						{
							txtJobOwnerLoginName.Focus();
						}
					}
					else
					{
						// Clear validation results when disabled
						if (pnlJobValidation != null) pnlJobValidation.Visible = false;
						if (txtJobOwnerLoginName != null) txtJobOwnerLoginName.Clear();
						if (txtJobScriptsFolder != null) txtJobScriptsFolder.Clear();
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error in SQL Agent Jobs checkbox handler", ex);
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void chkExecuteProcedures_CheckedChanged(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			bool enabled = chkExecuteProcedures.Checked;
			btnConfigureSmartProcedures.Enabled = enabled;
			lblSmartProcedureStatus.Enabled = enabled;

			// REPLACE: UpdateSmartProcedureStatus();
			// WITH:
			_configurationController.UpdateSmartProcedureStatus();
		}
		private async void btnConfigureSmartProcedures_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
				using (var smartDialog = new SmartProcedureDialog(_currentConfig))
				{
					if (smartDialog.ShowDialog() == DialogResult.OK)
					{
						_currentConfig = smartDialog.UpdatedConfiguration;
						_currentConfig.UseSmartProcedures = true;

						_configurationController.UpdateSmartProcedureStatus();

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
			if (IsInDesignMode) return;

			if (saveConfigDialog?.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// REPLACE: UpdateConfigurationFromUI();
					// WITH:
					_configurationController.UpdateConfigurationFromUI(_currentConfig);

					await _configurationService.SaveConfigurationAsync(_currentConfig, saveConfigDialog.FileName);

					MessageBox.Show("💾 Configuration saved successfully!", "Save Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					_logService.LogInfo("💾 Configuration saved: " + Path.GetFileName(saveConfigDialog.FileName));
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
			if (IsInDesignMode) return;

			if (openConfigDialog?.ShowDialog() == DialogResult.OK)
			{
				try
				{
					_currentConfig = await _configurationService.LoadConfigurationAsync(openConfigDialog.FileName);

					// REPLACE: UpdateUIFromConfiguration();
					// WITH:
					_configurationController.UpdateUIFromConfiguration(_currentConfig);

					MessageBox.Show("📂 Configuration loaded successfully!", "Load Configuration",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					_logService.LogInfo("📂 Configuration loaded: " + Path.GetFileName(openConfigDialog.FileName));
				}
				catch (Exception ex)
				{
					_logService.LogError("Failed to load configuration", ex);
					MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		private async void btnPublish_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
			
				await _deploymentController.ExecuteDeploymentAsync();
				await SafeSwitchToDataViewer();
			}
			catch (Exception ex)
			{
				_logService.LogError("Deployment button click failed", ex);
				MessageBox.Show($"Deployment failed: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		// NEW: Log deployment analytics for troubleshooting
		private void btnAutoDetectTarget_Click(object sender, EventArgs e)
		{
			AutoDetectTargetDatabase();
		}
		private async void BtnRefreshTables_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			try
			{
				if (string.IsNullOrEmpty(_currentConfig?.ServerName) || string.IsNullOrEmpty(_currentConfig?.Database))
				{
					MessageBox.Show("Please configure database connection first.\n\nGo to the Setup tab and:\n1. Enter server name\n2. Select target database\n3. Test connection",
						"Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);

					// Switch to setup tab
					if (tabControl?.TabPages != null)
					{
						foreach (TabPage tab in tabControl.TabPages)
						{
							if (tab.Text.Contains("Setup"))
							{
								tabControl.SelectedTab = tab;
								break;
							}
						}
					}
					return;
				}

				btnRefreshTables.Enabled = false;
				btnRefreshTables.Text = "🔄 Loading...";
				progressQuery.Visible = true;
				progressQuery.Style = ProgressBarStyle.Marquee;

				_configurationController.UpdateConfigurationFromUI(_currentConfig);
				var tables = await GetDatabaseTablesAsync();

				cboTables.Items.Clear();
				foreach (var table in tables)
				{
					cboTables.Items.Add(table);
				}

				lblTableCount.Text = $"Tables: {tables.Count}";

				if (tables.Count > 0)
				{
					// Update recommendations
					rtbRecommendations.Text = $"✅ Found {tables.Count} tables in database '{_currentConfig.Database}'\n\n" +
											"📋 Next steps:\n" +
											"• Select a table from the dropdown\n" +
											"• Click 'Query Table' to view data\n" +
											"• Use custom queries for analysis\n\n" +
											$"🕒 Refreshed at: {DateTime.Now:HH:mm:ss}";
				}

				_logService?.LogInfo($"📊 Loaded {tables.Count} tables in Data Viewer");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to refresh tables", ex);

				string errorMsg = "Failed to refresh tables.\n\n";
				if (ex.Message.Contains("connection"))
					errorMsg += "Connection issue - check server name and credentials.";
				else if (ex.Message.Contains("permission"))
					errorMsg += "Permission issue - verify database access rights.";
				else
					errorMsg += $"Error: {ex.Message}";

				MessageBox.Show(errorMsg, "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				rtbRecommendations.Text = $"❌ Failed to load tables\n\n" +
										$"Error: {ex.Message}\n\n" +
										"🔧 Troubleshooting:\n" +
										"• Check database connection\n" +
										"• Verify server name and database\n" +
										"• Test connection in Setup tab";
			}
			finally
			{
				btnRefreshTables.Enabled = true;
				btnRefreshTables.Text = "🔄 Refresh";
				progressQuery.Visible = false;
			}
		}
		private async void BtnQueryTable_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

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
			if (IsInDesignMode) return;
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
		// Job Scripts Folder TextBox validation
		private async void txtJobScriptsFolder_TextChanged(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

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

				// REPLACE: await ValidateJobScriptsFolderAsync();
				// WITH:
				await _validationController.ValidateJobScriptsFolderAsync();
			};
			_jobScriptValidationTimer.Start();
		}

		// Event handler for second DACPAC browse
		private void btnBrowseDacpac2_Click(object sender, EventArgs e)
		{
			if (openDacpacDialog2.ShowDialog() == DialogResult.OK)
			{
				txtDacpacPath2.Text = openDacpacDialog2.FileName;
			}
		}


		// Event handler for second database refresh
		private async void btnRefreshDatabases2_Click(object sender, EventArgs e)
		{
			if (IsInDesignMode) return;

			// REPLACE: await RefreshDatabases2Async();
			// WITH:
			await _databaseController.RefreshDatabases2Async();
		}

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

	
		}

		#endregion

		#region Helper Methods

		// 2. UPDATE: DacpacPublisherForm.Designer.cs - Replace complex controls with simple ones
		private void UpdateSynonymCheckboxText()
		{
			try
			{
				if (chkCreateSynonyms == null) return;

				if (!chkCreateSynonyms.Checked)
				{
					chkCreateSynonyms.Text = "✅ Create Synonyms";
					return;
				}

				var targetDb = txtSynonymTargetDatabase?.Text?.Trim() ?? "";

				if (string.IsNullOrEmpty(targetDb))
				{
					chkCreateSynonyms.Text = "✅ Create Synonyms (Select target database below)";
				}
				else
				{
					chkCreateSynonyms.Text = $"✅ Create Synonyms in: {targetDb}";
				}

				_logService?.LogInfo($"📝 Updated synonym checkbox: {chkCreateSynonyms.Text}");
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error updating synonym checkbox text", ex);
				chkCreateSynonyms.Text = "✅ Create Synonyms";
			}
		}
		private void AutoDetectTargetDatabase()
		{
			try
			{
				if (txtSynonymTargetDatabase == null) return;

				// Update configuration from UI first
				_configurationController?.UpdateConfigurationFromUI(_currentConfig);

				var detectedTarget = _currentConfig?.GetAutoDetectedTargetDatabase() ?? "";

				if (!string.IsNullOrEmpty(detectedTarget))
				{
					txtSynonymTargetDatabase.Text = detectedTarget;
					UpdateSynonymCheckboxText();

					_logService?.LogInfo($"🎯 Auto-detected target database: {detectedTarget}");

					// Show success feedback
					ShowSuccessMessage($"✅ Auto-detected target database: {detectedTarget}", "Auto-Detection Complete");
				}
				else
				{
					txtSynonymTargetDatabase.Text = "";
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error auto-detecting target database", ex);
			}
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
				if (string.IsNullOrEmpty(name)) return null;

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

		private void AnimatePanel(Panel panel, bool show)
		{
			try
			{
				if (panel == null) return;

				if (show)
				{
					panel.Visible = true;
					// Simple fade-in effect by adjusting opacity-like appearance
					panel.BackColor = Color.FromArgb(240, panel.BackColor);

					var timer = new Timer();
					timer.Interval = 50;
					timer.Tick += (s, e) =>
					{
						var currentAlpha = panel.BackColor.A;
						if (currentAlpha < 248)
						{
							panel.BackColor = Color.FromArgb(Math.Min(255, currentAlpha + 15),
								panel.BackColor.R, panel.BackColor.G, panel.BackColor.B);
						}
						else
						{
							timer.Stop();
							timer.Dispose();
						}
					};
					timer.Start();
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Error animating panel", ex);
			}
		}

		#endregion

		#region Helper Methods (Preserved and Enhanced)

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

		private void UpdateAuthenticationControls()
		{
			try
			{
				var useWindowsAuth = chkWindowsAuth?.Checked ?? true;

				if (lblUsername != null) lblUsername.Enabled = !useWindowsAuth;
				if (txtUsername != null) txtUsername.Enabled = !useWindowsAuth;
				if (lblPassword != null) lblPassword.Enabled = !useWindowsAuth;
				if (txtPassword != null) txtPassword.Enabled = !useWindowsAuth;

				if (useWindowsAuth)
				{
					if (txtUsername != null) txtUsername.Clear();
					if (txtPassword != null) txtPassword.Clear();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Auth controls update error: {ex.Message}");
			}
		}

		private void UpdateStatus(string message, bool showProgress)
		{
			if (IsInDesignMode) return;
			try
			{
				if (InvokeRequired)
				{
					Invoke(new Action(() => UpdateStatus(message, showProgress)));
					return;
				}

				if (toolStripStatusLabel != null)
					toolStripStatusLabel.Text = showProgress ? $"⏳ {message}" : $"🟢 {message}";

				if (toolStripProgressBar != null)
				{
					toolStripProgressBar.Visible = showProgress;
					if (showProgress)
						toolStripProgressBar.Style = ProgressBarStyle.Marquee;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Status update error: {ex.Message}");
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
		private async Task<List<string>> GetDatabaseTablesAsync()
		{
			var tables = new List<string>();

			try
			{
				if (string.IsNullOrEmpty(_currentConfig?.ServerName) || string.IsNullOrEmpty(_currentConfig?.Database))
				{
					throw new InvalidOperationException("Database connection not configured. Please set up connection first.");
				}

				var connectionInfo = CreateConnectionInfo(_currentConfig);

				using (var connection = new System.Data.SqlClient.SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					var query = @"
                SELECT 
                    SCHEMA_NAME(t.schema_id) + '.' + t.name as TableName
                FROM sys.tables t
                WHERE t.is_ms_shipped = 0
                ORDER BY SCHEMA_NAME(t.schema_id), t.name";

					using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							tables.Add(reader.GetString(0));
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError("Failed to get database tables", ex);
				throw;
			}

			return tables;
		}
		private async Task<System.Data.DataTable> ExecuteDataQueryAsync(string query)
		{
			try
			{
				if (string.IsNullOrEmpty(_currentConfig?.ServerName) || string.IsNullOrEmpty(_currentConfig?.Database))
				{
					throw new InvalidOperationException("Database connection not configured. Please set up connection first.");
				}

				var connectionInfo = CreateConnectionInfo(_currentConfig);

				using (var connection = new System.Data.SqlClient.SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					using (var adapter = new System.Data.SqlClient.SqlDataAdapter(query, connection))
					{
						adapter.SelectCommand.CommandTimeout = 30; // 30 second timeout
						var dataTable = new System.Data.DataTable();
						adapter.Fill(dataTable);
						return dataTable;
					}
				}
			}
			catch (Exception ex)
			{
				_logService?.LogError($"Failed to execute query: {query}", ex);
				throw;
			}
		}
		#endregion


	}
}