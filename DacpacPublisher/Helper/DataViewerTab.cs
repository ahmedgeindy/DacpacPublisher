using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DacpacPublisher.Helper
{
	public class DataViewerTab : UserControl
	{
		#region Fields

		private readonly IConnectionService _connectionService;
		private readonly ILogService _logService;

		// Controls
		private Panel topPanel;
		private ComboBox cboTables;
		private Button btnRefreshTables;
		private Button btnExecuteQuery;
		private TextBox txtQuery;
		private DataGridView dgvData;
		private Panel infoPanel;
		private Label lblRecordCount;
		private Label lblTableInfo;
		private ProgressBar progressBar;

		#endregion

		#region Constructor

		public DataViewerTab(IConnectionService connectionService, ILogService logService)
		{
			_connectionService = connectionService;
			_logService = logService;

			InitializeComponents();
		}

		#endregion

		#region Initialization

		private void InitializeComponents()
		{
			// Main layout
			this.Dock = DockStyle.Fill;
			this.Padding = new Padding(UIHelper.SpacingLarge);
			this.BackColor = UIHelper.Background;

			// Top Panel
			topPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 120,
				Padding = new Padding(0, 0, 0, UIHelper.Spacing)
			};

			// Table Selection
			var lblTable = new Label
			{
				Text = "Select Table:",
				Location = new Point(0, 0),
				AutoSize = true,
				Font = UIHelper.BodyFont,
				ForeColor = UIHelper.DarkGray
			};

			cboTables = new ComboBox
			{
				Location = new Point(0, 20),
				Width = 300,
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			UIHelper.StyleComboBox(cboTables);
			cboTables.SelectedIndexChanged += CboTables_SelectedIndexChanged;

			btnRefreshTables = new Button
			{
				Text = "🔄 Refresh",
				Location = new Point(310, 18),
				Width = 100
			};
			UIHelper.StyleButton(btnRefreshTables, UIHelper.ButtonStyle.Secondary);
			btnRefreshTables.Click += BtnRefreshTables_Click;

			// Query Section
			var lblQuery = new Label
			{
				Text = "SQL Query:",
				Location = new Point(0, 55),
				AutoSize = true,
				Font = UIHelper.BodyFont,
				ForeColor = UIHelper.DarkGray
			};

			txtQuery = new TextBox
			{
				Location = new Point(0, 75),
				Width = 500,
				Height = 27,
				Text = "SELECT TOP 100 * FROM [Select a table]"
			};
			UIHelper.StyleTextBox(txtQuery);

			btnExecuteQuery = new Button
			{
				Text = "▶️ Execute",
				Location = new Point(510, 73),
				Width = 100
			};
			UIHelper.StyleButton(btnExecuteQuery, UIHelper.ButtonStyle.Primary);
			btnExecuteQuery.Click += BtnExecuteQuery_Click;

			// Add controls to top panel
			topPanel.Controls.AddRange(new Control[] {
				lblTable, cboTables, btnRefreshTables,
				lblQuery, txtQuery, btnExecuteQuery
			});

			// Info Panel
			infoPanel = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 40,
				Padding = new Padding(0, UIHelper.Spacing, 0, 0)
			};

			lblRecordCount = new Label
			{
				Text = "Records: 0",
				Dock = DockStyle.Left,
				AutoSize = true,
				Font = UIHelper.BodyFont,
				ForeColor = UIHelper.DarkGray,
				TextAlign = ContentAlignment.MiddleLeft
			};

			lblTableInfo = new Label
			{
				Text = "Ready",
				Dock = DockStyle.Left,
				AutoSize = true,
				Font = UIHelper.BodyFont,
				ForeColor = UIHelper.MediumGray,
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(20, 0, 0, 0)
			};

			progressBar = new ProgressBar
			{
				Dock = DockStyle.Right,
				Width = 200,
				Visible = false,
				Style = ProgressBarStyle.Marquee
			};

			infoPanel.Controls.AddRange(new Control[] {
				lblRecordCount, lblTableInfo, progressBar
			});

			// Data Grid
			dgvData = new DataGridView
			{
				Dock = DockStyle.Fill,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				ReadOnly = true,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				BackgroundColor = UIHelper.Surface,
				BorderStyle = BorderStyle.FixedSingle,
				GridColor = UIHelper.Border
			};

			// Style the grid
			dgvData.ColumnHeadersDefaultCellStyle.BackColor = UIHelper.LightGray;
			dgvData.ColumnHeadersDefaultCellStyle.ForeColor = UIHelper.DarkGray;
			dgvData.ColumnHeadersDefaultCellStyle.Font = UIHelper.BodyFont;
			dgvData.DefaultCellStyle.Font = UIHelper.BodyFont;
			dgvData.AlternatingRowsDefaultCellStyle.BackColor = UIHelper.Background;
			dgvData.EnableHeadersVisualStyles = false;

			// Add all controls
			this.Controls.Add(dgvData);
			this.Controls.Add(topPanel);
			this.Controls.Add(infoPanel);
		}

		#endregion

		#region Public Methods

		public async Task LoadTablesAsync(ConnectionInfo connectionInfo)
		{
			try
			{
				SetLoading(true, "Loading tables...");

				var tables = await GetTablesAsync(connectionInfo);

				cboTables.Items.Clear();
				foreach (var table in tables)
				{
					cboTables.Items.Add(table);
				}

				if (cboTables.Items.Count > 0)
				{
					cboTables.SelectedIndex = 0;
				}

				SetStatus($"Found {tables.Count} tables");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to load tables", ex);
				MessageBox.Show($"Error loading tables: {ex.Message}",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				SetLoading(false);
			}
		}

		#endregion

		#region Event Handlers

		private async void BtnRefreshTables_Click(object sender, EventArgs e)
		{
			var parent = this.ParentForm as DacpacPublisherForm;
			if (parent != null)
			{
				parent.UpdateConfigurationFromUI();
				var connectionInfo = parent.CreateConnectionInfo();
				await LoadTablesAsync(connectionInfo);
			}
		}

		private void CboTables_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboTables.SelectedItem != null)
			{
				string tableName = cboTables.SelectedItem.ToString();
				txtQuery.Text = $"SELECT TOP 100 * FROM [{tableName}]";
				txtQuery.ForeColor = UIHelper.DarkGray;
			}
		}

		private async void BtnExecuteQuery_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtQuery.Text))
			{
				MessageBox.Show("Please enter a query.", "No Query",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			await ExecuteQueryAsync(txtQuery.Text);
		}

		#endregion

		#region Private Methods

		private async Task<List<string>> GetTablesAsync(ConnectionInfo connectionInfo)
		{
			var tables = new List<string>();

			using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
			{
				await connection.OpenAsync();

				const string query = @"
                    SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS TableName
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    ORDER BY TABLE_SCHEMA, TABLE_NAME";

				using (var command = new SqlCommand(query, connection))
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						tables.Add(reader["TableName"].ToString());
					}
				}
			}

			return tables;
		}

		private async Task ExecuteQueryAsync(string query)
		{
			try
			{
				SetLoading(true, "Executing query...");

				var parent = this.ParentForm as DacpacPublisherForm;
				if (parent == null) return;

				parent.UpdateConfigurationFromUI();
				var connectionInfo = parent.CreateConnectionInfo();

				var dataTable = new DataTable();

				using (var connection = new SqlConnection(_connectionService.BuildConnectionString(connectionInfo)))
				{
					await connection.OpenAsync();

					using (var adapter = new SqlDataAdapter(query, connection))
					{
						adapter.Fill(dataTable);
					}
				}

				dgvData.DataSource = dataTable;

				// Auto-size columns with max width
				foreach (DataGridViewColumn column in dgvData.Columns)
				{
					column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
					if (column.Width > 200)
					{
						column.Width = 200;
						column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
					}
				}

				SetStatus($"Query executed successfully - {dataTable.Rows.Count} rows returned");
				lblRecordCount.Text = $"Records: {dataTable.Rows.Count:N0}";
			}
			catch (Exception ex)
			{
				_logService.LogError("Query execution failed", ex);
				MessageBox.Show($"Query failed: {ex.Message}",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetStatus("Query failed");
			}
			finally
			{
				SetLoading(false);
			}
		}

		private void SetLoading(bool loading, string message = "")
		{
			progressBar.Visible = loading;
			btnRefreshTables.Enabled = !loading;
			btnExecuteQuery.Enabled = !loading;
			cboTables.Enabled = !loading;
			txtQuery.Enabled = !loading;

			if (loading && !string.IsNullOrEmpty(message))
			{
				SetStatus(message);
			}
		}

		private void SetStatus(string message)
		{
			lblTableInfo.Text = message;
		}

		#endregion
	}
}