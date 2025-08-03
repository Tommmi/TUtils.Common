using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace TUtils.Common.EF
{
	public static class EFConfiguration
	{
		private static readonly object _sync = new object();
		private static readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();

		/// <summary>
		/// Adds a connection string to the in-memory configuration for EF Core
		/// </summary>
		/// <param name="name">Connection string name</param>
		/// <param name="connectionString">The connection string</param>
		/// <param name="providerName">Provider name (for compatibility - not used in EF Core)</param>
		public static void AddConnectionString(string name, string connectionString, string providerName = null)
		{
			lock (_sync)
			{
				_connectionStrings[name] = connectionString;
			}
		}

		/// <summary>
		/// Gets a connection string by name
		/// </summary>
		/// <param name="name">Connection string name</param>
		/// <returns>Connection string or null if not found</returns>
		public static string GetConnectionString(string name)
		{
			lock (_sync)
			{
				return _connectionStrings.TryGetValue(name, out var connectionString) ? connectionString : null;
			}
		}

		/// <summary>
		/// Creates an IConfiguration instance with the registered connection strings
		/// </summary>
		/// <returns>Configuration instance</returns>
		public static IConfiguration BuildConfiguration()
		{
			lock (_sync)
			{
				var configBuilder = new ConfigurationBuilder();
				configBuilder.AddInMemoryCollection(_connectionStrings.Select(kvp => 
					new KeyValuePair<string, string>($"ConnectionStrings:{kvp.Key}", kvp.Value)));
				return configBuilder.Build();
			}
		}
	}
}
