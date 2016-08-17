using System;
using System.Web.Mvc;
using System.Web.Routing;
using TUtils.Common.Extensions;

namespace TUtils.Common.MVC
{
	/// <summary>
	/// Applied to an action or a controller [MustBeAuthorized]  is an [AuthorizeAttribute]
	/// which redirects unauthorized requests to AccountController.Login(string returnUrl).
	/// Assumes there is such an action.
	/// requirements:
	///		There must be a controller called "AccountController" with a HTTP-GET action called 
	///		"Login(string returnUrl)."
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class MustBeAuthorized : AuthorizeAttribute
	{
		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var returnUrl = filterContext.HttpContext?.Request.Url;
			
			var routeValues = new RouteValueDictionary
			{
				["controller"] = "Account",
				["action"] = "Login",
				["returnUrl"] = returnUrl?.PathAndQuery??string.Empty
			};
			filterContext.Result = new RedirectToRouteResult(routeValues);
		}
	}
}