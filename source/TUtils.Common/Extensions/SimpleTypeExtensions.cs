using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Extensions
{
	public static class SimpleTypeExtensions
	{
#if !SCRIPT
		public static void SerializeNumber(this long value, byte[] dest, int destOffset, int byteCount)
		{
			long pattern = 0xFF;
			int move = 0;
			var destIdx = destOffset;
			for (int i = 0; i < byteCount; i++)
			{
				dest[destIdx] = (byte)((value & pattern) >> move);
				destIdx++;
				pattern <<= 8;
				move += 8;
			}
		}
#endif
		public static string ToMillisecondsFormat(this DateTime dateTime)
		{
			return $"{dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}:{dateTime.Millisecond}";
		}

		public static bool HasInterface(this Type @this, Type type)
		{
			return @this.GetInterfaces().Any(interfc => interfc == type);
		}

		public static bool AreEqual(this byte[] @this, byte[] other)
		{
			if (other?.Length != @this.Length)
				return false;
			return !@this.Where((t, i) => t != other[i]).Any();
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> @this)
		{
			if (@this == null)
				return true;

			if (@this is string)
				return (@this as string).Length == 0;

			return !@this.Any();
		}

		public static bool IsEmpty(this string @this)
		{
			return @this.IsNullOrEmpty();
		}

		public static bool IsNullOrEmpty(this string @this)
		{
			if (@this == null)
				return true;

			return @this.Length == 0;
		}

		public static byte[] ConcatBytes(this byte[] @this, byte[] other)
		{
			if (other == null)
				return @this;
			var result = new byte[@this.Length+other.Length];
			Array.Copy(@this, 0, result, 0, @this.Length);
			Array.Copy(other, 0, result, @this.Length, other.Length);
			return result;
		}

		public static byte[] SubBytes(this byte[] @this, int startIdx, int count)
		{
			var bytes = new byte[count];
			if ( startIdx + count > @this.Length)
				throw new ApplicationException(
					$"67ejskdln329rj @this.Length={@this.Length}, startIdx={startIdx}, int count={count}");
			Array.Copy(@this, startIdx, bytes, 0, count);
			return bytes;
		}

		public static byte[] ToByteArray(this ushort @this)
		{
			var buffer = new byte[2];
			buffer[0] = (byte)(@this & 0x00FF);
			buffer[1] = (byte)((@this & 0xFF00)>>8);
			return buffer;
		}

		public static int ToInt32(this double value)
		{
#if SCRIPT
			return (int)value;
#else
			return Convert.ToInt32(value);
#endif
		}

		public static byte[] ToByteArray(this uint @this)
		{
			var buffer = new byte[4];
			buffer[0] = (byte)(@this & 0x000000FF);
			buffer[1] = (byte)((@this & 0x0000FF00) >> 8);
			buffer[2] = (byte)((@this & 0x00FF0000) >> 16);
			buffer[3] = (byte)((@this & 0xFF000000) >> 24);
			return buffer;
		}

		/// <summary>
		/// works only with string which can be represented by UTF8
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		// ReSharper disable once InconsistentNaming
		public static byte[] ToUTF8CodedByteArray(this string text)
		{
			return Encoding.UTF8.GetBytes(text);
		}

		/// <summary>
		/// also serializes high surrogate characters like in
		/// "Test\ud800Test"
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static byte[] ToUnderlyingByteArray(this string text)
		{
			var bytes = new byte[text.Length * sizeof(char)];
			Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary>
		/// converts Base64 string into original byte array
		/// </summary>
		/// <param name="base64String">
		/// characters a-z,A-Z,+,/,=
		/// </param>
		/// <returns></returns>
		public static byte[] ToByteArrayFromBase64String(this string base64String)
		{
			return Convert.FromBase64String(base64String);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns>
		/// characters a-z,A-Z,+,/,=
		/// </returns>
		public static string ToBase64String(this byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		/// <summary>
		/// converts to string from the UTF8 encoded byte array
		/// </summary>
		/// <param name="utf8CodedByteArray"></param>
		/// <returns></returns>
		// ReSharper disable once InconsistentNaming
		public static string ToStringFromUTF8CodedByteArray(this byte[] utf8CodedByteArray)
		{
			return Encoding.UTF8.GetString(utf8CodedByteArray);
		}

		/// <summary>
		/// converts to string from the underlying char array
		/// </summary>
		/// <param name="stringCharArray"></param>
		/// <returns></returns>
		public static string ToStringFromUnderlyingByteArray(this byte[] stringCharArray)
		{
			char[] chars = new char[stringCharArray.Length / sizeof(char)];
			Buffer.BlockCopy(stringCharArray, 0, chars, 0, stringCharArray.Length);
			return new string(chars);
		}

#if !SCRIPT
		private static TEnum ConvertToEnumInternal<TEnum>(object value, TEnum unknownValue)
		{
			TEnum destValue;
			value.TryConvertToType(out destValue, unknownValue);
			return destValue;
		}

		public static TEnum AsEnum<TEnum>(this byte value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this ushort value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this int value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this uint value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this long value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this ulong value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}

		public static TEnum AsEnum<TEnum>(this string value, TEnum unknownValue)
		{
			return ConvertToEnumInternal(value, unknownValue);
		}
#endif

	}
}
