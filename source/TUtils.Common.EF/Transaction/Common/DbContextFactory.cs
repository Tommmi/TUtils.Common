using Microsoft.EntityFrameworkCore;

namespace TUtils.Common.EF.Transaction.Common
{
	// Custom interface to match original TUtils pattern
	public interface IDbContextFactory<TDbContext>
		where TDbContext : DbContext
	{
		TDbContext Create();
	}

	public class DbContextFactory<TDbContext> : IDbContextFactory<TDbContext>
		where TDbContext : DbContext, new()
	{
		public TDbContext Create()
		{
			return new TDbContext();
		}
	}
}
