using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using TUtils.Common.Logging;

namespace TUtils.Common.EF6.Transaction.Common
{
	/// <summary>
	/// For unit test for example:
	/// <example><code><![CDATA[
	/// ILogWriter logWriter = new LogConsoleWriter(
	/// 	LogSeverityEnum.INFO, 
	/// 	new List<string> {"*"}, 
	/// 	new List<string>());
	/// ITLog logger = new TLog(logWriter,isLoggingOfMethodNameActivated:false);
	/// IDbContextFactory<MyTestDb> dbContextFactory = new DbContextFactory4Unittest<MyTestDb>("MyUniqueTestName");
	/// ITransactionService<MyTestDb> transactionService = new TransactionService<MyTestDb>(
	/// 	logger,
	/// 	dbContextFactory,
	/// 	IsolationLevel.ReadCommitted);
	/// transactionService.DoInTransaction(dbContext =>
	/// {
	///		...
	/// });
	/// ]]></code></example>
	/// 
	/// For productive code for example:
	/// <example><code><![CDATA[
	/// ILogWriter logWriter = new Log4NetWriter();
	/// ITLog logger = new TLog(logWriter, isLoggingOfMethodNameActivated: false);
	/// IDbContextFactory<MyTestDb> dbContextFactory = new DbContextFactory<MyTestDb>();
	/// ITransactionService<MyTestDb> transactionService = new TransactionService<MyTestDb>(
	/// 	logger,
	/// 	dbContextFactory,
	/// 	IsolationLevel.ReadCommitted);
	/// transactionService.DoInTransaction(dbContext =>
	/// {
	///		...
	/// });
	/// ]]></code></example>
	/// </summary>
	/// <typeparam name="TDbContext"></typeparam>
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

		void ITransactionService<TDbContext>.DoInTransaction(Action<TDbContext> action)
		{
			(this as ITransactionService<TDbContext>).DoInTransaction(reusedContext: null, action: action, onConcurrencyException: null);
		}

		void ITransactionService<TDbContext>.DoInTransaction(TDbContext reusedContext, Action<TDbContext> action)
		{
			(this as ITransactionService<TDbContext>).DoInTransaction(reusedContext: reusedContext, action: action, onConcurrencyException:null);
		}

		void ITransactionService<TDbContext>.DoInTransaction(
			Action<TDbContext> action, 
			Action onConcurrencyException)
		{
			(this as ITransactionService<TDbContext>).DoInTransaction(reusedContext: null, action: action, onConcurrencyException: onConcurrencyException);
		}

		void ITransactionService<TDbContext>.DoInTransaction(
			TDbContext reusedContext, 
			Action<TDbContext> action, 
			Action onConcurrencyException)
		{
			Exception lastException = null;

			for (int i = 0; i < 3; i++)
			{
				using (var dbContext = reusedContext ?? _dbContextFactory.Create())
				{
					dbContext.Database.Initialize(force: false);
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
			}

			if (onConcurrencyException == null)
			{
				_logger.LogException(lastException);
				throw new ApplicationException("concurrency exception", lastException);
			}

			onConcurrencyException();
		}
	}
}
