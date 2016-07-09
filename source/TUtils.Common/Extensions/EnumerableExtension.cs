using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Extensions
{
	public static class EnumerableExtension
	{


		public static T[] Append<T>(this T[] array, params T[] newObj)
		{
			int len = array.Length;
			T[] newArray = new T[len + newObj.Length];
			Array.Copy(array, newArray, len);

			for (int i = 0; i < newObj.Length; i++)
				newArray[len + i] = newObj[i];

			return newArray;
		}

		public static void ForEach<T>(this IEnumerable<T> @this, Action<T> bodyAction)
		{
			foreach (var item in @this)
				bodyAction(item);
		}

		/// <summary>
		/// Compares two enumerables, which contains comparable items.
		/// Returns the intersection and also the single items of the first and the second
		/// list.
		/// </summary>
		/// <typeparam name="T"> 
		/// must realize interface IComparable !
		/// </typeparam>
		/// <param name="firstList"></param>
		/// <param name="secondList"></param>
		/// <param name="singlesInFirstList"></param>
		/// <param name="singlesInSecondList"></param>
		/// <returns></returns>
		public static IEnumerable<T> Intersect2<T>(
			this IEnumerable<T> firstList, 
			IEnumerable<T> secondList,
			out IEnumerable<T> singlesInFirstList, 
			out IEnumerable<T> singlesInSecondList)
			where T: IComparable
		{
			var first = firstList.OrderBy(it => it).ToList();
			var second = secondList.OrderBy(it => it).ToList();
			int countFirst = first.Count;
			int countSecond = second.Count;
			var singlesFirst = new List<T>();
			singlesInFirstList = singlesFirst;
			var singlesSecond = new List<T>();
			singlesInSecondList = singlesSecond;
			var intersectList = new List<T>();

			while (countFirst>0 || countSecond>0)
			{
				if (countFirst > 0)
				{
					var obj1 = first[0];
					if (countSecond > 0)
					{
						var obj2 = second[0];

						int compareValue = obj1.CompareTo(obj2);

						if (compareValue == 0)
						{
							intersectList.Add(obj1);
							first.RemoveAt(0);
							countFirst--;
							second.RemoveAt(0);
							countSecond--;
						}
						// if obj1 > obj2
						else if (compareValue > 0)
						{
							singlesSecond.Add(obj2);
							second.RemoveAt(0);
							countSecond--;
						}
						// if obj1 < obj2
						else
						{
							singlesFirst.Add(obj1);
							first.RemoveAt(0);
							countFirst--;
						}
					}
					// if (countSecond == 0)
					else
					{
						singlesFirst.AddRange(first);
						break;
					}
				}
				// countFirst == 0
				else
				{
					singlesSecond.AddRange(second);
					break;
				}
			}

			return intersectList;
		}

	}
}
