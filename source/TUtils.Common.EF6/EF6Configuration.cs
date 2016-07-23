using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common.EF6
{
	public static class EF6Configuration
	{
		private static object _sync = new object();
		public static void AddConnectionString(string invariantName, string connectionString, string providerName)
		{
			lock (_sync)
			{
				var connectionStrings = ConfigurationManager.ConnectionStrings;
				var configurationElementReadonlyFieldInfo = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
				var configurationElementCollectionReadonlyFieldInfo = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

				configurationElementReadonlyFieldInfo.SetValue(connectionStrings, false);
				configurationElementCollectionReadonlyFieldInfo.SetValue(connectionStrings, false);

				connectionStrings.Add(new ConnectionStringSettings(
					name: invariantName,
					connectionString: connectionString,
					providerName: providerName));

				configurationElementCollectionReadonlyFieldInfo.SetValue(connectionStrings, true);
				configurationElementReadonlyFieldInfo.SetValue(connectionStrings, true);

			}
		}
	}
}
