using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable MemberCanBePrivate.Global

namespace TUtils.Common.Extensions
{
	public static class ObjectExtension
	{
		private enum EnumUnderlyingType
		{
			Int,
			Uint,
			Ulong,
			Ushort,
			Byte,
			Long
		}

		/// <summary>
		/// converts:
		/// - string => enum (flags)
		/// - enum (flags) => number
		/// - number => enum (flags)
		/// - string => byte[] (ASCII) see also: StringExtension.ToByteArray()
		/// - byte[] => string (ASCII) see also: StringExtension.ToHex()
		/// - string -> bool
		/// - short,int,.. -> bool
		/// - IConvertible -> ...
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="sourceValue"></param>
		/// <param name="destinationValue"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static bool TryConvertToType<TType>(this object sourceValue, out TType destinationValue, TType defaultValue)
		{
			return TryConvertToType(sourceValue, out destinationValue, defaultValue, true);
		}

		/// <summary>
		/// converts:
		/// - string => enum (flags)
		/// - enum (flags) => number
		/// - number => enum (flags)
		/// - string => byte[] (ASCII) (see also StringExtension.FromASCIIStringToByteArray())
		/// - byte[] (ASCII) => string 
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="sourceValue"></param>
		/// <param name="destinationValue"></param>
		/// <param name="defaultValue"></param>
		/// <param name="throw"></param>
		/// <returns></returns>
		public static bool TryConvertToType<TType>(this object sourceValue, out TType destinationValue, TType defaultValue, bool @throw)
		{
			destinationValue = defaultValue;

			if (sourceValue == null)
				return false;

			var destType = typeof(TType);
			var sourceType = sourceValue.GetType();

			// if same type
			if (destType == sourceType)
			{
				// no conversion neccessary
				destinationValue = (TType)sourceValue;
				return true;
			}

			var isSourceValueAString = sourceType == typeof(string);

			// convert to enum ?
			if (destType.IsEnum)
			{
				ulong ulSourceValue = 0;
				var underlyingType = Enum.GetUnderlyingType(destType);
				EnumUnderlyingType underlyingTypeConst;

				if (underlyingType == typeof(uint))
					underlyingTypeConst = EnumUnderlyingType.Uint;
				else if (underlyingType == typeof(ulong))
					underlyingTypeConst = EnumUnderlyingType.Ulong;
				else if (underlyingType == typeof(int))
					underlyingTypeConst = EnumUnderlyingType.Int;
				else if (underlyingType == typeof(ushort))
					underlyingTypeConst = EnumUnderlyingType.Ushort;
				else if (underlyingType == typeof(byte))
					underlyingTypeConst = EnumUnderlyingType.Byte;
				else if (underlyingType == typeof(long))
					underlyingTypeConst = EnumUnderlyingType.Long;
				else if (@throw)
					throw new NotImplementedException("2736zdshst15 " + underlyingType.FullName);
				else
					return false;

				// enum is EnumFlags ?
				if (destType.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
				{
					// source is string ?
					if (isSourceValueAString)
					{
						var strSourcevalue = (string)sourceValue;
						var cleanedValue = StringExtension.Remove(strSourcevalue,true, " ", "\t");

						var enumParts = cleanedValue
							.Split('|')
							.Where(enumPart => !enumPart.IsNullOrEmpty())
							.ToArray();

						foreach (var enumPart in enumParts)
						{
							try
							{
								var oVal = Enum.Parse(typeof(TType), enumPart, true);
								ulSourceValue |= Convert.ToUInt64(oVal);
							}
							catch
							{
								return false;
							}
						}
					}
					else // if source value isn't a string
					{
						// Try convert source value into a number
						try
						{
							ulSourceValue = Convert.ToUInt64(sourceValue);
						}
						catch
						{
							// source value isn't number nor a string
							return false;
						}
					}
				} // enum is EnumFlags ?
				else // enum isn't enumFlags
				{
					// source is string ?
					if (isSourceValueAString)
					{
						var strSourcevalue = (string)sourceValue;

						try
						{
							var objDestValue = Enum.Parse(typeof(TType), strSourcevalue, true);

							if (objDestValue == null)
								return false;

							destinationValue = (TType)objDestValue;

							return true;
						}
						catch
						{
							return false;
						}
					}

					// Try convert source value into a number
					try
					{
						ulSourceValue = Convert.ToUInt64(sourceValue);
						var objDestValue = Enum.Parse(typeof(TType), ulSourceValue.ToString(), true);

						if (objDestValue == null)
							return false;

						if (!GetEnumValues<TType, TType>().Any(val => val.Equals((TType)objDestValue)))
							return false;
					}
					catch
					{
						// source value isn't number nor a string
						return false;
					}
				}

				switch (underlyingTypeConst)
				{
					case EnumUnderlyingType.Int:
						destinationValue = (TType)(object)Convert.ToInt32(ulSourceValue);
						break;

					case EnumUnderlyingType.Uint:
						destinationValue = (TType)(object)Convert.ToUInt32(ulSourceValue);
						break;

					case EnumUnderlyingType.Ulong:
						destinationValue = (TType)(object)ulSourceValue;
						break;

					case EnumUnderlyingType.Ushort:
						destinationValue = (TType)(object)Convert.ToUInt16(ulSourceValue);
						break;

					case EnumUnderlyingType.Byte:
						destinationValue = (TType)(object)Convert.ToByte(ulSourceValue);
						break;

					case EnumUnderlyingType.Long:
						destinationValue = (TType)(object)Convert.ToInt64(ulSourceValue);
						break;

					default:
						if (@throw)
							throw new NotImplementedException("76172e4hghhsfd3r " + underlyingTypeConst);

						return false;
				}

				return true;
			}

			// if convert from enum
			if (sourceType.IsEnum)
			{
				var ulSourceValue = Convert.ToUInt64(sourceValue);

				return TryConvertToType(ulSourceValue, out destinationValue, defaultValue, @throw);
			}

			// if convert to boolean
			if (destType == typeof(bool))
			{
				// if convert from string
				if (isSourceValueAString)
				{
					var strSourcevalue = ((string)sourceValue).ToLower();

					switch (strSourcevalue)
					{
						case "true":
							destinationValue = (TType)(object)true;
							break;

						case "false":
							destinationValue = (TType)(object)false;
							break;

						default:
							return false;
					}
				}
				// check, if source is a number with value 0 or 1
				else if (sourceValue is IConvertible)
				{
					try
					{
						var usVal = Convert.ToInt16(sourceValue);

						switch (usVal)
						{
							case 1:
								destinationValue = (TType)(object)true;
								break;

							case 0:
								destinationValue = (TType)(object)false;
								break;

							default:
								return false;
						}
					}
					catch
					{
						return false;
					}
				}

				return true;
			}

			var s = sourceValue as string;
			if (s != null && destType == typeof(byte[]))
			{
				var destVal = s.FromAsciiStringToByteArray();

				if (destVal == null)
					return false;

				destinationValue = (TType)(object)destVal;

				return true;
			}

			var bytes = sourceValue as byte[];
			if (bytes != null && destType == typeof(string))
			{
				var destVal = bytes.ToStringAscii();

				if (destVal == null)
					return false;

				destinationValue = (TType)(object)destVal;

				return true;
			}

			if (sourceValue is IConvertible)
			{
				try
				{
					destinationValue = (TType)Convert.ChangeType(sourceValue, destType, null);

					return true;
				}
				catch
				{
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns all values of the specified enum 
		/// </summary>
		/// <typeparam name="TValue">The enum to get the values of</typeparam>
		/// <typeparam name="TEnum"></typeparam>
		/// <returns>The values of the enum</returns>
		private static List<TValue> GetEnumValues<TEnum, TValue>()
		{

			List<TValue> retValues = new List<TValue>();

			if (typeof(TEnum).IsEnum)
			{

				FieldInfo[] fields = typeof(TEnum).GetFields();
				// ReSharper disable once LoopCanBeConvertedToQuery
				foreach (FieldInfo curField in fields)
				{
					if (curField.FieldType == typeof(TEnum))
					{
						retValues.Add(
							(TValue)Convert.ChangeType(
								(TEnum)Enum.Parse(
									typeof(TEnum),
									Convert.ToString(curField.Name),
									true),
								typeof(TValue),
								null));
					}
				}

			}

			return retValues;

		}


	}
}
