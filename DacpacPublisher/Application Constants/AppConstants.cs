using System.Drawing;

namespace DacpacPublisher.Application_Constants
{
	public static class AppConstants
	{
		// Application Settings
		public const string AppTitle = "IST DacPac Publisher";
		public const string ConfigFileFilter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
		public const string DacpacFileFilter = "DACPAC Files (*.dacpac)|*.dacpac|All Files (*.*)|*.*";

		public const string LogFileFilter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

		// IST Brand Colors
		public static readonly Color ISTBlue = Color.FromArgb(0, 155, 223);
		public static readonly Color ISTLightBlue = Color.FromArgb(100, 195, 240);
		public static readonly Color ISTYellow = Color.FromArgb(255, 204, 0);
		public static readonly Color ISTGray = Color.FromArgb(102, 102, 102);
		public static readonly Color ISTLightGray = Color.FromArgb(240, 240, 240);

		// SQL Agent Jobs
		public static readonly string[] DefaultJobNames =
		{
			"HiveCFMAutoArchiveSurveys",
			"HiveCFMAutoPublishSurveys",
			"HiveCFMStopStartQuarantineMode",
			"HiveCFMSendFollowup"
		};
	}
}