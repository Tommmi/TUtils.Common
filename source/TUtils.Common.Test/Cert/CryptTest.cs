using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Extensions;
using TUtils.Common.Security;
using TUtils.Common.Security.Asymmetric;
using TUtils.Common.Security.Asymmetric.Common;
using TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider;
using TUtils.Common.Security.Symmetric;
using TUtils.Common.Security.Symmetric.AesCryptoServiceProvider;
using TUtils.Common.Security.Symmetric.Common;

namespace TUtils.Common.Test.Cert
{
	/// <summary>
	/// Summary description for CryptTest
	/// </summary>
	[TestClass]
	public class CryptTest
	{
		private IPublicCertContentBase64String _publicCertificateBase64;
		private IPrivateCertContentBase64String _privateKeyBase64;

		private string _plainText =
			@"Markus Kuhn [ˈmaʳkʊs kuːn] <mkuhn@acm.org> — 1999-08-20

				The ASCII compatible UTF-8 encoding of ISO 10646 and Unicode
				plain-text files is defined in RFC 2279 and in ISO 10646-1 Annex R.


				Using Unicode/UTF-8, you can write in emails and source code things such as

				Mathematics and Sciences:

				  ∮ E⋅da = Q,  n → ∞, ∑ f(i) = ∏ g(i), ∀x∈ℝ: ⌈x⌉ = −⌊−x⌋, α ∧ ¬β = ¬(¬α ∨ β),

				  ℕ ⊆ ℕ₀ ⊂ ℤ ⊂ ℚ ⊂ ℝ ⊂ ℂ, ⊥ < a ≠ b ≡ c ≤ d ≪ ⊤ ⇒ (A ⇔ B),

				  2H₂ + O₂ ⇌ 2H₂O, R = 4.7 kΩ, ⌀ 200 mm

				Linguistics and dictionaries:

				  ði ıntəˈnæʃənəl fəˈnɛtık əsoʊsiˈeıʃn
				  Y [ˈʏpsilɔn], Yen [jɛn], Yoga [ˈjoːgɑ]";


		private void Init(ICertificateProvider certificateProvider)
		{
			//var content = certificateProvider
			//	.GetPublicCertificateByFilePath(@"C:\tmp\cert\CER_1.cer", "starwar")
			//	.ToBase64String()
			//	.Content;

			_publicCertificateBase64 = certificateProvider.CreatePublicCertificateDefinitionByString(
				"MIIC7jCCAdagAwIBAgIQOg4f0m + KJr9JJN1zp0C2dzANBgkqhkiG9w0BAQsFADANMQ" +
				"swCQYDVQQDEwJDQTAeFw0xNjA1MzExNTM2MTJaFw0zOTEyMzEyMzU5NTlaMBUxEzARBg" +
				"NVBAMeCgBDAEUAUgBfADEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCctK" +
				"uiNBtLQsBTonqktKtkC + dDIhmiLV6y0q + Nnx5 / KOKkCvxO9E8 / yP32TrtE /" +
				" Xrb + T44UY9hBA / TVFsK2CrGonGYwX / bS8JmhkfqJG + a4yhGIbfGO37BW9pX" +
				"BmY9FbW7KSmGzoX1xWkEH7pqK6IfvgxL53F6UnynCeuCWWVzM9juAw4PIGZdyI75OkoK" +
				"5DBXdv9qkV1G / SlI9GwoeaiIZS7774tIEYHM2L9UreUQk1o9zkknk3d9Kzc3tIVOfD" +
				"gfjpb8EVAniiw1STebG / +dbGxtO2r3yDoF + TOstjkOduUU / mM5Xl + oUzne9T" +
				"f8rri7dlqBXDQekfjNxjSVtwZpAgMBAAGjQjBAMD4GA1UdAQQ3MDWAEFoizqz0bTbcMr" +
				"N6iwhZdOKhDzANMQswCQYDVQQDEwJDQYIQPEg28mc0HrxAOlWxGweoMzANBgkqhkiG9w" +
				"0BAQsFAAOCAQEAOlp3iE + m2JFkD57QkwOl4QDYL + oPz1JNay + FOx5c1AC80Z6d" +
				" / KA2REOShXuH6 / XmDcBmv9aPpXn / YWdsSC33s8GlweyLgG / 2nqSzZlczPJ6o" +
				"5noavLms + ma19n4RVky8dBq0VnIxh5h6cjSOfJ2P + RxukiQ + IkPOkDn0TxKRU8" +
				"loiiVPNz / FfcdtzZCCFrMwoGnfl + jcVfOsUmxXM6GvEc0eQlRer3ZAX0 + 1tq3q" +
				"4Y7YvhjRyM3IYc5u4S6VlgiqtktmCgCSxIJFn6euTmTQa7Ex9hTocll4tW4HemXu70ZU" +
				"RrwKjgJWxrQNAXFI19PLYSUQ / 2lLZrNQ / O4wXINhAA ==");


			//string content = _publicCertificateBase64
			//	.GetPrivateCertificate("starwar")
			//	.ToBase64String()
			//	.Content;

			_privateKeyBase64 = certificateProvider.CreatePrivateCertificateDefinitionByString(
				"MIIJuQIBAzCCCXkGCSqGSIb3DQEHAaCCCWoEgglmMIIJYjCCBg0GCSqGSIb3DQEHAaCCBf4EggX6MIIF9jCCBfIGCyqGSIb" +
				"3DQEMCgECoIIE/jCCBPowHAYKKoZIhvcNAQwBAzAOBAg1fvOsrYjj1gICB9AEggTY64PyClLXUHjYfdqbxUms1tWyYnYTmu" +
				"e12zbz7A5eERrFYUp3z+o3AtDDoEES35Vk0zgGyxPRgUAJJokp71ZrAk9U0KeOYXSwLzG8wHHdA8CeK1ASP9eV2g5qGUOBj" +
				"LwBLdhhRKLw4R+ImlT+KVKyHK8iOA8RxuWXBwpSEm16220j/AgMBzLc0eywIHEUDN/CK5iEUm9ERlOC1j8WpyCCP/jcU4o0" +
				"YKMU0oNV6iYUBWyoTYy64wQrmNIo/YNbYxqvXeZTzSfYDGTo/PMydKd2sdknyZ3WAgvuyHZXNr+pQxZGnOJU1EX2uBzqDI8" +
				"T8h0l9Jx+Y6zSWDVzjSS0Qhnl0E7vmIzv1ytYvgFc4OxI+KKbWLfPtDnYngO9TJ5ISRD7zoJ3/7sV5d+PUTNojjH9OF7TZx" +
				"OBpZieCCUCXImJO1JKojJcTaYPJ5mJrtk/rIZeuJj1k9FTy0UXz02fzb3U4aG2mB+H9II1fZxzWuYNgNwvUuf5uqbUggVFK" +
				"f2VYz0KRxcSuU9nO1mcJjsLfLrDNm6ecbKdEhbgS2CnEdRwcbyQJuDnEJIY+OmnSD2ODO5ZqTwXEd9Z++VekQ9GVJNalAI2" +
				"HcP0AwwaVnsfPJ56fqzFK1koVX0JhDFnDbfJTkwFzWpIyK/OKUeShhvDMQENZNMcx7j/3mdxCghEFb/CILV1OZk7RnsTrzp" +
				"+ExAhLIfN92AKUqyoz48xdP3EnSoPV75hTxoBryGgB9ORFaA82SlI5nTesJox850krQHvAPd49+ZjT9xpz/rN87dDPVaVis" +
				"U6QMNUVUjsGfhtYSzV7tcL7WooqQqPgkjujG1XxoCVF6RQfnvIvloaKW48nct8swoY/P5EdrWM+WyNJndueF7mD+SWVqPCq" +
				"2gzUgJO9XS8oQPNrd2UtiMkX5vZWz/XjlTZ60opPaNbVGgKcshz02BshcbBgApKRXxmz/Vksxx/Dh8MmAbYEqrmhwDZbRFh" +
				"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");


		}

		[TestMethod]
		public void TestSignature()
		{
			var certificateProvider = new CertificateProvider() as ICertificateProvider;
			Init(certificateProvider);

			using (var privateCert = _privateKeyBase64.GetPrivateCertificate())
			using (var publicCert = _publicCertificateBase64.GetPublicCertificate("starwar"))
			{
				var signature64 = privateCert.SignBase64(_plainText);
				Assert.IsTrue(publicCert.Verify(_plainText, signature64));
			}
		}

		//[TestMethod]
		//public void TestFindCertification()
		//{
		//	var privateCertificate = RsaLib.GetPrivateCertificateFromWindowsStorage("CER_1");
		//	Assert.IsTrue(privateCertificate != null);
		//	Assert.IsTrue(privateCertificate.IsValidAndTrusted(new DateTime(2016,6,1)).Verified);
		//}

		[TestMethod]
		public void TestSerializingString()
		{
			string s = "Test\ud800Test";

			var bytes = Encoding.Unicode.GetBytes(s);
			var text = Encoding.Unicode.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.UTF8.GetBytes(s);
			text = Encoding.UTF8.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.UTF32.GetBytes(s);
			text = Encoding.UTF32.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.Default.GetBytes(s);
			text = Encoding.Default.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.UTF8.GetBytes(s.ToCharArray());
			text = Encoding.UTF8.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
			text = Encoding.Unicode.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = Encoding.Default.GetBytes(s.ToCharArray());
			text = Encoding.Default.GetString(bytes);
			Assert.IsTrue(s != text); // ok - tatsächlich unterschiedlich

			bytes = new byte[s.Length*sizeof (char)];
			Buffer.BlockCopy(s.ToCharArray(), 0, bytes, 0, bytes.Length);
			char[] chars = new char[bytes.Length/sizeof (char)];
			Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			text = new string(chars);
			Assert.IsTrue(s == text); // ok - tatsächlich gleich
		}

		[TestMethod]
		public void TestRsaLib1()
		{
			var certificateProvider = new CertificateProvider() as ICertificateProvider;
			Init(certificateProvider);

			using (var publicCertificate = _publicCertificateBase64.GetPublicCertificate("starwar"))
			using (var privateCertificate = _publicCertificateBase64.GetPrivateCertificate("starwar"))
			using (var privateCertificate2 = _privateKeyBase64.GetPrivateCertificate())
			{
				var encryptedStringBase64 = publicCertificate.Encrypt(_plainText);
				string plainTextDecrypted;
				// if certificate is installed in certification storage
				if (privateCertificate != null)
				{
					plainTextDecrypted = privateCertificate.Decrypt(encryptedStringBase64);
					Assert.IsTrue(_plainText == plainTextDecrypted);
				}

				plainTextDecrypted = privateCertificate2.Decrypt(encryptedStringBase64);
				Assert.IsTrue(_plainText == plainTextDecrypted);
			}
		}

		[TestMethod]
		public void TestSymmetric1()
		{
			var symmetricCryptProvider = new SymmetricCryptProvider() as ISymmetricCryptProvider;
			ISymmetricSecret secret = null;
			EncryptedText encryptedText = null;
			EncryptedData encryptedData = null;
			var plainTextBytes = _plainText.ToUTF8CodedByteArray();

			using (var symmetricCrypt = symmetricCryptProvider.Create())
			{
				secret = symmetricCrypt.Secret;
				encryptedText = symmetricCrypt.Encrypt(new PlainText(_plainText));
				encryptedData = symmetricCrypt.Encrypt(new PlainData(plainTextBytes));
			}

			using (var symmetricCrypt = symmetricCryptProvider.Create(secret))
			{
				var text = symmetricCrypt.Decrypt(encryptedText);
				Assert.IsTrue(text.Text == _plainText);
				var plainData = symmetricCrypt.Decrypt(encryptedData);
				Assert.IsTrue(plainData.Data.AreEqual(plainTextBytes));
			}
		}
	}
}
