using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUtils.Common.Extensions;

namespace TUtils.Common.MVC
{
	public static class UriExtension
	{
		/// <summary>
		/// Gets the value of the given query parameter
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="name"></param>
		/// <returns>null if not found</returns>
		public static string GetQueryParameter(this Uri uri, string name)
		{
			return uri.QueryParamters().Where(p => p.Key == name).Select(p => p.Value).FirstOrDefault();
		}

		/// <summary>
		/// gets all query parameters
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, string>> QueryParamters(this Uri uri)
		{
			var query = uri.Query.Replace("?", string.Empty);
			var queryParts = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			var result = new List<KeyValuePair<string, string>>();
			foreach (var queryPart in queryParts)
			{
				var parts = queryPart.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 1)
				{
					result.Add(new KeyValuePair<string, string>(parts[0],string.Empty));		
				}
				else if (parts.Length == 2)
				{
					result.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
				}
			}

			return result;
		}

		/// <summary>
		/// removes the query part of the Uri
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static Uri RemoveQuery(this Uri uri)
		{
			return new Uri(uri.AbsoluteUri.CutRight(uri.Query.Length));
		}

		/// <summary>
		/// Adds all passed query parameters to the Uri
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="queryParams"></param>
		/// <returns></returns>
		public static Uri AddQueryParameters(this Uri uri, params KeyValuePair<string, string>[] queryParams)
		{
			var url = uri.AbsoluteUri;
			if (queryParams.Any() && uri.Query.Length == 0)
				url += "?";
			foreach (var keyValuePair in queryParams)
			{
				if (!url.EndsWith("?"))
					url += "&";
				url += $"{keyValuePair.Key}={keyValuePair.Value}";
			}
			return new Uri(url);
		}

		/// <summary>
		/// removes the given query parameter from Uri
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Uri RemoveQueryParameter(this Uri uri, string name)
		{
			var queryParameter = uri.QueryParamters().ToList();
			queryParameter.RemoveWhere(x => x.Key == name);
			return uri.RemoveQuery().AddQueryParameters(queryParameter.ToArray());
		}
	}
}
