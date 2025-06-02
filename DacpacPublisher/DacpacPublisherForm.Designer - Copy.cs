using System.Drawing;
using System.Windows.Forms;

namespace DacpacPublisher
{
    
        partial class DacpacPublisherForm
        {
            private System.ComponentModel.IContainer components = null;
            private PictureBox logoPictureBox;
            private Label headerTitleLabel;
            private Label headerSubtitleLabel;
            private Panel headerPanel;
            private System.Windows.Forms.GroupBox grpMultipleDeployment;
            private System.Windows.Forms.DataGridView dgvDeploymentTargets;

            private System.Windows.Forms.GroupBox grpTarget2;
            private System.Windows.Forms.CheckBox chkEnableMultipleDatabases;
            private System.Windows.Forms.Label lblDatabase2;
            private System.Windows.Forms.ComboBox cboDatabases2;
            private System.Windows.Forms.Button btnRefreshDatabases2;
            private System.Windows.Forms.Label lblDacpacPath2;
            private System.Windows.Forms.TextBox txtDacpacPath2;
            private System.Windows.Forms.Button btnBrowseDacpac2;
            private System.Windows.Forms.OpenFileDialog openDacpacDialog2;

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

                // Initialize all controls first
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

                // Multiple database controls
                this.chkEnableMultipleDatabases = new System.Windows.Forms.CheckBox();
                this.lblDatabase2 = new System.Windows.Forms.Label();
                this.cboDatabases2 = new System.Windows.Forms.ComboBox();
                this.btnRefreshDatabases2 = new System.Windows.Forms.Button();

                this.grpDacPac = new System.Windows.Forms.GroupBox();
                this.btnBrowseDacpac = new System.Windows.Forms.Button();
                this.txtDacpacPath = new System.Windows.Forms.TextBox();
                this.lblDacpacPath = new System.Windows.Forms.Label();

                // Second DACPAC controls
                this.lblDacpacPath2 = new System.Windows.Forms.Label();
                this.txtDacpacPath2 = new System.Windows.Forms.TextBox();
                this.btnBrowseDacpac2 = new System.Windows.Forms.Button();
                this.openDacpacDialog2 = new System.Windows.Forms.OpenFileDialog();

                this.grpSynonyms = new System.Windows.Forms.GroupBox();
                this.txtSynonymSourceDb = new System.Windows.Forms.TextBox();
                this.lblSynonymSourceDb = new System.Windows.Forms.Label();
                this.chkCreateSynonyms = new System.Windows.Forms.CheckBox();

                this.grpSQLAgentJobs = new System.Windows.Forms.GroupBox();
                this.lblJobDescriptions = new System.Windows.Forms.Label();
                this.txtJobOwnerLoginName = new System.Windows.Forms.TextBox();
                this.lblJobOwnerLoginName = new System.Windows.Forms.Label();
                this.chkCreateSqlAgentJobs = new System.Windows.Forms.CheckBox();

                this.grpStoredProcedures = new System.Windows.Forms.GroupBox();
                this.listStoredProcedures = new System.Windows.Forms.ListBox();
                this.contextMenuStoredProcs = new System.Windows.Forms.ContextMenuStrip(this.components);
                this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.btnAddProcedure = new System.Windows.Forms.Button();
                this.chkExecuteProcedures = new System.Windows.Forms.CheckBox(); // FIXED: Proper initialization
                this.lblProcedureStrategy = new System.Windows.Forms.Label();
                this.cboProcedureStrategy = new System.Windows.Forms.ComboBox();

                this.btnPublish = new System.Windows.Forms.Button();
                this.btnSaveConfig = new System.Windows.Forms.Button();
                this.btnLoadConfig = new System.Windows.Forms.Button();

                this.statusStrip = new System.Windows.Forms.StatusStrip();
                this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
                this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();

                this.tabControl = new System.Windows.Forms.TabControl();
                this.tabSetup = new System.Windows.Forms.TabPage();
                this.grpScriptPaths = new System.Windows.Forms.GroupBox();
                this.txtJobScriptsFolder = new System.Windows.Forms.TextBox();
                this.lblJobScriptsFolder = new System.Windows.Forms.Label();
                this.btnBrowseJobScripts = new System.Windows.Forms.Button();

                this.tabLog = new System.Windows.Forms.TabPage();
                this.txtLog = new System.Windows.Forms.TextBox();
                this.btnClearLog = new System.Windows.Forms.Button();
                this.btnExportLog = new System.Windows.Forms.Button();

                this.footerPanel = new System.Windows.Forms.Panel();
                this.lblStatus = new System.Windows.Forms.Label();
                this.toolTip = new System.Windows.Forms.ToolTip(this.components);

                this.openDacpacDialog = new System.Windows.Forms.OpenFileDialog();
                this.openConfigDialog = new System.Windows.Forms.OpenFileDialog();
                this.saveConfigDialog = new System.Windows.Forms.SaveFileDialog();
                this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                this.saveLogDialog = new System.Windows.Forms.SaveFileDialog();

                // Suspend layout
                this.grpConnection.SuspendLayout();
                this.grpDacPac.SuspendLayout();
                this.grpSynonyms.SuspendLayout();
                this.grpSQLAgentJobs.SuspendLayout();
                this.grpStoredProcedures.SuspendLayout();
                this.contextMenuStoredProcs.SuspendLayout();
                this.statusStrip.SuspendLayout();
                this.tabControl.SuspendLayout();
                this.tabSetup.SuspendLayout();
                this.grpScriptPaths.SuspendLayout();
                this.tabLog.SuspendLayout();
                this.footerPanel.SuspendLayout();
                this.SuspendLayout();

                // 
                // grpConnection
                // 
                this.grpConnection.Controls.Add(this.chkEnableMultipleDatabases);
                this.grpConnection.Controls.Add(this.lblDatabase2);
                this.grpConnection.Controls.Add(this.cboDatabases2);
                this.grpConnection.Controls.Add(this.btnRefreshDatabases2);
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
                this.grpConnection.Location = new System.Drawing.Point(11, 7);
                this.grpConnection.Name = "grpConnection";
                this.grpConnection.Size = new System.Drawing.Size(1006, 185);
                this.grpConnection.TabIndex = 0;
                this.grpConnection.TabStop = false;
                this.grpConnection.Text = "SQL Server Connection";

                // 
                // lblServerName
                // 
                this.lblServerName.AutoSize = true;
                this.lblServerName.Location = new System.Drawing.Point(20, 30);
                this.lblServerName.Name = "lblServerName";
                this.lblServerName.Size = new System.Drawing.Size(95, 17);
                this.lblServerName.TabIndex = 0;
                this.lblServerName.Text = "Server Name:";

                // 
                // txtServerName
                // 
                this.txtServerName.Location = new System.Drawing.Point(163, 26);
                this.txtServerName.Name = "txtServerName";
                this.txtServerName.Size = new System.Drawing.Size(245, 22);
                this.txtServerName.TabIndex = 1;
                this.txtServerName.Text = "(local)";

                // 
                // chkWindowsAuth
                // 
                this.chkWindowsAuth.AutoSize = true;
                this.chkWindowsAuth.Checked = true;
                this.chkWindowsAuth.CheckState = System.Windows.Forms.CheckState.Checked;
                this.chkWindowsAuth.Location = new System.Drawing.Point(580, 27);
                this.chkWindowsAuth.Name = "chkWindowsAuth";
                this.chkWindowsAuth.Size = new System.Drawing.Size(209, 21);
                this.chkWindowsAuth.TabIndex = 2;
                this.chkWindowsAuth.Text = "Use Windows Authentication";
                this.chkWindowsAuth.UseVisualStyleBackColor = true;
                this.chkWindowsAuth.CheckedChanged += new System.EventHandler(this.chkWindowsAuth_CheckedChanged);

                // 
                // btnTestConnection
                // 
                this.btnTestConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnTestConnection.Location = new System.Drawing.Point(832, 23);
                this.btnTestConnection.Name = "btnTestConnection";
                this.btnTestConnection.Size = new System.Drawing.Size(157, 35);
                this.btnTestConnection.TabIndex = 3;
                this.btnTestConnection.Text = "Test Connection";
                this.btnTestConnection.UseVisualStyleBackColor = false;
                this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);

                // 
                // lblUsername
                // 
                this.lblUsername.AutoSize = true;
                this.lblUsername.Location = new System.Drawing.Point(493, 86);
                this.lblUsername.Name = "lblUsername";
                this.lblUsername.Size = new System.Drawing.Size(77, 17);
                this.lblUsername.TabIndex = 5;
                this.lblUsername.Text = "Username:";

                // 
                // txtUsername
                // 
                this.txtUsername.Location = new System.Drawing.Point(577, 82);
                this.txtUsername.Name = "txtUsername";
                this.txtUsername.Size = new System.Drawing.Size(245, 22);
                this.txtUsername.TabIndex = 6;

                // 
                // lblPassword
                // 
                this.lblPassword.AutoSize = true;
                this.lblPassword.Location = new System.Drawing.Point(493, 122);
                this.lblPassword.Name = "lblPassword";
                this.lblPassword.Size = new System.Drawing.Size(73, 17);
                this.lblPassword.TabIndex = 7;
                this.lblPassword.Text = "Password:";

                // 
                // txtPassword
                // 
                this.txtPassword.Location = new System.Drawing.Point(577, 118);
                this.txtPassword.Name = "txtPassword";
                this.txtPassword.PasswordChar = '*';
                this.txtPassword.Size = new System.Drawing.Size(245, 22);
                this.txtPassword.TabIndex = 8;

                // 
                // lblDatabase
                // 
                this.lblDatabase.AutoSize = true;
                this.lblDatabase.Location = new System.Drawing.Point(20, 86);
                this.lblDatabase.Name = "lblDatabase";
                this.lblDatabase.Size = new System.Drawing.Size(119, 17);
                this.lblDatabase.TabIndex = 9;
                this.lblDatabase.Text = "Target Database:";

                // 
                // cboDatabases
                // 
                this.cboDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.cboDatabases.FormattingEnabled = true;
                this.cboDatabases.Location = new System.Drawing.Point(163, 82);
                this.cboDatabases.Name = "cboDatabases";
                this.cboDatabases.Size = new System.Drawing.Size(245, 24);
                this.cboDatabases.TabIndex = 10;

                // 
                // btnRefreshDatabases
                // 
                this.btnRefreshDatabases.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnRefreshDatabases.Location = new System.Drawing.Point(163, 116);
                this.btnRefreshDatabases.Name = "btnRefreshDatabases";
                this.btnRefreshDatabases.Size = new System.Drawing.Size(247, 28);
                this.btnRefreshDatabases.TabIndex = 11;
                this.btnRefreshDatabases.Text = "Refresh Databases";
                this.btnRefreshDatabases.UseVisualStyleBackColor = false;
                this.btnRefreshDatabases.Click += new System.EventHandler(this.btnRefreshDatabases_Click);

                // 
                // chkEnableMultipleDatabases
                // 
                this.chkEnableMultipleDatabases.AutoSize = true;
                this.chkEnableMultipleDatabases.Location = new System.Drawing.Point(163, 150);
                this.chkEnableMultipleDatabases.Name = "chkEnableMultipleDatabases";
                this.chkEnableMultipleDatabases.Size = new System.Drawing.Size(200, 21);
                this.chkEnableMultipleDatabases.TabIndex = 12;
                this.chkEnableMultipleDatabases.Text = "Deploy to Multiple Databases";
                this.chkEnableMultipleDatabases.UseVisualStyleBackColor = true;
                this.chkEnableMultipleDatabases.CheckedChanged += new System.EventHandler(this.chkEnableMultipleDatabases_CheckedChanged);

                // 
                // lblDatabase2
                // 
                this.lblDatabase2.AutoSize = true;
                this.lblDatabase2.Enabled = false;
                this.lblDatabase2.Location = new System.Drawing.Point(430, 152);
                this.lblDatabase2.Name = "lblDatabase2";
                this.lblDatabase2.Size = new System.Drawing.Size(120, 17);
                this.lblDatabase2.TabIndex = 13;
                this.lblDatabase2.Text = "Second Database:";

                // 
                // cboDatabases2
                // 
                this.cboDatabases2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.cboDatabases2.Enabled = false;
                this.cboDatabases2.FormattingEnabled = true;
                this.cboDatabases2.Location = new System.Drawing.Point(560, 149);
                this.cboDatabases2.Name = "cboDatabases2";
                this.cboDatabases2.Size = new System.Drawing.Size(160, 24);
                this.cboDatabases2.TabIndex = 14;

                // 
                // btnRefreshDatabases2
                // 
                this.btnRefreshDatabases2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnRefreshDatabases2.Enabled = false;
                this.btnRefreshDatabases2.Location = new System.Drawing.Point(730, 147);
                this.btnRefreshDatabases2.Name = "btnRefreshDatabases2";
                this.btnRefreshDatabases2.Size = new System.Drawing.Size(90, 28);
                this.btnRefreshDatabases2.TabIndex = 15;
                this.btnRefreshDatabases2.Text = "Refresh";
                this.btnRefreshDatabases2.UseVisualStyleBackColor = false;
                this.btnRefreshDatabases2.Click += new System.EventHandler(this.btnRefreshDatabases2_Click);

                // 
                // grpDacPac
                // 
                this.grpDacPac.Controls.Add(this.lblDacpacPath2);
                this.grpDacPac.Controls.Add(this.txtDacpacPath2);
                this.grpDacPac.Controls.Add(this.btnBrowseDacpac2);
                this.grpDacPac.Controls.Add(this.btnBrowseDacpac);
                this.grpDacPac.Controls.Add(this.txtDacpacPath);
                this.grpDacPac.Controls.Add(this.lblDacpacPath);
                this.grpDacPac.Location = new System.Drawing.Point(11, 200);
                this.grpDacPac.Name = "grpDacPac";
                this.grpDacPac.Size = new System.Drawing.Size(1006, 110);
                this.grpDacPac.TabIndex = 1;
                this.grpDacPac.TabStop = false;
                this.grpDacPac.Text = "DACPAC File(s)";

                // 
                // lblDacpacPath
                // 
                this.lblDacpacPath.AutoSize = true;
                this.lblDacpacPath.Location = new System.Drawing.Point(15, 30);
                this.lblDacpacPath.Name = "lblDacpacPath";
                this.lblDacpacPath.Size = new System.Drawing.Size(137, 17);
                this.lblDacpacPath.TabIndex = 12;
                this.lblDacpacPath.Text = "Path to .dacpac File:";

                // 
                // txtDacpacPath
                // 
                this.txtDacpacPath.Location = new System.Drawing.Point(189, 26);
                this.txtDacpacPath.Name = "txtDacpacPath";
                this.txtDacpacPath.Size = new System.Drawing.Size(634, 22);
                this.txtDacpacPath.TabIndex = 13;

                // 
                // btnBrowseDacpac
                // 
                this.btnBrowseDacpac.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnBrowseDacpac.Location = new System.Drawing.Point(832, 23);
                this.btnBrowseDacpac.Name = "btnBrowseDacpac";
                this.btnBrowseDacpac.Size = new System.Drawing.Size(157, 28);
                this.btnBrowseDacpac.TabIndex = 14;
                this.btnBrowseDacpac.Text = "Browse...";
                this.btnBrowseDacpac.UseVisualStyleBackColor = false;
                this.btnBrowseDacpac.Click += new System.EventHandler(this.btnBrowseDacpac_Click);

                // 
                // lblDacpacPath2
                // 
                this.lblDacpacPath2.AutoSize = true;
                this.lblDacpacPath2.Enabled = false;
                this.lblDacpacPath2.Location = new System.Drawing.Point(15, 70);
                this.lblDacpacPath2.Name = "lblDacpacPath2";
                this.lblDacpacPath2.Size = new System.Drawing.Size(170, 17);
                this.lblDacpacPath2.TabIndex = 15;
                this.lblDacpacPath2.Text = "Second .dacpac File Path:";

                // 
                // txtDacpacPath2
                // 
                this.txtDacpacPath2.Enabled = false;
                this.txtDacpacPath2.Location = new System.Drawing.Point(189, 67);
                this.txtDacpacPath2.Name = "txtDacpacPath2";
                this.txtDacpacPath2.Size = new System.Drawing.Size(634, 22);
                this.txtDacpacPath2.TabIndex = 16;

                // 
                // btnBrowseDacpac2
                // 
                this.btnBrowseDacpac2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnBrowseDacpac2.Enabled = false;
                this.btnBrowseDacpac2.Location = new System.Drawing.Point(832, 65);
                this.btnBrowseDacpac2.Name = "btnBrowseDacpac2";
                this.btnBrowseDacpac2.Size = new System.Drawing.Size(157, 28);
                this.btnBrowseDacpac2.TabIndex = 17;
                this.btnBrowseDacpac2.Text = "Browse...";
                this.btnBrowseDacpac2.UseVisualStyleBackColor = false;
                this.btnBrowseDacpac2.Click += new System.EventHandler(this.btnBrowseDacpac2_Click);

                // 
                // grpSynonyms
                // 
                this.grpSynonyms.Controls.Add(this.txtSynonymSourceDb);
                this.grpSynonyms.Controls.Add(this.lblSynonymSourceDb);
                this.grpSynonyms.Controls.Add(this.chkCreateSynonyms);
                this.grpSynonyms.Location = new System.Drawing.Point(11, 320);
                this.grpSynonyms.Name = "grpSynonyms";
                this.grpSynonyms.Size = new System.Drawing.Size(1006, 70);
                this.grpSynonyms.TabIndex = 2;
                this.grpSynonyms.TabStop = false;
                this.grpSynonyms.Text = "Synonyms";

                // 
                // chkCreateSynonyms
                // 
                this.chkCreateSynonyms.AutoSize = true;
                this.chkCreateSynonyms.Location = new System.Drawing.Point(163, 28);
                this.chkCreateSynonyms.Name = "chkCreateSynonyms";
                this.chkCreateSynonyms.Size = new System.Drawing.Size(141, 21);
                this.chkCreateSynonyms.TabIndex = 15;
                this.chkCreateSynonyms.Text = "Create Synonyms";
                this.chkCreateSynonyms.UseVisualStyleBackColor = true;
                this.chkCreateSynonyms.CheckedChanged += new System.EventHandler(this.chkCreateSynonyms_CheckedChanged);

                // 
                // lblSynonymSourceDb
                // 
                this.lblSynonymSourceDb.AutoSize = true;
                this.lblSynonymSourceDb.Location = new System.Drawing.Point(428, 30);
                this.lblSynonymSourceDb.Name = "lblSynonymSourceDb";
                this.lblSynonymSourceDb.Size = new System.Drawing.Size(122, 17);
                this.lblSynonymSourceDb.TabIndex = 16;
                this.lblSynonymSourceDb.Text = "Source Database:";

                // 
                // txtSynonymSourceDb
                // 
                this.txtSynonymSourceDb.Enabled = false;
                this.txtSynonymSourceDb.Location = new System.Drawing.Point(577, 26);
                this.txtSynonymSourceDb.Name = "txtSynonymSourceDb";
                this.txtSynonymSourceDb.Size = new System.Drawing.Size(245, 22);
                this.txtSynonymSourceDb.TabIndex = 17;

                // 
                // grpScriptPaths
                // 
                this.grpScriptPaths.Controls.Add(this.txtJobScriptsFolder);
                this.grpScriptPaths.Controls.Add(this.lblJobScriptsFolder);
                this.grpScriptPaths.Controls.Add(this.btnBrowseJobScripts);
                this.grpScriptPaths.Location = new System.Drawing.Point(11, 400);
                this.grpScriptPaths.Name = "grpScriptPaths";
                this.grpScriptPaths.Size = new System.Drawing.Size(1006, 70);
                this.grpScriptPaths.TabIndex = 3;
                this.grpScriptPaths.TabStop = false;
                this.grpScriptPaths.Text = "Script Paths";

                // 
                // lblJobScriptsFolder
                // 
                this.lblJobScriptsFolder.AutoSize = true;
                this.lblJobScriptsFolder.Location = new System.Drawing.Point(15, 30);
                this.lblJobScriptsFolder.Name = "lblJobScriptsFolder";
                this.lblJobScriptsFolder.Size = new System.Drawing.Size(134, 17);
                this.lblJobScriptsFolder.TabIndex = 17;
                this.lblJobScriptsFolder.Text = "Job Scripts Folder:";

                // 
                // txtJobScriptsFolder
                // 
                this.txtJobScriptsFolder.Location = new System.Drawing.Point(189, 26);
                this.txtJobScriptsFolder.Name = "txtJobScriptsFolder";
                this.txtJobScriptsFolder.Size = new System.Drawing.Size(634, 22);
                this.txtJobScriptsFolder.TabIndex = 18;

                // 
                // btnBrowseJobScripts
                // 
                this.btnBrowseJobScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnBrowseJobScripts.Location = new System.Drawing.Point(832, 23);
                this.btnBrowseJobScripts.Name = "btnBrowseJobScripts";
                this.btnBrowseJobScripts.Size = new System.Drawing.Size(157, 28);
                this.btnBrowseJobScripts.TabIndex = 19;
                this.btnBrowseJobScripts.Text = "Browse...";
                this.btnBrowseJobScripts.UseVisualStyleBackColor = false;
                this.btnBrowseJobScripts.Click += new System.EventHandler(this.btnBrowseJobScripts_Click);

                // 
                // grpSQLAgentJobs
                // 
                this.grpSQLAgentJobs.Controls.Add(this.lblJobDescriptions);
                this.grpSQLAgentJobs.Controls.Add(this.txtJobOwnerLoginName);
                this.grpSQLAgentJobs.Controls.Add(this.lblJobOwnerLoginName);
                this.grpSQLAgentJobs.Controls.Add(this.chkCreateSqlAgentJobs);
                this.grpSQLAgentJobs.Location = new System.Drawing.Point(11, 475);
                this.grpSQLAgentJobs.Name = "grpSQLAgentJobs";
                this.grpSQLAgentJobs.Size = new System.Drawing.Size(1006, 155);
                this.grpSQLAgentJobs.TabIndex = 4;
                this.grpSQLAgentJobs.TabStop = false;
                this.grpSQLAgentJobs.Text = "SQL Agent Jobs";

                // 
                // chkCreateSqlAgentJobs
                // 
                this.chkCreateSqlAgentJobs.AutoSize = true;
                this.chkCreateSqlAgentJobs.Location = new System.Drawing.Point(163, 28);
                this.chkCreateSqlAgentJobs.Name = "chkCreateSqlAgentJobs";
                this.chkCreateSqlAgentJobs.Size = new System.Drawing.Size(163, 21);
                this.chkCreateSqlAgentJobs.TabIndex = 20;
                this.chkCreateSqlAgentJobs.Text = "Create SQL Agent Jobs";
                this.chkCreateSqlAgentJobs.UseVisualStyleBackColor = true;
                this.chkCreateSqlAgentJobs.CheckedChanged += new System.EventHandler(this.chkCreateSqlAgentJobs_CheckedChanged);

                // 
                // lblJobOwnerLoginName
                // 
                this.lblJobOwnerLoginName.AutoSize = true;
                this.lblJobOwnerLoginName.Location = new System.Drawing.Point(428, 30);
                this.lblJobOwnerLoginName.Name = "lblJobOwnerLoginName";
                this.lblJobOwnerLoginName.Size = new System.Drawing.Size(144, 17);
                this.lblJobOwnerLoginName.TabIndex = 21;
                this.lblJobOwnerLoginName.Text = "Job Owner Login Name:";

                // 
                // txtJobOwnerLoginName
                // 
                this.txtJobOwnerLoginName.Enabled = false;
                this.txtJobOwnerLoginName.Location = new System.Drawing.Point(577, 26);
                this.txtJobOwnerLoginName.Name = "txtJobOwnerLoginName";
                this.txtJobOwnerLoginName.Size = new System.Drawing.Size(245, 22);
                this.txtJobOwnerLoginName.TabIndex = 22;

                // 
                // lblJobDescriptions
                // 
                this.lblJobDescriptions.Location = new System.Drawing.Point(163, 70);
                this.lblJobDescriptions.Name = "lblJobDescriptions";
                this.lblJobDescriptions.Size = new System.Drawing.Size(659, 77);
                this.lblJobDescriptions.TabIndex = 23;
                this.lblJobDescriptions.Text = "Select a job scripts folder to see available jobs...";

                // 
                // grpStoredProcedures
                // 
                this.grpStoredProcedures.Controls.Add(this.lblProcedureStrategy);
                this.grpStoredProcedures.Controls.Add(this.cboProcedureStrategy);
                this.grpStoredProcedures.Controls.Add(this.listStoredProcedures);
                this.grpStoredProcedures.Controls.Add(this.btnAddProcedure);
                this.grpStoredProcedures.Controls.Add(this.chkExecuteProcedures);
                this.grpStoredProcedures.Location = new System.Drawing.Point(11, 635);
                this.grpStoredProcedures.Name = "grpStoredProcedures";
                this.grpStoredProcedures.Size = new System.Drawing.Size(1006, 170);
                this.grpStoredProcedures.TabIndex = 5;
                this.grpStoredProcedures.TabStop = false;
                this.grpStoredProcedures.Text = "Stored Procedures";

                // 
                // chkExecuteProcedures
                // 
                this.chkExecuteProcedures.AutoSize = true;
                this.chkExecuteProcedures.Location = new System.Drawing.Point(163, 28);
                this.chkExecuteProcedures.Name = "chkExecuteProcedures";
                this.chkExecuteProcedures.Size = new System.Drawing.Size(156, 21);
                this.chkExecuteProcedures.TabIndex = 24;
                this.chkExecuteProcedures.Text = "Execute Procedures";
                this.chkExecuteProcedures.UseVisualStyleBackColor = true;
                this.chkExecuteProcedures.CheckedChanged += new System.EventHandler(this.chkExecuteProcedures_CheckedChanged);

                // 
                // btnAddProcedure
                // 
                this.btnAddProcedure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnAddProcedure.Enabled = false;
                this.btnAddProcedure.Location = new System.Drawing.Point(665, 58);
                this.btnAddProcedure.Name = "btnAddProcedure";
                this.btnAddProcedure.Size = new System.Drawing.Size(157, 28);
                this.btnAddProcedure.TabIndex = 25;
                this.btnAddProcedure.Text = "Add Procedure";
                this.btnAddProcedure.UseVisualStyleBackColor = false;
                this.btnAddProcedure.Click += new System.EventHandler(this.btnAddProcedure_Click);

                // 
                // listStoredProcedures
                // 
                this.listStoredProcedures.ContextMenuStrip = this.contextMenuStoredProcs;
                this.listStoredProcedures.Enabled = false;
                this.listStoredProcedures.FormattingEnabled = true;
                this.listStoredProcedures.ItemHeight = 16;
                this.listStoredProcedures.Location = new System.Drawing.Point(163, 58);
                this.listStoredProcedures.Name = "listStoredProcedures";
                this.listStoredProcedures.Size = new System.Drawing.Size(491, 68);
                this.listStoredProcedures.TabIndex = 26;

                // 
                // lblProcedureStrategy
                // 
                this.lblProcedureStrategy.AutoSize = true;
                this.lblProcedureStrategy.Location = new System.Drawing.Point(163, 135);
                this.lblProcedureStrategy.Name = "lblProcedureStrategy";
                this.lblProcedureStrategy.Size = new System.Drawing.Size(120, 17);
                this.lblProcedureStrategy.TabIndex = 27;
                this.lblProcedureStrategy.Text = "Procedure Strategy:";
                this.lblProcedureStrategy.Visible = false;

                // 
                // cboProcedureStrategy
                // 
                this.cboProcedureStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.cboProcedureStrategy.FormattingEnabled = true;
                this.cboProcedureStrategy.Items.AddRange(new object[] {
                "All Procedures",
                "No Procedures",
                "Minimal Setup"});
                this.cboProcedureStrategy.Location = new System.Drawing.Point(290, 132);
                this.cboProcedureStrategy.Name = "cboProcedureStrategy";
                this.cboProcedureStrategy.SelectedIndex = 0;
                this.cboProcedureStrategy.Size = new System.Drawing.Size(200, 24);
                this.cboProcedureStrategy.TabIndex = 28;
                this.cboProcedureStrategy.Visible = false;

                // 
                // contextMenuStoredProcs
                // 
                this.contextMenuStoredProcs.ImageScalingSize = new System.Drawing.Size(20, 20);
                this.contextMenuStoredProcs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.addToolStripMenuItem,
                this.editToolStripMenuItem,
                this.removeToolStripMenuItem});
                this.contextMenuStoredProcs.Name = "contextMenuStoredProcs";
                this.contextMenuStoredProcs.Size = new System.Drawing.Size(129, 76);

                // 
                // addToolStripMenuItem
                // 
                this.addToolStripMenuItem.Name = "addToolStripMenuItem";
                this.addToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
                this.addToolStripMenuItem.Text = "Add";
                this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);

                // 
                // editToolStripMenuItem
                // 
                this.editToolStripMenuItem.Name = "editToolStripMenuItem";
                this.editToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
                this.editToolStripMenuItem.Text = "Edit";
                this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);

                // 
                // removeToolStripMenuItem
                // 
                this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
                this.removeToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
                this.removeToolStripMenuItem.Text = "Remove";
                this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);

                // 
                // btnPublish
                // 
                this.btnPublish.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnPublish.Location = new System.Drawing.Point(11, 815);
                this.btnPublish.Name = "btnPublish";
                this.btnPublish.Size = new System.Drawing.Size(150, 35);
                this.btnPublish.TabIndex = 29;
                this.btnPublish.Text = "Publish";
                this.btnPublish.UseVisualStyleBackColor = false;
                this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);

                // 
                // btnSaveConfig
                // 
                this.btnSaveConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnSaveConfig.Location = new System.Drawing.Point(170, 815);
                this.btnSaveConfig.Name = "btnSaveConfig";
                this.btnSaveConfig.Size = new System.Drawing.Size(150, 35);
                this.btnSaveConfig.TabIndex = 30;
                this.btnSaveConfig.Text = "Save Config";
                this.btnSaveConfig.UseVisualStyleBackColor = false;
                this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);

                // 
                // btnLoadConfig
                // 
                this.btnLoadConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnLoadConfig.Location = new System.Drawing.Point(329, 815);
                this.btnLoadConfig.Name = "btnLoadConfig";
                this.btnLoadConfig.Size = new System.Drawing.Size(150, 35);
                this.btnLoadConfig.TabIndex = 31;
                this.btnLoadConfig.Text = "Load Config";
                this.btnLoadConfig.UseVisualStyleBackColor = false;
                this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);

                // 
                // statusStrip
                // 
                this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
                this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripStatusLabel,
                this.toolStripProgressBar});
                this.statusStrip.Location = new System.Drawing.Point(0, 860);
                this.statusStrip.Name = "statusStrip";
                this.statusStrip.Size = new System.Drawing.Size(1029, 26);
                this.statusStrip.TabIndex = 32;
                this.statusStrip.Text = "statusStrip1";

                // 
                // toolStripStatusLabel
                // 
                this.toolStripStatusLabel.Name = "toolStripStatusLabel";
                this.toolStripStatusLabel.Size = new System.Drawing.Size(49, 20);
                this.toolStripStatusLabel.Text = "Ready";

                // 
                // toolStripProgressBar
                // 
                this.toolStripProgressBar.Name = "toolStripProgressBar";
                this.toolStripProgressBar.Size = new System.Drawing.Size(100, 18);
                this.toolStripProgressBar.Visible = false;

                // 
                // tabControl
                // 
                this.tabControl.Controls.Add(this.tabSetup);
                this.tabControl.Controls.Add(this.tabLog);
                this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
                this.tabControl.Location = new System.Drawing.Point(0, 60);
                this.tabControl.Name = "tabControl";
                this.tabControl.SelectedIndex = 0;
                this.tabControl.Size = new System.Drawing.Size(1029, 768);
                this.tabControl.TabIndex = 33;

                // 
                // tabSetup
                // 
                this.tabSetup.AutoScroll = true;
                this.tabSetup.AutoScrollMinSize = new System.Drawing.Size(1000, 850);
                this.tabSetup.Controls.Add(this.grpConnection);
                this.tabSetup.Controls.Add(this.grpDacPac);
                this.tabSetup.Controls.Add(this.grpSynonyms);
                this.tabSetup.Controls.Add(this.grpScriptPaths);
                this.tabSetup.Controls.Add(this.grpSQLAgentJobs);
                this.tabSetup.Controls.Add(this.grpStoredProcedures);
                this.tabSetup.Controls.Add(this.btnPublish);
                this.tabSetup.Controls.Add(this.btnSaveConfig);
                this.tabSetup.Controls.Add(this.btnLoadConfig);
                this.tabSetup.Location = new System.Drawing.Point(4, 25);
                this.tabSetup.Name = "tabSetup";
                this.tabSetup.Padding = new System.Windows.Forms.Padding(3);
                this.tabSetup.Size = new System.Drawing.Size(1021, 800);
                this.tabSetup.TabIndex = 0;
                this.tabSetup.Text = "Setup";
                this.tabSetup.UseVisualStyleBackColor = true;

                // 
                // tabLog
                // 
                this.tabLog.Controls.Add(this.txtLog);
                this.tabLog.Controls.Add(this.btnClearLog);
                this.tabLog.Controls.Add(this.btnExportLog);
                this.tabLog.Location = new System.Drawing.Point(4, 25);
                this.tabLog.Name = "tabLog";
                this.tabLog.Padding = new System.Windows.Forms.Padding(3);
                this.tabLog.Size = new System.Drawing.Size(1021, 739);
                this.tabLog.TabIndex = 1;
                this.tabLog.Text = "Log";
                this.tabLog.UseVisualStyleBackColor = true;

                // 
                // txtLog
                // 
                this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
                this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.txtLog.Location = new System.Drawing.Point(6, 6);
                this.txtLog.Multiline = true;
                this.txtLog.Name = "txtLog";
                this.txtLog.ReadOnly = true;
                this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                this.txtLog.Size = new System.Drawing.Size(1009, 685);
                this.txtLog.TabIndex = 0;

                // 
                // btnClearLog
                // 
                this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.btnClearLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
                this.btnClearLog.ForeColor = System.Drawing.Color.White;
                this.btnClearLog.Location = new System.Drawing.Point(6, 697);
                this.btnClearLog.Name = "btnClearLog";
                this.btnClearLog.Size = new System.Drawing.Size(100, 35);
                this.btnClearLog.TabIndex = 1;
                this.btnClearLog.Text = "Clear Log";
                this.btnClearLog.UseVisualStyleBackColor = false;
                this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

                // 
                // btnExportLog
                // 
                this.btnExportLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                this.btnExportLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
                this.btnExportLog.ForeColor = System.Drawing.Color.White;
                this.btnExportLog.Location = new System.Drawing.Point(112, 697);
                this.btnExportLog.Name = "btnExportLog";
                this.btnExportLog.Size = new System.Drawing.Size(100, 35);
                this.btnExportLog.TabIndex = 2;
                this.btnExportLog.Text = "Export Log";
                this.btnExportLog.UseVisualStyleBackColor = false;
                this.btnExportLog.Click += new System.EventHandler(this.btnExportLog_Click);

                // 
                // footerPanel
                // 
                this.footerPanel.Controls.Add(this.lblStatus);
                this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
                this.footerPanel.Location = new System.Drawing.Point(0, 828);
                this.footerPanel.Name = "footerPanel";
                this.footerPanel.Size = new System.Drawing.Size(1029, 26);
                this.footerPanel.TabIndex = 34;

                // 
                // lblStatus
                // 
                this.lblStatus.AutoSize = true;
                this.lblStatus.Location = new System.Drawing.Point(12, 6);
                this.lblStatus.Name = "lblStatus";
                this.lblStatus.Size = new System.Drawing.Size(49, 17);
                this.lblStatus.TabIndex = 0;
                this.lblStatus.Text = "Ready";

                // 
                // openDacpacDialog
                // 
                this.openDacpacDialog.DefaultExt = "dacpac";
                this.openDacpacDialog.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
                this.openDacpacDialog.Title = "Select DACPAC File";

                // 
                // openDacpacDialog2
                // 
                this.openDacpacDialog2.DefaultExt = "dacpac";
                this.openDacpacDialog2.Filter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";
                this.openDacpacDialog2.Title = "Select Second DACPAC File";

                // 
                // openConfigDialog
                // 
                this.openConfigDialog.DefaultExt = "json";
                this.openConfigDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                this.openConfigDialog.Title = "Open Configuration";

                // 
                // saveConfigDialog
                // 
                this.saveConfigDialog.DefaultExt = "json";
                this.saveConfigDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                this.saveConfigDialog.Title = "Save Configuration";

                // 
                // saveLogDialog
                // 
                this.saveLogDialog.DefaultExt = "txt";
                this.saveLogDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
                this.saveLogDialog.Title = "Export Log";

                // 
                // folderBrowserDialog
                // 
                this.folderBrowserDialog.Description = "Select SQL Agent Job Scripts Folder";

                // 
                // DacpacPublisherForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(1029, 890);
                this.Controls.Add(this.tabControl);
                this.Controls.Add(this.statusStrip);
                this.Name = "DacpacPublisherForm";
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                this.Text = "DACPAC Publisher";
                this.Load += new System.EventHandler(this.DacpacPublisherForm_Load);

                this.grpConnection.ResumeLayout(false);
                this.grpConnection.PerformLayout();
                this.grpDacPac.ResumeLayout(false);
                this.grpDacPac.PerformLayout();
                this.grpSynonyms.ResumeLayout(false);
                this.grpSynonyms.PerformLayout();
                this.grpSQLAgentJobs.ResumeLayout(false);
                this.grpSQLAgentJobs.PerformLayout();
                this.grpStoredProcedures.ResumeLayout(false);
                this.grpStoredProcedures.PerformLayout();
                this.contextMenuStoredProcs.ResumeLayout(false);
                this.statusStrip.ResumeLayout(false);
                this.statusStrip.PerformLayout();
                this.tabControl.ResumeLayout(false);
                this.tabSetup.ResumeLayout(false);
                this.grpScriptPaths.ResumeLayout(false);
                this.grpScriptPaths.PerformLayout();
                this.tabLog.ResumeLayout(false);
                this.tabLog.PerformLayout();
                this.footerPanel.ResumeLayout(false);
                this.footerPanel.PerformLayout();
                this.ResumeLayout(false);
                this.PerformLayout();
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
                headerSubtitleLabel.Text = "Deploy database using DACPAC format";
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

            #endregion

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
            private System.Windows.Forms.GroupBox grpStoredProcedures;
            private System.Windows.Forms.ListBox listStoredProcedures;
            private System.Windows.Forms.ContextMenuStrip contextMenuStoredProcs;
            private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
            private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
            private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
            private System.Windows.Forms.Button btnAddProcedure;
            private System.Windows.Forms.CheckBox chkExecuteProcedures;
            private System.Windows.Forms.Label lblProcedureStrategy;
            private System.Windows.Forms.ComboBox cboProcedureStrategy;
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
            private System.Windows.Forms.Button btnClearLog;
            private System.Windows.Forms.Button btnExportLog;
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
            private System.Windows.Forms.Panel footerPanel;
            private System.Windows.Forms.Label lblStatus;
        }
    }