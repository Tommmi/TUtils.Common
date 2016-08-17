using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.EF6.Transaction;
using TUtils.Common.EF6.Transaction.Common;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects;
using System.Linq.Expressions;
using LinqKit;

namespace TUtils.Common.Test
{
	namespace ManyToMany
	{
		public class Student
		{
			public int Id { get; set; }

			[Column(TypeName = "VARCHAR")]
			[StringLength(200)]
			[Index]
			public string Name { get; set; }
		}

		public class Teacher
		{
			public int Id { get; set; }

			[Column(TypeName = "VARCHAR")]
			[StringLength(200)]
			[Index]
			public string Name { get; set; }
		}

		public class Student2Teacher
		{
			[Key, Column(Order = 0)]
			public int StudentId { get; set; }

			[Key, Column(Order = 1)]
			public int TeacherId { get; set; }
		}

		public class MyManyToManyDbContext : DbContextBase<MyManyToManyDbContext>
		{
			public MyManyToManyDbContext()
			{
				//Configuration.ProxyCreationEnabled = true;
				//Configuration.LazyLoadingEnabled = true;
			}

			public DbSet<Teacher> Teachers { get; set; }
			public DbSet<Student> Students { get; set; }
			public DbSet<Student2Teacher> Student2Teachers { get; set; }
		}

		public static class TeacherExtension
		{
			public static IQueryable<Student> GetStudents(this IQueryable<Teacher> teachers,
				IQueryable<Student2Teacher> student2Teachers, IQueryable<Student> students)
			{
				return
					from teacher in teachers
					join student2Teacher in student2Teachers on teacher.Id equals student2Teacher.TeacherId
					join student in students on student2Teacher.StudentId equals student.Id
					select student;
			}

			public static IQueryable<Student> GetStudents(this Teacher teacher, IQueryable<Student2Teacher> student2Teachers,
				IQueryable<Student> students)
			{
				return
					from student2Teacher in student2Teachers
					join student in students on student2Teacher.StudentId equals student.Id
					where teacher.Id == student2Teacher.TeacherId
					select student;
			}

		}

		[TestClass]
		public class ManyToManyTest
		{
			[TestMethod]
			public void TestManyToMany()
			{
				ILogWriter logWriter = new LogConsoleWriter(
					LogSeverityEnum.INFO,
					new List<string> {"*"},
					new List<string>());
				ITLog logger = new TLog(logWriter, isLoggingOfMethodNameActivated: false);
				IDbContextFactory<MyManyToManyDbContext> dbContextFactory =
					new DbContextFactory4Unittest<MyManyToManyDbContext>(nameof(TestManyToMany));
				ITransactionService<MyManyToManyDbContext> transactionService = new TransactionService<MyManyToManyDbContext>(
					logger,
					dbContextFactory,
					IsolationLevel.ReadCommitted);

				transactionService.DoInTransaction(db =>
				{
					var tommi = db.Students.Create();
					tommi.Name = "Tommi";
					var milli = db.Students.Create();
					milli.Name = "Milli";
					db.Students.AddRange(new[] {tommi, milli});

					var teacher1 = db.Teachers.Create();
					teacher1.Name = "Teacher 1";
					var teacher2 = db.Teachers.Create();
					teacher2.Name = "Teacher 2";
					db.Teachers.AddRange(new[] {teacher1, teacher2});

					db.SaveChanges();

					var student2Teacher = db.Student2Teachers.Create();
					student2Teacher.StudentId = tommi.Id;
					student2Teacher.TeacherId = teacher2.Id;
					db.Student2Teachers.Add(student2Teacher);

					student2Teacher = db.Student2Teachers.Create();
					student2Teacher.StudentId = tommi.Id;
					student2Teacher.TeacherId = teacher1.Id;
					db.Student2Teachers.Add(student2Teacher);

					student2Teacher = db.Student2Teachers.Create();
					student2Teacher.StudentId = milli.Id;
					student2Teacher.TeacherId = teacher1.Id;
					db.Student2Teachers.Add(student2Teacher);

					db.SaveChanges();
				});


				transactionService.DoInTransaction(db =>
				{
					var tommi = db.Students.First(s => s.Name == "Tommi");
					var milli = db.Students.First(s => s.Name == "Milli");
					var teacher1 = db.Teachers.First(s => s.Name == "Teacher 1");
					var teacher2 = db.Teachers.First(s => s.Name == "Teacher 2");

					db.Database.Log = text => logger.LogInfo(this, text);

					var teachers = (
						from teacher in db.Teachers
						join student2Teacher in db.Student2Teachers on teacher.Id equals student2Teacher.TeacherId
						join student in db.Students on student2Teacher.StudentId equals student.Id
						where student.Name == "Tommi"
						group teacher by teacher
						into teachersOfTommi
						select teachersOfTommi.Key)
						.ToList();






				});


			}

			private Expression<Func<Teacher, bool>> WhereAnyStudentIsTommi(MyManyToManyDbContext db)
			{
				var predicate = PredicateBuilder.False<Teacher>();
				predicate = predicate.Or(teacher => db.Student2Teachers
					.Join(db.Students, st => st.StudentId, s => s.Id, (st, s) => new {st, s})
					.Where(o => o.st.TeacherId == teacher.Id)
					.Any(o => o.s.Name == "Tommi"));
				return predicate;
			}
		}
	}
}

