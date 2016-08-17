using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Extensions
{
    public static class StringExtension
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path2"></param>
		/// <returns></returns>
		public static string Combine(this string @this, string path2)
		{
			return Path.Combine(@this, path2);
		}

	    public static string CutRight(this string text, int countOfCharacters)
	    {
		    return text.Substring(0, text.Length - countOfCharacters);
	    }

		public static string ToHex(this int value)
        {
            return $"0x{value:X}";
        }

        public static string ToHex(this uint value)
        {
            return $"0x{value:X}";
        }

        public static string ToHex(this short value)
        {
            return $"0x{value:X}";
        }

        public static string ToHex(this ushort value)
        {
            return $"0x{value:X}";
        }

        public static string ToHex(this long value)
        {
            return $"0x{value:X}";
        }

        public static string ToHex(this ulong value)
        {
            return $"0x{value:X}";
        }

		/// <summary>
		/// byte[] { 89, AB } => "89AB"
		/// </summary>
		/// <param name="byte"></param>
		/// <returns></returns>
        public static string ToHex(this byte[] @byte)
        {
            var hex = BitConverter.ToString(@byte);

            return hex.Replace("-", "");
        }

		/// <summary>
		/// byte[] { 97, 98 } => "ab"
		/// </summary>
		/// <param name="this"></param>
		/// <returns></returns>
		public static string ToStringAscii(this byte[] @this)
		{
			var value = new StringBuilder();
			foreach (var b in @this)
				value.Append((char)b);
			return value.ToString();
		}

		/// <summary>
		/// "89AB"  => byte[] { 89, AB }
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] FromHexStringToByteArray(this string hex)
		{
			byte[] bytes = new byte[hex.Length / 2];

			for (int i = 0; i < hex.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

			return bytes;
		}

	    /// <summary>
	    /// "ab"  => byte[] { 97, 98 }
	    /// </summary>
	    /// <returns></returns>
	    public static byte[] FromAsciiStringToByteArray(this string asciiText)
		{
			if (asciiText == null)
				return null;

			int len = asciiText.Length;

			byte[] bytes = new byte[len];

			for (int i = 0; i < asciiText.Length; i++)
				bytes[i] = Convert.ToByte(asciiText[i]);

			return bytes;
		}

		/// <summary>
		/// returns true, if both strings are equal
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool EqualsIgnoreCase(this string obj, string text)
		{
			if (obj == null && text == null)
				return true;

			return obj != null && obj.ToLower().Equals(text.ToLower());
		}

		/// <summary>
		/// var name = "HomeController".RemoveController(); // name = "Home"
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string RemoveController(this string obj)
		{
			return obj?.Replace("Controller",string.Empty);
		}

		public static string RemoveRight(this string obj, int countCharacters)
		{
			int len = obj.Length;
			if (len < countCharacters)
				countCharacters = obj.Length;
			if (countCharacters == 0)
				return obj;

			return obj.Substring(0, len - countCharacters);
		}

		public static string RemoveLeft(this string obj, int countCharacters)
		{
			var len = obj.Length;
			if (len < countCharacters)
				countCharacters = 0;
			if (countCharacters == 0)
				return obj;

			return obj.Substring(countCharacters);
		}

		public static string Left(this string @this, int count)
		{
			return @this.Length >= count ? @this.Substring(0, count) : @this;
		}

		public static string Right(this string obj, int countCharacters)
		{
			int len = obj.Length;
			if (len < countCharacters)
				countCharacters = obj.Length;

			if (countCharacters == 0)
				return "";

			return obj.Substring(len - countCharacters, countCharacters);
		}

		public static bool ContainsIgnoreCase(this string text, string substring)
		{
			return text.ToLower().Contains(substring.ToLower());
		}


		/// <summary>
		/// e.g.: source="1,2,3;4", pattern=",",";"
		/// ==> "1234"
		/// </summary>
		/// <param name="source"></param>
		/// <param name="ignoreCase"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static string Remove(this string source, bool ignoreCase, params string[] pattern)
		{
			string resLower = null;

			if (ignoreCase)
			{
				resLower = source.ToLower();
			}

			foreach (var pt in pattern)
			{
				if (ignoreCase)
				{
					string ptLower = pt.ToLower();

					int foundIdx;

					do
					{
						foundIdx = resLower.IndexOf(ptLower, StringComparison.Ordinal);
						if (foundIdx >= 0)
						{
							resLower = resLower.Substring(0, foundIdx) + resLower.Substring(foundIdx + ptLower.Length);
							source = source.Substring(0, foundIdx) + source.Substring(foundIdx + ptLower.Length);
						}
					}
					while (foundIdx >= 0);
				}
				else
				{
					source = source.Replace(pt, String.Empty);
				}
			}

			return source;
		}

        public static String LineBreakAt(this String text, Int32 interval, Char splitter)
        {
            if (text.Length <= interval)
                return text;

            var result = new StringBuilder();

            var splits = text.Split(splitter);

            var maxLength = splits.Max(s => s.Length);

            if (maxLength > interval)
                interval = maxLength;

            int count = 0;

            foreach (var item in splits)
            {
                if (count >= interval)
                {
                    result.Append("\r\n");
                    count = 0;
                }

                count += item.Length + 1;
                result.Append(item + splitter);
            }

            return result.ToString();
        }

		public static bool TryConvertDecToLong(this string source, out long dest)
		{
			try
			{
				dest = long.Parse(source, NumberStyles.Integer);
				return true;
			}
			catch
			{
				dest = 0;
				return false;
			}
		}

		public static bool TryConvertHexToLong(this string source, out long dest)
		{
			try
			{
				var val = source.ToLower();
				var startIdx = val.IndexOf('x');

				if (startIdx >= 0)
				{
					val = val.Substring(startIdx + 1);
				}

				dest = long.Parse(val, NumberStyles.HexNumber);
				return true;
			}
			catch
			{
				dest = 0;
				return false;
			}
		}

		public static string CleanFromExcelSymbols(this string text)
		{
			StringBuilder resText = new StringBuilder();
			resText.Append('\"');
			foreach (char c in text)
			{
				switch (c)
				{
					case '\x0D':
					case '\"':
						break;
					case ';':
						resText.Append(',');
						break;
					case '\n':
						resText.Append('\x0A');
						break;
					default:
						resText.Append(c);
						break;
				}

			}
			resText.Append('\"');
			return resText.ToString();
		}

	}
}
