using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Common;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;

namespace TUtils.Common.Test
{
	[TestClass]
	public class LinqTest
	{
		private class Address
		{
			public int Id { get; set; }
			public string Town { get; set; }
		}

		private class Student
		{
			public Student()
			{
				Courses = new HashSet<Course>();
			}

			public int Id { get; set; }
			public string Name { get; set; }
			public virtual Address Address { get; set; }
			public virtual ICollection<Course> Courses { get; set; }
		}

		private class Course
		{
			public Course()
			{
				Students = new HashSet<Student>();
			}

			public int Id { get; set; }
			public string Name { get; set; }
			public virtual ICollection<Student> Students { get; set; }
		}



		private class MyDbContext : DbContextBase<MyDbContext>
		{
			public MyDbContext() : base()
			{
				Configuration.ProxyCreationEnabled = true;
			}
			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				modelBuilder.Types().Configure(t => t.MapToStoredProcedures());
				base.OnModelCreating(modelBuilder);
			}

			public virtual DbSet<Student> Students { get; set; }
			public virtual DbSet<Address> Addresses { get; set; }
			public virtual DbSet<Course> Courses { get; set; }
		}

		[TestMethod]
		public void TestGroupBy()
		{
			ILogWriter logWriter = new LogConsoleWriter(
				LogSeverityEnum.INFO,
				new List<string> { "*" },
				new List<string>());
			ITLog logger = new TLog(logWriter, isLoggingOfMethodNameActivated: false);
			IDbContextFactory<MyDbContext> dbContextFactory = new DbContextFactory4Unittest<MyDbContext>(nameof(TestGroupBy));
			ITransactionService<MyDbContext> transactionService = new TransactionService<MyDbContext>(
				logger,
				dbContextFactory,
				IsolationLevel.ReadCommitted);

			transactionService.DoInTransaction(db =>
			{
				var hamburg = new Address { Town = "Hamburg" };
				var hannover = new Address { Town = "Hannover" };
				db.Addresses.AddRange(new[] {hamburg, hannover});
				db.SaveChanges();

				var latin = new Course { Name = "Latin" };
				var spanish = new Course { Name = "Español" };
				db.Courses.AddRange(new[] { latin,spanish });
				db.SaveChanges();

				var tommi = new Student { Address = hamburg, Name = "Tommi" };
				var milli = new Student { Address = hamburg, Name = "Milli" };
				var martin = new Student { Address = hamburg, Name = "Martin" };
				var paul = new Student { Address = hamburg, Name = "Paul" };

				var frank = new Student { Address = hannover, Name = "Frank" };
				var anna = new Student { Address = hannover, Name = "Anna" };
				var celine = new Student { Address = hannover, Name = "Celine" };
				var jorge = new Student { Address = hannover, Name = "Jorge" };

				db.Students.AddRange(new[] { tommi, milli, martin, paul, frank, anna, celine, jorge });

				latin.Students.Add(tommi);
				latin.Students.Add(martin);
				latin.Students.Add(paul);
				latin.Students.Add(anna);
				latin.Students.Add(celine);

				spanish.Students.Add(tommi);
				spanish.Students.Add(milli);
				spanish.Students.Add(paul);
				spanish.Students.Add(frank);
				spanish.Students.Add(anna);
				spanish.Students.Add(jorge);
				db.SaveChanges();

			});

			transactionService.DoInTransaction(db =>
			{
				db.Database.Log = text => logger.LogInfo(this, text);
				Assert.IsTrue(db.Courses.Count() == 2);
				Assert.IsTrue(db.Addresses.Count() == 2);
				Assert.IsTrue(db.Students.Count() == 8);

				var results = (
					from student in db.Students
					from course in student.Courses
					group student
						by new {Town = student.Address.Town, Course = course}
						into studentsOfSameTownAndCourse
					select new
					{
						Town = studentsOfSameTownAndCourse.Key.Town,
						Course = studentsOfSameTownAndCourse.Key.Course.Name,
						StudentCount = studentsOfSameTownAndCourse.Count()
					}).ToList();
				Assert.IsTrue(results.First(r=>r.Course=="Latin" && r.Town=="Hannover").StudentCount==2);
			});
		}
	}
}
