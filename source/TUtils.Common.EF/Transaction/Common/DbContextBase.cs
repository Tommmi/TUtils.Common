using Microsoft.EntityFrameworkCore;

namespace TUtils.Common.EF.Transaction.Common
{
	/// <summary>
	/// Base class for DbContext that supports configurable connection strings
	/// Useful for unit testing. See DbContextFactory4Unittest.
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

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionStringIdentifier))
			{
				var connectionString = EFConfiguration.GetConnectionString(_connectionStringIdentifier);
				if (!string.IsNullOrEmpty(connectionString))
				{
					// Default to SQL Server, but this can be overridden in derived classes
					optionsBuilder.UseSqlServer(connectionString);
				}
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
