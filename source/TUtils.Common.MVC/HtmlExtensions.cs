using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web;
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

			string editorFor = html.EditorFor(expression, new { htmlAttributes = new { @class = "form-control" } }).ToString();
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

		///  <summary>
		///  renders a modal confirmation dialog with a link as Yes-Button.
		///  To show the dialog you may either use a link
		///  <code><![CDATA[
		/// 		<a href="#myDialogId" data-toggle="modal"></a>
		///  ]]></code>
		///  or a button
		///  <code><![CDATA[
		/// 		<button data-target="#myDialogId" data-toggle="modal">Launch Demo Modal</button>
		///  ]]></code>
		///  or java script
		///  <code><![CDATA[
		/// 		$("#myDialogId").modal('show');
		///  ]]></code>
		///  
		///  </summary>
		///  <typeparam name="TModel"></typeparam>
		///  <param name="html">MVC HtmlHelper</param>
		///  <param name="modalDlgId">
		///  please use a page unique string
		///  </param>
		///  <param name="title">
		///  title of the dialog
		///  </param>
		///  <param name="dlgText">
		///  question of the confirmation dialog
		///  </param>
		/// <param name="yes">
		/// the text of the Yes button in the conformation dialog
		/// </param>
		/// <param name="no">
		///  localized name of the no button
		///  </param>
		/// <param name="hrefOnOk">
		/// the Uri of the yes - link
		/// </param>
		/// <returns></returns>
		public static MvcHtmlString RenderConfirmationModalDlg<TModel>(
			this HtmlHelper<TModel> html,
			string modalDlgId,
			string title,
			string dlgText,
			string yes,
			string no,
			string hrefOnOk)
		{
			return RenderConfirmationModalDlgInternal<TModel>(
				html,
				modalDlgId,
				title,
				dlgText,
				no,
				okButtonHtml: $@"<a href=""{hrefOnOk}"" class=""btn btn-primary"">{yes}</a>");
		}

		/// <summary>
		/// String extension: encodes a string in such a way that you may use it as value in an Uri.
		/// Use HtmlExtension.UrlDecode() to decode the value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string UrlEncoded(this string value)
		{
			return Uri.EscapeDataString(value);
		}

		/// <summary>
		/// Decodes a value, which was encoded by HtmlExtension.UrlEncoded()
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string UrlDecode(this string value)
		{
			return Uri.UnescapeDataString(value);
		}

		/// <summary>
		/// Byte array extension: encodes a byte array in such a way that you may use it as value in an Uri.
		/// Use HtmlExtension.ToBytesFromUrlEncodedString() to decode the value.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToUrlEncodedString(this byte[] bytes)
		{
			return HttpUtility.UrlEncode(bytes);
		}

		/// <summary>
		/// String extension:  Decodes a byte array, which was encoded by HtmlExtension.ToUrlEncodedString()
		/// </summary>
		/// <param name="urlEncodedBytes"></param>
		/// <returns></returns>
		public static byte[] ToBytesFromUrlEncodedString(this string urlEncodedBytes)
		{
			return HttpUtility.UrlDecodeToBytes(urlEncodedBytes);
		}

		/// <summary>
		/// renders a link button which rises a modal confirmation dialog.
		/// The Yes button of the confirmation dialog is a link.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="html">MVC HtmlHelper</param>
		/// <param name="modalDlgId">
		/// please use a page unique string
		/// </param>
		/// <param name="htmlAttributes">
		/// additional attributes to the link tag, which rises the dialog.
		/// </param>
		/// <param name="dlgText">
		/// question of the confirmation dialog
		/// </param>
		/// <param name="yes">
		/// localized name of the yes button
		/// </param>
		/// <param name="no">
		/// localized name of the no button
		/// </param>
		/// <param name="linkButtonText">
		/// The text of the link button
		/// </param>
		/// <param name="hrefOnOk">
		/// the href of the yes link.
		/// </param>
		/// <returns></returns>
		public static MvcHtmlString RenderConfirmationModalDlg<TModel>(
			this HtmlHelper<TModel> html,
			string modalDlgId,
			string linkButtonText,
			object htmlAttributes,
			string dlgText,
			string yes,
			string no,
			string hrefOnOk)
		{
			return RenderConfirmationModalDlgEx<TModel>(
				html: html,
				modalDlgId: modalDlgId,
				linkButtonText: linkButtonText,
				htmlAttributes: htmlAttributes,
				dlgText: dlgText,
				no: no,
				buttonOnOkHtml: $"<a href={hrefOnOk} class=\"btn btn-primary\">{yes}</a>");
		}

		/// <summary>
		/// renders a link button which rises a modal confirmation dialog.
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="html">MVC HtmlHelper</param>
		/// <param name="modalDlgId">
		/// please use a page unique string
		/// </param>
		/// <param name="htmlAttributes"></param>
		/// <param name="dlgText">
		/// question of the confirmation dialog
		/// </param>
		/// <param name="no">
		/// localized name of the no button
		/// </param>
		/// <param name="linkButtonText">
		/// The text of the link button
		/// </param>
		/// <param name="buttonOnOkHtml">
		/// the html of the ok button
		/// </param>
		/// <returns></returns>
		public static MvcHtmlString RenderConfirmationModalDlgEx<TModel>(
			this HtmlHelper<TModel> html,
			string modalDlgId,
			string linkButtonText,
			object htmlAttributes,
			string dlgText,
			string no,
			string buttonOnOkHtml)
		{
			return
				new MvcHtmlString(
					$@"
				<a href=""#{modalDlgId}"" {GetHtmlAttributes(htmlAttributes)} data-toggle=""modal"">{linkButtonText}</a>
				{RenderConfirmationModalDlgInternal<TModel>(
							html,
							modalDlgId,
							linkButtonText,
							dlgText,
							no,
							okButtonHtml: buttonOnOkHtml)}");
		}



		///  <summary>
		///  renders a modal confirmation dialog.
		///  To show the dialog you may either use a link
		///  <code><![CDATA[
		/// 		<a href="#myDialogId" data-toggle="modal"></a>
		///  ]]></code>
		///  or a button
		///  <code><![CDATA[
		/// 		<button data-target="#myDialogId" data-toggle="modal">Launch Demo Modal</button>
		///  ]]></code>
		///  or java script
		///  <code><![CDATA[
		/// 		$("#myDialogId").modal('show');
		///  ]]></code>
		///  
		///  </summary>
		///  <typeparam name="TModel"></typeparam>
		///  <param name="html">MVC HtmlHelper</param>
		///  <param name="modalDlgId">
		///  please use a page unique string
		///  </param>
		///  <param name="title">
		///  title of the dialog
		///  </param>
		///  <param name="dlgText">
		///  question of the confirmation dialog
		///  </param>
		///  <param name="no">
		///  localized name of the no button
		///  </param>
		/// <param name="okButtonHtml">
		/// the html of the ok button
		/// </param>
		/// <returns></returns>
		private static MvcHtmlString RenderConfirmationModalDlgInternal<TModel>(
			this HtmlHelper<TModel> html,
			string modalDlgId,
			string title,
			string dlgText,
			string no,
			string okButtonHtml)
		{
			return new MvcHtmlString($@"
                        <div id=""{modalDlgId}"" class=""modal fade"">
							<div class=""modal-dialog"">
                                <div class=""modal-content"" >
                                    <div class=""modal-header"" >
                                        <button type=""button"" class=""close"" data-dismiss=""modal"" >&times;</button>
                                        <h4 class=""modal-title"">{title}</h4>
                                    </div>
                                    <div class=""modal-body"" >
                                        <p>{dlgText}</p>
                                    </div>
                                    <div class=""modal-footer"" >
                                        <button type=""button"" class=""btn btn-default"" data-dismiss=""modal"">{no}</button>
                                        {okButtonHtml}
                                    </div>
                                </div>
                            </div>
                        </div>
						");
		}

		/// <summary>
		/// renders a link, which opens email tool presenting a pre-filled email.
		/// <example><code><![CDATA[
		/// Html.RenderMailLink(
		///     to: Model.InvitationMailContent.ReceipientMailAddresses,
		///     cc: null,
		///     bcc: null,
		///     subject: Model.InvitationMailContent.Subject,
		///     textBody: Model.InvitationMailContent.Body,
		///     linkText: Resource1.Start,
		///     htmlAttributes: new
		///     {
		///         @class = "btn btn-success",
		///         target = "mailto"
		///     });
		/// <iframe name="mailto" src="about:blank" style="display:none;"></iframe>
		/// ]]></code></example>
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <param name="html"></param>
		/// <param name="to">email addresses</param>
		/// <param name="cc">email addresses</param>
		/// <param name="bcc">email addresses</param>
		/// <param name="subject">subject of pre-filled email</param>
		/// <param name="textBody">body of pre-filled email. The text should contain plaint text. It will be encoded by this method.</param>
		/// <param name="linkText">the textof the link (content of link)</param>
		/// <param name="htmlAttributes">additonal attributes of the link. e.g.: new {myAttr="yes"}</param>
		/// <returns></returns>
		public static MvcHtmlString RenderMailLink<TModel>(
			this HtmlHelper<TModel> html,
			IEnumerable<string> to,
			IEnumerable<string> cc,
			IEnumerable<string> bcc,
			string subject,
			string textBody,
			string linkText,
			object htmlAttributes)
		{
			var ccText = cc.IsNotNullOrEmpty() ? "&cc=" + Combine<TModel>(cc) : string.Empty;
			var bccText = bcc.IsNotNullOrEmpty() ? "&bcc=" + Combine<TModel>(bcc) : string.Empty;
			var htmlAttributesString = GetHtmlAttributes(htmlAttributes);
			return new MvcHtmlString($"<a {htmlAttributesString} href=\"mailto:{Combine<TModel>(to)}?subject={subject.UrlEncoded()}{ccText}{bccText}&body={textBody.UrlEncoded()}\">{linkText}</a>");
		}

		private static string GetHtmlAttributes(object htmlAttributes)
		{
			string htmlAttributesString = string.Empty;
			if (htmlAttributes != null)
			{
				foreach (var property in htmlAttributes.GetType().GetProperties())
				{
					string val = property.GetValue(htmlAttributes).ToString();
					htmlAttributesString += $"{property.Name.Remove(ignoreCase: false, pattern: "@")}=\"{val}\" ";
				}
			}
			return htmlAttributesString;
		}

		private static string Combine<TModel>(IEnumerable<string> addresses)
		{
			var addressesAsText = new StringBuilder();
			foreach (var address in addresses)
			{
				if (addressesAsText.Length != 0)
					addressesAsText.Append(";");
				addressesAsText.Append(address);
			}
			return addressesAsText.ToString();
		}
    }
}
