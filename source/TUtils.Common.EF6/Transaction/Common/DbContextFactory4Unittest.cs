using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;

namespace TUtils.Common.EF6.Transaction.Common
{
	/// <summary>
	/// Generates without the need of an app.config or a web.config a database entity context (DbContext).
	/// The associated database is located in a local database file (LocalDB) %TEMP%\Unittest_{Testname}.mdf.
	/// A new instance of DbContextFactory4Unittest starts with an empty database and recreates the database structure
	/// of TDbContext. 
	/// 
	/// You can examin the database file in SQL Server Object Explorer (not in Server Explorer !).
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <typeparam name="TDbContext">
	/// Must be derived from DbContextBase and has a default constructor.
	/// Note ! DbContextBase has a default constructor only.
	/// If there is a DbContext in the project allready and you have to use that DbContext,
	/// normally that class isn't drived from DbContextBase. So in that case it is recommended
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
		private static readonly object _sync = new object();
		private bool _firstCallOfCreate = true;

		/// <summary>
		/// 
		/// 
		/// </summary>
		/// <param name="testName">
		/// Must be unique: Each testmethod must pass it's own string in order to avoid
		/// concurrent access to a test database.
		/// </param>
		public DbContextFactory4Unittest(string testName)
		{
			_testName = testName;
		}

		/// <summary>
		/// Creates database context. It's allready initialized (dbContext.Database.Initialize(force:true);)
		/// </summary>
		/// <returns></returns>
		public TDbContext Create()
		{
			var tempDir = Path.GetTempPath();
			lock (_sync)
			{
				var connectionStringIdentifier = "UnitTest_" + _testName;
				var dbFilePath = $"{tempDir}{connectionStringIdentifier}.mdf";

				if (_firstCallOfCreate)
				{
					EF6Configuration.AddConnectionString(
						invariantName: connectionStringIdentifier,
						connectionString:
							$"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={dbFilePath};Integrated Security=True;",
						providerName: "System.Data.SqlClient");
				}

				DbContextBase<TDbContext>.SetExistingConnectionStringIdentifier(connectionStringIdentifier);

				var dbContext = new TDbContext();

				if (_firstCallOfCreate)
				{
					Database.SetInitializer(new DropCreateDatabaseAlways<TDbContext>());
					_firstCallOfCreate = false;
					try
					{
						dbContext.Database.Initialize(force: true);
					}
					catch (SqlException e) when (
						e.Message.Contains("Cannot create file")
						&& e.Message.Contains("because it already exists")
						&& e.Message.Contains(dbFilePath))
					{
						throw new ApplicationException(
							$"Oups ! Unit test {_testName} tried to drop it's testdatabase, but didn't succeeded.\n"+
							"I simply didn't find out why this sometimes happens. A possible work-around:\n" +
							"1. go to the machine, where test was executed\n" +
							"2. go to Visual Studio->SQL Server Object Explorer->SQL Server->(localdb)\\MSSQLLocalDB->Databases->UNITTEST_*\n" +
							"3. drop (select and press DEL) all test databases: UNITTEST_*", e);
					}
					Database.SetInitializer<TDbContext>(null);
				}
				else // don't destroy database until this factory has been recreated
				{
					Database.SetInitializer<TDbContext>(null);
				}
				
				return dbContext;
			}
		}

	}
}