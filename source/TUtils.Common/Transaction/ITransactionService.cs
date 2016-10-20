using System;

namespace TUtils.Common.Transaction
{
	public interface ITransactionService
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
		/// all transactions called in "action" will use the same DbContext object if possible.
		/// May be nested
		/// </summary>
		/// <param name="action">
		/// 
		/// </param>
		T DoWithSameDbContext<T>(Func<T> action);

		/// <summary>
		/// all transactions called in "action" will run in same transactions.
		/// May be nested
		/// </summary>
		/// <param name="action">
		/// 
		/// </param>
		void DoInTransaction(Action action);

		/// <summary>
		/// all transactions called in "action" will run in same transactions.
		/// May be nested
		/// </summary>
		/// <param name="action">
		/// 
		/// </param>
		T DoInTransaction<T>(Func<T> action);

	}
}