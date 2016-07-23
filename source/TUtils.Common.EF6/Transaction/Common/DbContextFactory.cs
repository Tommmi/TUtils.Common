using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace TUtils.Common.EF6.Transaction.Common
{
	public class DbContextFactory<TDbContext> : IDbContextFactory<TDbContext>
		where TDbContext : DbContext, new()
	{
		public TDbContext Create()
		{
			return new TDbContext();
		}
	}
}