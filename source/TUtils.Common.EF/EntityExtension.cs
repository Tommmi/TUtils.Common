using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TUtils.Common.Extensions;

namespace TUtils.Common.EF
{
	public static class EntityExtension
	{
		private static readonly string _simpleTypes;

		static EntityExtension()
		{
			_simpleTypes =
				typeof(string).Name + " " +
				typeof(int).Name + " " +
				typeof(decimal).Name + " " +
				typeof(float).Name + " " +
				typeof(DateTime).Name + " " +
				typeof(bool).Name + " " +
				typeof(byte).Name + " " +
				typeof(short).Name + " " +
				typeof(long).Name + " " +
				typeof(double).Name + " " +

				typeof(int?).Name + " " +
				typeof(decimal?).Name + " " +
				typeof(float?).Name + " " +
				typeof(DateTime?).Name + " " +
				typeof(bool?).Name + " " +
				typeof(byte?).Name + " " +
				typeof(short?).Name + " " +
				typeof(long?).Name + " " +
				typeof(double?).Name;
		}

		/// <summary>
		/// Copies all simple properties from srcEntity to destEntity.
		/// Simple properties are properties of type int, int?, string and so on.
		/// They may not have attribute [NotMapped] or [Key].
		/// The primary key won't be copied.
		/// If there was no change, the destination entity won't be touched.
		/// </summary>
		/// <typeparam name="TEntityType"></typeparam>
		/// <param name="sourceEntity"></param>
		/// <param name="destEntity"></param>
		/// <param name="ignoredProperties">
		/// properties which should be ignored in any case
		/// </param>
		/// <returns>
		/// true, if there were changes.
		/// </returns>
		public static bool ApplyChanges<TEntityType>(TEntityType sourceEntity, TEntityType destEntity, params Expression<Func<TEntityType, object>>[] ignoredProperties)
		{
			var entityType = typeof(TEntityType);
			var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var simpleProperties = GetSimpleProperties<TEntityType>(properties, ignoredProperties);
			var changedValues = new List<Tuple<PropertyInfo, object>>();

			foreach (var property in simpleProperties)
			{
				var destVal = property.GetValue(destEntity);
				var srcVal = property.GetValue(sourceEntity);
				if ((destVal != null && !destVal.Equals(srcVal))
					|| (destVal == null && srcVal != null))
				{
					changedValues.Add(new Tuple<PropertyInfo, object>(property, srcVal));
				}
			}

			if (!changedValues.Any())
				return false;

			foreach (var changedValue in changedValues)
				changedValue.Item1.SetValue(destEntity, changedValue.Item2);

			return true;
		}

		private static List<PropertyInfo> GetSimpleProperties<TEntityType>(PropertyInfo[] properties, Expression<Func<TEntityType, object>>[] ignoredProperties)
		{
			var type = typeof(TEntityType);
			var keyProperties = GetPrimaryKeys(properties, type);
			var ignoredPropertiesInfos =
				ignoredProperties.Select(ig => type.GetProperty(StaticReflection.GetMemberName(ig))).ToList();
			return properties
				.Except(keyProperties)
				.Except(ignoredPropertiesInfos)
				.Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null
							&& IsSimpleType(p)).ToList();
		}

		private static bool IsSimpleType(PropertyInfo p)
		{
			return _simpleTypes.Contains(p.PropertyType.Name) || p.PropertyType.IsEnum;
		}

		private static IEnumerable<PropertyInfo> GetPrimaryKeys(PropertyInfo[] properties, Type entityType)
		{
			var keys = new List<PropertyInfo>();
			foreach (var p in properties)
			{
				if (p.Name == "Id" || p.Name == entityType.Name + "Id" || p.GetCustomAttribute<KeyAttribute>() != null)
					keys.Add(p);
			}
			return keys;
		}
	}
}
