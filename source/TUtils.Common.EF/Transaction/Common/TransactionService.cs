using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TUtils.Common.Logging;
using TUtils.Common.Transaction;

namespace TUtils.Common.EF.Transaction.Common
{
	internal class ThreadsDbContextType
	{
		public DbContext DbContext { get; set; }
		public Guid TransactionService { get; set; }
		public bool HasTransactionBegun { get; set; }
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
		private readonly Guid _serviceId;

		private class ThreadsDbContextSafeType
		{
			private readonly ThreadsDbContextType _threadDbContext;

			public ThreadsDbContextSafeType(ThreadsDbContextType threadDbContext)
			{
				_threadDbContext = threadDbContext;
			}

			public bool HasTransactionBegun
			{
				get { return _threadDbContext.HasTransactionBegun; }
				set { _threadDbContext.HasTransactionBegun = value; }
			}

			public TDbContext DbContext
			{
				get { return _threadDbContext.DbContext as TDbContext; }
				set { _threadDbContext.DbContext = value; }
			}

			// ReSharper disable once UnusedMember.Local
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

		void ITransactionService.DoWithSameDbContext(Action action)
		{
			DoWithSameDbContext(dbContext =>
			{
				action();
				return new Tuple<bool, int>(true, 0);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		/// <returns>
		/// Item1: succeeded ?
		/// Item2: result data in case of successful execution
		/// </returns>
		private Tuple<bool, T> DoWithSameDbContext<T>(Func<TDbContext, Tuple<bool, T>> action)
		{
			var dbContext = ThreadsDbContext.DbContext;
			bool isOuterContext = false;
			try
			{
				if (dbContext == null)
				{
					dbContext = _dbContextFactory.Create();
					ThreadsDbContext.DbContext = dbContext;
					isOuterContext = true;
				}

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
			(this as ITransactionService<TDbContext>).DoInTransaction(
				db =>
				{
					action(db);
					return 0;
				},
				onConcurrencyException);
		}

		T ITransactionService.DoWithSameDbContext<T>(Func<T> action)
		{
			return DoWithSameDbContext(dbContext =>
			{
				var resultVal = action();
				return new Tuple<bool, T>(true, resultVal);
			}).Item2;
		}

		void ITransactionService.DoInTransaction(Action action)
		{
			(this as ITransactionService<TDbContext>).DoInTransaction(db =>
			{
				action();
			});
		}

		T ITransactionService.DoInTransaction<T>(Func<T> action)
		{
			return (this as ITransactionService<TDbContext>).DoInTransaction(
				db => action());
		}

		T ITransactionService<TDbContext>.DoInTransaction<T>(Func<TDbContext, T> action)
		{
			return (this as ITransactionService<TDbContext>).DoInTransaction(
				action, onConcurrencyException: null);
		}

		T ITransactionService<TDbContext>.DoInTransaction<T>(Func<TDbContext, T> action, Action onConcurrencyException)
		{
			if (ThreadsDbContext.HasTransactionBegun)
				return DoWithSameDbContext(dbContext => new Tuple<bool, T>(true, action(dbContext))).Item2;

			Exception lastException = null;
			bool succeeded = false;
			var returnVal = default(T);
			for (int i = 0; i < 3 && !succeeded; i++)
			{
				// if transaction failed due to concurrency exception
				if (i > 0)
				{
					ThreadsDbContext.DbContext?.Dispose();
					ThreadsDbContext.DbContext = null;
					ThreadsDbContext.DbContext = _dbContextFactory.Create();
				}

				var result = DoWithSameDbContext(dbContext =>
				{
					try
					{
						using (var transaction = dbContext.Database.BeginTransaction(_isolationLevel))
						{
							ThreadsDbContext.HasTransactionBegun = true;

							try
							{
								returnVal = action(dbContext);
								transaction.Commit();
								return new Tuple<bool, T>(true, returnVal);
							}
							catch (DbUpdateConcurrencyException e)
							{
								lastException = Rollback(e, transaction);
							}
							catch (Exception e) when (IsConcurrencyException(e))
							{
								lastException = Rollback(e, transaction);
							}
							catch (Exception e)
							{
								lastException = Rollback(e, transaction);
								_logger.LogException(e);
								throw;
							}

							return new Tuple<bool, T>(false, default(T));
						}
					}
					finally
					{
						ThreadsDbContext.HasTransactionBegun = false;
					}
				});

				succeeded = result.Item1;
			}

			if (succeeded)
				return returnVal;

			if (onConcurrencyException == null)
			{
				_logger.LogException(lastException);
				throw new ApplicationException("concurrency exception", lastException);
			}

			onConcurrencyException();
			return returnVal;
		}

		private static bool IsConcurrencyException(Exception e)
		{
			// Check for common concurrency-related exceptions in EF Core
			return e.Message.Contains("concurrency") ||
			       e.Message.Contains("conflict") ||
			       e is DbUpdateConcurrencyException;
		}

		private Exception Rollback(Exception e, IDbContextTransaction transaction)
		{
			Exception lastException = null;
			try
			{
				lastException = e;
				transaction.Rollback();
			}
			catch (Exception e2)
			{
				_logger.LogException(e2);
			}
			return lastException;
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
