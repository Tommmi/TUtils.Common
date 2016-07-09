using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TUtils.Common.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Reflection
{
	public static class AssemblyLib
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootAssemblies">
		/// if null, the method assumes Assembly.GetEntryAssembly()
		/// </param>
		/// <returns></returns>
		public static IEnumerable<Assembly> GetAllAssemblies(List<Assembly> rootAssemblies)
		{
			var assemblies = rootAssemblies ?? new List<Assembly>{Assembly.GetEntryAssembly()};

			if (assemblies.IsNullOrEmpty())
				return new Assembly[0];

			return assemblies
				.Aggregate(
					new List<AssemblyName>(),
					(accumulatedList, currentAssembly) =>
					{
						accumulatedList.Add(currentAssembly.GetName());
						accumulatedList.AddRange(currentAssembly.GetReferencedAssemblies());
						return accumulatedList;
					})
				.Distinct()
				.Select(assemblyName => Assembly.Load(assemblyName.ToString()))
				.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rootAssemblies">
		/// if null, the method assumes Assembly.GetEntryAssembly()
		/// </param>
		/// <param name="onType"></param>
		public static void DoForAllTypesOfAllAssemblies(List<Assembly> rootAssemblies, Action<Type> onType)
		{
			foreach (var assembly in GetAllAssemblies(rootAssemblies))
			{
				foreach (var type in assembly.GetTypes())
				{
					onType(type);
				}
			}
		}

		public static void DoForAllTypesOfAllAssemblies(List<Assembly> rootAssemblies, Type attributeType, Action<Type> onType)
		{
			foreach (var assembly in GetAllAssemblies(rootAssemblies))
			{
				foreach (var type in assembly.GetTypes().Where(t => t.GetCustomAttributes(attributeType, true).Any()))
				{
					onType(type);
				}
			}
		}
	}
}
