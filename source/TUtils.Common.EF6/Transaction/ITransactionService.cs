using System;
using System.Data.Entity;

namespace TUtils.Common.EF6.Transaction
{
	/// <summary>
	/// provides DbContext and encapsulates actions with a transaction.
	/// </summary>
	/// <typeparam name="TDbContext"></typeparam>
	public interface ITransactionService<TDbContext>
		where TDbContext : DbContext
	{
		void DoInTransaction(Action<TDbContext> action);
		void DoInTransaction(TDbContext reusedContext, Action<TDbContext> action);
		void DoInTransaction(Action<TDbContext> action, Action onConcurrencyException);
		void DoInTransaction(TDbContext reusedContext, Action<TDbContext> action, Action onConcurrencyException);
	}
}