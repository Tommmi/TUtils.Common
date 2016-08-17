using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common.DependencyInjection
{
	// ReSharper disable once InconsistentNaming
	// ReSharper disable once UnusedMember.Global
	public interface IDIContainer
	{
		void Register<TType>();
		void Register<TType>(Func<IDIContainer, TType> creator);
		void Register<TInterfaceType, TImplementationType>() where TImplementationType : TInterfaceType;
		void Register<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator) where TImplementationType : TInterfaceType;

		void RegisterSingleton<TType>();
		void RegisterSingleton<TType>(Func<IDIContainer, TType> creator);
		void RegisterSingleton<TInterfaceType, TImplementationType>() where TImplementationType : TInterfaceType;
		void RegisterSingleton<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator) where TImplementationType : TInterfaceType;

		void RegisterPerRequest<TType>();
		void RegisterPerRequest<TType>(Func<IDIContainer, TType> creator);
		void RegisterPerRequest<TInterfaceType, TImplementationType>() where TImplementationType : TInterfaceType;
		void RegisterPerRequest<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator) where TImplementationType : TInterfaceType;

		TType Get<TType>();
	}


}
