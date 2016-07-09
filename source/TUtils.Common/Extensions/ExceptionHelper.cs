using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TUtils.Common.Extensions
{
	[AttributeUsage(AttributeTargets.Property)]
	public class LoggingAdditionalInfoAttribute : Attribute
	{
	}

	/// <summary>
	/// Hilfsklasse für Exceptionhandling
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// Erzeugt aus einer Exception einen lesbaren Fehlerlogeintrag.<br />
		/// Properties von Exceptionklassen, die mit einem 
		/// <see cref="LoggingAdditionalInfoAttribute"/> Attribut
		/// versehen sind, werden automatisch mit geloggt.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string DumpException(this Exception e)
		{
			StringBuilder buf = new StringBuilder();
			buf.AppendFormat("ERROR: \n");
			DumpException(e, buf);
			return buf.ToString();
		}

		private static void DumpException(this Exception e, StringBuilder buf)
		{
			if ( e == null )
				return;

			buf.AppendFormat("Exception {0} was thrown\n", e.GetType().FullName);
			buf.AppendFormat("Message: {0}\n", e.Message);
			buf.AppendFormat(
				"Stack: {0}\n",
				e.StackTrace);

			PropertyInfo[] properties = e.GetType().GetProperties();
			foreach (PropertyInfo pi in properties)
			{
				if (pi.GetCustomAttribute<LoggingAdditionalInfoAttribute>() != null)
				{
					try
					{
						object val = pi.GetValue(e, null);
						string strVal = val?.ToString() ?? "null";
						buf.AppendFormat("{0}={1}\n", pi.Name, strVal);
						break;
					}
					catch
					{
						buf.Append("..\n");
					}
				}
			}

			buf.AppendFormat("...............................................\n");

			DumpException(e.InnerException, buf);
			AggregateException aggregateException = e as AggregateException;
			if (aggregateException?.InnerExceptions != null)
			{
				foreach (var innerException in aggregateException.InnerExceptions)
					DumpException(innerException, buf);
			}
		}



		/// <summary>
		/// Prüft für die gegebene Exception und alle inneren Exceptions,
		/// ob sie dem gegebenen Typ entsprechen und gibt die erste gefundene
		/// Entsprechung zurück, sonst null.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="innerExceptionType"></param>
		/// <returns></returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public static Exception FindInnerException(this Exception ex, Type innerExceptionType)
		{
			if (ex.GetType() == innerExceptionType)
			{
				return ex;
			}
			else
			{
				if (ex.InnerException != null)
					return FindInnerException(ex.InnerException, innerExceptionType);
			}
			return null;
		}
	}
}
