using System;
using System.Drawing;
using System.Windows.Forms;
using DacpacPublisher.Helper;

namespace DacpacPublisher
{
	partial class DacpacPublisherForm
	{
		private System.ComponentModel.IContainer components = null;
		private PictureBox logoPictureBox;
		private Label headerTitleLabel;
		private Label headerSubtitleLabel;
		private Panel headerPanel;

		// ADDED: Multi-database controls
		private System.Windows.Forms.CheckBox chkEnableMultipleDatabases;
		private System.Windows.Forms.Label lblDatabase2;
		private System.Windows.Forms.ComboBox cboDatabases2;
		private System.Windows.Forms.Button btnRefreshDatabases2;
		private System.Windows.Forms.Label lblDacpacPath2;
		private System.Windows.Forms.TextBox txtDacpacPath2;
		private System.Windows.Forms.Button btnBrowseDacpac2;
		private System.Windows.Forms.OpenFileDialog openDacpacDialog2;

		// ADDED: Data viewer controls
		private System.Windows.Forms.TabPage tabDataViewer;
		private System.Windows.Forms.DataGridView dgvTableData;
		private System.Windows.Forms.ComboBox cboTables;
		private System.Windows.Forms.Button btnRefreshTables;
		private System.Windows.Forms.Button btnQueryTable;
		private System.Windows.Forms.TextBox txtCustomQuery;
		private System.Windows.Forms.Panel pnlRecommendations;
		private System.Windows.Forms.RichTextBox rtbRecommendations;
		private System.Windows.Forms.Label lblTableCount;
		private System.Windows.Forms.Label lblRowCount;
		private System.Windows.Forms.ProgressBar progressQuery;

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
			this.grpConnection = new System.Windows.Forms.GroupBox();
			this.btnTestConnection = new System.Windows.Forms.Button();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblUsername = new System.Windows.Forms.Label();
			this.chkWindowsAuth = new System.Windows.Forms.CheckBox();
			this.cboDatabases = new System.Windows.Forms.ComboBox();
			this.btnRefreshDatabases = new System.Windows.Forms.Button();
			this.lblDatabase = new System.Windows.Forms.Label();
			this.txtServerName = new System.Windows.Forms.TextBox();
			this.lblServerName = new System.Windows.Forms.Label();

			// ADDED: Multi-database controls initialization
			this.chkEnableMultipleDatabases = new System.Windows.Forms.CheckBox();
			this.lblDatabase2 = new System.Windows.Forms.Label();
			this.cboDatabases2 = new System.Windows.Forms.ComboBox();
			this.btnRefreshDatabases2 = new System.Windows.Forms.Button();
			this.lblDacpacPath2 = new System.Windows.Forms.Label();
			this.txtDacpacPath2 = new System.Windows.Forms.TextBox();
			this.btnBrowseDacpac2 = new System.Windows.Forms.Button();
			this.openDacpacDialog2 = new System.Windows.Forms.OpenFileDialog();

			this.grpDacPac = new System.Windows.Forms.GroupBox();
			this.btnBrowseDacpac = new System.Windows.Forms.Button();
			this.txtDacpacPath = new System.Windows.Forms.TextBox();
			this.lblDacpacPath = new System.Windows.Forms.Label();
			this.grpSynonyms = new System.Windows.Forms.GroupBox();
			this.txtSynonymSourceDb = new System.Windows.Forms.TextBox();
			this.lblSynonymSourceDb = new System.Windows.Forms.Label();
			this.chkCreateSynonyms = new System.Windows.Forms.CheckBox();
			this.grpSQLAgentJobs = new System.Windows.Forms.GroupBox();
			this.lblJobDescriptions = new System.Windows.Forms.Label();
			this.txtJobOwnerLoginName = new System.Windows.Forms.TextBox();
			this.lblJobOwnerLoginName = new System.Windows.Forms.Label();
			this.chkCreateSqlAgentJobs = new System.Windows.Forms.CheckBox();
			this.grpSmartProcedures = new System.Windows.Forms.GroupBox();
			this.chkExecuteProcedures = new System.Windows.Forms.CheckBox();
			this.btnConfigureSmartProcedures = new System.Windows.Forms.Button();
			this.lblSmartProcedureStatus = new System.Windows.Forms.Label();
			this.chkCreateBackup = new System.Windows.Forms.CheckBox();
			this.btnPublish = new System.Windows.Forms.Button();
			this.btnSaveConfig = new System.Windows.Forms.Button();
			this.btnLoadConfig = new System.Windows.Forms.Button();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabSetup = new System.Windows.Forms.TabPage();
			this.grpScriptPaths = new System.Windows.Forms.GroupBox();
			this.txtJobScriptsFolder = new System.Windows.Forms.TextBox();
			this.lblJobScriptsFolder = new System.Windows.Forms.Label();
			this.btnBrowseJobScripts = new System.Windows.Forms.Button();
			this.tabLog = new System.Windows.Forms.TabPage();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.openDacpacDialog = new System.Windows.Forms.OpenFileDialog();
			this.openConfigDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveConfigDialog = new System.Windows.Forms.SaveFileDialog();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.saveLogDialog = new System.Windows.Forms.SaveFileDialog();
			this.grpConnection.SuspendLayout();
			this.grpDacPac.SuspendLayout();
			this.grpSynonyms.SuspendLayout();
			this.grpSQLAgentJobs.SuspendLayout();
			this.grpSmartProcedures.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabSetup.SuspendLayout();
			this.grpScriptPaths.SuspendLayout();
			this.tabLog.SuspendLayout();
			this.SuspendLayout();

			// 
			// grpConnection - UPDATED to include multi-database controls
			// 
			this.grpConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpConnection.Controls.Add(this.chkEnableMultipleDatabases);  // ADDED
			this.grpConnection.Controls.Add(this.lblDatabase2);                // ADDED
			this.grpConnection.Controls.Add(this.cboDatabases2);               // ADDED
			this.grpConnection.Controls.Add(this.btnRefreshDatabases2);        // ADDED
			this.grpConnection.Controls.Add(this.btnTestConnection);
			this.grpConnection.Controls.Add(this.txtPassword);
			this.grpConnection.Controls.Add(this.txtUsername);
			this.grpConnection.Controls.Add(this.lblPassword);
			this.grpConnection.Controls.Add(this.lblUsername);
			this.grpConnection.Controls.Add(this.chkWindowsAuth);
			this.grpConnection.Controls.Add(this.cboDatabases);
			this.grpConnection.Controls.Add(this.btnRefreshDatabases);
			this.grpConnection.Controls.Add(this.lblDatabase);
			this.grpConnection.Controls.Add(this.txtServerName);
			this.grpConnection.Controls.Add(this.lblServerName);
			this.grpConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.grpConnection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpConnection.Location = new System.Drawing.Point(20, 15);
			this.grpConnection.Name = "grpConnection";
			this.grpConnection.Size = new System.Drawing.Size(980, 190);  // INCREASED HEIGHT for multi-database controls
			this.grpConnection.TabIndex = 0;
			this.grpConnection.TabStop = false;
			this.grpConnection.Text = "🔗 Database Connection";

			// [Keep all your existing connection controls - just showing the new ones]

			// 
			// lblServerName (existing)
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
			// txtServerName (existing)
			// 
			this.txtServerName.BackColor = System.Drawing.Color.White;
			this.txtServerName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtServerName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtServerName.Location = new System.Drawing.Point(25, 60);
			this.txtServerName.Name = "txtServerName";
			this.txtServerName.Size = new System.Drawing.Size(280, 27);
			this.txtServerName.TabIndex = 1;
			this.txtServerName.Text = "(local)";

			// 
			// chkWindowsAuth (existing)
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
			// btnTestConnection (existing)
			// 
			this.btnTestConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnTestConnection.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnTestConnection.FlatAppearance.BorderSize = 0;
			this.btnTestConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTestConnection.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnTestConnection.ForeColor = System.Drawing.Color.White;
			this.btnTestConnection.Location = new System.Drawing.Point(800, 35);
			this.btnTestConnection.Name = "btnTestConnection";
			this.btnTestConnection.Size = new System.Drawing.Size(160, 40);
			this.btnTestConnection.TabIndex = 3;
			this.btnTestConnection.Text = "🔌 Test Connection";
			this.btnTestConnection.UseVisualStyleBackColor = false;
			this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);

			// 
			// lblUsername (existing)
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblUsername.Location = new System.Drawing.Point(320, 70);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(75, 20);
			this.lblUsername.TabIndex = 4;
			this.lblUsername.Text = "Username";

			// 
			// txtUsername (existing)
			// 
			this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtUsername.Location = new System.Drawing.Point(320, 90);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(180, 27);
			this.txtUsername.TabIndex = 5;

			// 
			// lblPassword (existing)
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblPassword.Location = new System.Drawing.Point(520, 70);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(70, 20);
			this.lblPassword.TabIndex = 6;
			this.lblPassword.Text = "Password";

			// 
			// txtPassword (existing)
			// 
			this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtPassword.Location = new System.Drawing.Point(520, 90);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '●';
			this.txtPassword.Size = new System.Drawing.Size(180, 27);
			this.txtPassword.TabIndex = 7;

			// 
			// lblDatabase (existing)
			// 
			this.lblDatabase.AutoSize = true;
			this.lblDatabase.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblDatabase.Location = new System.Drawing.Point(25, 100);
			this.lblDatabase.Name = "lblDatabase";
			this.lblDatabase.Size = new System.Drawing.Size(117, 20);
			this.lblDatabase.TabIndex = 8;
			this.lblDatabase.Text = "Target Database";

			// 
			// cboDatabases (existing)
			// 
			this.cboDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDatabases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboDatabases.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.cboDatabases.FormattingEnabled = true;
			this.cboDatabases.Location = new System.Drawing.Point(25, 120);
			this.cboDatabases.Name = "cboDatabases";
			this.cboDatabases.Size = new System.Drawing.Size(200, 28);
			this.cboDatabases.TabIndex = 9;

			// 
			// btnRefreshDatabases (existing)
			// 
			this.btnRefreshDatabases.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnRefreshDatabases.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnRefreshDatabases.FlatAppearance.BorderSize = 0;
			this.btnRefreshDatabases.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefreshDatabases.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnRefreshDatabases.ForeColor = System.Drawing.Color.White;
			this.btnRefreshDatabases.Location = new System.Drawing.Point(240, 115);
			this.btnRefreshDatabases.Name = "btnRefreshDatabases";
			this.btnRefreshDatabases.Size = new System.Drawing.Size(160, 35);
			this.btnRefreshDatabases.TabIndex = 10;
			this.btnRefreshDatabases.Text = "🔄 Refresh Databases";
			this.btnRefreshDatabases.UseVisualStyleBackColor = false;
			this.btnRefreshDatabases.Click += new System.EventHandler(this.btnRefreshDatabases_Click);

			// 
			// chkEnableMultipleDatabases - NEW
			// 
			this.chkEnableMultipleDatabases.AutoSize = true;
			this.chkEnableMultipleDatabases.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.chkEnableMultipleDatabases.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkEnableMultipleDatabases.Location = new System.Drawing.Point(25, 160);
			this.chkEnableMultipleDatabases.Name = "chkEnableMultipleDatabases";
			this.chkEnableMultipleDatabases.Size = new System.Drawing.Size(260, 24);
			this.chkEnableMultipleDatabases.TabIndex = 11;
			this.chkEnableMultipleDatabases.Text = "🗄️ Deploy to Multiple Databases";
			this.chkEnableMultipleDatabases.UseVisualStyleBackColor = true;
			this.chkEnableMultipleDatabases.CheckedChanged += new System.EventHandler(this.chkEnableMultipleDatabases_CheckedChanged);

			// 
			// lblDatabase2 - NEW
			// 
			this.lblDatabase2.AutoSize = true;
			this.lblDatabase2.Enabled = false;
			this.lblDatabase2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDatabase2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblDatabase2.Location = new System.Drawing.Point(320, 162);
			this.lblDatabase2.Name = "lblDatabase2";
			this.lblDatabase2.Size = new System.Drawing.Size(136, 20);
			this.lblDatabase2.TabIndex = 12;
			this.lblDatabase2.Text = "Secondary Database";

			// 
			// cboDatabases2 - NEW
			// 
			this.cboDatabases2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDatabases2.Enabled = false;
			this.cboDatabases2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboDatabases2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.cboDatabases2.FormattingEnabled = true;
			this.cboDatabases2.Location = new System.Drawing.Point(470, 159);
			this.cboDatabases2.Name = "cboDatabases2";
			this.cboDatabases2.Size = new System.Drawing.Size(180, 28);
			this.cboDatabases2.TabIndex = 13;

			// 
			// btnRefreshDatabases2 - NEW
			// 
			this.btnRefreshDatabases2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnRefreshDatabases2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnRefreshDatabases2.Enabled = false;
			this.btnRefreshDatabases2.FlatAppearance.BorderSize = 0;
			this.btnRefreshDatabases2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefreshDatabases2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnRefreshDatabases2.ForeColor = System.Drawing.Color.White;
			this.btnRefreshDatabases2.Location = new System.Drawing.Point(660, 157);
			this.btnRefreshDatabases2.Name = "btnRefreshDatabases2";
			this.btnRefreshDatabases2.Size = new System.Drawing.Size(100, 30);
			this.btnRefreshDatabases2.TabIndex = 14;
			this.btnRefreshDatabases2.Text = "🔄 Refresh";
			this.btnRefreshDatabases2.UseVisualStyleBackColor = false;
			this.btnRefreshDatabases2.Click += new System.EventHandler(this.btnRefreshDatabases2_Click);

			// 
			// grpDacPac - UPDATED to include second DACPAC controls
			// 
			this.grpDacPac.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpDacPac.Controls.Add(this.lblDacpacPath2);       // ADDED
			this.grpDacPac.Controls.Add(this.txtDacpacPath2);       // ADDED
			this.grpDacPac.Controls.Add(this.btnBrowseDacpac2);     // ADDED
			this.grpDacPac.Controls.Add(this.btnBrowseDacpac);
			this.grpDacPac.Controls.Add(this.txtDacpacPath);
			this.grpDacPac.Controls.Add(this.lblDacpacPath);
			this.grpDacPac.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpDacPac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpDacPac.Location = new System.Drawing.Point(20, 215);  // ADJUSTED position
			this.grpDacPac.Name = "grpDacPac";
			this.grpDacPac.Size = new System.Drawing.Size(980, 120);      // INCREASED height
			this.grpDacPac.TabIndex = 1;
			this.grpDacPac.TabStop = false;
			this.grpDacPac.Text = "📦 DACPAC Package(s)";

			// 
			// lblDacpacPath (existing)
			// 
			this.lblDacpacPath.AutoSize = true;
			this.lblDacpacPath.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDacpacPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblDacpacPath.Location = new System.Drawing.Point(25, 35);
			this.lblDacpacPath.Name = "lblDacpacPath";
			this.lblDacpacPath.Size = new System.Drawing.Size(151, 20);
			this.lblDacpacPath.TabIndex = 0;
			this.lblDacpacPath.Text = "Primary Package File";

			// 
			// txtDacpacPath (existing)
			// 
			this.txtDacpacPath.BackColor = System.Drawing.Color.White;
			this.txtDacpacPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDacpacPath.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtDacpacPath.Location = new System.Drawing.Point(200, 32);
			this.txtDacpacPath.Name = "txtDacpacPath";
			this.txtDacpacPath.Size = new System.Drawing.Size(600, 27);
			this.txtDacpacPath.TabIndex = 1;

			// 
			// btnBrowseDacpac (existing)
			// 
			this.btnBrowseDacpac.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseDacpac.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBrowseDacpac.FlatAppearance.BorderSize = 0;
			this.btnBrowseDacpac.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseDacpac.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseDacpac.ForeColor = System.Drawing.Color.White;
			this.btnBrowseDacpac.Location = new System.Drawing.Point(820, 28);
			this.btnBrowseDacpac.Name = "btnBrowseDacpac";
			this.btnBrowseDacpac.Size = new System.Drawing.Size(140, 35);
			this.btnBrowseDacpac.TabIndex = 2;
			this.btnBrowseDacpac.Text = "📁 Browse...";
			this.btnBrowseDacpac.UseVisualStyleBackColor = false;
			this.btnBrowseDacpac.Click += new System.EventHandler(this.btnBrowseDacpac_Click);

			// 
			// lblDacpacPath2 - NEW
			// 
			this.lblDacpacPath2.AutoSize = true;
			this.lblDacpacPath2.Enabled = false;
			this.lblDacpacPath2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDacpacPath2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblDacpacPath2.Location = new System.Drawing.Point(25, 75);
			this.lblDacpacPath2.Name = "lblDacpacPath2";
			this.lblDacpacPath2.Size = new System.Drawing.Size(168, 20);
			this.lblDacpacPath2.TabIndex = 3;
			this.lblDacpacPath2.Text = "Secondary Package File";

			// 
			// txtDacpacPath2 - NEW
			// 
			this.txtDacpacPath2.BackColor = System.Drawing.Color.White;
			this.txtDacpacPath2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtDacpacPath2.Enabled = false;
			this.txtDacpacPath2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtDacpacPath2.Location = new System.Drawing.Point(200, 72);
			this.txtDacpacPath2.Name = "txtDacpacPath2";
			this.txtDacpacPath2.Size = new System.Drawing.Size(600, 27);
			this.txtDacpacPath2.TabIndex = 4;

			// 
			// btnBrowseDacpac2 - NEW
			// 
			this.btnBrowseDacpac2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseDacpac2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBrowseDacpac2.Enabled = false;
			this.btnBrowseDacpac2.FlatAppearance.BorderSize = 0;
			this.btnBrowseDacpac2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseDacpac2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseDacpac2.ForeColor = System.Drawing.Color.White;
			this.btnBrowseDacpac2.Location = new System.Drawing.Point(820, 68);
			this.btnBrowseDacpac2.Name = "btnBrowseDacpac2";
			this.btnBrowseDacpac2.Size = new System.Drawing.Size(140, 35);
			this.btnBrowseDacpac2.TabIndex = 5;
			this.btnBrowseDacpac2.Text = "📁 Browse...";
			this.btnBrowseDacpac2.UseVisualStyleBackColor = false;
			this.btnBrowseDacpac2.Click += new System.EventHandler(this.btnBrowseDacpac2_Click);

			// 
			// openDacpacDialog2 - NEW
			// 
			this.openDacpacDialog2.DefaultExt = "dacpac";
			this.openDacpacDialog2.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			this.openDacpacDialog2.Title = "Select Secondary DACPAC Package File";

			// 
			// grpSynonyms (existing - adjusted position)
			// 
			this.grpSynonyms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpSynonyms.Controls.Add(this.txtSynonymSourceDb);
			this.grpSynonyms.Controls.Add(this.lblSynonymSourceDb);
			this.grpSynonyms.Controls.Add(this.chkCreateSynonyms);
			this.grpSynonyms.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSynonyms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSynonyms.Location = new System.Drawing.Point(20, 345);  // ADJUSTED position
			this.grpSynonyms.Name = "grpSynonyms";
			this.grpSynonyms.Size = new System.Drawing.Size(480, 80);
			this.grpSynonyms.TabIndex = 2;
			this.grpSynonyms.TabStop = false;
			this.grpSynonyms.Text = "🔗 Database Synonyms";

			// 
			// chkCreateSynonyms (existing)
			// 
			this.chkCreateSynonyms.AutoSize = true;
			this.chkCreateSynonyms.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateSynonyms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkCreateSynonyms.Location = new System.Drawing.Point(25, 35);
			this.chkCreateSynonyms.Name = "chkCreateSynonyms";
			this.chkCreateSynonyms.Size = new System.Drawing.Size(169, 24);
			this.chkCreateSynonyms.TabIndex = 0;
			this.chkCreateSynonyms.Text = "✅ Create Synonyms";
			this.chkCreateSynonyms.UseVisualStyleBackColor = true;
			this.chkCreateSynonyms.CheckedChanged += new System.EventHandler(this.chkCreateSynonyms_CheckedChanged);

			// 
			// lblSynonymSourceDb (existing)
			// 
			this.lblSynonymSourceDb.AutoSize = true;
			this.lblSynonymSourceDb.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblSynonymSourceDb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblSynonymSourceDb.Location = new System.Drawing.Point(220, 40);
			this.lblSynonymSourceDb.Name = "lblSynonymSourceDb";
			this.lblSynonymSourceDb.Size = new System.Drawing.Size(121, 20);
			this.lblSynonymSourceDb.TabIndex = 1;
			this.lblSynonymSourceDb.Text = "Source Database";
			// Label for target databases
			this.lblSynonymTargets = new Label();
			this.lblSynonymTargets.AutoSize = true;
			this.lblSynonymTargets.Location = new Point(10, 80);
			this.lblSynonymTargets.Text = "Target Databases (where synonyms will be created):";
			this.lblSynonymTargets.Visible = false;

			// Checked list box for selecting target databases
			this.clbSynonymTargets = new CheckedListBox();
			this.clbSynonymTargets.Location = new Point(10, 100);
			this.clbSynonymTargets.Size = new Size(350, 80);
			this.clbSynonymTargets.CheckOnClick = true;
			this.clbSynonymTargets.Visible = false;

			// Auto-detect button
			this.btnAutoDetectTargets = new Button();
			this.btnAutoDetectTargets.Location = new Point(370, 100);
			this.btnAutoDetectTargets.Size = new Size(100, 25);
			this.btnAutoDetectTargets.Text = "Auto Detect";
			this.btnAutoDetectTargets.Visible = false;
			this.btnAutoDetectTargets.Click += BtnAutoDetectTargets_Click;

			// Add controls to the group
			grpSynonyms.Controls.Add(lblSynonymTargets);
			grpSynonyms.Controls.Add(clbSynonymTargets);
			grpSynonyms.Controls.Add(btnAutoDetectTargets);

			// 
			// txtSynonymSourceDb (existing)
			// 
			this.txtSynonymSourceDb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSynonymSourceDb.Enabled = false;
			this.txtSynonymSourceDb.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtSynonymSourceDb.Location = new System.Drawing.Point(350, 37);
			this.txtSynonymSourceDb.Name = "txtSynonymSourceDb";
			this.txtSynonymSourceDb.Size = new System.Drawing.Size(120, 27);
			this.txtSynonymSourceDb.TabIndex = 2;

			// 
			// grpScriptPaths (existing - adjusted position)
			// 
			this.grpScriptPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpScriptPaths.Controls.Add(this.txtJobScriptsFolder);
			this.grpScriptPaths.Controls.Add(this.lblJobScriptsFolder);
			this.grpScriptPaths.Controls.Add(this.btnBrowseJobScripts);
			this.grpScriptPaths.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpScriptPaths.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpScriptPaths.Location = new System.Drawing.Point(520, 345);  // ADJUSTED position
			this.grpScriptPaths.Name = "grpScriptPaths";
			this.grpScriptPaths.Size = new System.Drawing.Size(480, 80);
			this.grpScriptPaths.TabIndex = 3;
			this.grpScriptPaths.TabStop = false;
			this.grpScriptPaths.Text = "📂 Script Paths";

			// 
			// lblJobScriptsFolder (existing)
			// 
			this.lblJobScriptsFolder.AutoSize = true;
			this.lblJobScriptsFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobScriptsFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblJobScriptsFolder.Location = new System.Drawing.Point(20, 35);
			this.lblJobScriptsFolder.Name = "lblJobScriptsFolder";
			this.lblJobScriptsFolder.Size = new System.Drawing.Size(126, 20);
			this.lblJobScriptsFolder.TabIndex = 0;
			this.lblJobScriptsFolder.Text = "Job Scripts Folder";

			// 
			// txtJobScriptsFolder (existing)
			// 
			this.txtJobScriptsFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtJobScriptsFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtJobScriptsFolder.Location = new System.Drawing.Point(20, 50);
			this.txtJobScriptsFolder.Name = "txtJobScriptsFolder";
			this.txtJobScriptsFolder.Size = new System.Drawing.Size(320, 27);
			this.txtJobScriptsFolder.TabIndex = 1;
			this.txtJobScriptsFolder.TextChanged += new System.EventHandler(this.txtJobScriptsFolder_TextChanged);

			// 
			// btnBrowseJobScripts (existing)
			// 
			this.btnBrowseJobScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnBrowseJobScripts.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBrowseJobScripts.FlatAppearance.BorderSize = 0;
			this.btnBrowseJobScripts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseJobScripts.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnBrowseJobScripts.ForeColor = System.Drawing.Color.White;
			this.btnBrowseJobScripts.Location = new System.Drawing.Point(350, 47);
			this.btnBrowseJobScripts.Name = "btnBrowseJobScripts";
			this.btnBrowseJobScripts.Size = new System.Drawing.Size(120, 32);
			this.btnBrowseJobScripts.TabIndex = 2;
			this.btnBrowseJobScripts.Text = "📁 Browse...";
			this.btnBrowseJobScripts.UseVisualStyleBackColor = false;
			this.btnBrowseJobScripts.Click += new System.EventHandler(this.btnBrowseJobScripts_Click);

			// 
			// grpSQLAgentJobs (existing - adjusted position)
			// 
			this.grpSQLAgentJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpSQLAgentJobs.Controls.Add(this.lblJobDescriptions);
			this.grpSQLAgentJobs.Controls.Add(this.txtJobOwnerLoginName);
			this.grpSQLAgentJobs.Controls.Add(this.lblJobOwnerLoginName);
			this.grpSQLAgentJobs.Controls.Add(this.chkCreateSqlAgentJobs);
			this.grpSQLAgentJobs.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSQLAgentJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSQLAgentJobs.Location = new System.Drawing.Point(20, 435);  // ADJUSTED position
			this.grpSQLAgentJobs.Name = "grpSQLAgentJobs";
			this.grpSQLAgentJobs.Size = new System.Drawing.Size(980, 120);
			this.grpSQLAgentJobs.TabIndex = 4;
			this.grpSQLAgentJobs.TabStop = false;
			this.grpSQLAgentJobs.Text = "⚡ SQL Agent Jobs";

			// 
			// chkCreateSqlAgentJobs (existing)
			// 
			this.chkCreateSqlAgentJobs.AutoSize = true;
			this.chkCreateSqlAgentJobs.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateSqlAgentJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkCreateSqlAgentJobs.Location = new System.Drawing.Point(25, 35);
			this.chkCreateSqlAgentJobs.Name = "chkCreateSqlAgentJobs";
			this.chkCreateSqlAgentJobs.Size = new System.Drawing.Size(205, 24);
			this.chkCreateSqlAgentJobs.TabIndex = 0;
			this.chkCreateSqlAgentJobs.Text = "⚡ Create SQL Agent Jobs";
			this.chkCreateSqlAgentJobs.UseVisualStyleBackColor = true;
			this.chkCreateSqlAgentJobs.CheckedChanged += new System.EventHandler(this.chkCreateSqlAgentJobs_CheckedChanged);

			// 
			// lblJobOwnerLoginName (existing)
			// 
			this.lblJobOwnerLoginName.AutoSize = true;
			this.lblJobOwnerLoginName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobOwnerLoginName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.lblJobOwnerLoginName.Location = new System.Drawing.Point(250, 40);
			this.lblJobOwnerLoginName.Name = "lblJobOwnerLoginName";
			this.lblJobOwnerLoginName.Size = new System.Drawing.Size(120, 20);
			this.lblJobOwnerLoginName.TabIndex = 1;
			this.lblJobOwnerLoginName.Text = "Job Owner Login";

			// 
			// txtJobOwnerLoginName (existing)
			// 
			this.txtJobOwnerLoginName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtJobOwnerLoginName.Enabled = false;
			this.txtJobOwnerLoginName.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.txtJobOwnerLoginName.Location = new System.Drawing.Point(400, 37);
			this.txtJobOwnerLoginName.Name = "txtJobOwnerLoginName";
			this.txtJobOwnerLoginName.Size = new System.Drawing.Size(200, 27);
			this.txtJobOwnerLoginName.TabIndex = 2;

			// 
			// lblJobDescriptions (existing)
			// 
			this.lblJobDescriptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.lblJobDescriptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblJobDescriptions.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblJobDescriptions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.lblJobDescriptions.Location = new System.Drawing.Point(25, 70);
			this.lblJobDescriptions.Name = "lblJobDescriptions";
			this.lblJobDescriptions.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.lblJobDescriptions.Size = new System.Drawing.Size(940, 40);
			this.lblJobDescriptions.TabIndex = 3;
			this.lblJobDescriptions.Text = "💡 Select a job scripts folder to see available jobs...";
			this.lblJobDescriptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			// 
			// grpSmartProcedures (existing - adjusted position)
			// 
			this.grpSmartProcedures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.grpSmartProcedures.Controls.Add(this.chkExecuteProcedures);
			this.grpSmartProcedures.Controls.Add(this.btnConfigureSmartProcedures);
			this.grpSmartProcedures.Controls.Add(this.lblSmartProcedureStatus);
			this.grpSmartProcedures.Controls.Add(this.chkCreateBackup);
			this.grpSmartProcedures.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.grpSmartProcedures.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.grpSmartProcedures.Location = new System.Drawing.Point(20, 565);  // ADJUSTED position
			this.grpSmartProcedures.Name = "grpSmartProcedures";
			this.grpSmartProcedures.Size = new System.Drawing.Size(980, 140);
			this.grpSmartProcedures.TabIndex = 5;
			this.grpSmartProcedures.TabStop = false;
			this.grpSmartProcedures.Text = "🧠 Smart Stored Procedures";

			// 
			// chkExecuteProcedures (existing)
			// 
			this.chkExecuteProcedures.AutoSize = true;
			this.chkExecuteProcedures.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkExecuteProcedures.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkExecuteProcedures.Location = new System.Drawing.Point(25, 35);
			this.chkExecuteProcedures.Name = "chkExecuteProcedures";
			this.chkExecuteProcedures.Size = new System.Drawing.Size(184, 24);
			this.chkExecuteProcedures.TabIndex = 0;
			this.chkExecuteProcedures.Text = "🧠 Execute Procedures";
			this.chkExecuteProcedures.UseVisualStyleBackColor = true;
			this.chkExecuteProcedures.CheckedChanged += new System.EventHandler(this.chkExecuteProcedures_CheckedChanged);

			// 
			// btnConfigureSmartProcedures (existing)
			// 
			this.btnConfigureSmartProcedures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
			this.btnConfigureSmartProcedures.Cursor = System.Windows.Forms.Cursors.Hand;
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
			// chkCreateBackup (existing)
			// 
			this.chkCreateBackup.AutoSize = true;
			this.chkCreateBackup.Checked = true;  // DEFAULT to checked
			this.chkCreateBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkCreateBackup.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.chkCreateBackup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.chkCreateBackup.Location = new System.Drawing.Point(25, 70);
			this.chkCreateBackup.Name = "chkCreateBackup";
			this.chkCreateBackup.Size = new System.Drawing.Size(246, 24);
			this.chkCreateBackup.TabIndex = 3;
			this.chkCreateBackup.Text = "🛡️ Create Backup Before Deploy";
			this.chkCreateBackup.UseVisualStyleBackColor = true;

			// 
			// lblSmartProcedureStatus (existing)
			// 
			this.lblSmartProcedureStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.lblSmartProcedureStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblSmartProcedureStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblSmartProcedureStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.lblSmartProcedureStatus.Location = new System.Drawing.Point(300, 85);
			this.lblSmartProcedureStatus.Name = "lblSmartProcedureStatus";
			this.lblSmartProcedureStatus.Padding = new System.Windows.Forms.Padding(15, 10, 10, 10);
			this.lblSmartProcedureStatus.Size = new System.Drawing.Size(650, 40);
			this.lblSmartProcedureStatus.TabIndex = 2;
			this.lblSmartProcedureStatus.Text = "💡 Click \'Configure Smart Procedures\' to get started with intelligent database deployment";
			this.lblSmartProcedureStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			// 
			// Action Buttons (existing - adjusted positions)
			// 
			this.btnPublish.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
			this.btnPublish.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnPublish.FlatAppearance.BorderSize = 0;
			this.btnPublish.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPublish.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
			this.btnPublish.ForeColor = System.Drawing.Color.White;
			this.btnPublish.Location = new System.Drawing.Point(20, 720);  // ADJUSTED position
			this.btnPublish.Name = "btnPublish";
			this.btnPublish.Size = new System.Drawing.Size(180, 50);
			this.btnPublish.TabIndex = 6;
			this.btnPublish.Text = "🚀 Deploy Now";
			this.btnPublish.UseVisualStyleBackColor = false;
			this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);

			// 
			// btnSaveConfig (existing)
			// 
			this.btnSaveConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
			this.btnSaveConfig.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnSaveConfig.FlatAppearance.BorderSize = 0;
			this.btnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSaveConfig.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnSaveConfig.ForeColor = System.Drawing.Color.White;
			this.btnSaveConfig.Location = new System.Drawing.Point(220, 720);  // ADJUSTED position
			this.btnSaveConfig.Name = "btnSaveConfig";
			this.btnSaveConfig.Size = new System.Drawing.Size(150, 50);
			this.btnSaveConfig.TabIndex = 7;
			this.btnSaveConfig.Text = "💾 Save Configuration";
			this.btnSaveConfig.UseVisualStyleBackColor = false;
			this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);

			// 
			// btnLoadConfig (existing)
			// 
			this.btnLoadConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
			this.btnLoadConfig.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnLoadConfig.FlatAppearance.BorderSize = 0;
			this.btnLoadConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLoadConfig.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.btnLoadConfig.ForeColor = System.Drawing.Color.White;
			this.btnLoadConfig.Location = new System.Drawing.Point(390, 720);  // ADJUSTED position
			this.btnLoadConfig.Name = "btnLoadConfig";
			this.btnLoadConfig.Size = new System.Drawing.Size(150, 50);
			this.btnLoadConfig.TabIndex = 8;
			this.btnLoadConfig.Text = "📂 Load Configuration";
			this.btnLoadConfig.UseVisualStyleBackColor = false;
			this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);

			// 
			// statusStrip (existing)
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
			this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripStatusLabel,
			this.toolStripProgressBar});
			this.statusStrip.Location = new System.Drawing.Point(0, 760);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1029, 26);
			this.statusStrip.TabIndex = 9;

			// 
			// toolStripStatusLabel (existing)
			// 
			this.toolStripStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripStatusLabel.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(75, 20);
			this.toolStripStatusLabel.Text = "🟢 Ready";

			// 
			// toolStripProgressBar (existing)
			// 
			this.toolStripProgressBar.Name = "toolStripProgressBar";
			this.toolStripProgressBar.Size = new System.Drawing.Size(200, 18);
			this.toolStripProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.toolStripProgressBar.Visible = false;

			// 
			// tabControl (existing)
			// 
			this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl.Controls.Add(this.tabSetup);
			this.tabControl.Controls.Add(this.tabLog);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.tabControl.ItemSize = new System.Drawing.Size(120, 35);
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1029, 760);
			this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabControl.TabIndex = 10;

			// 
			// tabSetup (existing - updated with new auto scroll height)
			// 
			this.tabSetup.AutoScroll = true;
			this.tabSetup.AutoScrollMinSize = new System.Drawing.Size(1000, 800);  // INCREASED height
			this.tabSetup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.tabSetup.Controls.Add(this.grpConnection);
			this.tabSetup.Controls.Add(this.grpDacPac);
			this.tabSetup.Controls.Add(this.grpSynonyms);
			this.tabSetup.Controls.Add(this.grpScriptPaths);
			this.tabSetup.Controls.Add(this.grpSQLAgentJobs);
			this.tabSetup.Controls.Add(this.grpSmartProcedures);
			this.tabSetup.Controls.Add(this.btnPublish);
			this.tabSetup.Controls.Add(this.btnSaveConfig);
			this.tabSetup.Controls.Add(this.btnLoadConfig);
			this.tabSetup.Location = new System.Drawing.Point(4, 39);
			this.tabSetup.Name = "tabSetup";
			this.tabSetup.Padding = new System.Windows.Forms.Padding(3);
			this.tabSetup.Size = new System.Drawing.Size(1021, 717);
			this.tabSetup.TabIndex = 0;
			this.tabSetup.Text = "⚙️ Setup";

			// 
			// tabLog (existing)
			// 
			this.tabLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.tabLog.Controls.Add(this.txtLog);
			this.tabLog.Location = new System.Drawing.Point(4, 39);
			this.tabLog.Name = "tabLog";
			this.tabLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabLog.Size = new System.Drawing.Size(1021, 717);
			this.tabLog.TabIndex = 1;
			this.tabLog.Text = "📋 Activity Log";

			// 
			// txtLog (existing)
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.txtLog.Location = new System.Drawing.Point(15, 15);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(990, 640);
			this.txtLog.TabIndex = 0;

			// 
			// File Dialogs (existing)
			// 
			this.openDacpacDialog.DefaultExt = "dacpac";
			this.openDacpacDialog.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
			this.openDacpacDialog.Title = "Select Primary DACPAC Package File";

			this.openConfigDialog.DefaultExt = "json";
			this.openConfigDialog.Filter = "Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
			this.openConfigDialog.Title = "Load Deployment Configuration";

			this.saveConfigDialog.DefaultExt = "json";
			this.saveConfigDialog.Filter = "Configuration Files (*.json)|*.json|All Files (*.*)|*.*";
			this.saveConfigDialog.Title = "Save Deployment Configuration";

			this.folderBrowserDialog.Description = "Select SQL Agent Job Scripts Folder";

			this.saveLogDialog.DefaultExt = "txt";
			this.saveLogDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
			this.saveLogDialog.Title = "Export Activity Log";

			// 
			// DacpacPublisherForm (main form)
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.ClientSize = new System.Drawing.Size(1029, 786);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.statusStrip);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.MinimumSize = new System.Drawing.Size(1045, 825);
			this.Name = "DacpacPublisherForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DACPAC Publisher - Professional Edition";
			this.Load += new System.EventHandler(this.DacpacPublisherForm_Load);

			// Resume layouts
			this.grpConnection.ResumeLayout(false);
			this.grpConnection.PerformLayout();
			this.grpDacPac.ResumeLayout(false);
			this.grpDacPac.PerformLayout();
			this.grpSynonyms.ResumeLayout(false);
			this.grpSynonyms.PerformLayout();
			this.grpSQLAgentJobs.ResumeLayout(false);
			this.grpSQLAgentJobs.PerformLayout();
			this.grpSmartProcedures.ResumeLayout(false);
			this.grpSmartProcedures.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabSetup.ResumeLayout(false);
			this.grpScriptPaths.ResumeLayout(false);
			this.grpScriptPaths.PerformLayout();
			this.tabLog.ResumeLayout(false);
			this.tabLog.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		// UPDATED: Helper methods for enhanced functionality
		private void AddHoverEffects()
		{
			foreach (Control control in this.Controls)
			{
				AddHoverEffectsRecursive(control);
			}
		}

		private void AddHoverEffectsRecursive(Control parent)
		{
			foreach (Control control in parent.Controls)
			{
				if (control is Button btn && btn.FlatStyle == FlatStyle.Flat)
				{
					var originalColor = btn.BackColor;
					var hoverColor = ControlPaint.Light(originalColor, 0.1f);

					btn.MouseEnter += (s, e) => {
						btn.BackColor = hoverColor;
						btn.Cursor = Cursors.Hand;
					};

					btn.MouseLeave += (s, e) => {
						btn.BackColor = originalColor;
					};
				}

				// Recursively apply to child controls
				if (control.HasChildren)
				{
					AddHoverEffectsRecursive(control);
				}
			}
		}

		private void InitializeCustomHeader()
		{
			headerPanel = new Panel();
			headerPanel.Dock = DockStyle.Top;
			headerPanel.Height = 60;
			headerPanel.BackColor = Helper.UIHelper.ISTBlue;
			headerPanel.Padding = new Padding(10);

			logoPictureBox = new PictureBox();
			logoPictureBox.Image = Properties.Resources.ISTLogo;
			logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			logoPictureBox.Size = new Size(50, 40);
			logoPictureBox.Location = new Point(10, 10);

			headerTitleLabel = new Label();
			headerTitleLabel.Text = "DACPAC Deployment Tool";
			headerTitleLabel.ForeColor = Color.White;
			headerTitleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
			headerTitleLabel.AutoSize = true;

			headerSubtitleLabel = new Label();
			headerSubtitleLabel.Text = "Smart Database Deployment with Multi-Database Support";
			headerSubtitleLabel.ForeColor = Color.White;
			headerSubtitleLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
			headerSubtitleLabel.AutoSize = true;

			int logoRightEdge = logoPictureBox.Location.X + logoPictureBox.Width + 20;
			headerTitleLabel.Location = new Point(logoRightEdge + 50, 10);
			headerSubtitleLabel.Location = new Point(logoRightEdge + 50, 35);

			headerPanel.Controls.Add(logoPictureBox);
			headerPanel.Controls.Add(headerTitleLabel);
			headerPanel.Controls.Add(headerSubtitleLabel);
			this.Controls.Add(headerPanel);
		}

		// OPTIONAL: Data Viewer Tab Initialization (comment out if not needed)
		private void InitializeDataViewerTab()
		{
			// Create the data viewer tab
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

			((System.ComponentModel.ISupportInitialize)(this.dgvTableData)).BeginInit();
			this.pnlRecommendations.SuspendLayout();
			this.tabDataViewer.SuspendLayout();

			// Configure tabDataViewer
			this.tabDataViewer.Controls.Add(this.dgvTableData);
			this.tabDataViewer.Controls.Add(this.cboTables);
			this.tabDataViewer.Controls.Add(this.btnRefreshTables);
			this.tabDataViewer.Controls.Add(this.btnQueryTable);
			this.tabDataViewer.Controls.Add(this.txtCustomQuery);
			this.tabDataViewer.Controls.Add(this.pnlRecommendations);
			this.tabDataViewer.Controls.Add(this.lblTableCount);
			this.tabDataViewer.Controls.Add(this.lblRowCount);
			this.tabDataViewer.Controls.Add(this.progressQuery);
			this.tabDataViewer.Location = new System.Drawing.Point(4, 39);
			this.tabDataViewer.Name = "tabDataViewer";
			this.tabDataViewer.Size = new System.Drawing.Size(1021, 717);
			this.tabDataViewer.TabIndex = 2;
			this.tabDataViewer.Text = "📊 Data Viewer";
			this.tabDataViewer.UseVisualStyleBackColor = true;

			// Table Selection Controls
			var lblSelectTable = new Label
			{
				Text = "Select Table:",
				Location = new Point(20, 15),
				Size = new Size(100, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			this.cboTables.Location = new Point(130, 12);
			this.cboTables.Size = new Size(300, 25);
			this.cboTables.DropDownStyle = ComboBoxStyle.DropDownList;

			this.btnRefreshTables.Text = "🔄 Refresh Tables";
			this.btnRefreshTables.Location = new Point(440, 10);
			this.btnRefreshTables.Size = new Size(120, 30);
			this.btnRefreshTables.BackColor = UIHelper.ISTBlue;
			this.btnRefreshTables.ForeColor = Color.White;
			this.btnRefreshTables.FlatStyle = FlatStyle.Flat;

			this.btnQueryTable.Text = "📋 Query Table";
			this.btnQueryTable.Location = new Point(570, 10);
			this.btnQueryTable.Size = new Size(120, 30);
			this.btnQueryTable.BackColor = UIHelper.SuccessGreen;
			this.btnQueryTable.ForeColor = Color.White;
			this.btnQueryTable.FlatStyle = FlatStyle.Flat;

			// Custom Query Section
			var lblCustomQuery = new Label
			{
				Text = "Custom Query:",
				Location = new Point(20, 55),
				Size = new Size(100, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			this.txtCustomQuery.Location = new Point(130, 52);
			this.txtCustomQuery.Size = new Size(450, 25);
			this.txtCustomQuery.Text = "SELECT * FROM [Select a table first]";
			this.txtCustomQuery.ForeColor = Color.Gray;
			this.txtCustomQuery.Name = "txtCustomQuery";
			this.txtCustomQuery.ReadOnly = true;

			var btnExecuteQuery = new Button
			{
				Text = "▶️ Execute",
				Location = new Point(590, 50),
				Size = new Size(100, 30),
				BackColor = Color.Orange,
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat
			};

			// Data Grid
			this.dgvTableData.Location = new Point(20, 95);
			this.dgvTableData.Size = new Size(670, 350);
			this.dgvTableData.AllowUserToAddRows = false;
			this.dgvTableData.AllowUserToDeleteRows = false;
			this.dgvTableData.ReadOnly = true;
			this.dgvTableData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvTableData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

			// Recommendations Panel
			this.pnlRecommendations.Location = new Point(700, 95);
			this.pnlRecommendations.Size = new Size(300, 350);
			this.pnlRecommendations.BorderStyle = BorderStyle.FixedSingle;
			this.pnlRecommendations.BackColor = Color.LightYellow;
			this.pnlRecommendations.Visible = false;

			var lblRecommendations = new Label
			{
				Text = "🎯 Smart Recommendations",
				Location = new Point(10, 10),
				Size = new Size(280, 25),
				Font = new Font("Segoe UI", 10F, FontStyle.Bold),
				ForeColor = UIHelper.ISTDarkBlue,
				Visible = false
			};

			this.rtbRecommendations.Location = new Point(10, 40);
			this.rtbRecommendations.Size = new Size(275, 295);
			this.rtbRecommendations.ReadOnly = true;
			this.rtbRecommendations.Font = new Font("Segoe UI", 9F);

			// Status Labels
			this.lblTableCount.Location = new Point(20, 460);
			this.lblTableCount.Size = new Size(200, 20);
			this.lblTableCount.Text = "Tables: 0";

			this.lblRowCount.Location = new Point(250, 460);
			this.lblRowCount.Size = new Size(200, 20);
			this.lblRowCount.Text = "Rows: 0";

			this.progressQuery.Location = new Point(500, 460);
			this.progressQuery.Size = new Size(200, 20);
			this.progressQuery.Visible = false;

			// Add controls to tab
			this.tabDataViewer.Controls.AddRange(new Control[] {
				lblSelectTable, lblCustomQuery, lblRecommendations,
				btnExecuteQuery
			});

			this.pnlRecommendations.Controls.AddRange(new Control[] {
				lblRecommendations, this.rtbRecommendations
			});

			// Add tab to main tab control
			this.tabControl.Controls.Add(this.tabDataViewer);

			// Wire up events
			this.btnRefreshTables.Click += BtnRefreshTables_Click;
			this.btnQueryTable.Click += BtnQueryTable_Click;
			btnExecuteQuery.Click += BtnExecuteQuery_Click;
			this.cboTables.SelectedIndexChanged += CboTables_SelectedIndexChanged;

			((System.ComponentModel.ISupportInitialize)(this.dgvTableData)).EndInit();
			this.pnlRecommendations.ResumeLayout(false);
			this.tabDataViewer.ResumeLayout(false);
		}
		#endregion

		// UPDATED Control declarations with multi-database support
		private System.Windows.Forms.GroupBox grpConnection;
		private System.Windows.Forms.Button btnTestConnection;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.CheckBox chkWindowsAuth;
		private System.Windows.Forms.ComboBox cboDatabases;
		private System.Windows.Forms.Button btnRefreshDatabases;
		private System.Windows.Forms.Label lblDatabase;
		private System.Windows.Forms.TextBox txtServerName;
		private System.Windows.Forms.Label lblServerName;

		private System.Windows.Forms.GroupBox grpDacPac;
		private System.Windows.Forms.Button btnBrowseDacpac;
		private System.Windows.Forms.TextBox txtDacpacPath;
		private System.Windows.Forms.Label lblDacpacPath;

		private System.Windows.Forms.GroupBox grpSynonyms;
		private System.Windows.Forms.TextBox txtSynonymSourceDb;
		private System.Windows.Forms.Label lblSynonymSourceDb;
		private System.Windows.Forms.CheckBox chkCreateSynonyms;

		private System.Windows.Forms.GroupBox grpSQLAgentJobs;
		private System.Windows.Forms.Label lblJobDescriptions;
		private System.Windows.Forms.TextBox txtJobOwnerLoginName;
		private System.Windows.Forms.Label lblJobOwnerLoginName;
		private System.Windows.Forms.CheckBox chkCreateSqlAgentJobs;

		// Enhanced Smart Procedures Group
		private System.Windows.Forms.GroupBox grpSmartProcedures;
		private System.Windows.Forms.CheckBox chkExecuteProcedures;
		private System.Windows.Forms.Button btnConfigureSmartProcedures;
		private System.Windows.Forms.Label lblSmartProcedureStatus;
		private System.Windows.Forms.CheckBox chkCreateBackup;

		private System.Windows.Forms.Button btnPublish;
		private System.Windows.Forms.Button btnSaveConfig;
		private System.Windows.Forms.Button btnLoadConfig;

		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabSetup;
		private System.Windows.Forms.TabPage tabLog;
		private System.Windows.Forms.TextBox txtLog;

		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.GroupBox grpScriptPaths;
		private System.Windows.Forms.TextBox txtJobScriptsFolder;
		private System.Windows.Forms.Label lblJobScriptsFolder;
		private System.Windows.Forms.Button btnBrowseJobScripts;

		private System.Windows.Forms.OpenFileDialog openDacpacDialog;
		private System.Windows.Forms.OpenFileDialog openConfigDialog;
		private System.Windows.Forms.SaveFileDialog saveConfigDialog;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.SaveFileDialog saveLogDialog;
	}
}