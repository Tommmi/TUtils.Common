using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TUtils.Common.Test
{
	[TestClass]
	public class IndexedTupleSetTest
	{
		[TestMethod]
		public void TestIndexedTupleSet2()
		{
			var set = new IndexedTable<int, object, string>();
			set.AddIndex(true,true,false);
			set.Insert(new Tuple<int, object, string>(1,null,"hello world"));
			var val = set.FindByItem1_2(1, null);
			Assert.IsTrue(val.Count() == 1);
			Assert.IsTrue(val.First().Item3 == "hello world");
		}

		[TestMethod]
		public void TestIndexedTupleSet1()
		{
			var set = new IndexedTable<int, int, string>();

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					set.Insert(new Tuple<int, int, string>(i, j, i.ToString() + j));
				}
			}

			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem1(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 10);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item1 == i));
			}

			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem2(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 10);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item2 == i));
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					var list = set.FindByItem1_2(i,j);
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.Count() == 1);
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.All(tuple => tuple.Item1 == i && tuple.Item2 == j));
				}
			}

			// remove item1 == 2
			set.RemoveAllMatchingItem1(2);

			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem1(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == (i==2 ? 0 : 10));
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item1 == i));
			}

			for (int i = 0; i < 10; i++)
			{
				var list = set.FindByItem2(i);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.Count() == 9);
				// ReSharper disable once PossibleMultipleEnumeration
				Assert.IsTrue(list.All(tuple => tuple.Item2 == i));
			}

			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					var list = set.FindByItem1_2(i, j);
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.Count() == (i==2 ? 0 : 1));
					// ReSharper disable once PossibleMultipleEnumeration
					Assert.IsTrue(list.All(tuple => tuple.Item1 == i && tuple.Item2 == j));
				}
			}
		}
	}
}
