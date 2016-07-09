using System;

namespace TUtils.Common
{
	/// <summary>
	/// Represents an array of objects, which may be compared by Dictionary, ...
	/// Overrides Equality Operator
	/// </summary>
	internal class ComparableObjectArray : IEquatable<ComparableObjectArray>
	{
		private readonly int _hashCode;
		public object[] Array { get; }

		bool IEquatable<ComparableObjectArray>.Equals(ComparableObjectArray other)
		{
			if (ReferenceEquals(other, null))
				return false;
			return _hashCode == other._hashCode && Equals(Array, other.Array);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return ((IEquatable<ComparableObjectArray>)this).Equals((ComparableObjectArray)obj);
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}


		public ComparableObjectArray(object[] array) : this(array, CalcHash(array))
		{
		}

		public ComparableObjectArray(object[] array, int hashCode)
		{
			_hashCode = hashCode;
			Array = array;
		}

		private static int CalcHash(object[] key)
		{
			int hashCode = 0;
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var k in key)
				hashCode = k?.GetHashCode() ?? 0 + (hashCode << 6) + (hashCode << 16) - hashCode;

			return hashCode;
		}

		public static bool operator==(ComparableObjectArray o1, ComparableObjectArray o2)
		{
			if (ReferenceEquals(o1, null))
				return ReferenceEquals(o2, null);
			if (ReferenceEquals(o2, null))
				return false;
			return Equals(o1.Array, o2.Array);
		}

		public static bool operator !=(ComparableObjectArray o1, ComparableObjectArray o2)
		{
			return !(o1 == o2);
		}

		private static bool Equals(object[] array1, object[] array2)
		{
			for (int i = 0; i < array1.Length; i++)
			{
				var obj1 = array1[i];
				var obj2 = array2[i];
				if (ReferenceEquals(obj1, null))
				{
					if (!ReferenceEquals(obj2, null))
						return false;
				}
				else
				{
					if (ReferenceEquals(obj2, null))
						return false;
					if (!obj1.Equals(obj2))
						return false;
				}
			}
			return true;
		}
	}
}