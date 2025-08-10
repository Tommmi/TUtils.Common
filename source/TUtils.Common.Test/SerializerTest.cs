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
        [TestClass]
        public class SerializerTest
        {
            // ==== Hilfstypen für Tests ====

            private enum Status
            {
                Unknown = 0,
                Active = 1,
                Paused = 2
            }

            private class SampleDto
            {
                public string Name { get; set; } = "Alice";

                public int? NullableCount { get; set; } // bleibt null -> soll weggelassen werden

                public Status State { get; set; } = Status.Active;

                [JsonIgnore]
                public string Secret { get; set; } = "TOP-SECRET";

                [JsonPropertyName("renamedValue")]
                public string OriginalValueName { get; set; } = "Visible";

                public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 10, 12, 34, 56, DateTimeKind.Unspecified);
            }

            private class Node
            {
                public string Id { get; set; } = Guid.NewGuid().ToString();
                public Node? Next { get; set; }
                public Node? Prev { get; set; }
            }

            // ==== Tests ====

            [TestMethod]
            public void Serialize_Should_OmitNulls_RespectIgnore_UseEnumStrings_AndPropertyName()
            {
                var dto = new SampleDto
                {
                    Name = "Bob",
                    NullableCount = null,      // soll fehlen
                    State = Status.Paused,     // soll als "Paused" erscheinen
                    Secret = "SHOULD_NOT_APPEAR",
                    OriginalValueName = "X"
                };

                var json = dto.SerializeByTUtils();

                // menschenlesbar (eingerückt)
                StringAssert.Contains(json, "\n");

                // Null soll fehlen
                Assert.IsFalse(json.Contains("NullableCount", StringComparison.Ordinal));

                // [JsonIgnore] respektiert
                Assert.IsFalse(json.Contains("Secret", StringComparison.Ordinal));

                // [JsonPropertyName] respektiert
                StringAssert.Contains(json, "\"renamedValue\": \"X\"");

                // Enum als String
                StringAssert.Contains(json, "\"State\": \"Paused\"");
            }

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

                // Unspecified: kein 'Z' und typischerweise kein Offset-Anhang
                // (wir prüfen hier, dass kein 'Z' direkt vor dem Schluss-Anführungszeichen steht)
                StringAssert.Contains(json, "\"Unspec\": \"" + unspecified.ToString("O") + "\"");

                // UTC: endet auf 'Z'
                StringAssert.Contains(json, "\"Utc\": \"" + utc.ToString("O") + "\"");

                // Local: enthält Offset (+HH:MM oder -HH:MM). Wir validieren via Regex.
                var localIso = local.ToString("O"); // enthält Offset der lokalen Zone
                StringAssert.Contains(json, "\"Local\": \"" + localIso + "\"");

                // Zusatz-Check: Regex auf Offset-Muster
                var rx = new Regex(@"""Local"":\s*""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}([+-]\d{2}:\d{2})""");
                Assert.IsTrue(rx.IsMatch(json), "Local DateTime sollte einen Offset enthalten.");
            }

            [TestMethod]
            public void Serialize_DateTimeOffset_Should_Preserve_Exact_Offset()
            {
                var dto = new
                {
                    When = new DateTimeOffset(2025, 8, 10, 12, 34, 56, TimeSpan.FromHours(+2))
                };

                var json = dto.SerializeByTUtils();

                // exakter Offset +02:00 muss drinstehen
                StringAssert.Contains(json, "\"When\": \"" + dto.When.ToString("O") + "\"");
            }

            [TestMethod]
            public void Serialize_Should_Not_Throw_On_Cycles_And_Ignore_Revisited_References()
            {
                var a = new Node { Id = "A" };
                var b = new Node { Id = "B" };
                a.Next = b;
                b.Prev = a; // Zyklus

                // Sollte NICHT werfen dank ReferenceHandler.IgnoreCycles
                var json = a.SerializeByTUtils();

                // A ist da
                StringAssert.Contains(json, "\"Id\": \"A\"");
                // B ist da
                StringAssert.Contains(json, "\"Id\": \"B\"");
                // Die Rückreferenz von B -> A wird ignoriert (kein unendlicher Baum)
                // Wir erwarten, dass "Prev" existiert, aber ohne vollständige erneute Serialisierung (kann null sein oder fehlen)
                // Minimalprüfung: Es wurde ein JSON erzeugt (wir testen hier primär "kein Throw").
                Assert.IsFalse(string.IsNullOrWhiteSpace(json));
            }

            [TestMethod]
            public void Deserialize_Should_Respect_PropertyName_Case_Insensitivity_And_Tolerate_Trailing_Commas_And_Comments()
            {
                // JSON mit anderem Propertynamen-Case, Kommentar und trailing comma
                var json = @"
            {
                // Kommentar sollte ignoriert werden
                ""name"": ""Carol"",
                ""STATE"": ""Active"",
                ""renamedValue"": ""Shown"",
                ""NullableCount"": null, // trailing comma folgt
            }";

                var dto = json.DeserializeByTUtils<SampleDto>();
                Assert.IsNotNull(dto);
                Assert.AreEqual("Carol", dto!.Name);
                Assert.AreEqual(Status.Active, dto.State);
                Assert.AreEqual("Shown", dto.OriginalValueName);
                Assert.IsNull(dto.NullableCount); // wurde korrekt als null gelesen
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

                // Einrückungen (Leerzeichen/Zeilenumbrüche) vorhanden
                Assert.IsTrue(json.Contains("\n") || json.Contains("\r"));
                // Schlüssel und Werte getrennt (heuristisch)
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
