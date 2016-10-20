using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
