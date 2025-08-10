using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUtils.Common.Extensions;

namespace TUtils.Common.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace YourProject.Tests
    {
        /// <summary>
        /// Test suite for TUtils serialization extension methods.
        /// Tests JSON serialization and deserialization functionality with various edge cases,
        /// custom configurations, and error handling scenarios.
        /// </summary>
        [TestClass]
        public class SerializerTest
        {
            // ==== Helper types for tests ====

            /// <summary>
            /// Test enumeration for serialization behavior verification.
            /// </summary>
            private enum Status
            {
                Unknown = 0,
                Active = 1,
                Paused = 2
            }

            /// <summary>
            /// Sample DTO class with various serialization attributes and edge cases.
            /// Used to test null handling, property renaming, and attribute behavior.
            /// </summary>
            private class SampleDto
            {
                public string Name { get; set; } = "Alice";

                public int? NullableCount { get; set; } // stays null -> should be omitted

                public Status State { get; set; } = Status.Active;

                [JsonIgnore]
                public string Secret { get; set; } = "TOP-SECRET";

                [JsonPropertyName("renamedValue")]
                public string OriginalValueName { get; set; } = "Visible";

                public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 10, 12, 34, 56, DateTimeKind.Unspecified);
            }

            /// <summary>
            /// Test class for cyclic reference scenarios.
            /// Used to verify that circular references are handled correctly during serialization.
            /// </summary>
            private class Node
            {
                public string Id { get; set; } = Guid.NewGuid().ToString();
                public Node? Next { get; set; }
                public Node? Prev { get; set; }
            }

            // ==== Tests ====

            /// <summary>
            /// Tests core serialization features including null omission, JsonIgnore attribute,
            /// enum string conversion, and JsonPropertyName attribute handling.
            /// </summary>
            [TestMethod]
            public void Serialize_Should_OmitNulls_RespectIgnore_UseEnumStrings_AndPropertyName()
            {
                var dto = new SampleDto
                {
                    Name = "Bob",
                    NullableCount = null,      // should be omitted
                    State = Status.Paused,     // should appear as "Paused"
                    Secret = "SHOULD_NOT_APPEAR",
                    OriginalValueName = "X"
                };

                var json = dto.SerializeByTUtils();

                // human-readable (indented)
                StringAssert.Contains(json, "\n");

                // Null should be missing
                Assert.IsFalse(json.Contains("NullableCount", StringComparison.Ordinal));

                // [JsonIgnore] respected
                Assert.IsFalse(json.Contains("Secret", StringComparison.Ordinal));

                // [JsonPropertyName] respected
                StringAssert.Contains(json, "\"renamedValue\": \"X\"");

                // Enum as string
                StringAssert.Contains(json, "\"State\": \"Paused\"");
            }

            /// <summary>
            /// Tests DateTime serialization with proper ISO 8601 formatting while preserving DateTimeKind.
            /// Verifies that Unspecified, UTC, and Local DateTimes are serialized with correct format strings.
            /// </summary>
            [TestMethod]
            public void Serialize_Should_Write_DateTime_With_RoundTrip_O_Format_Without_Conversion()
            {
                var unspecified = new DateTime(2025, 8, 10, 12, 34, 56, DateTimeKind.Unspecified);
                var utc = new DateTime(2025, 8, 10, 12, 34, 56, DateTimeKind.Utc);
                var local = new DateTime(2025, 8, 10, 12, 34, 56, DateTimeKind.Local);

                var obj = new
                {
                    Unspec = unspecified,
                    Utc = utc,
                    Local = local
                };

                var json = obj.SerializeByTUtils();

                // Unspecified: no 'Z' and typically no offset suffix
                // (we check here that no 'Z' appears directly before the closing quote)
                StringAssert.Contains(json, "\"Unspec\": \"" + unspecified.ToString("O") + "\"");

                // UTC: ends with 'Z'
                StringAssert.Contains(json, "\"Utc\": \"" + utc.ToString("O") + "\"");

                // Local: contains offset (+HH:MM or -HH:MM). We validate via regex.
                var localIso = local.ToString("O"); // contains local zone offset
                StringAssert.Contains(json, "\"Local\": \"" + localIso + "\"");

                // Additional check: Regex for offset pattern
                var rx = new Regex(@"""Local"":\s*""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}([+-]\d{2}:\d{2})""");
                Assert.IsTrue(rx.IsMatch(json), "Local DateTime should contain an offset.");
            }

            [TestMethod]
            public void Serialize_DateTimeOffset_Should_Preserve_Exact_Offset()
            {
                var dto = new
                {
                    When = new DateTimeOffset(2025, 8, 10, 12, 34, 56, TimeSpan.FromHours(+2))
                };

                var json = dto.SerializeByTUtils();

                // exact offset +02:00 must be present
                StringAssert.Contains(json, "\"When\": \"" + dto.When.ToString("O") + "\"");
            }

            [TestMethod]
            public void Serialize_Should_Not_Throw_On_Cycles_And_Ignore_Revisited_References()
            {
                var a = new Node { Id = "A" };
                var b = new Node { Id = "B" };
                a.Next = b;
                b.Prev = a; // cycle

                // Should NOT throw thanks to ReferenceHandler.IgnoreCycles
                var json = a.SerializeByTUtils();

                // A is present
                StringAssert.Contains(json, "\"Id\": \"A\"");
                // B is present
                StringAssert.Contains(json, "\"Id\": \"B\"");
                // The back-reference from B -> A is ignored (no infinite tree)
                // We expect "Prev" to exist but without complete re-serialization (can be null or missing)
                // Minimal check: JSON was generated (we're primarily testing "no throw" here).
                Assert.IsFalse(string.IsNullOrWhiteSpace(json));
            }

            [TestMethod]
            public void Deserialize_Should_Respect_PropertyName_Case_Insensitivity_And_Tolerate_Trailing_Commas_And_Comments()
            {
                // JSON with different property name case, comment and trailing comma
                var json = @"
            {
                // Comment should be ignored
                ""name"": ""Carol"",
                ""STATE"": ""Active"",
                ""renamedValue"": ""Shown"",
                ""NullableCount"": null, // trailing comma follows
            }";

                var dto = json.DeserializeByTUtils<SampleDto>();
                Assert.IsNotNull(dto);
                Assert.AreEqual("Carol", dto!.Name);
                Assert.AreEqual(Status.Active, dto.State);
                Assert.AreEqual("Shown", dto.OriginalValueName);
                Assert.IsNull(dto.NullableCount); // was correctly read as null
            }

            [TestMethod]
            public void TryDeserialize_Should_ReturnFalse_On_Invalid_Json()
            {
                var invalid = "{ this is not valid json ";

                bool ok = invalid.TryDeserializeByTUtils<SampleDto>(out var result);

                Assert.IsFalse(ok);
                Assert.IsNull(result);
            }

            [TestMethod]
            public void Serialize_Should_Indent_For_Readability()
            {
                var dto = new SampleDto { Name = "Pretty" };
                var json = dto.SerializeByTUtils();

                // Indentations (spaces/line breaks) present
                Assert.IsTrue(json.Contains("\n") || json.Contains("\r"));
                // Keys and values separated (heuristic)
                StringAssert.Contains(json, "  \"Name\": \"Pretty\"");
            }

            [TestMethod]
            public void Deserialize_To_Runtime_Type_Should_Work()
            {
                var dto = new SampleDto { Name = "Dyn", State = Status.Active };
                var json = dto.SerializeByTUtils();

                var obj = json.DeserializeByTUtils(typeof(SampleDto));
                Assert.IsInstanceOfType(obj, typeof(SampleDto));
                Assert.AreEqual("Dyn", ((SampleDto)obj!).Name);
            }
        }
    }
}
