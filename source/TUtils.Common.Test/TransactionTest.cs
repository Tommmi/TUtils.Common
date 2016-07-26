using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using LinqKit;
using TUtils.Common.Common;

namespace TUtils.Common.Test
{
	[TestClass]
	public class TransactionTest
	{
		private class Student
		{
			[Key]
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public int Id { get; set; }

			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string Name { get; set; }

			public virtual ICollection<Course> Courses { get; set; }
			public Student()
			{
				Courses = new HashSet<Course>();
			}

			public Student(string name)
			{
				Name = name;
			}
		}

		private class Course
		{
			public int Id { get; set; }

			public Course()
			{
				Students = new HashSet<Student>();
			}

			public virtual ICollection<Student> Students { get; set; }
		}

		private class MyTestDb : DbContextBase<MyTestDb>
		{
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public virtual DbSet<Student> Students { get; set; }
			public virtual DbSet<Course> Courses { get; set; }

			public MyTestDb() : base()
			{
				Configuration.ProxyCreationEnabled = true;
			}

			protected override void OnModelCreating(DbModelBuilder modelBuilder)
			{
				base.OnModelCreating(modelBuilder);
			}
		}

		[TestMethod]
		public void TestTransaction()
		{
			ILogWriter logWriter = new LogConsoleWriter(
				LogSeverityEnum.INFO, 
				new List<string> {"*"}, 
				new List<string>());
			ITLog logger = new TLog(logWriter,isLoggingOfMethodNameActivated:false);
			IDbContextFactory<MyTestDb> dbContextFactory = new DbContextFactory4Unittest<MyTestDb>(nameof(TestTransaction));

			ITransactionService<MyTestDb> transactionService = new TransactionService<MyTestDb>(
				logger,
				dbContextFactory,
				IsolationLevel.ReadCommitted);

			transactionService.DoInTransaction(dbContext =>
			{
				int? maxId = dbContext.Students.Max<Student, int?>(student => student.Id);
				Assert.IsTrue(!maxId.HasValue);
				var tommi = dbContext.Students.Add(new Student($"Tommi ({(maxId.HasValue ? maxId.Value : 0)})"));
				var milli = dbContext.Students.Add(new Student($"Milli ({(maxId.HasValue ? maxId.Value : 0)})"));
				dbContext.Students.Add(new Student($"Ralf ({(maxId.HasValue ? maxId.Value : 0)})"));
				var newCourse = dbContext.Courses.Add(new Course());
				newCourse.Students.Add(tommi);
				newCourse.Students.Add(milli);
				dbContext.SaveChanges();
				var ids = (	from course in dbContext.Courses
							where course.Students.Any(student => student.Name.StartsWith("T"))
							select course.Id)
						  .ToList();
				Assert.IsTrue(ids.Count == 1 && ids[0] == 1);
			});

			transactionService.DoInTransaction(dbContext =>
			{
				int? maxId = dbContext.Students.Max<Student, int?>(student => student.Id);
				Assert.IsTrue(maxId.HasValue && maxId == 3);
				dbContext.Students.Add(new Student($"Tommi ({(maxId.HasValue ? maxId.Value : 0)})"));
				dbContext.SaveChanges();
			});

			transactionService.DoInTransaction(dbContext =>
			{
				dbContext.Database.Log = text => logger.LogInfo(this, text);
				int? maxId = dbContext.Students.Max<Student, int?>(student => student.Id);
				Assert.IsTrue(maxId.HasValue && maxId == 4);
				dbContext.Students.Add(new Student($"Tommi ({(maxId.HasValue ? maxId.Value : 0)})"));
				var allStudents = dbContext.Students.ToList();
				Assert.IsTrue(!dbContext.Students.Any(s => s.Name == "Tommi (4)"));
				dbContext.SaveChanges();
				Assert.IsTrue(dbContext.Students.Any(s => s.Name == "Tommi (4)"));

				var filter = PredicateBuilder.False<Student>();
				filter = filter.Or(s => s.Name == "Milli (4)");
				filter = filter.Or(s => s.Name == "Tommi (4)");
				var exists = dbContext.Students.AsExpandable().Any(filter);
			});

			transactionService.DoInTransaction(dbContext =>
			{
				dbContext.Database.Log = text => logger.LogInfo(this, text);
				var existingStudent = new Student() { Id = 1, Name = "Tommi (0) updated" };
				dbContext.Entry(existingStudent).State = EntityState.Modified;
				dbContext.SaveChanges();
			});

			transactionService.DoInTransaction(dbContext =>
			{
				Assert.IsTrue(dbContext.Students.Find(1).Name== "Tommi (0) updated");
			});

		}
	}
}
