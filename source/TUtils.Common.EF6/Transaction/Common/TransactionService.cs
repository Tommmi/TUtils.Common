using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using TUtils.Common.Logging;

namespace TUtils.Common.EF6.Transaction.Common
{
	internal class ThreadsDbContextType
	{
		public DbContext DbContext { get; set; }
		public Guid TransactionService { get; set; }
	}

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
		where TDbContext : DbContext, new()
	{
		private readonly ITLog _logger;
		private readonly IDbContextFactory<TDbContext> _dbContextFactory;
		private readonly IsolationLevel _isolationLevel;
		private Guid _serviceId;

		private class ThreadsDbContextSafeType
		{
			private readonly ThreadsDbContextType _threadDbContext;

			public ThreadsDbContextSafeType(ThreadsDbContextType threadDbContext)
			{
				_threadDbContext = threadDbContext;
			}

			public TDbContext DbContext
			{
				get { return _threadDbContext.DbContext as TDbContext; }
				set { _threadDbContext.DbContext = value; }
			}

			public Guid TransactionService
			{
				get { return _threadDbContext.TransactionService; }
				set { _threadDbContext.TransactionService = value; }
			}
		}

		public TransactionService(
			ITLog logger,
			IDbContextFactory<TDbContext> dbContextFactory,
			IsolationLevel isolationLevel)
		{
			_logger = logger;
			_dbContextFactory = dbContextFactory;
			_isolationLevel = isolationLevel;
			_serviceId = Guid.NewGuid();
		}

		void ITransactionService<TDbContext>.DoWithSameDbContext(Action action)
		{
			DoWithSameDbContext(dbContext =>
			{
				action();
				return true;
			});
		}

		private bool DoWithSameDbContext(Func<TDbContext, bool> action)
		{
			var dbContext = ThreadsDbContext.DbContext;
			bool isOuterContext = false;
			if (dbContext == null)
			{
				dbContext = _dbContextFactory.Create();
				ThreadsDbContext.DbContext = dbContext;
				isOuterContext = true;
			}

			try
			{
				return action(dbContext);
			}
			finally
			{
				if (isOuterContext)
				{
					ThreadsDbContext.DbContext = null;
					dbContext?.Dispose();
				}
			}
		}

		void ITransactionService<TDbContext>.DoInTransaction(Action<TDbContext> action)
		{
			(this as ITransactionService<TDbContext>).DoInTransaction(action: action, onConcurrencyException: null);
		}

		void ITransactionService<TDbContext>.DoInTransaction(
			Action<TDbContext> action,
			Action onConcurrencyException)
		{
			Exception lastException = null;
			bool succeeded = false;
			for (int i = 0; i < 3 && !succeeded; i++)
			{
				// if transaction failed due to concurrency exception
				if (i > 0)
				{
					ThreadsDbContext.DbContext = null;
					ThreadsDbContext.DbContext?.Dispose();
					ThreadsDbContext.DbContext = _dbContextFactory.Create();
				}

				succeeded = DoWithSameDbContext(dbContext =>
				{
					using (var transaction = dbContext.Database.BeginTransaction(_isolationLevel))
					{
						try
						{
							action(dbContext);
							transaction.Commit();
							return true;
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

						return false;
					}
				});
			}

			if (succeeded)
				return;

			if (onConcurrencyException == null)
			{
				_logger.LogException(lastException);
				throw new ApplicationException("concurrency exception", lastException);
			}

			onConcurrencyException();
		}

		private ThreadsDbContextSafeType ThreadsDbContext
		{
			get
			{
				var context = TThreadStorage<ThreadsDbContextType>.GetData(DbContextId);
				if (context.TransactionService != _serviceId)
				{
					context.TransactionService = _serviceId;
					context.DbContext = null;
				}
				return new ThreadsDbContextSafeType(context);
			}
		}

		private const string DbContextId = "TransactionService.DbContextId";
	}
}
