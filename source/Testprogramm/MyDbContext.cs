using System.Data.Entity;
using TUtils.Common.EF6.Transaction.Common;

namespace Testprogramm
{
	public class MyDbContext : DbContextBase<MyDbContext>
	{
		public virtual DbSet<Student> Students { get; set; }
		public MyDbContext() : base()
		{
			
		}
	}
}