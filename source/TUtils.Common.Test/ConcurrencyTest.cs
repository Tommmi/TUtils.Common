using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Common;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;

namespace TUtils.Common.Test
{
	[TestClass]
	public class ConcurrencyTest
	{
		private class MyEntity
		{
			public MyEntity()
			{

			}

			public int Id { get; set; }
			public string Text { get; set; }
			[Timestamp, ConcurrencyCheck]
			public byte[] RowVersion { get; set; }
		}

		private class MyDbContext : DbContextBase<MyDbContext>
		{
			public MyDbContext()
			{
				Configuration.ProxyCreationEnabled = true;
			}


			public virtual DbSet<MyEntity> MyEntities { get; set; }
		}

		[TestMethod]
		public void TestConcurrency()
		{
			ILogWriter logWriter = new LogConsoleWriter(
				LogSeverityEnum.INFO,
				new List<string> { "*" },
				new List<string>());
			ITLog logger = new TLog(logWriter, isLoggingOfMethodNameActivated: false);
			IDbContextFactory<MyDbContext> dbContextFactory = new DbContextFactory4Unittest<MyDbContext>(nameof(TestConcurrency));
			ITransactionService<MyDbContext> transactionService = new TransactionService<MyDbContext>(
				logger,
				dbContextFactory,
				IsolationLevel.ReadCommitted);

			MyEntity myEntity = null;

			transactionService.DoInTransaction(dbContext =>
			{
				myEntity = new MyEntity { Text = "hello" };
				dbContext.MyEntities.Add(myEntity);
				dbContext.SaveChanges();
			});

			transactionService.DoInTransaction(dbContext =>
			{
				var newMyEntity = new MyEntity { Id = myEntity.Id, Text = "hello world", RowVersion = myEntity.RowVersion};
				// dbContext.MyEntities.Attach(newMyEntity);
				dbContext.Entry(newMyEntity).State = EntityState.Modified;
				dbContext.SaveChanges();
				Assert.IsTrue(dbContext.MyEntities.Find(myEntity.Id).Text == "hello world");
			});

			bool concurrencyExceptionOccurred = false;

			transactionService.DoInTransaction(dbContext =>
			{
				var state = dbContext.Entry(myEntity).State;
				Assert.IsTrue(state == EntityState.Detached);
				myEntity.Text += " World !";
				dbContext.Entry(myEntity).State = EntityState.Modified;
				int changes = dbContext.SaveChanges();
				Assert.IsTrue(changes == 0);
			},
			onConcurrencyException: () =>
			{
				concurrencyExceptionOccurred = true;
			});

			Assert.IsTrue(concurrencyExceptionOccurred);
		}
	}

}
