using System.Data.Entity;

namespace TUtils.Common.EF6.Transaction.Common
{
	/// <summary>
	/// just for unit testing. see DbContextFactory4Unittest.
	/// </summary>
	/// <typeparam name="TDbContext"></typeparam>
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
		}
	}
}