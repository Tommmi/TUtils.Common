using System;

namespace TUtils.Common.Logging.Common
{
	public static class PredefinedLoggingValueIDs 
	{
		/// <summary>
		/// LoggingText
		/// </summary>
		public static readonly ILoggingValueKey LoggingText = new LoggingFilterableValue().Init(
			guid: new Guid("{3016AA40-DD5E-446F-BFC5-F44AD27B9647}"),
			isFilterable: false,
			guidName: () => LoggingText);
		/// <summary>
		/// ThreadId
		/// </summary>
		public static readonly ILoggingValueKey ThreadId = new LoggingFilterableValue().Init(
			guid: new Guid("{126D3CE7-90A4-4773-97A3-7FE93FA6730F}"),
			isFilterable: false,
			guidName: () => ThreadId);
		/// <summary>
		/// ThreadName
		/// </summary>
		public static readonly ILoggingValueKey ThreadName = new LoggingFilterableValue().Init(
			guid: new Guid("{5126F5E5-2296-4F4F-B409-E5CE5B0A9FA2}"),
			isFilterable: false,
			guidName: () => ThreadName);
		/// <summary>
		/// StackTrace
		/// </summary>
		public static readonly ILoggingValueKey StackTrace = new LoggingFilterableValue().Init(
			guid: new Guid("{46E4D787-8E52-4975-BA13-474C23C5F9B6}"),
			isFilterable: false,
			guidName: () => StackTrace);
		/// <summary>
		/// Severity
		/// </summary>
		public static readonly ILoggingValueKey Severity = new LoggingFilterableValue().Init(
			guid: new Guid("{2AE28CC2-ADD8-480E-A571-4BDD61C6F6D5}"),
			isFilterable: true,
			guidName: () => Severity);
		/// <summary>
		/// Namespace
		/// </summary>
		public static readonly ILoggingValueKey Namespace = new LoggingFilterableValue().Init(
			guid: new Guid("{AB705BB7-6A9B-4D23-8480-C8EEB9085D02}"),
			isFilterable: true,
			guidName: () => Namespace);
		/// <summary>
		/// LoggingInstanceID
		/// </summary>
		public static readonly ILoggingValueKey LoggingInstanceId = new LoggingFilterableValue().Init(
			guid: new Guid("{1588B00D-7592-4B92-AEC6-BA3BABE0DDC9}"),
			isFilterable: false,
			guidName: () => LoggingInstanceId);
		/// <summary>
		/// MethodName
		/// </summary>
		public static readonly ILoggingValueKey MethodName = new LoggingFilterableValue().Init(
			guid: new Guid("{A05FE0F5-0C6F-411C-A2B2-BABDFFD8D23A}"),
			isFilterable: false,
			guidName: () => MethodName);
		/// <summary>
		/// ExceptionObject
		/// </summary>
		public static readonly ILoggingValueKey ExceptionObject = new LoggingFilterableValue().Init(
			guid: new Guid("{5678D7EC-434D-42D3-94EB-359B8D293D02}"),
			isFilterable: false,
			guidName: () => ExceptionObject);
		/// <summary>
		/// Timestamp
		/// </summary>
		public static readonly ILoggingValueKey Timestamp = new LoggingFilterableValue().Init(
			guid: new Guid("{24D3AF33-E5BA-4E27-A49B-0126C2647448}"),
			isFilterable: false,
			guidName: () => Timestamp);
		/// <summary>
		/// Line
		/// </summary>
		public static readonly ILoggingValueKey Line = new LoggingFilterableValue().Init(
			guid: new Guid("{B9CFE246-CBB0-45C9-946D-EEEA35A43D76}"),
			isFilterable: false,
			guidName: () => Line);
		/// <summary>
		/// Filename
		/// </summary>
		public static readonly ILoggingValueKey Filename = new LoggingFilterableValue().Init(
			guid: new Guid("{65AD1980-C494-43D4-A6EC-04D61EA4E285}"),
			isFilterable: false,
			guidName: () => Filename);
	}
}