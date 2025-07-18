using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DacpacPublisher.Interfaces;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Handles all database-related operations
	/// Extracted from DacpacPublisherForm to reduce class size and improve maintainability
	/// </summary>
	public class DatabaseController
	{
		private readonly DacpacPublisherForm _form;
		private readonly IConnectionService _connectionService;
		private readonly ILogService _logService;

		public DatabaseController(DacpacPublisherForm form,
			IConnectionService connectionService,
			ILogService logService)
		{
			_form = form ?? throw new ArgumentNullException(nameof(form));
			_connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
			_logService = logService ?? throw new ArgumentNullException(nameof(logService));
		}

		/// <summary>
		/// Refreshes the primary database list
		/// MOVED FROM: DacpacPublisherForm.RefreshDatabasesAsync()
		/// </summary>
		public async Task RefreshDatabasesAsync()
		{
			try
			{
				var configController = new ConfigurationController(_form, _logService);
				configController.UpdateConfigurationFromUI(_form._currentConfig);
				var connectionInfo = configController.CreateConnectionInfo(_form._currentConfig);

				UpdateStatus("Refreshing databases...", true);

				var databases = await _connectionService.GetDatabasesAsync(connectionInfo);

				string currentSelection = _form.cboDatabases?.SelectedItem?.ToString();

				UpdateDatabaseComboBox(_form.cboDatabases, databases, currentSelection);

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
				UpdateStatus("Ready", false);
			}
		}

		/// <summary>
		/// Refreshes the secondary database list
		/// MOVED FROM: DacpacPublisherForm.RefreshDatabases2Async()
		/// </summary>
		public async Task RefreshDatabases2Async()
		{
			try
			{
				var configController = new ConfigurationController(_form, _logService);
				configController.UpdateConfigurationFromUI(_form._currentConfig);
				var connectionInfo = configController.CreateConnectionInfo(_form._currentConfig);

				UpdateStatus("Refreshing databases for target 2...", true);

				var databases = await _connectionService.GetDatabasesAsync(connectionInfo);

				var cboDatabases2 = FindControlByName("cboDatabases2") as ComboBox;
				if (cboDatabases2 != null)
				{
					string currentSelection = cboDatabases2.SelectedItem?.ToString();
					UpdateDatabaseComboBox(cboDatabases2, databases, currentSelection);
				}

				_logService.LogInfo($"Refreshed {databases.Count} databases for secondary target");
			}
			catch (Exception ex)
			{
				_logService.LogError("Failed to refresh databases for target 2", ex);
				MessageBox.Show($"Failed to refresh databases: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				UpdateStatus("Ready", false);
			}
		}

		/// <summary>
		/// Tests database connection with current settings
		/// </summary>
		public async Task<bool> TestConnectionAsync()
		{
			try
			{
				var configController = new ConfigurationController(_form, _logService);
				configController.UpdateConfigurationFromUI(_form._currentConfig);
				var connectionInfo = configController.CreateConnectionInfo(_form._currentConfig);

				UpdateStatus("Testing connection...", true);

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

				return success;
			}
			catch (Exception ex)
			{
				_logService.LogError("Connection test error", ex);
				MessageBox.Show($"❌ Connection test failed: {ex.Message}", "Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			finally
			{
				UpdateStatus("Ready", false);
			}
		}

		/// <summary>
		/// Gets all CFM-related databases from the server
		/// </summary>
		public async Task<List<string>> GetCFMDatabasesAsync()
		{
			try
			{
				var configController = new ConfigurationController(_form, _logService);
				configController.UpdateConfigurationFromUI(_form._currentConfig);
				var connectionInfo = configController.CreateConnectionInfo(_form._currentConfig);
				connectionInfo.Database = "master";

				var allDatabases = await _connectionService.GetDatabasesAsync(connectionInfo);

				return allDatabases
					.Where(db => db.IndexOf("CFM", StringComparison.OrdinalIgnoreCase) >= 0)
					.OrderBy(db => db)
					.ToList();
			}
			catch (Exception ex)
			{
				_logService.LogError("Error getting CFM databases", ex);
				return new List<string>();
			}
		}

		#region Private Helper Methods

		private void UpdateDatabaseComboBox(ComboBox comboBox, List<string> databases, string currentSelection = null)
		{
			try
			{
				if (_form.InvokeRequired)
				{
					_form.Invoke(new Action(() => UpdateDatabaseComboBox(comboBox, databases, currentSelection)));
					return;
				}

				if (comboBox == null) return;

				comboBox.Items.Clear();

				foreach (string db in databases.OrderBy(d => d))
				{
					comboBox.Items.Add(db);
				}

				// Restore selection if possible
				if (!string.IsNullOrEmpty(currentSelection) && comboBox.Items.Contains(currentSelection))
				{
					comboBox.SelectedItem = currentSelection;
				}
				else if (comboBox.Items.Count > 0)
				{
					comboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Database combo update error: {ex.Message}");
			}
		}

		//private void PopulateSynonymTargetsList(List<string> databases)
		//{
		//	try
		//	{
		//		if (_form.clbSynonymTargets == null) return;

		//		_form.clbSynonymTargets.Items.Clear();

		//		if (databases == null || databases.Count == 0)
		//		{
		//			_form.clbSynonymTargets.Items.Add("No CFM databases found");
		//			return;
		//		}

		//		Categorize databases
		//		var hiveCFMAppDatabases = databases
		//			.Where(db => db.IndexOf("HiveCFMApp", StringComparison.OrdinalIgnoreCase) >= 0)
		//			.ToList();

		//		var hiveCFMSurveyDatabases = databases
		//			.Where(db => db.IndexOf("HiveCFMSurvey", StringComparison.OrdinalIgnoreCase) >= 0)
		//			.ToList();

		//		var otherCFMDatabases = databases
		//			.Where(db => !hiveCFMAppDatabases.Contains(db) && !hiveCFMSurveyDatabases.Contains(db))
		//			.ToList();

		//		Add categorized databases to the list
		//		if (hiveCFMAppDatabases.Any())
		//		{
		//			foreach (var db in hiveCFMAppDatabases)
		//			{
		//				int index = _form.clbSynonymTargets.Items.Add($"✅ {db}");
		//				_form.clbSynonymTargets.SetItemChecked(index, true); // Auto-check
		//			}
		//		}

		//		if (hiveCFMSurveyDatabases.Any())
		//		{
		//			foreach (var db in hiveCFMSurveyDatabases)
		//			{
		//				int index = _form.clbSynonymTargets.Items.Add($"📋 {db}");
		//				_form.clbSynonymTargets.SetItemChecked(index, false); // Don't check
		//			}
		//		}

		//		if (otherCFMDatabases.Any())
		//		{
		//			foreach (var db in otherCFMDatabases)
		//			{
		//				int index = _form.clbSynonymTargets.Items.Add($"📁 {db}");
		//				_form.clbSynonymTargets.SetItemChecked(index, false); // Don't check
		//			}
		//		}

		//		_logService.LogInfo($"✅ Populated {databases.Count} databases in synonym targets list");
		//	}
		//	catch (Exception ex)
		//	{
		//		_logService.LogError("Error populating synonym list", ex);
		//		if (_form.clbSynonymTargets != null)
		//		{
		//			_form.clbSynonymTargets.Items.Clear();
		//			_form.clbSynonymTargets.Items.Add($"Error: {ex.Message}");
		//		}
		//	}
		//}

		private void UpdateStatus(string message, bool showProgress)
		{
			try
			{
				if (_form.InvokeRequired)
				{
					_form.Invoke(new Action(() => UpdateStatus(message, showProgress)));
					return;
				}

				if (_form.toolStripStatusLabel != null)
					_form.toolStripStatusLabel.Text = showProgress ? $"⏳ {message}" : $"🟢 {message}";

				if (_form.toolStripProgressBar != null)
				{
					_form.toolStripProgressBar.Visible = showProgress;
					if (showProgress)
						_form.toolStripProgressBar.Style = ProgressBarStyle.Marquee;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Status update error: {ex.Message}");
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

		#endregion
	}
}