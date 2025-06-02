using DacpacPublisher.Data_Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DacpacPublisher
{
	public partial class SmartProcedureDialog : Form
	{
		private Button _btnAddProcedure;
		private Button _btnCancel;
		private Button _btnOK;
		private Button _btnRemoveProcedure;
		private CheckBox _chkDB1;
		private CheckBox _chkDB2;
		private ComboBox _cmbCategory;

		private readonly PublisherConfiguration _config;
		private Label _lblStatus;
		private ListView _procedureListView;
		private TextBox _txtProcedureName;

		public SmartProcedureDialog(PublisherConfiguration config)
		{
			_config = config;
			UpdatedConfiguration = CloneConfiguration(config);
			InitializeDialog();
			LoadProcedures();
			UpdateStatus();
		}

		public PublisherConfiguration UpdatedConfiguration { get; }

		private void InitializeDialog()
		{
			Text = "🎯 Smart Procedure Configuration";
			Size = new Size(800, 600);
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.Sizable;

			// Header
			var lblHeader = new Label
			{
				Text = "Configure which database each procedure runs on:",
				Location = new Point(20, 20),
				Size = new Size(600, 25),
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = Color.FromArgb(0, 102, 204)
			};

			// Database info
			var lblDB1 = new Label
			{
				Text = $"🗄️ Primary Database: {_config.Database ?? "Primary"}",
				Location = new Point(20, 50),
				Size = new Size(300, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			var lblDB2 = new Label
			{
				Text = "🗄️ Secondary Database: Secondary",
				Location = new Point(400, 50),
				Size = new Size(300, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			// Procedure list
			var lblProcList = new Label
			{
				Text = "📝 Stored Procedures:",
				Location = new Point(20, 80),
				Size = new Size(200, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			_procedureListView = new ListView
			{
				Location = new Point(20, 105),
				Size = new Size(500, 300),
				View = View.Details,
				FullRowSelect = true,
				GridLines = true,
				MultiSelect = false
			};

			_procedureListView.Columns.Add("Procedure Name", 200);
			_procedureListView.Columns.Add("Primary DB", 80);
			_procedureListView.Columns.Add("Secondary DB", 100);
			_procedureListView.Columns.Add("Category", 120);

			// Add procedure section
			var lblAdd = new Label
			{
				Text = "➕ Add New Procedure:",
				Location = new Point(540, 105),
				Size = new Size(200, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			var lblProcName = new Label
			{
				Text = "Procedure Name:",
				Location = new Point(540, 135),
				Size = new Size(120, 20)
			};

			_txtProcedureName = new TextBox
			{
				Location = new Point(540, 160),
				Size = new Size(200, 23),
				Text = "Enter procedure name..."
			};

			// Placeholder text behavior for .NET Framework
			_txtProcedureName.ForeColor = Color.Gray;
			_txtProcedureName.Enter += (s, e) =>
			{
				if (_txtProcedureName.Text == "Enter procedure name...")
				{
					_txtProcedureName.Text = "";
					_txtProcedureName.ForeColor = Color.Black;
				}
			};
			_txtProcedureName.Leave += (s, e) =>
			{
				if (string.IsNullOrWhiteSpace(_txtProcedureName.Text))
				{
					_txtProcedureName.Text = "Enter procedure name...";
					_txtProcedureName.ForeColor = Color.Gray;
				}
			};

			var lblCategory = new Label
			{
				Text = "Category:",
				Location = new Point(540, 195),
				Size = new Size(80, 20)
			};

			_cmbCategory = new ComboBox
			{
				Location = new Point(540, 220),
				Size = new Size(150, 23),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			_cmbCategory.Items.AddRange(new[] { "Setup", "DataProcessing", "Maintenance", "Reporting", "General" });
			_cmbCategory.SelectedIndex = 4;

			var lblDatabases = new Label
			{
				Text = "Run on Databases:",
				Location = new Point(540, 255),
				Size = new Size(150, 20),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			_chkDB1 = new CheckBox
			{
				Text = "✅ Primary Database",
				Location = new Point(540, 280),
				Size = new Size(200, 20),
				Checked = true,
				ForeColor = Color.Green
			};

			_chkDB2 = new CheckBox
			{
				Text = "✅ Secondary Database",
				Location = new Point(540, 305),
				Size = new Size(200, 20),
				ForeColor = Color.Blue
			};

			_btnAddProcedure = new Button
			{
				Text = "➕ Add Procedure",
				Location = new Point(540, 340),
				Size = new Size(120, 30),
				BackColor = Color.FromArgb(40, 167, 69),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold)
			};

			_btnRemoveProcedure = new Button
			{
				Text = "🗑️ Remove",
				Location = new Point(670, 340),
				Size = new Size(85, 30),
				BackColor = Color.FromArgb(220, 53, 69),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Enabled = false
			};

			// Status
			_lblStatus = new Label
			{
				Location = new Point(20, 420),
				Size = new Size(600, 20),
				Text = "Ready to configure procedures...",
				ForeColor = Color.Gray
			};

			// Buttons
			_btnOK = new Button
			{
				Text = "✅ Apply Changes",
				Location = new Point(580, 460),
				Size = new Size(120, 35),
				BackColor = Color.FromArgb(40, 167, 69),
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold),
				DialogResult = DialogResult.OK
			};

			_btnCancel = new Button
			{
				Text = "❌ Cancel",
				Location = new Point(710, 460),
				Size = new Size(80, 35),
				BackColor = Color.Gray,
				ForeColor = Color.White,
				FlatStyle = FlatStyle.Flat,
				DialogResult = DialogResult.Cancel
			};

			// Add all controls
			Controls.AddRange(new Control[]
			{
				lblHeader, lblDB1, lblDB2, lblProcList, _procedureListView,
				lblAdd, lblProcName, _txtProcedureName, lblCategory, _cmbCategory,
				lblDatabases, _chkDB1, _chkDB2, _btnAddProcedure, _btnRemoveProcedure,
				_lblStatus, _btnOK, _btnCancel
			});

			// Wire up events
			_btnAddProcedure.Click += BtnAddProcedure_Click;
			_btnRemoveProcedure.Click += BtnRemoveProcedure_Click;
			_procedureListView.SelectedIndexChanged += ProcedureListView_SelectedIndexChanged;
			_btnOK.Click += BtnOK_Click;

			AcceptButton = _btnOK;
			CancelButton = _btnCancel;
		}

		private void LoadProcedures()
		{
			_procedureListView.Items.Clear();

			if (UpdatedConfiguration.SmartProcedures?.Any() == true)
			{
				foreach (var proc in UpdatedConfiguration.SmartProcedures.OrderBy(p => p.ExecutionOrder))
					AddProcedureToList(proc);
			}
			else if (UpdatedConfiguration.StoredProcedures?.Any() == true)
			{
				// Convert legacy procedures
				var order = 100;
				foreach (var proc in UpdatedConfiguration.StoredProcedures)
				{
					var smartProc = new SmartStoredProcedureInfo
					{
						Name = proc.Name,
						Parameters = proc.Parameters ?? "",
						ExecuteOnDatabase1 = true,
						ExecuteOnDatabase2 = false,
						Category = ProcedureCategory.General,
						ExecutionOrder = order,
						Description = proc.Description ?? $"Procedure: {proc.Name}"
					};

					if (UpdatedConfiguration.SmartProcedures == null)
						UpdatedConfiguration.SmartProcedures = new List<SmartStoredProcedureInfo>();

					UpdatedConfiguration.SmartProcedures.Add(smartProc);
					AddProcedureToList(smartProc);
					order += 100;
				}
			}
		}

		private void AddProcedureToList(SmartStoredProcedureInfo proc)
		{
			var item = new ListViewItem(proc.Name);
			item.SubItems.Add(proc.ExecuteOnDatabase1 ? "✅ Yes" : "❌ No");
			item.SubItems.Add(proc.ExecuteOnDatabase2 ? "✅ Yes" : "❌ No");
			item.SubItems.Add(proc.Category.ToString());
			item.Tag = proc;

			// Color coding
			if (proc.ExecuteOnDatabase1 && proc.ExecuteOnDatabase2)
				item.BackColor = Color.LightYellow; // Both databases
			else if (proc.ExecuteOnDatabase1)
				item.BackColor = Color.LightBlue; // Primary only
			else if (proc.ExecuteOnDatabase2)
				item.BackColor = Color.LightGreen; // Secondary only

			_procedureListView.Items.Add(item);
		}

		private void BtnAddProcedure_Click(object sender, EventArgs e)
		{
			try
			{
				var procName = _txtProcedureName.Text.Trim();

				// Check for placeholder text
				if (string.IsNullOrEmpty(procName) || procName == "Enter procedure name...")
				{
					MessageBox.Show(@"Please enter a procedure name.", @"Validation",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// Check for duplicates
				if (IsDuplicateProcedureConfiguration(procName, _chkDB1.Checked, _chkDB2.Checked))
				{
					return; // User cancelled or duplicate found
				}

				if (!_chkDB1.Checked && !_chkDB2.Checked)
				{
					MessageBox.Show(@"Please select at least one database for this procedure.", @"Validation",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// FIXED: Safe category parsing with error handling
				ProcedureCategory category;
				if (_cmbCategory.SelectedItem == null ||
					!Enum.TryParse(_cmbCategory.SelectedItem.ToString(), out category))
					category = ProcedureCategory.General; // Default fallback

				// Initialize SmartProcedures if null
				if (UpdatedConfiguration.SmartProcedures == null)
					UpdatedConfiguration.SmartProcedures = new List<SmartStoredProcedureInfo>();

				// FIXED: Safe Max calculation to avoid "Sequence contains no elements" error
				var nextExecutionOrder = 100; // Default starting order
				if (UpdatedConfiguration.SmartProcedures.Any())
					nextExecutionOrder = UpdatedConfiguration.SmartProcedures.Max(p => p.ExecutionOrder) + 100;

				var newProc = new SmartStoredProcedureInfo
				{
					Name = procName,
					ExecuteOnDatabase1 = _chkDB1.Checked,
					ExecuteOnDatabase2 = _chkDB2.Checked,
					Category = category,
					ExecutionOrder = nextExecutionOrder,
					Description = $"Procedure: {procName}",
					Parameters = ""
				};

				UpdatedConfiguration.SmartProcedures.Add(newProc);
				AddProcedureToList(newProc);

				// Clear form - Reset placeholder
				_txtProcedureName.Text = "Enter procedure name...";
				_txtProcedureName.ForeColor = Color.Gray;
				_chkDB1.Checked = true;
				_chkDB2.Checked = false;
				_cmbCategory.SelectedIndex = 4;

				UpdateStatus();
				_txtProcedureName.Focus();
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error adding procedure: {ex.Message}", @"Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void BtnRemoveProcedure_Click(object sender, EventArgs e)
		{
			try
			{
				if (_procedureListView.SelectedItems.Count == 0) return;

				var selectedProc = _procedureListView.SelectedItems[0].Tag as SmartStoredProcedureInfo;
				var result = MessageBox.Show($@"Remove procedure '{selectedProc.Name}'?", @"Confirm",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					UpdatedConfiguration.SmartProcedures?.Remove(selectedProc);
					_procedureListView.Items.Remove(_procedureListView.SelectedItems[0]);
					UpdateStatus();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error removing procedure: {ex.Message}", @"Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ProcedureListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			_btnRemoveProcedure.Enabled = _procedureListView.SelectedItems.Count > 0;
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			try
			{
				// Update legacy procedures for backward compatibility
				if (UpdatedConfiguration.SmartProcedures?.Any() == true)
					UpdatedConfiguration.StoredProcedures = UpdatedConfiguration.SmartProcedures
						.Where(sp => sp.ExecuteOnDatabase1) // Include procedures that run on primary
						.Select(sp => new StoredProcedureInfo
						{
							Name = sp.Name,
							Parameters = sp.Parameters ?? "",
							ExecutionOrder = sp.ExecutionOrder,
							Description = sp.Description ?? ""
						}).ToList();

				UpdatedConfiguration.UseSmartProcedures = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error applying changes: {ex.Message}", @"Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None; // Prevent dialog from closing
			}
		}

		private void UpdateStatus()
		{
			try
			{
				if (UpdatedConfiguration.SmartProcedures?.Any() == true)
				{
					var total = UpdatedConfiguration.SmartProcedures.Count;
					var db1Count = UpdatedConfiguration.SmartProcedures.Count(p => p.ExecuteOnDatabase1);
					var db2Count = UpdatedConfiguration.SmartProcedures.Count(p => p.ExecuteOnDatabase2);

					_lblStatus.Text = $"📊 {total} procedures | Primary DB: {db1Count} | Secondary DB: {db2Count}";
					_lblStatus.ForeColor = Color.Green;
				}
				else
				{
					_lblStatus.Text = "No procedures configured. Add procedures above.";
					_lblStatus.ForeColor = Color.Gray;
				}
			}
			catch (Exception ex)
			{
				_lblStatus.Text = $"Status update error: {ex.Message}";
				_lblStatus.ForeColor = Color.Red;
			}
		}

		private PublisherConfiguration CloneConfiguration(PublisherConfiguration original)
		{
			try
			{
				return new PublisherConfiguration
				{
					ServerName = original.ServerName ?? "",
					Database = original.Database ?? "",
					WindowsAuth = original.WindowsAuth,
					Username = original.Username ?? "",
					Password = original.Password ?? "",
					DacpacPath = original.DacpacPath ?? "",
					ExecuteProcedures = original.ExecuteProcedures,
					UseSmartProcedures = original.UseSmartProcedures,
					EnableMultipleDatabases = original.EnableMultipleDatabases,
					SmartProcedures = original.SmartProcedures?.Select(sp => new SmartStoredProcedureInfo
					{
						Name = sp.Name ?? "",
						Parameters = sp.Parameters ?? "",
						ExecuteOnDatabase1 = sp.ExecuteOnDatabase1,
						ExecuteOnDatabase2 = sp.ExecuteOnDatabase2,
						Category = sp.Category,
						ExecutionOrder = sp.ExecutionOrder,
						Description = sp.Description ?? ""
					}).ToList() ?? new List<SmartStoredProcedureInfo>(),
					StoredProcedures = original.StoredProcedures?.ToList() ?? new List<StoredProcedureInfo>(),
					DeploymentTargets = original.DeploymentTargets?.ToList() ?? new List<DatabaseDeploymentTarget>()
				};
			}
			catch (Exception ex)
			{
				// Return a minimal valid configuration if cloning fails
				return new PublisherConfiguration
				{
					SmartProcedures = new List<SmartStoredProcedureInfo>(),
					StoredProcedures = new List<StoredProcedureInfo>(),
					DeploymentTargets = new List<DatabaseDeploymentTarget>()
				};
			}
		}
		private bool IsDuplicateProcedureConfiguration(string procName, bool runOnPrimary, bool runOnSecondary)
		{
			if (UpdatedConfiguration.SmartProcedures == null)
				return false;

			// Check if exact same configuration already exists (same name + same database combination)
			var exactMatch = UpdatedConfiguration.SmartProcedures.FirstOrDefault(p =>
				p.Name.Equals(procName, StringComparison.OrdinalIgnoreCase) &&
				p.ExecuteOnDatabase1 == runOnPrimary &&
				p.ExecuteOnDatabase2 == runOnSecondary);

			if (exactMatch != null)
			{
				// Simple message for exact duplicate
				string dbConfig = GetDatabaseConfigDescription(runOnPrimary, runOnSecondary);
				MessageBox.Show($"Procedure '{procName}' already exists with the same database configuration ({dbConfig}).\n\nPlease use a different name or change the database selection.",
					"Duplicate Procedure",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return true; // Block the addition
			}

			return false; // Allow addition - same name with different database combination is OK
		}
		private string GetDatabaseConfigDescription(bool runOnPrimary, bool runOnSecondary)
		{
			if (runOnPrimary && runOnSecondary)
				return "Both Primary and Secondary databases";
			else if (runOnPrimary)
				return "Primary database only";
			else if (runOnSecondary)
				return "Secondary database only";
			else
				return "No databases (Invalid)";
		}
	}
}