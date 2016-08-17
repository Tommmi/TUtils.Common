using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using TUtils.Common.Extensions;

namespace TUtils.Common.MVC
{
	public class HtmExtensionConfiguration
	{
		/// <summary>
		/// width of left label column 
		/// default: col-sm-2
		/// </summary>
		public string LabelBootstrapClasses { get; set; } = "col-sm-2";

		/// <summary>
		/// width of right input column 
		/// default: col-sm-10
		/// </summary>
		public string InputBootstrapClasses { get; set; } = "col-sm-10";
	}

	public static class HtmlExtensions
    {
		/// <summary>
		/// CSS für asterix (required):
		/// <code><![CDATA[
		/// label.required {
		///     font-weight: bold;
		/// }
		/// label.required:after {
		///     color: @asterixColorNormal;
		///     content: ' *';
		///     font-size: x-large;
		///     right:5px;
		///     top:0;
		///     display:inline;
		///     position:absolute;
		/// }
		/// ]]></code>
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="html"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static MvcHtmlString RenderFormGroup<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			return RenderFormGroup(html, expression, null);
		}

		/// <summary>
		/// Creates 
		/// a form group:
		/// <code><![CDATA[
		/// <div class=""form-group"">
		/// 	{labelFor}
		/// 	<div class=""{configuration.InputBootstrapClasses}"">
		/// 		{editorFor}
		/// 		{validator}
		/// 	</div>
		/// </div>
		/// ]]></code>
		/// </summary>
		/// <remarks>
		/// CSS für asterix (required):
		/// <code><![CDATA[
		/// label.required {
		///     font-weight: bold;
		/// }
		/// label.required:after {
		///     color: @asterixColorNormal;
		///     content: ' *';
		///     font:xx-large;
		///     right:5px;
		///     display:inline;
		///     position:absolute;
		/// }
		/// ]]></code>
		/// </remarks>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="html"></param>
		/// <param name="expression">
		/// what model property should be edited ?
		/// </param>
		/// <param name="configuration">
		/// optional configuration
		/// </param>
		/// <returns></returns>
		public static MvcHtmlString RenderFormGroup<TModel, TValue>(
			this HtmlHelper<TModel> html, 
			Expression<Func<TModel, TValue>> expression,
			HtmExtensionConfiguration configuration)
		{
			if ( configuration == null )
				configuration = new HtmExtensionConfiguration();

			var editorFor = html.EditorFor(expression, new { htmlAttributes = new { @class = "form-control" } });
			var validator = html.ValidationMessageFor(expression, "", new { @class = "text-danger" });
			var isRequired = html.ViewData.ModelMetadata.IsRequired;
			var requiredClass = isRequired ? " required" : "";
			var labelFor = html.LabelFor(expression, htmlAttributes: new { @class = $"control-label {configuration.LabelBootstrapClasses}{requiredClass}" });
			return new MvcHtmlString($@"
				<div class=""form-group"">
					{labelFor}
					<div class=""{configuration.InputBootstrapClasses}"">
						{editorFor}
						{validator}
					</div>
				</div>
				");
		}
	}
}
