using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Extensions
{
	public static class ListExtensions
	{
		public static void RemoveWhere<TItemType>(this IList<TItemType> @this, Func<TItemType, bool> mustRemove)
		{
			for (int i = @this.Count - 1; i >= 0; i--)
			{
				if ( mustRemove(@this[i]) )
					@this.RemoveAt(i);
			}
		}

		/// <summary>
		/// Clears the list and sets the new items.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="items"></param>
		public static void SetRange<T>(this List<T> list, IEnumerable<T> items)
		{
			list.Clear();
			list.AddRange(items);
		}

		/// <summary>
		/// True, if enumerations have same size and elements are equal (object.Equals() - method).
		/// Parameters @this and enumeration may be null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <param name="enumeration"></param>
		/// <returns></returns>
		public static bool AreEquals<T>(this IEnumerable<T> @this, IEnumerable<T> enumeration)
		{
			if (@this == null)
				return enumeration == null;

			if (enumeration == null)
				return false;

			var enumerable = @this as T[] ?? @this.ToArray();
			int count = enumerable.Length;
			var enumeration1 = enumeration as T[] ?? enumeration.ToArray();
			if (count != enumeration1.Length)
				return false;

			var thisIdx = enumerable.GetEnumerator();
			var otherIdx = enumeration1.GetEnumerator();

			for (int i = 0; i < count; i++)
			{
				thisIdx.MoveNext();
				otherIdx.MoveNext();
				object val1 = thisIdx.Current;
				object val2 = otherIdx.Current;
				if (ReferenceEquals(val1,null))
					return ReferenceEquals(val2,null);

				if (!val1.Equals(val2))
					return false;
			}

			return true;
		}

	}
}
