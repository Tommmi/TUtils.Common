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
		/// <summary>
		/// all transactions called in "action" will use the same DbContext object if possible.
		/// May be nested
		/// </summary>
		/// <param name="action">
		/// 
		/// </param>
		void DoWithSameDbContext(Action action);

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
	}
}