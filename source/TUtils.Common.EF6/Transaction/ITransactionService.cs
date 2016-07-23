using System;
using System.Data.Entity;

namespace TUtils.Common.EF6.Transaction
{
	public interface ITransactionService<out TDbContext>
		where TDbContext : DbContext
	{
		void DoInTransaction(Action<TDbContext> action);
	}
}