using System;
using System.Drawing;
using System.Windows.Forms;

namespace DacpacPublisher
{
	partial class DacpacPublisherForm
	{
		private System.ComponentModel.IContainer components = null;

		// Declare ALL controls as class fields
		private PictureBox logoPictureBox;
		private Label headerTitleLabel;
		private Label headerSubtitleLabel;
		private Panel headerPanel;
		private GroupBox grpConnection;
		private Button btnTestConnection;
		private TextBox txtPassword;
		private TextBox txtUsername;
		private Label lblPassword;
		private Label lblUsername;
		private CheckBox chkWindowsAuth;
		private ComboBox cboDatabases;
		private Button btnRefreshDatabases;
		private Label lblDatabase;
		private TextBox txtServerName;
		private Label lblServerName;
		private CheckBox chkEnableMultipleDatabases;
		private Label lblDatabase2;
		private ComboBox cboDatabases2;
		private Button btnRefreshDatabases2;
		private Label lblDacpacPath2;
		private TextBox txtDacpacPath2;
		private Button btnBrowseDacpac2;
		private OpenFileDialog openDacpacDialog2;
		private GroupBox grpDacPac;
		private Button btnBrowseDacpac;
		private TextBox txtDacpacPath;
		private Label lblDacpacPath;
		private GroupBox grpSynonyms;
		private TextBox txtSynonymSourceDb;
		private Label lblSynonymSourceDb;
		private CheckBox chkCreateSynonyms;
		private CheckedListBox clbSynonymTargets;
		private Label lblSynonymTargets;
		private Button btnAutoDetectTargets;
		private Label lblSynonymSourceInfo;
		private CheckBox chkShowSynonymTargets;
		private Panel pnlSynonymConfig;
		private GroupBox grpSQLAgentJobs;
		private Label lblJobDescriptions;
		private TextBox txtJobOwnerLoginName;
		private Label lblJobOwnerLoginName;
		private CheckBox chkCreateSqlAgentJobs;
		private TextBox txtJobScriptsFolder;
		private Label lblJobScriptsFolder;
		private Button btnBrowseJobScripts;
		private Panel pnlJobConfig;
		private Label lblJobValidationResults;
		private Panel pnlJobValidation;
		private ListView lstJobScripts;
		private GroupBox grpSmartProcedures;
		private CheckBox chkExecuteProcedures;
		private Button btnConfigureSmartProcedures;
		private Label lblSmartProcedureStatus;
		private CheckBox chkCreateBackup;
		private Button btnPublish;
		private Button btnSaveConfig;
		private Button btnLoadConfig;
		private StatusStrip statusStrip;
		private ToolStripProgressBar toolStripProgressBar;
		private ToolStripStatusLabel toolStripStatusLabel;
		private TabControl tabControl;
		private TabPage tabSetup;
		private TabPage tabLog;
		private TextBox txtLog;
		private TabPage tabDataViewer;
		private DataGridView dgvTableData;
		private ComboBox cboTables;
		private Button btnRefreshTables;
		private Button btnQueryTable;
		private TextBox txtCustomQuery;
		private Panel pnlRecommendations;
		private RichTextBox rtbRecommendations;
		private Label lblTableCount;
		private Label lblRowCount;
		private ProgressBar progressQuery;
		private ToolTip toolTip;
		private OpenFileDialog openDacpacDialog;
		private OpenFileDialog openConfigDialog;
		private SaveFileDialog saveConfigDialog;
		private FolderBrowserDialog folderBrowserDialog;
		private SaveFileDialog saveLogDialog;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.headerPanel = new System.Windows.Forms.Panel();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.headerTitleLabel = new System.Windows.Forms.Label();
			this.headerSubtitleLabel = new System.Windows.Forms.Label();
			this.grpConnection = new System.Windows.Forms.GroupBox();
			this.lblServerName = new System.Windows.Forms.Label();
			this.txtServerName = new System.Windows.Forms.TextBox();
			this.chkWindowsAuth = new System.Windows.Forms.CheckBox();
			this.btnTestConnection = new System.Windows.Forms.Button();
			this.lblUsername = new System.Windows.Forms.Label();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblDatabase = new System.Windows.Forms.Label();
			this.cboDatabases = new System.Windows.Forms.ComboBox();
			this.btnRefreshDatabases = new System.Windows.Forms.Button();
			this.chkEnableMultipleDatabases = new System.Windows.Forms.CheckBox();
			this.lblDatabase2 = new System.Windows.Forms.Label();
			this.cboDatabases2 = new System.Windows.Forms.ComboBox();
			this.btnRefreshDatabases2 = new System.Windows.Forms.Button();
			this.lblDacpacPath2 = new System.Windows.Forms.Label();
			this.txtDacpacPath2 = new System.Windows.Forms.TextBox();
			this.btnBrowseDacpac2 = new System.Windows.Forms.Button();
			this.grpDacPac = new System.Windows.Forms.GroupBox();
			this.lblDacpacPath = new System.Windows.Forms.Label();
			this.txtDacpacPath = new System.Windows.Forms.TextBox();
			this.btnBrowseDacpac = new System.Windows.Forms.Button();
			this.grpSynonyms = new System.Windows.Forms.GroupBox();
			this.chkCreateSynonyms = new System.Windows.Forms.CheckBox();
			this.pnlSynonymConfig = new System.Windows.Forms.Panel();
			this.lblSynonymSourceInfo = new System.Windows.Forms.Label();
			this.chkShowSynonymTargets = new System.Windows.Forms.CheckBox();
			this.lblSynonymTargets = new System.Windows.Forms.Label();
			this.clbSynonymTargets = new System.Windows.Forms.CheckedListBox();
			this.btnAutoDetectTargets = new System.Windows.Forms.Button();
			this.lblSynonymSourceDb = new System.Windows.Forms.Label();
			this.txtSynonymSourceDb = new System.Windows.Forms.TextBox();
			this.grpSQLAgentJobs = new System.Windows.Forms.GroupBox();
			this.chkCreateSqlAgentJobs = new System.Windows.Forms.CheckBox();
			this.pnlJobConfig = new System.Windows.Forms.Panel();
			this.lblJobOwnerLoginName = new System.Windows.Forms.Label();
			this.txtJobOwnerLoginName = new System.Windows.Forms.TextBox();
			this.lblJobScriptsFolder = new System.Windows.Forms.Label();
			this.txtJobScriptsFolder = new System.Windows.Forms.TextBox();
			this.btnBrowseJobScripts = new System.Windows.Forms.Button();
			this.pnlJobValidation = new System.Windows.Forms.Panel();
			this.lblJobValidationResults = new System.Windows.Forms.Label();
			this.lstJobScripts = new System.Windows.Forms.ListView();
			this.lblJobDescriptions = new System.Windows.Forms.Label();
			this.grpSmartProcedures = new System.Windows.Forms.GroupBox();
			this.chkExecuteProcedures = new System.Windows.Forms.CheckBox();
			this.btnConfigureSmartProcedures = new System.Windows.Forms.Button();
			this.chkCreateBackup = new System.Windows.Forms.CheckBox();
			this.lblSmartProcedureStatus = new System.Windows.Forms.Label();
			this.btnPublish = new System.Windows.Forms.Button();
			this.btnSaveConfig = new System.Windows.Forms.Button();
			this.btnLoadConfig = new System.Windows.Forms.Button();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabSetup = new System.Windows.Forms.TabPage();
			this.tabLog = new System.Windows.Forms.TabPage();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.tabDataViewer = new System.Windows.Forms.TabPage();
			this.dgvTableData = new System.Windows.Forms.DataGridView();
			this.cboTables = new System.Windows.Forms.ComboBox();
			this.btnRefreshTables = new System.Windows.Forms.Button();
			this.btnQueryTable = new System.Windows.Forms.Button();
			this.txtCustomQuery = new System.Windows.Forms.TextBox();
			this.pnlRecommendations = new System.Windows.Forms.Panel();
			this.rtbRecommendations = new System.Windows.Forms.RichTextBox();
			this.lblTableCount = new System.Windows.Forms.Label();
			this.lblRowCount = new System.Windows.Forms.Label();
			this.progressQuery = new System.Windows.Forms.ProgressBar();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.openDacpacDialog = new System.Windows.Forms.OpenFileDialog();
			this.openDacpacDialog2 = new System.Windows.Forms.OpenFileDialog();
			this.openConfigDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveConfigDialog = new System.Windows.Forms.SaveFileDialog();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.saveLogDialog = new System.Windows.Forms.SaveFileDialog();
			this.headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
			this.grpConnection.SuspendLayout();
			this.grpDacPac.SuspendLayout();
			this.grpSynonyms.SuspendLayout();
			this.pnlSynonymConfig.SuspendLayout();
			this.grpSQLAgentJobs.SuspendLayout();
			this.pnlJobConfig.SuspendLayout();
			this.pnlJobValidation.SuspendLayout();
			this.grpSmartProcedures.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabSetup.SuspendLayout();
			this.tabLog.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTableData)).BeginInit();
			this.SuspendLayout();
			// 
			// headerPanel
			// 
			this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.headerPanel.Controls.Add(this.logoPictureBox);
			this.headerPanel.Controls.Add(this.headerTitleLabel);
			this.headerPanel.Controls.Add(this.headerSubtitleLabel);
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(1200, 70);
			this.headerPanel.TabIndex = 2;
			// 
			// logoPictureBox
			// 
			this.logoPictureBox.BackColor = System.Drawing.Color.White;
			this.logoPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.logoPictureBox.Location = new System.Drawing.Point(20, 15);
			this.logoPictureBox.Name = "logoPictureBox";
			this.logoPictureBox.Size = new System.Drawing.Size(40, 40);
			this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.logoPictureBox.TabIndex = 0;
			this.logoPictureBox.TabStop = false;
			// 
			// headerTitleLabel
			// 
			this.headerTitleLabel.AutoSize = true;
			this.headerTitleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
			this.headerTitleLabel.ForeColor = System.Drawing.Color.White;
			this.headerTitleLabel.Location = new System.Drawing.Point(75, 15);
			this.headerTitleLabel.Name = "headerTitleLabel";
			this.headerTitleLabel.Size = new System.Drawing.Size(353, 37);
			this.headerTitleLabel.TabIndex = 1;
			this.headerTitleLabel.Text = "DACPAC Deployment Tool";
			// 
			// headerSubtitleLabel
			// 
			this.headerSubtitleLabel.AutoSize = true;
			this.headerSubtitleLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.headerSubtitleLabel.ForeColor = System.Drawing.Color.White;
			this.headerSubtitleLabel.Location = new System.Drawing.Point(75, 40);
			this.headerSubtitleLabel.Name = "headerSubtitleLabel";
			this.headerSubtitleLabel.Size = new System.Drawing.Size(454, 23);
			this.headerSubtitleLabel.TabIndex = 2;
			this.headerSubtitleLabel.Text = "Smart Database Deployment with Multi-Database Support";
			// 
			// grpConnection
			// 
			this.grpConnection.BackColor = System.Drawing.Color.White;
			this.grpConnection.Controls.Add(this.lblServerName);
			this.grpConnection.Controls.Add(this.txtServerName);
			this.grpConnection.Controls.Add(this.chkWindowsAuth);
			this.grpConnection.Controls.Add(this.btnTestConnection);
			this.grpConnection.Controls.Add(this.lblUsername);
			this.grpConnection.Controls.Add(this.txtUsername);
			this.grpConnection.Controls.Add(this.lblPassword);
			this.grpConnection.Controls.Add(this.txtPassword);
			this.grpConnection.Controls.Add(this.lblDatabase);
			this.grpConnection.Controls.Add(this.cboDatabases);
			this.grpConnection.Controls.Add(this.btnRefreshDatabases);
			this.grpConnection.Controls.Add(this.chkEnableMultipleDatabases);
			this.grpConnection.Controls.Add(this.lblDatabase2);
			this.grpConnection.Controls.Add(this.cboDatabases2);
			this.grpConnection.Controls.Add(this.btnRefreshDatabases2);
			this.grpConnection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpConnection.Location = new System.Drawing.Point(20, 15);
			this.grpConnection.Name = "grpConnection";
			this.grpConnection.Size = new System.Drawing.Size(1150, 200);
			this.grpConnection.TabIndex = 0;
			this.grpConnection.TabStop = false;
			this.grpConnection.Text = "🔗 Database Connection";
			// 
			// lblServerName
			// 
			this.lblServerName.AutoSize = true;
			this.lblServerName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblServerName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblServerName.Location = new System.Drawing.Point(25, 40);
			this.lblServerName.Name = "lblServerName";
			this.lblServerName.Size = new System.Drawing.Size(94, 20);
			this.lblServerName.TabIndex = 0;
			this.lblServerName.Text = "Server Name";
			// 
			// txtServerName
			// 
			this.txtServerName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtServerName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtServerName.Location = new System.Drawing.Point(25, 60);
			this.txtServerName.Name = "txtServerName";
			this.txtServerName.Size = new System.Drawing.Size(280, 27);
			this.txtServerName.TabIndex = 1;
			// 
			// chkWindowsAuth
			// 
			this.chkWindowsAuth.AutoSize = true;
			this.chkWindowsAuth.Checked = true;
			this.chkWindowsAuth.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkWindowsAuth.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkWindowsAuth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkWindowsAuth.Location = new System.Drawing.Point(320, 40);
			this.chkWindowsAuth.Name = "chkWindowsAuth";
			this.chkWindowsAuth.Size = new System.Drawing.Size(218, 24);
			this.chkWindowsAuth.TabIndex = 2;
			this.chkWindowsAuth.Text = "🔐 Windows Authentication";
			this.chkWindowsAuth.UseVisualStyleBackColor = true;
			this.chkWindowsAuth.CheckedChanged += new System.EventHandler(this.chkWindowsAuth_CheckedChanged);
			// 
			// btnTestConnection
			// 
			this.btnTestConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnTestConnection.FlatAppearance.BorderSize = 0;
			this.btnTestConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTestConnection.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnTestConnection.ForeColor = System.Drawing.Color.White;
			this.btnTestConnection.Location = new System.Drawing.Point(950, 35);
			this.btnTestConnection.Name = "btnTestConnection";
			this.btnTestConnection.Size = new System.Drawing.Size(160, 40);
			this.btnTestConnection.TabIndex = 3;
			this.btnTestConnection.Text = "🔌 Test Connection";
			this.btnTestConnection.UseVisualStyleBackColor = false;
			this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
			// 
			// lblUsername
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.Enabled = false;
			this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblUsername.Location = new System.Drawing.Point(320, 70);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(75, 20);
			this.lblUsername.TabIndex = 4;
			this.lblUsername.Text = "Username";
			// 
			// txtUsername
			// 
			this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtUsername.Enabled = false;
			this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtUsername.Location = new System.Drawing.Point(320, 90);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(180, 27);
			this.txtUsername.TabIndex = 5;
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Enabled = false;
			this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblPassword.Location = new System.Drawing.Point(520, 70);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(70, 20);
			this.lblPassword.TabIndex = 6;
			this.lblPassword.Text = "Password";
			// 
			// txtPassword
			// 
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Enabled = false;
			this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtPassword.Location = new System.Drawing.Point(520, 90);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '●';
			this.txtPassword.Size = new System.Drawing.Size(180, 27);
			this.txtPassword.TabIndex = 7;
			// 
			// lblDatabase
			// 
			this.lblDatabase.AutoSize = true;
			this.lblDatabase.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDatabase.Location = new System.Drawing.Point(25, 130);
			this.lblDatabase.Name = "lblDatabase";
			this.lblDatabase.Size = new System.Drawing.Size(117, 20);
			this.lblDatabase.TabIndex = 8;
			this.lblDatabase.Text = "Target Database";
			// 
			// cboDatabases
			// 
			this.cboDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDatabases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboDatabases.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.cboDatabases.FormattingEnabled = true;
			this.cboDatabases.Location = new System.Drawing.Point(25, 150);
			this.cboDatabases.Name = "cboDatabases";
			this.cboDatabases.Size = new System.Drawing.Size(200, 28);
			this.cboDatabases.TabIndex = 9;
			// 
			// btnRefreshDatabases
			// 
			this.btnRefreshDatabases.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnRefreshDatabases.FlatAppearance.BorderSize = 0;
			this.btnRefreshDatabases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefreshDatabases.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnRefreshDatabases.ForeColor = System.Drawing.Color.White;
			this.btnRefreshDatabases.Location = new System.Drawing.Point(240, 145);
			this.btnRefreshDatabases.Name = "btnRefreshDatabases";
			this.btnRefreshDatabases.Size = new System.Drawing.Size(160, 35);
			this.btnRefreshDatabases.TabIndex = 10;
			this.btnRefreshDatabases.Text = "🔄 Refresh Databases";
			this.btnRefreshDatabases.UseVisualStyleBackColor = false;
			this.btnRefreshDatabases.Click += new System.EventHandler(this.btnRefreshDatabases_Click);
			// 
			// chkEnableMultipleDatabases
			// 
			this.chkEnableMultipleDatabases.AutoSize = true;
			this.chkEnableMultipleDatabases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.chkEnableMultipleDatabases.Location = new System.Drawing.Point(450, 132);
			this.chkEnableMultipleDatabases.Name = "chkEnableMultipleDatabases";
			this.chkEnableMultipleDatabases.Size = new System.Drawing.Size(257, 24);
			this.chkEnableMultipleDatabases.TabIndex = 11;
			this.chkEnableMultipleDatabases.Text = "🗄️ Deploy to Multiple Databases";
			this.chkEnableMultipleDatabases.UseVisualStyleBackColor = true;
			this.chkEnableMultipleDatabases.CheckedChanged += new System.EventHandler(this.chkEnableMultipleDatabases_CheckedChanged);
			// 
			// lblDatabase2
			// 
			this.lblDatabase2.AutoSize = true;
			this.lblDatabase2.Enabled = false;
			this.lblDatabase2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDatabase2.Location = new System.Drawing.Point(450, 162);
			this.lblDatabase2.Name = "lblDatabase2";
			this.lblDatabase2.Size = new System.Drawing.Size(145, 20);
			this.lblDatabase2.TabIndex = 12;
			this.lblDatabase2.Text = "Secondary Database";
			// 
			// cboDatabases2
			// 
			this.cboDatabases2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDatabases2.Enabled = false;
			this.cboDatabases2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboDatabases2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.cboDatabases2.FormattingEnabled = true;
			this.cboDatabases2.Location = new System.Drawing.Point(580, 159);
			this.cboDatabases2.Name = "cboDatabases2";
			this.cboDatabases2.Size = new System.Drawing.Size(180, 28);
			this.cboDatabases2.TabIndex = 13;
			// 
			// btnRefreshDatabases2
			// 
			this.btnRefreshDatabases2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnRefreshDatabases2.Enabled = false;
			this.btnRefreshDatabases2.FlatAppearance.BorderSize = 0;
			this.btnRefreshDatabases2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefreshDatabases2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnRefreshDatabases2.ForeColor = System.Drawing.Color.White;
			this.btnRefreshDatabases2.Location = new System.Drawing.Point(770, 157);
			this.btnRefreshDatabases2.Name = "btnRefreshDatabases2";
			this.btnRefreshDatabases2.Size = new System.Drawing.Size(100, 30);
			this.btnRefreshDatabases2.TabIndex = 14;
			this.btnRefreshDatabases2.Text = "🔄 Refresh";
			this.btnRefreshDatabases2.UseVisualStyleBackColor = false;
			this.btnRefreshDatabases2.Click += new System.EventHandler(this.btnRefreshDatabases2_Click);
			// 
			// lblDacpacPath2
			// 
			this.lblDacpacPath2.AutoSize = true;
			this.lblDacpacPath2.Enabled = false;
			this.lblDacpacPath2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDacpacPath2.Location = new System.Drawing.Point(25, 85);
			this.lblDacpacPath2.Name = "lblDacpacPath2";
			this.lblDacpacPath2.Size = new System.Drawing.Size(163, 20);
			this.lblDacpacPath2.TabIndex = 3;
			this.lblDacpacPath2.Text = "Secondary Package File";
			// 
			// txtDacpacPath2
			// 
			this.txtDacpacPath2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDacpacPath2.Enabled = false;
			this.txtDacpacPath2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtDacpacPath2.Location = new System.Drawing.Point(200, 82);
			this.txtDacpacPath2.Name = "txtDacpacPath2";
			this.txtDacpacPath2.Size = new System.Drawing.Size(775, 27);
			this.txtDacpacPath2.TabIndex = 4;
			// 
			// btnBrowseDacpac2
			// 
			this.btnBrowseDacpac2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseDacpac2.Enabled = false;
			this.btnBrowseDacpac2.FlatAppearance.BorderSize = 0;
			this.btnBrowseDacpac2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseDacpac2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseDacpac2.ForeColor = System.Drawing.Color.White;
			this.btnBrowseDacpac2.Location = new System.Drawing.Point(990, 78);
			this.btnBrowseDacpac2.Name = "btnBrowseDacpac2";
			this.btnBrowseDacpac2.Size = new System.Drawing.Size(140, 35);
			this.btnBrowseDacpac2.TabIndex = 5;
			this.btnBrowseDacpac2.Text = "📁 Browse...";
			this.btnBrowseDacpac2.UseVisualStyleBackColor = false;
			this.btnBrowseDacpac2.Click += new System.EventHandler(this.btnBrowseDacpac2_Click);
			// 
			// grpDacPac
			// 
			this.grpDacPac.BackColor = System.Drawing.Color.White;
			this.grpDacPac.Controls.Add(this.lblDacpacPath);
			this.grpDacPac.Controls.Add(this.txtDacpacPath);
			this.grpDacPac.Controls.Add(this.btnBrowseDacpac);
			this.grpDacPac.Controls.Add(this.lblDacpacPath2);
			this.grpDacPac.Controls.Add(this.txtDacpacPath2);
			this.grpDacPac.Controls.Add(this.btnBrowseDacpac2);
			this.grpDacPac.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpDacPac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpDacPac.Location = new System.Drawing.Point(20, 230);
			this.grpDacPac.Name = "grpDacPac";
			this.grpDacPac.Size = new System.Drawing.Size(1150, 120);
			this.grpDacPac.TabIndex = 1;
			this.grpDacPac.TabStop = false;
			this.grpDacPac.Text = "📦 DACPAC Package(s)";
			// 
			// lblDacpacPath
			// 
			this.lblDacpacPath.AutoSize = true;
			this.lblDacpacPath.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDacpacPath.Location = new System.Drawing.Point(25, 51);
			this.lblDacpacPath.Name = "lblDacpacPath";
			this.lblDacpacPath.Size = new System.Drawing.Size(144, 20);
			this.lblDacpacPath.TabIndex = 0;
			this.lblDacpacPath.Text = "Primary Package File";
			// 
			// txtDacpacPath
			// 
			this.txtDacpacPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDacpacPath.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtDacpacPath.Location = new System.Drawing.Point(200, 44);
			this.txtDacpacPath.Name = "txtDacpacPath";
			this.txtDacpacPath.Size = new System.Drawing.Size(775, 27);
			this.txtDacpacPath.TabIndex = 1;
			// 
			// btnBrowseDacpac
			// 
			this.btnBrowseDacpac.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseDacpac.FlatAppearance.BorderSize = 0;
			this.btnBrowseDacpac.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseDacpac.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseDacpac.ForeColor = System.Drawing.Color.White;
			this.btnBrowseDacpac.Location = new System.Drawing.Point(990, 37);
			this.btnBrowseDacpac.Name = "btnBrowseDacpac";
			this.btnBrowseDacpac.Size = new System.Drawing.Size(140, 35);
			this.btnBrowseDacpac.TabIndex = 2;
			this.btnBrowseDacpac.Text = "📁 Browse...";
			this.btnBrowseDacpac.UseVisualStyleBackColor = false;
			this.btnBrowseDacpac.Click += new System.EventHandler(this.btnBrowseDacpac_Click);
			// 
			// grpSynonyms
			// 
			this.grpSynonyms.BackColor = System.Drawing.Color.White;
			this.grpSynonyms.Controls.Add(this.chkCreateSynonyms);
			this.grpSynonyms.Controls.Add(this.pnlSynonymConfig);
			this.grpSynonyms.Controls.Add(this.lblSynonymSourceDb);
			this.grpSynonyms.Controls.Add(this.txtSynonymSourceDb);
			this.grpSynonyms.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSynonyms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSynonyms.Location = new System.Drawing.Point(20, 360);
			this.grpSynonyms.Name = "grpSynonyms";
			this.grpSynonyms.Size = new System.Drawing.Size(570, 280);
			this.grpSynonyms.TabIndex = 2;
			this.grpSynonyms.TabStop = false;
			this.grpSynonyms.Text = "🔗 Database Synonyms";
			// 
			// chkCreateSynonyms
			// 
			this.chkCreateSynonyms.AutoSize = true;
			this.chkCreateSynonyms.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateSynonyms.Location = new System.Drawing.Point(25, 35);
			this.chkCreateSynonyms.Name = "chkCreateSynonyms";
			this.chkCreateSynonyms.Size = new System.Drawing.Size(169, 24);
			this.chkCreateSynonyms.TabIndex = 0;
			this.chkCreateSynonyms.Text = "✅ Create Synonyms";
			this.chkCreateSynonyms.UseVisualStyleBackColor = true;
			this.chkCreateSynonyms.CheckedChanged += new System.EventHandler(this.chkCreateSynonyms_CheckedChanged);
			// 
			// pnlSynonymConfig
			// 
			this.pnlSynonymConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
			this.pnlSynonymConfig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSynonymConfig.Controls.Add(this.lblSynonymSourceInfo);
			this.pnlSynonymConfig.Controls.Add(this.chkShowSynonymTargets);
			this.pnlSynonymConfig.Controls.Add(this.lblSynonymTargets);
			this.pnlSynonymConfig.Controls.Add(this.clbSynonymTargets);
			this.pnlSynonymConfig.Controls.Add(this.btnAutoDetectTargets);
			this.pnlSynonymConfig.Location = new System.Drawing.Point(20, 65);
			this.pnlSynonymConfig.Name = "pnlSynonymConfig";
			this.pnlSynonymConfig.Size = new System.Drawing.Size(530, 200);
			this.pnlSynonymConfig.TabIndex = 1;
			this.pnlSynonymConfig.Visible = false;
			// 
			// lblSynonymSourceInfo
			// 
			this.lblSynonymSourceInfo.AutoSize = true;
			this.lblSynonymSourceInfo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
			this.lblSynonymSourceInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.lblSynonymSourceInfo.Location = new System.Drawing.Point(10, 10);
			this.lblSynonymSourceInfo.Name = "lblSynonymSourceInfo";
			this.lblSynonymSourceInfo.Size = new System.Drawing.Size(430, 19);
			this.lblSynonymSourceInfo.TabIndex = 0;
			this.lblSynonymSourceInfo.Text = "🤖 Source database will be auto-detected (HiveCFMSurvey pattern)";
			// 
			// chkShowSynonymTargets
			// 
			this.chkShowSynonymTargets.AutoSize = true;
			this.chkShowSynonymTargets.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkShowSynonymTargets.Location = new System.Drawing.Point(10, 35);
			this.chkShowSynonymTargets.Name = "chkShowSynonymTargets";
			this.chkShowSynonymTargets.Size = new System.Drawing.Size(488, 24);
			this.chkShowSynonymTargets.TabIndex = 1;
			this.chkShowSynonymTargets.Text = "📋 Configure target databases (auto-selects HiveCFMApp databases)";
			this.chkShowSynonymTargets.UseVisualStyleBackColor = true;
			this.chkShowSynonymTargets.CheckedChanged += new System.EventHandler(this.ChkShowSynonymTargets_CheckedChanged);
			// 
			// lblSynonymTargets
			// 
			this.lblSynonymTargets.AutoSize = true;
			this.lblSynonymTargets.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblSynonymTargets.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.lblSynonymTargets.Location = new System.Drawing.Point(10, 65);
			this.lblSynonymTargets.Name = "lblSynonymTargets";
			this.lblSynonymTargets.Size = new System.Drawing.Size(440, 20);
			this.lblSynonymTargets.TabIndex = 2;
			this.lblSynonymTargets.Text = "🎯 Select target databases (where synonyms will be created):";
			this.lblSynonymTargets.Visible = false;
			// 
			// clbSynonymTargets
			// 
			this.clbSynonymTargets.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.clbSynonymTargets.CheckOnClick = true;
			this.clbSynonymTargets.FormattingEnabled = true;
			this.clbSynonymTargets.Location = new System.Drawing.Point(10, 90);
			this.clbSynonymTargets.Name = "clbSynonymTargets";
			this.clbSynonymTargets.Size = new System.Drawing.Size(420, 56);
			this.clbSynonymTargets.TabIndex = 3;
			this.clbSynonymTargets.Visible = false;
			// 
			// btnAutoDetectTargets
			// 
			this.btnAutoDetectTargets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnAutoDetectTargets.FlatAppearance.BorderSize = 0;
			this.btnAutoDetectTargets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAutoDetectTargets.Font = new System.Drawing.Font("Segoe UI", 8F);
			this.btnAutoDetectTargets.ForeColor = System.Drawing.Color.White;
			this.btnAutoDetectTargets.Location = new System.Drawing.Point(440, 90);
			this.btnAutoDetectTargets.Name = "btnAutoDetectTargets";
			this.btnAutoDetectTargets.Size = new System.Drawing.Size(80, 30);
			this.btnAutoDetectTargets.TabIndex = 4;
			this.btnAutoDetectTargets.Text = "🔄 Auto";
			this.btnAutoDetectTargets.UseVisualStyleBackColor = false;
			this.btnAutoDetectTargets.Visible = false;
			this.btnAutoDetectTargets.Click += new System.EventHandler(this.BtnAutoDetectTargets_Click);
			// 
			// lblSynonymSourceDb
			// 
			this.lblSynonymSourceDb.AutoSize = true;
			this.lblSynonymSourceDb.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblSynonymSourceDb.Location = new System.Drawing.Point(25, 55);
			this.lblSynonymSourceDb.Name = "lblSynonymSourceDb";
			this.lblSynonymSourceDb.Size = new System.Drawing.Size(121, 20);
			this.lblSynonymSourceDb.TabIndex = 2;
			this.lblSynonymSourceDb.Text = "Source Database";
			this.lblSynonymSourceDb.Visible = false;
			// 
			// txtSynonymSourceDb
			// 
			this.txtSynonymSourceDb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSynonymSourceDb.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtSynonymSourceDb.Location = new System.Drawing.Point(200, 52);
			this.txtSynonymSourceDb.Name = "txtSynonymSourceDb";
			this.txtSynonymSourceDb.Size = new System.Drawing.Size(200, 27);
			this.txtSynonymSourceDb.TabIndex = 3;
			this.txtSynonymSourceDb.Visible = false;
			// 
			// grpSQLAgentJobs
			// 
			this.grpSQLAgentJobs.BackColor = System.Drawing.Color.White;
			this.grpSQLAgentJobs.Controls.Add(this.chkCreateSqlAgentJobs);
			this.grpSQLAgentJobs.Controls.Add(this.pnlJobConfig);
			this.grpSQLAgentJobs.Controls.Add(this.lblJobDescriptions);
			this.grpSQLAgentJobs.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSQLAgentJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSQLAgentJobs.Location = new System.Drawing.Point(600, 360);
			this.grpSQLAgentJobs.Name = "grpSQLAgentJobs";
			this.grpSQLAgentJobs.Size = new System.Drawing.Size(570, 280);
			this.grpSQLAgentJobs.TabIndex = 3;
			this.grpSQLAgentJobs.TabStop = false;
			this.grpSQLAgentJobs.Text = "⚡ SQL Agent Jobs";
			// 
			// chkCreateSqlAgentJobs
			// 
			this.chkCreateSqlAgentJobs.AutoSize = true;
			this.chkCreateSqlAgentJobs.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateSqlAgentJobs.Location = new System.Drawing.Point(25, 35);
			this.chkCreateSqlAgentJobs.Name = "chkCreateSqlAgentJobs";
			this.chkCreateSqlAgentJobs.Size = new System.Drawing.Size(205, 24);
			this.chkCreateSqlAgentJobs.TabIndex = 0;
			this.chkCreateSqlAgentJobs.Text = "⚡ Create SQL Agent Jobs";
			this.chkCreateSqlAgentJobs.UseVisualStyleBackColor = true;
			this.chkCreateSqlAgentJobs.CheckedChanged += new System.EventHandler(this.chkCreateSqlAgentJobs_CheckedChanged);
			// 
			// pnlJobConfig
			// 
			this.pnlJobConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
			this.pnlJobConfig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlJobConfig.Controls.Add(this.lblJobOwnerLoginName);
			this.pnlJobConfig.Controls.Add(this.txtJobOwnerLoginName);
			this.pnlJobConfig.Controls.Add(this.lblJobScriptsFolder);
			this.pnlJobConfig.Controls.Add(this.txtJobScriptsFolder);
			this.pnlJobConfig.Controls.Add(this.btnBrowseJobScripts);
			this.pnlJobConfig.Controls.Add(this.pnlJobValidation);
			this.pnlJobConfig.Location = new System.Drawing.Point(20, 65);
			this.pnlJobConfig.Name = "pnlJobConfig";
			this.pnlJobConfig.Size = new System.Drawing.Size(530, 200);
			this.pnlJobConfig.TabIndex = 1;
			this.pnlJobConfig.Visible = false;
			// 
			// lblJobOwnerLoginName
			// 
			this.lblJobOwnerLoginName.AutoSize = true;
			this.lblJobOwnerLoginName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobOwnerLoginName.Location = new System.Drawing.Point(10, 15);
			this.lblJobOwnerLoginName.Name = "lblJobOwnerLoginName";
			this.lblJobOwnerLoginName.Size = new System.Drawing.Size(120, 20);
			this.lblJobOwnerLoginName.TabIndex = 0;
			this.lblJobOwnerLoginName.Text = "Job Owner Login";
			// 
			// txtJobOwnerLoginName
			// 
			this.txtJobOwnerLoginName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtJobOwnerLoginName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtJobOwnerLoginName.Location = new System.Drawing.Point(120, 12);
			this.txtJobOwnerLoginName.Name = "txtJobOwnerLoginName";
			this.txtJobOwnerLoginName.Size = new System.Drawing.Size(200, 27);
			this.txtJobOwnerLoginName.TabIndex = 1;
			// 
			// lblJobScriptsFolder
			// 
			this.lblJobScriptsFolder.AutoSize = true;
			this.lblJobScriptsFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobScriptsFolder.Location = new System.Drawing.Point(10, 50);
			this.lblJobScriptsFolder.Name = "lblJobScriptsFolder";
			this.lblJobScriptsFolder.Size = new System.Drawing.Size(126, 20);
			this.lblJobScriptsFolder.TabIndex = 2;
			this.lblJobScriptsFolder.Text = "Job Scripts Folder";
			// 
			// txtJobScriptsFolder
			// 
			this.txtJobScriptsFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtJobScriptsFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtJobScriptsFolder.Location = new System.Drawing.Point(10, 70);
			this.txtJobScriptsFolder.Name = "txtJobScriptsFolder";
			this.txtJobScriptsFolder.Size = new System.Drawing.Size(380, 27);
			this.txtJobScriptsFolder.TabIndex = 3;
			this.txtJobScriptsFolder.TextChanged += new System.EventHandler(this.txtJobScriptsFolder_TextChanged);
			// 
			// btnBrowseJobScripts
			// 
			this.btnBrowseJobScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseJobScripts.FlatAppearance.BorderSize = 0;
			this.btnBrowseJobScripts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseJobScripts.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseJobScripts.ForeColor = System.Drawing.Color.White;
			this.btnBrowseJobScripts.Location = new System.Drawing.Point(400, 67);
			this.btnBrowseJobScripts.Name = "btnBrowseJobScripts";
			this.btnBrowseJobScripts.Size = new System.Drawing.Size(120, 28);
			this.btnBrowseJobScripts.TabIndex = 4;
			this.btnBrowseJobScripts.Text = "📁 Browse...";
			this.btnBrowseJobScripts.UseVisualStyleBackColor = false;
			this.btnBrowseJobScripts.Click += new System.EventHandler(this.btnBrowseJobScripts_Click);
			// 
			// pnlJobValidation
			// 
			this.pnlJobValidation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(245)))), ((int)(((byte)(233)))));
			this.pnlJobValidation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlJobValidation.Controls.Add(this.lblJobValidationResults);
			this.pnlJobValidation.Controls.Add(this.lstJobScripts);
			this.pnlJobValidation.Location = new System.Drawing.Point(10, 110);
			this.pnlJobValidation.Name = "pnlJobValidation";
			this.pnlJobValidation.Size = new System.Drawing.Size(510, 80);
			this.pnlJobValidation.TabIndex = 5;
			this.pnlJobValidation.Visible = false;
			// 
			// lblJobValidationResults
			// 
			this.lblJobValidationResults.AutoSize = true;
			this.lblJobValidationResults.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblJobValidationResults.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.lblJobValidationResults.Location = new System.Drawing.Point(5, 5);
			this.lblJobValidationResults.Name = "lblJobValidationResults";
			this.lblJobValidationResults.Size = new System.Drawing.Size(185, 20);
			this.lblJobValidationResults.TabIndex = 0;
			this.lblJobValidationResults.Text = "✅ Job Scripts Validation";
			// 
			// lstJobScripts
			// 
			this.lstJobScripts.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstJobScripts.Font = new System.Drawing.Font("Segoe UI", 8F);
			this.lstJobScripts.HideSelection = false;
			this.lstJobScripts.Location = new System.Drawing.Point(5, 30);
			this.lstJobScripts.Name = "lstJobScripts";
			this.lstJobScripts.Size = new System.Drawing.Size(495, 45);
			this.lstJobScripts.TabIndex = 1;
			this.lstJobScripts.UseCompatibleStateImageBehavior = false;
			this.lstJobScripts.View = System.Windows.Forms.View.List;
			// 
			// lblJobDescriptions
			// 
			this.lblJobDescriptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.lblJobDescriptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblJobDescriptions.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobDescriptions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.lblJobDescriptions.Location = new System.Drawing.Point(25, 250);
			this.lblJobDescriptions.Name = "lblJobDescriptions";
			this.lblJobDescriptions.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.lblJobDescriptions.Size = new System.Drawing.Size(520, 25);
			this.lblJobDescriptions.TabIndex = 2;
			this.lblJobDescriptions.Text = "💡 Select a job scripts folder to see available jobs...";
			this.lblJobDescriptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// grpSmartProcedures
			// 
			this.grpSmartProcedures.BackColor = System.Drawing.Color.White;
			this.grpSmartProcedures.Controls.Add(this.chkExecuteProcedures);
			this.grpSmartProcedures.Controls.Add(this.btnConfigureSmartProcedures);
			this.grpSmartProcedures.Controls.Add(this.chkCreateBackup);
			this.grpSmartProcedures.Controls.Add(this.lblSmartProcedureStatus);
			this.grpSmartProcedures.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSmartProcedures.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSmartProcedures.Location = new System.Drawing.Point(20, 650);
			this.grpSmartProcedures.Name = "grpSmartProcedures";
			this.grpSmartProcedures.Size = new System.Drawing.Size(1150, 120);
			this.grpSmartProcedures.TabIndex = 4;
			this.grpSmartProcedures.TabStop = false;
			this.grpSmartProcedures.Text = "🧠 Smart Stored Procedures";
			// 
			// chkExecuteProcedures
			// 
			this.chkExecuteProcedures.AutoSize = true;
			this.chkExecuteProcedures.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkExecuteProcedures.Location = new System.Drawing.Point(25, 35);
			this.chkExecuteProcedures.Name = "chkExecuteProcedures";
			this.chkExecuteProcedures.Size = new System.Drawing.Size(184, 24);
			this.chkExecuteProcedures.TabIndex = 0;
			this.chkExecuteProcedures.Text = "🧠 Execute Procedures";
			this.chkExecuteProcedures.UseVisualStyleBackColor = true;
			this.chkExecuteProcedures.CheckedChanged += new System.EventHandler(this.chkExecuteProcedures_CheckedChanged);
			// 
			// btnConfigureSmartProcedures
			// 
			this.btnConfigureSmartProcedures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnConfigureSmartProcedures.Enabled = false;
			this.btnConfigureSmartProcedures.FlatAppearance.BorderSize = 0;
			this.btnConfigureSmartProcedures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnConfigureSmartProcedures.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnConfigureSmartProcedures.ForeColor = System.Drawing.Color.White;
			this.btnConfigureSmartProcedures.Location = new System.Drawing.Point(300, 32);
			this.btnConfigureSmartProcedures.Name = "btnConfigureSmartProcedures";
			this.btnConfigureSmartProcedures.Size = new System.Drawing.Size(220, 45);
			this.btnConfigureSmartProcedures.TabIndex = 1;
			this.btnConfigureSmartProcedures.Text = "⚙️ Configure Smart Procedures";
			this.btnConfigureSmartProcedures.UseVisualStyleBackColor = false;
			this.btnConfigureSmartProcedures.Click += new System.EventHandler(this.btnConfigureSmartProcedures_Click);
			// 
			// chkCreateBackup
			// 
			this.chkCreateBackup.AutoSize = true;
			this.chkCreateBackup.Checked = true;
			this.chkCreateBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkCreateBackup.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateBackup.Location = new System.Drawing.Point(25, 85);
			this.chkCreateBackup.Name = "chkCreateBackup";
			this.chkCreateBackup.Size = new System.Drawing.Size(246, 24);
			this.chkCreateBackup.TabIndex = 2;
			this.chkCreateBackup.Text = "🛡️ Create Backup Before Deploy";
			this.chkCreateBackup.UseVisualStyleBackColor = true;
			// 
			// lblSmartProcedureStatus
			// 
			this.lblSmartProcedureStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.lblSmartProcedureStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblSmartProcedureStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblSmartProcedureStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.lblSmartProcedureStatus.Location = new System.Drawing.Point(540, 35);
			this.lblSmartProcedureStatus.Name = "lblSmartProcedureStatus";
			this.lblSmartProcedureStatus.Padding = new System.Windows.Forms.Padding(15, 10, 10, 10);
			this.lblSmartProcedureStatus.Size = new System.Drawing.Size(590, 45);
			this.lblSmartProcedureStatus.TabIndex = 3;
			this.lblSmartProcedureStatus.Text = "💡 Click \'Configure Smart Procedures\' to get started with intelligent database de" +
    "ployment";
			this.lblSmartProcedureStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnPublish
			// 
			this.btnPublish.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
			this.btnPublish.FlatAppearance.BorderSize = 0;
			this.btnPublish.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPublish.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
			this.btnPublish.ForeColor = System.Drawing.Color.White;
			this.btnPublish.Location = new System.Drawing.Point(20, 780);
			this.btnPublish.Name = "btnPublish";
			this.btnPublish.Size = new System.Drawing.Size(180, 50);
			this.btnPublish.TabIndex = 5;
			this.btnPublish.Text = "🚀 Deploy Now";
			this.btnPublish.UseVisualStyleBackColor = false;
			this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
			// 
			// btnSaveConfig
			// 
			this.btnSaveConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnSaveConfig.FlatAppearance.BorderSize = 0;
			this.btnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSaveConfig.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnSaveConfig.ForeColor = System.Drawing.Color.White;
			this.btnSaveConfig.Location = new System.Drawing.Point(220, 780);
			this.btnSaveConfig.Name = "btnSaveConfig";
			this.btnSaveConfig.Size = new System.Drawing.Size(150, 50);
			this.btnSaveConfig.TabIndex = 6;
			this.btnSaveConfig.Text = "💾 Save Configuration";
			this.btnSaveConfig.UseVisualStyleBackColor = false;
			this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
			// 
			// btnLoadConfig
			// 
			this.btnLoadConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.btnLoadConfig.FlatAppearance.BorderSize = 0;
			this.btnLoadConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLoadConfig.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnLoadConfig.ForeColor = System.Drawing.Color.White;
			this.btnLoadConfig.Location = new System.Drawing.Point(390, 780);
			this.btnLoadConfig.Name = "btnLoadConfig";
			this.btnLoadConfig.Size = new System.Drawing.Size(150, 50);
			this.btnLoadConfig.TabIndex = 7;
			this.btnLoadConfig.Text = "📂 Load Configuration";
			this.btnLoadConfig.UseVisualStyleBackColor = false;
			this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripProgressBar});
			this.statusStrip.Location = new System.Drawing.Point(0, 874);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1200, 26);
			this.statusStrip.TabIndex = 1;
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripStatusLabel.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(75, 20);
			this.toolStripStatusLabel.Text = "🟢 Ready";
			// 
			// toolStripProgressBar
			// 
			this.toolStripProgressBar.Name = "toolStripProgressBar";
			this.toolStripProgressBar.Size = new System.Drawing.Size(200, 20);
			this.toolStripProgressBar.Visible = false;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl.Controls.Add(this.tabSetup);
			this.tabControl.Controls.Add(this.tabLog);
			this.tabControl.Controls.Add(this.tabDataViewer);
			this.tabControl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.tabControl.ItemSize = new System.Drawing.Size(120, 35);
			this.tabControl.Location = new System.Drawing.Point(0, 70);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1200, 800);
			this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabControl.TabIndex = 0;
			// 
			// tabSetup
			// 
			this.tabSetup.AutoScroll = true;
			this.tabSetup.AutoScrollMinSize = new System.Drawing.Size(1150, 850);
			this.tabSetup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.tabSetup.Controls.Add(this.grpConnection);
			this.tabSetup.Controls.Add(this.grpDacPac);
			this.tabSetup.Controls.Add(this.grpSynonyms);
			this.tabSetup.Controls.Add(this.grpSQLAgentJobs);
			this.tabSetup.Controls.Add(this.grpSmartProcedures);
			this.tabSetup.Controls.Add(this.btnPublish);
			this.tabSetup.Controls.Add(this.btnSaveConfig);
			this.tabSetup.Controls.Add(this.btnLoadConfig);
			this.tabSetup.Location = new System.Drawing.Point(4, 39);
			this.tabSetup.Name = "tabSetup";
			this.tabSetup.Padding = new System.Windows.Forms.Padding(3);
			this.tabSetup.Size = new System.Drawing.Size(1192, 757);
			this.tabSetup.TabIndex = 0;
			this.tabSetup.Text = "⚙️ Setup";
			this.tabSetup.UseVisualStyleBackColor = true;
			// 
			// tabLog
			// 
			this.tabLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.tabLog.Controls.Add(this.txtLog);
			this.tabLog.Location = new System.Drawing.Point(4, 39);
			this.tabLog.Name = "tabLog";
			this.tabLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabLog.Size = new System.Drawing.Size(1192, 757);
			this.tabLog.TabIndex = 1;
			this.tabLog.Text = "📋 Activity Log";
			this.tabLog.UseVisualStyleBackColor = true;
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
			this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.txtLog.Location = new System.Drawing.Point(15, 15);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(1160, 720);
			this.txtLog.TabIndex = 0;
			// 
			// tabDataViewer
			// 
			this.tabDataViewer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.tabDataViewer.Location = new System.Drawing.Point(4, 39);
			this.tabDataViewer.Name = "tabDataViewer";
			this.tabDataViewer.Size = new System.Drawing.Size(1192, 757);
			this.tabDataViewer.TabIndex = 2;
			this.tabDataViewer.Text = "📊 Data Viewer";
			this.tabDataViewer.UseVisualStyleBackColor = true;
			// 
			// dgvTableData
			// 
			this.dgvTableData.ColumnHeadersHeight = 29;
			this.dgvTableData.Location = new System.Drawing.Point(0, 0);
			this.dgvTableData.Name = "dgvTableData";
			this.dgvTableData.RowHeadersWidth = 51;
			this.dgvTableData.Size = new System.Drawing.Size(240, 150);
			this.dgvTableData.TabIndex = 0;
			// 
			// cboTables
			// 
			this.cboTables.Location = new System.Drawing.Point(0, 0);
			this.cboTables.Name = "cboTables";
			this.cboTables.Size = new System.Drawing.Size(121, 24);
			this.cboTables.TabIndex = 0;
			// 
			// btnRefreshTables
			// 
			this.btnRefreshTables.Location = new System.Drawing.Point(0, 0);
			this.btnRefreshTables.Name = "btnRefreshTables";
			this.btnRefreshTables.Size = new System.Drawing.Size(75, 23);
			this.btnRefreshTables.TabIndex = 0;
			// 
			// btnQueryTable
			// 
			this.btnQueryTable.Location = new System.Drawing.Point(0, 0);
			this.btnQueryTable.Name = "btnQueryTable";
			this.btnQueryTable.Size = new System.Drawing.Size(75, 23);
			this.btnQueryTable.TabIndex = 0;
			// 
			// txtCustomQuery
			// 
			this.txtCustomQuery.Location = new System.Drawing.Point(0, 0);
			this.txtCustomQuery.Name = "txtCustomQuery";
			this.txtCustomQuery.Size = new System.Drawing.Size(100, 22);
			this.txtCustomQuery.TabIndex = 0;
			// 
			// pnlRecommendations
			// 
			this.pnlRecommendations.Location = new System.Drawing.Point(0, 0);
			this.pnlRecommendations.Name = "pnlRecommendations";
			this.pnlRecommendations.Size = new System.Drawing.Size(200, 100);
			this.pnlRecommendations.TabIndex = 0;
			// 
			// rtbRecommendations
			// 
			this.rtbRecommendations.Location = new System.Drawing.Point(0, 0);
			this.rtbRecommendations.Name = "rtbRecommendations";
			this.rtbRecommendations.Size = new System.Drawing.Size(100, 96);
			this.rtbRecommendations.TabIndex = 0;
			this.rtbRecommendations.Text = "";
			// 
			// lblTableCount
			// 
			this.lblTableCount.Location = new System.Drawing.Point(0, 0);
			this.lblTableCount.Name = "lblTableCount";
			this.lblTableCount.Size = new System.Drawing.Size(100, 23);
			this.lblTableCount.TabIndex = 0;
			// 
			// lblRowCount
			// 
			this.lblRowCount.Location = new System.Drawing.Point(0, 0);
			this.lblRowCount.Name = "lblRowCount";
			this.lblRowCount.Size = new System.Drawing.Size(100, 23);
			this.lblRowCount.TabIndex = 0;
			// 
			// progressQuery
			// 
			this.progressQuery.Location = new System.Drawing.Point(0, 0);
			this.progressQuery.Name = "progressQuery";
			this.progressQuery.Size = new System.Drawing.Size(100, 23);
			this.progressQuery.TabIndex = 0;
			// 
			// openDacpacDialog
			// 
			this.openDacpacDialog.DefaultExt = "dacpac";
			this.openDacpacDialog.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			this.openDacpacDialog.Title = "Select Primary DACPAC Package File";
			// 
			// openDacpacDialog2
			// 
			this.openDacpacDialog2.DefaultExt = "dacpac";
			this.openDacpacDialog2.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			this.openDacpacDialog2.Title = "Select Secondary DACPAC Package File";
			// 
			// openConfigDialog
			// 
			this.openConfigDialog.DefaultExt = "json";
			this.openConfigDialog.Filter = "Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
			this.openConfigDialog.Title = "Load Deployment Configuration";
			// 
			// saveConfigDialog
			// 
			this.saveConfigDialog.DefaultExt = "json";
			this.saveConfigDialog.Filter = "Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
			this.saveConfigDialog.Title = "Save Deployment Configuration";
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select SQL Agent Job Scripts Folder";
			// 
			// saveLogDialog
			// 
			this.saveLogDialog.DefaultExt = "txt";
			this.saveLogDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
			this.saveLogDialog.Title = "Export Activity Log";
			// 
			// DacpacPublisherForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.ClientSize = new System.Drawing.Size(1200, 900);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.headerPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MinimumSize = new System.Drawing.Size(1200, 900);
			this.Name = "DacpacPublisherForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DACPAC Publisher - Professional Edition";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.DacpacPublisherForm_Load);
			this.headerPanel.ResumeLayout(false);
			this.headerPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
			this.grpConnection.ResumeLayout(false);
			this.grpConnection.PerformLayout();
			this.grpDacPac.ResumeLayout(false);
			this.grpDacPac.PerformLayout();
			this.grpSynonyms.ResumeLayout(false);
			this.grpSynonyms.PerformLayout();
			this.pnlSynonymConfig.ResumeLayout(false);
			this.pnlSynonymConfig.PerformLayout();
			this.grpSQLAgentJobs.ResumeLayout(false);
			this.grpSQLAgentJobs.PerformLayout();
			this.pnlJobConfig.ResumeLayout(false);
			this.pnlJobConfig.PerformLayout();
			this.pnlJobValidation.ResumeLayout(false);
			this.pnlJobValidation.PerformLayout();
			this.grpSmartProcedures.ResumeLayout(false);
			this.grpSmartProcedures.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabSetup.ResumeLayout(false);
			this.tabLog.ResumeLayout(false);
			this.tabLog.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTableData)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}