using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TUtils.Common.EF.Transaction.Common
{
	/// <summary>
	/// Generates without the need of an app.config or a web.config a database entity context (DbContext).
	/// The associated database is located in a local database file (LocalDB) %TEMP%\Unittest_{Testname}.mdf.
	/// A new instance of DbContextFactory4Unittest starts with an empty database and recreates the database structure
	/// of TDbContext. 
	/// 
	/// You can examine the database file in SQL Server Object Explorer (not in Server Explorer !).
	/// </summary>
	/// <remarks>
	/// For EF Core, this uses EnsureDeleted/EnsureCreated instead of database initializers
	/// </remarks>
	/// <typeparam name="TDbContext">
	/// Must be derived from DbContextBase and has a default constructor.
	/// Note ! DbContextBase has a default constructor only.
	/// If there is a DbContext in the project already and you have to use that DbContext,
	/// normally that class isn't derived from DbContextBase. So in that case it is recommended
	/// to create a second context class derived from DbContextBase and let both context classes
	/// implement a common interface. Then all classes which need the first class should be modified
	/// so that they need only a generic type implementing the common interface and DbContext.
	/// <example><code><![CDATA[
	///		interface IMyDbContext
	/// 	{
	/// 	    DbSet<Player> MyEntities { get; set; }
	/// 	}
	/// 	class MyProductiveDbContext : DbContext, IMyDbContext
	/// 	{
	/// 	    public virtual DbSet<Player> MyEntities { get; set; }
	/// 	}
	/// 	class MyTestDbContext : DbContextBase, IMyDbContext
	/// 	{
	/// 	    public virtual DbSet<Player> MyEntities { get; set; }
	/// 	}
	/// 	class MyDataAccessLayer<TDbContext>
	///			where TDbContext: DbContext, IMyDbContext
	///		{
	///			public MyDataAccessLayer(TDbContext dbContext)
	///			{
	///				...
	///			}
	///		}
	/// ]]></code></example>
	/// </typeparam>
	public class DbContextFactory4Unittest<TDbContext> : IDbContextFactory<TDbContext>
		where TDbContext : DbContextBase<TDbContext>, new()
	{
        private readonly string _testName;
        private static readonly object _sync = new();
        private static int _counter = 0;


        public DbContextFactory4Unittest(string testName)
        {
            _testName = testName;
        }

        public TDbContext Create()
        {
            lock (_sync)
            {
                SqlConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var dbName = $"UnitTest_{Guid.NewGuid():N}";
                var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;";

                EFConfiguration.AddConnectionString(dbName, connectionString);
                DbContextBase<TDbContext>.SetExistingConnectionStringIdentifier(dbName);

                var dbContext = new TDbContext();

                try
                {
                    dbContext.Database.EnsureDeleted(); // Safety first
                    dbContext.Database.EnsureCreated();
                }
                catch (SqlException e)
                {
                    Console.WriteLine($"[Fehler] Datenbank konnte nicht erstellt werden: {e.Message}");
                    throw;
                }

                return dbContext;
            }
        }

        private static void CleanupFiles(string dbFilePath, string logFilePath)
        {
            SqlConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            TryDetachDatabaseByFile(dbFilePath);
            ForceDropDatabaseIfExists(dbFilePath);

            TryDeleteFile(dbFilePath);
            TryDeleteFile(logFilePath);
        }

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"[Info] Datei gelöscht: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warnung] Datei konnte nicht gelöscht werden ({filePath}): {ex.Message}");
            }
        }

        private static void TryDetachDatabaseByFile(string dbFilePath)
        {
            try
            {
                using var connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;");
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT DB_NAME(database_id)
            FROM sys.master_files
            WHERE physical_name = @path";
                cmd.Parameters.AddWithValue("@path", dbFilePath);

                var dbName = cmd.ExecuteScalar() as string;
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    Console.WriteLine($"[Info] Trenne Datenbank '{dbName}'...");
                    using var detach = connection.CreateCommand();
                    detach.CommandText = $@"
                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                EXEC sp_detach_db '{dbName}', 'true';";
                    detach.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warnung] Detach fehlgeschlagen: {ex.Message}");
            }
        }

        private static void ForceDropDatabaseIfExists(string dbFilePath)
        {
            try
            {
                using var connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;");
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT DB_NAME(database_id)
            FROM sys.master_files
            WHERE physical_name = @path";
                cmd.Parameters.AddWithValue("@path", dbFilePath);

                var dbName = cmd.ExecuteScalar() as string;
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    Console.WriteLine($"[Info] Erzwinge Drop von DB '{dbName}'...");
                    using var dropCmd = connection.CreateCommand();
                    dropCmd.CommandText = $@"
                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{dbName}];";
                    dropCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warnung] Drop fehlgeschlagen: {ex.Message}");
            }
        }
    }
}
