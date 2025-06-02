using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DacpacPublisher
{
	internal static class Program
	{
		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Set exception handling
			Application.ThreadException += Application_ThreadException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			try
			{
				Application.Run(new DacpacPublisherForm());
			}
			catch (Exception ex)
			{
				HandleUnhandledException(ex);
			}
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			HandleUnhandledException(e.Exception);
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleUnhandledException(e.ExceptionObject as Exception);
		}

		private static void HandleUnhandledException(Exception ex)
		{
			var message = "An unexpected error occurred in the application.\n\n";

			if (ex != null)
			{
				message += $"Error: {ex.Message}\n\n";

				if (ex.InnerException != null) message += $"Inner Error: {ex.InnerException.Message}\n\n";

				message += "Please check the application log file for more details.";

				// Log to file
				try
				{
					var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] Unhandled exception: {ex}";
					File.AppendAllText("Publish_Log.txt", logEntry + Environment.NewLine);
				}
				catch
				{
					// Ignore logging errors
				}
			}

			MessageBox.Show(message, @"Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}