using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace TUtils.Common.EF6.Transaction.Common
{
	public class DbContextBase<TDbContext> : DbContext
		where TDbContext : DbContextBase<TDbContext>
	{
		private static string _connectionStringIdentifier;

		public static void SetExistingConnectionStringIdentifier(string connectionStringIdentifier)
		{
			_connectionStringIdentifier = connectionStringIdentifier;
		}

		public DbContextBase() : base(_connectionStringIdentifier)
		{
			Database.SetInitializer(new DropCreateDatabaseAlways<TDbContext>());
		}
	}

	/// <summary>
	/// Generates without app.config a database entity context to a LocalDB file based database.
	/// The database file will be created and overwritten in users %TEMP% directory (MyTestdataBase.mdf).
	/// </summary>
	/// <typeparam name="TDbContext"></typeparam>
	public class DbContextFactory4Unittest<TDbContext> : IDbContextFactory<TDbContext>
		where TDbContext : DbContextBase<TDbContext>, new()
	{
		private static object _sync = new object();
		private string _connectionStringIdentifier;
		private static int _id = 0;

		public TDbContext Create()
		{
			var tempDir = Path.GetTempPath();
			lock (_sync)
			{
				var connectionStringIdentifier = "MyTestdataBase" + _id++;
				EF6Configuration.AddConnectionString(
					invariantName: connectionStringIdentifier,
					connectionString: $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={tempDir}\\{connectionStringIdentifier}.mdf;Integrated Security=True;",
					providerName: "System.Data.SqlClient");
				DbContextBase<TDbContext>.SetExistingConnectionStringIdentifier(connectionStringIdentifier);
				return new TDbContext();
			}
		}
	}
}