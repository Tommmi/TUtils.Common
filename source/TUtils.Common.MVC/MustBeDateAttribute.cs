using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common.MVC
{
	/// <summary>
	/// Applied to a string property of a view model class [MustBeDate] marks the string as being a date.
	/// [MustBeDate]  is a RegularExpressionAttribute, which checks the Format ""
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class MustBeDateAttribute : RegularExpressionAttribute
	{
		public MustBeDateAttribute() : base(@"(^(19|[2-9][0-9])\d\d[-](((0[1-9]|1[012])[-](0[1-9]|1[0-9]|2[0-8]))|((0[13578]|1[02])[-](29|30|31))|((0[4,6,9]|11)[-](29|30)))$)|(^(19|[2-9][0-9])(00|04|08|12|16|20|24|28|32|36|40|44|48|52|56|60|64|68|72|76|80|84|88|9|296)[-]02[-]29$)")
		{
		}
	}
}
