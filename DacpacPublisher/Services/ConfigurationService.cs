// FIXED ConfigurationService.cs - Make LoadConfigurationAsync truly async
using DacpacPublisher.Data_Models;
using DacpacPublisher.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DacpacPublisher.Services
{
	public class ConfigurationService : IConfigurationService
	{
		private readonly ILogService _logService;

		public ConfigurationService(ILogService logService)
		{
			_logService = logService;
		}

		public async Task<bool> SaveConfigurationAsync(PublisherConfiguration config, string filePath)
		{
			try
			{
				var json = JsonConvert.SerializeObject(config, Formatting.Indented);
				await Task.Run(() => File.WriteAllText(filePath, json));
				_logService.LogInfo($"Configuration saved to {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to save configuration to {filePath}", ex);
				return false;
			}
		}

		// FIXED: Make this method truly async
		public async Task<PublisherConfiguration> LoadConfigurationAsync(string filePath)
		{
			try
			{
				// Read file asynchronously
				var json = await Task.Run(() => File.ReadAllText(filePath));
				var config = JsonConvert.DeserializeObject<PublisherConfiguration>(json);
				_logService.LogInfo($"Configuration loaded from {filePath}");
				return config ?? new PublisherConfiguration();
			}
			catch (Exception ex)
			{
				_logService.LogError($"Failed to load configuration from {filePath}", ex);
				return new PublisherConfiguration();
			}
		}
	}
}