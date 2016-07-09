using System;

// ReSharper disable UnusedMember.Global

namespace TUtils.Common.Logging.Common
{
	public static class MPredefinedLoggingValueIDs
	{
		/// <summary>
		/// ServiceName
		/// </summary>
		[LoggingFilterableValue]
		public static readonly ILoggingValueKey ServiceName = new LoggingFilterableValue().Init(
			guid: new Guid("{CC21DC6D-6D1B-493C-BFE4-D6154A797A1A}"),
			isFilterable: false,
			guidName: () => ServiceName);
		/// <summary>
		/// OriginalMessageID
		/// </summary>
		[LoggingFilterableValue]
		public static readonly ILoggingValueKey OriginalMessageId = new LoggingFilterableValue().Init(
			guid: new Guid("{73E7162B-B117-43E7-AD1B-CF178EDFDEEE}"),
			isFilterable: false,
			guidName: () => OriginalMessageId);
		/// <summary>
		/// CurrentMessageID
		/// </summary>
		[LoggingFilterableValue]
		public static readonly ILoggingValueKey CurrentMessageId = new LoggingFilterableValue().Init(
			guid: new Guid("{AE752CD0-DF0C-4790-A0B2-4660DDDEF8CB}"),
			isFilterable: false,
			guidName: () => CurrentMessageId);
		/// <summary>
		/// CurrentMessageContent
		/// </summary>
		public static readonly ILoggingValueKey CurrentMessageContent = new LoggingFilterableValue().Init(
			guid: new Guid("{F0D8BB67-13D0-4109-874C-597373EEEA0B}"),
			isFilterable: false,
			guidName: () => CurrentMessageContent);
		/// <summary>
		/// OriginalMessageContent
		/// </summary>
		public static readonly ILoggingValueKey OriginalMessageContent = new LoggingFilterableValue().Init(
			guid: new Guid("{B9628A52-1761-4860-9F59-F7A30AFAADE8}"),
			isFilterable: false,
			guidName: () => OriginalMessageContent);
		/// <summary>
		/// SendingMessageID
		/// </summary>
		public static readonly ILoggingValueKey SendingMessageId = new LoggingFilterableValue().Init(
			guid: new Guid("{EE18CAD6-511F-4ECA-AC44-F79830678AF5}"),
			isFilterable: false,
			guidName: () => SendingMessageId);
		/// <summary>
		/// SendingMessageContent
		/// </summary>
		public static readonly ILoggingValueKey SendingMessageContent = new LoggingFilterableValue().Init(
			guid: new Guid("{EB52A609-9E58-4F96-ABCE-56A5161B54E3}"),
			isFilterable: false,
			guidName: () => SendingMessageContent);
	}
}