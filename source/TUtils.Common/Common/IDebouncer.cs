using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common
{
	/// <summary>
	/// Some input communication channels may repeat a signal in very short time.
	/// This is often the case with mechanic electronic switches for example.
	/// Debouncing means that the handler of a signal handles a signal and ignores following signals for 
	/// a short time afterwards.
	/// </summary>
	public interface IDebouncer
	{
		/// <summary>
		/// Some input communication channels may repeat a signal in very short time.
		/// This is often the case with mechanic electronic switches for example.
		/// Debouncing means that the handler of a signal handles a signal and ignores following signals for 
		/// a short time afterwards.
		/// </summary>
		/// <param name="inputChannel">
		/// May be any object that is comparable with itself by using the Equals method
		/// The debouncer ignores signals comming shortly successively on the same channel
		/// </param>
		/// <returns></returns>
		bool ShouldIgnore(object inputChannel);
	}
}
