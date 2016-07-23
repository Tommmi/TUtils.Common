using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;
using System.Data;
using System.Data.Entity;

namespace TUtils.Common.Test
{
	[TestClass]
	public class TransactionTest
	{
		private class Student
		{
			[Key]
			public int Id { get; set; }

			public string Name { get; set; }

			public Student(string name)
			{
				Name = name;
			}
		}

		private class MyTestDb : DbContextBase<MyTestDb>
		{
			public DbSet<Student> Students { get; set; }
		}

		[TestMethod]
		public void TestTransaction()
		{
			ILogWriter logWriter = new LogConsoleWriter(
				LogSeverityEnum.INFO, 
				new List<string> {"*"}, 
				new List<string> {});
			ITLog logger = new TLog(logWriter,isLoggingOfMethodNameActivated:false);
			IDbContextFactory<MyTestDb> dbContextFactory = new DbContextFactory4Unittest<MyTestDb>();
			ITransactionService<MyTestDb> transactionService = new TransactionService<MyTestDb>(
				logger,
				dbContextFactory,
				IsolationLevel.ReadCommitted);
			transactionService.DoInTransaction(dbContext =>
			{
				int? maxId = dbContext.Students.Max<Student,int?>(student => student.Id);
				dbContext.Students.Add(new Student($"Tommi ({(maxId.HasValue ? maxId.Value : 0)})"));
				dbContext.SaveChanges();
			});
		}
	}
}
