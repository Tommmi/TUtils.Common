using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TUtils.Common.Test
{
	/// <summary>
	/// Test suite for IndexedTable functionality.
	/// Tests the high-performance indexed data structure for tuple storage and retrieval.
	/// </summary>
	[TestClass]
	public class IndexedTupleSetTest
	{
		/// <summary>
		/// Tests IndexedTable with null values and composite key lookup.
		/// Verifies that the table can handle null values correctly and perform multi-key searches.
		/// </summary>
		[TestMethod]
		public void TestIndexedTupleSet2()
		{
			// Arrange: Create indexed table with composite index on first two items
			var set = new IndexedTable<int, object, string>();
			set.AddIndex(true, true, false); // Index on Item1 and Item2, not Item3
			
			// Act: Insert tuple with null value and search by composite key
			set.Insert(new Tuple<int, object, string>(1, null, "hello world"));
			var val = set.FindByItem1_2(1, null);
			
			// Assert: Verify correct retrieval
			Assert.IsTrue(val.Count() == 1, "Should find exactly one matching tuple");
			Assert.IsTrue(val.First().Item3 == "hello world", "Retrieved tuple should contain expected string value");
		}

		/// <summary>
		/// Comprehensive test for IndexedTable with multiple query types and removal operations.
		/// Tests single-key queries, composite-key queries, and selective removal functionality.
		/// </summary>
		[TestMethod]
		public void TestIndexedTupleSet1()
		{
			// Arrange: Create indexed table and populate with test data
			var set = new IndexedTable<int, int, string>();

			// Populate table with 10x10 grid of tuples
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					set.Insert(new Tuple<int, int, string>(i, j, i.ToString() + j));
				}
			}

			// Act & Assert: Test single-key queries on Item1
			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem1(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 10, $"Should find 10 tuples with Item1={i}");
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item1 == i), $"All returned tuples should have Item1={i}");
			}

			// Act & Assert: Test single-key queries on Item2
			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem2(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 10, $"Should find 10 tuples with Item2={i}");
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item2 == i), $"All returned tuples should have Item2={i}");
			}

			// Act & Assert: Test composite-key queries
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					var list = set.FindByItem1_2(i, j);
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.Count() == 1, $"Should find exactly 1 tuple with Item1={i} and Item2={j}");
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.All(tuple => tuple.Item1 == i && tuple.Item2 == j), 
						$"Returned tuple should match both Item1={i} and Item2={j}");
				}
			}

			// Act: Test removal functionality - remove all tuples where Item1 == 2
			set.RemoveAllMatchingItem1(2);

			// Assert: Verify removal effects on Item1 queries
			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem1(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == (i == 2 ? 0 : 10), 
					$"After removal, Item1={i} should return {(i == 2 ? 0 : 10)} tuples");
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item1 == i), 
					$"All remaining tuples should still have Item1={i}");
			}

			// Assert: Verify removal effects on Item2 queries
			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem2(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 9, 
					$"After removal, Item2={i} should return 9 tuples (removed one with Item1=2)");
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item2 == i), 
					$"All remaining tuples should still have Item2={i}");
			}

			// Assert: Verify removal effects on composite queries
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					var list = set.FindByItem1_2(i, j);
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.Count() == (i == 2 ? 0 : 1), 
						$"After removal, Item1={i} and Item2={j} should return {(i == 2 ? 0 : 1)} tuples");
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.All(tuple => tuple.Item1 == i && tuple.Item2 == j), 
						$"Any remaining tuple should match both Item1={i} and Item2={j}");
				}
			}
		}
	}
}
