using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common
{
	public interface ISystemTimeProvider
	{
		DateTime LocalTime { get; }
		DateTime UtcTime { get; }
	}
}
