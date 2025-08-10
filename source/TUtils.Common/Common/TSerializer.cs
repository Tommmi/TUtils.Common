using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TUtils.Common.Common
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal static class TSerializer
    {
        private static readonly JsonSerializerOptions _options = CreateOptions();

        public static string Serialize(object value)
            => JsonSerializer.Serialize(value, _options);

        public static T? Deserialize<T>(string json)
            => JsonSerializer.Deserialize<T>(json, _options);

        public static object Deserialize(string json, Type returnType)
            => JsonSerializer.Deserialize(json, returnType, _options);

        public static bool TryDeserialize<T>(string json, out T? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, _options);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,

                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                ReferenceHandler = ReferenceHandler.IgnoreCycles,

                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,

                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            opts.Converters.Add(new JsonStringEnumConverter());

            opts.Converters.Add(new DateTimeRoundtripConverter());
            opts.Converters.Add(new DateTimeOffsetRoundtripConverter());

            return opts;
        }

        private sealed class DateTimeRoundtripConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => reader.GetDateTime(); 

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString("O"));
        }

        private sealed class DateTimeOffsetRoundtripConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => reader.GetDateTimeOffset();

            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString("O"));
        }
    }
}
