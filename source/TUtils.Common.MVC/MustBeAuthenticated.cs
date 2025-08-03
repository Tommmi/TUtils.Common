using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TUtils.Common.Extensions;

namespace TUtils.Common.MVC
{
	/// <summary>
	/// Applied to an action or a controller
	/// [MustBeAuthorized] is an authorization attribute
	/// which redirects unauthorized requests to AccountController.Login(string returnUrl).
	/// Assumes there is such an action.
	/// requirements:
	///		There must be a controller called "AccountController" with a HTTP-GET action called 
	///		"Login(string returnUrl)."
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class MustBeAuthorized : Attribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationFilterContext context)
		{
			// Check if user is authenticated
			if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
			{
				var request = context.HttpContext.Request;
				var returnUrl = $"{request.Path}{request.QueryString}";
				
				context.Result = new RedirectToActionResult(
					actionName: "Login",
					controllerName: "Account",
					routeValues: new { returnUrl = returnUrl });
			}
		}
	}
}
