using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using TUtils.Common.Logging;

namespace TUtils.Common.EF6.Transaction.Common
{
	public class TransactionService<TDbContext> : ITransactionService<TDbContext>
		where TDbContext : DbContext
	{
		private readonly ITLog _logger;
		private readonly IDbContextFactory<TDbContext> _dbContextFactory;
		private readonly IsolationLevel _isolationLevel;

		public TransactionService(
			ITLog logger,
			IDbContextFactory<TDbContext> dbContextFactory,
			IsolationLevel isolationLevel)
		{
			_logger = logger;
			_dbContextFactory = dbContextFactory;
			_isolationLevel = isolationLevel;
		}

		public void DoInTransaction(Action<TDbContext> action)
		{
			Exception lastException = null;
			for (int i = 0; i < 3; i++)
			{
				using (var dbContext = _dbContextFactory.Create())
				using (var transaction = dbContext.Database.BeginTransaction(_isolationLevel))
				{
					try
					{
						action(dbContext);
						transaction.Commit();
						return;
					}
					catch (DBConcurrencyException e)
					{
						transaction.Rollback();
						lastException = e;
					}
					catch (DbUpdateConcurrencyException e)
					{
						transaction.Rollback();
						lastException = e;
					}
					catch (OptimisticConcurrencyException e)
					{
						transaction.Rollback();
						lastException = e;
					}
					catch (Exception e)
					{
						transaction.Rollback();
						_logger.LogException(e);
						throw;
					}
				}
			}

			_logger.LogException(lastException);
			throw new ApplicationException("concurrency exception", lastException);
		}
	}
}
