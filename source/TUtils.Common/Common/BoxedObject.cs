namespace TUtils.Common.Common
{
	public class BoxedObject<T>
	{
		// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
		public T Value { get; set; }

		public BoxedObject(T value)
		{
			Value = value;
		}

		public BoxedObject()
		{
			Value = default(T);
		}

		public static implicit operator BoxedObject<T>(T value)
		{
			return new BoxedObject<T>(value);
		}

		public static explicit operator T(BoxedObject<T> value)
		{
			return value.Value;
		}

		public override bool Equals(object other)
		{
			if (typeof(T).IsByRef && Value == null)
				return other == null;
			if (other == null)
				return false;
			return Value.Equals(other);
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			if (typeof(T).IsByRef && Value == null)
				return 0;
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			if (typeof(T).IsByRef && Value == null)
				return "";
			return Value.ToString();
		}
	}
}
