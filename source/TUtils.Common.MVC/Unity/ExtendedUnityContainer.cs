using System;
using Microsoft.Practices.Unity;
using TUtils.Common.DependencyInjection;

namespace TUtils.Common.MVC
{
	public class ExtendedUnityContainer : IDIContainer
	{
		private readonly IUnityContainer _container;

		public ExtendedUnityContainer(IUnityContainer container)
		{
			_container = container;
		}

		void IDIContainer.Register<TInterfaceType, TImplementationType>()
		{
			_container.RegisterType<TInterfaceType, TImplementationType>();
		}

		void IDIContainer.Register<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator)
		{
			_container.RegisterType<TInterfaceType, TImplementationType>(new InjectionFactory(c=>creator(this)));
		}

		void IDIContainer.RegisterSingleton<TInterfaceType, TImplementationType>()
		{
			_container.RegisterType<TInterfaceType, TImplementationType>(new ContainerControlledLifetimeManager());
		}

		void IDIContainer.RegisterSingleton<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator)
		{
			_container.RegisterType<TInterfaceType, TImplementationType>(
				new ContainerControlledLifetimeManager(),
				new InjectionFactory(c => creator(this)));
		}

		void IDIContainer.RegisterPerRequest<TInterfaceType, TImplementationType>()
		{
			_container.RegisterType<TInterfaceType, TImplementationType>(new PerRequestLifetimeManager());
		}

		void IDIContainer.RegisterPerRequest<TInterfaceType, TImplementationType>(Func<IDIContainer, TImplementationType> creator)
		{
			_container.RegisterType<TInterfaceType, TImplementationType>(
				new PerRequestLifetimeManager(),
				new InjectionFactory(c => creator(this)));
		}

		void IDIContainer.Register<TType>()
		{
			_container.RegisterType<TType>();
		}

		void IDIContainer.Register<TType>(Func<IDIContainer, TType> creator)
		{
			_container.RegisterType<TType>(new InjectionFactory(c => creator(this)));
		}

		void IDIContainer.RegisterSingleton<TType>()
		{
			_container.RegisterType<TType>(new ContainerControlledLifetimeManager());
		}

		void IDIContainer.RegisterSingleton<TType>(Func<IDIContainer, TType> creator)
		{
			_container.RegisterType<TType>(
				new ContainerControlledLifetimeManager(),
				new InjectionFactory(c => creator(this)));
		}

		void IDIContainer.RegisterPerRequest<TType>()
		{
			_container.RegisterType<TType>(new PerRequestLifetimeManager());
		}

		void IDIContainer.RegisterPerRequest<TType>(Func<IDIContainer, TType> creator)
		{
			_container.RegisterType<TType>(
				new PerRequestLifetimeManager(),
				new InjectionFactory(c => creator(this)));
		}

		TType IDIContainer.Get<TType>()
		{
			return _container.Resolve<TType>();
		}
	}
}