using DacpacPublisher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DacpacPublisher.Services
{
	public class LogService : ILogService
	{
		private readonly List<string> _logs = new List<string>();
		public event Action<string> MessageLogged;

		public void LogMessage(string message)
		{
			LogInfo(message);
		}

		public void LogError(string message, Exception exception = null)
		{
			var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
			if (exception != null)
				logEntry += $" - {exception.Message}";

			AddLog(logEntry);
		}

		public void LogWarning(string message)
		{
			var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}";
			AddLog(logEntry);
		}

		public void LogInfo(string message)
		{
			var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
			AddLog(logEntry);
		}

		public string GetAllLogs()
		{
			return string.Join(Environment.NewLine, _logs);
		}

		public void ClearLogs()
		{
			_logs.Clear();
			MessageLogged?.Invoke("Log cleared");
		}

		private void AddLog(string logEntry)
		{
			_logs.Add(logEntry);
			MessageLogged?.Invoke(logEntry);
			Debug.WriteLine(logEntry);
		}
	}
}