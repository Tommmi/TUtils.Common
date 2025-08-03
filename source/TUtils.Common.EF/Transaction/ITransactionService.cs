using System;
using Microsoft.EntityFrameworkCore;
using TUtils.Common.Transaction;

namespace TUtils.Common.EF.Transaction
{
	/// <summary>
	/// Provides DbContext and encapsulates actions with a transaction.
	/// </summary>
	/// <typeparam name="TDbContext"></typeparam>
	public interface ITransactionService<TDbContext> : ITransactionService
		where TDbContext : DbContext
	{

		/// <summary>
		/// May be nested
		/// </summary>
		/// <param name="action"></param>
		void DoInTransaction(Action<TDbContext> action);
		
		/// <summary>
		/// May be nested
		/// </summary>
		/// <param name="action"></param>
		/// <param name="onConcurrencyException">
		/// called for example in case of a lost update situation
		/// </param>
		void DoInTransaction(Action<TDbContext> action, Action onConcurrencyException);

		/// <summary>
		/// May be nested
		/// </summary>
		/// <param name="action"></param>
		T DoInTransaction<T>(Func<TDbContext, T> action);
		
		/// <summary>
		/// May be nested
		/// </summary>
		/// <param name="action"></param>
		/// <param name="onConcurrencyException">
		/// called for example in case of a lost update situation
		/// </param>
		T DoInTransaction<T>(Func<TDbContext, T> action, Action onConcurrencyException);
	}
}
