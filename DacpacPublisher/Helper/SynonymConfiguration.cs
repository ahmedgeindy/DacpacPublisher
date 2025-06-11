using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacPublisher.Helper
{
	/// <summary>
	/// Configuration class for synonym management
	/// </summary>
	public class SynonymConfiguration
	{
		public bool IsEnabled { get; set; }
		public bool UseAutoDetection { get; set; }
		public string SourceDatabase { get; set; }
		public List<string> TargetDatabases { get; set; } = new List<string>();
		public string ValidationSummary { get; set; }

		public bool IsValid => IsEnabled ? (!string.IsNullOrEmpty(SourceDatabase) && TargetDatabases?.Any() == true) : true;

		public string GetDisplaySummary()
		{
			if (!IsEnabled)
				return "Synonym creation disabled";

			if (UseAutoDetection)
				return "Auto-detection enabled";

			return $"Manual: {SourceDatabase} → {TargetDatabases?.Count ?? 0} target(s)";
		}
	}

	/// <summary>
	/// Event arguments for synonym configuration changes
	/// </summary>
	public class SynonymConfigChangedEventArgs : EventArgs
	{
		public SynonymConfiguration Configuration { get; set; }
	}
}