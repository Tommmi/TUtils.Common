using System;
using System.Data.SqlClient;

namespace TUtils.Common.Extensions
{
	public static class ExceptionHelperSql
	{
		/// <summary>
		/// Liefert true zurück, wenn die geg. exception oder eine innere
		/// Exception auf eine verletzung eines Unique key Constraints hindeutet
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		// ReSharper disable once UnusedMember.Global
		public static bool HasSqlException_UniqueKeyViolation(Exception ex)
		{
			Exception e = ex.FindInnerException(typeof(SqlException));
			SqlException es = (SqlException)e;
			if (es?.Number == 2601)
				return true;
			return false;
		}

	}
}