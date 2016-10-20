namespace TUtils.Common.DependencyInjection
{
	/// <summary>
	/// implementation of ILazy, using the general interface of a dependency injection container
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Lazy<T> : ILazy<T>
	{
		private readonly IDIContainer _di;

		public Lazy(IDIContainer di)
		{
			_di = di;
		}

		T ILazy<T>.Value => _di.Get<T>();
	}
}