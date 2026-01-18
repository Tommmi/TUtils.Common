namespace TUtils.Common.DependencyInjection
{
	/// <summary>
	/// interface for a lazy loaded object
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ILazy<out T>
	{
		T Value { get; }
	}
}
