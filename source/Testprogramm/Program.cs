using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUtils.Common.Common;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.Log4Net;

namespace Testprogramm
{
	class Program
	{
		static void Main(string[] args)
		{
			ILogWriter logWriter = new Log4NetWriter();
			ITLog logger = new TLog(logWriter, isLoggingOfMethodNameActivated: false);
			IDbContextFactory<MyDbContext> dbContextFactory = new DbContextFactory4Unittest<MyDbContext>(typeof(Program).FullName);
			ITransactionService<MyDbContext> transactionService = new TransactionService<MyDbContext>(
				logger,
				dbContextFactory,
				IsolationLevel.ReadCommitted);
			transactionService.DoInTransaction(dbContext =>
			{
				dbContext.Students.Add(new Student());
				dbContext.SaveChanges();
				var count = dbContext.Students.Count();
			});
		}
	}
}