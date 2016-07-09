/*
 *  RSA Public-Key encryption for the .Net framework
 *  Copyright (C) 2007, Paul Sanders.
 *  http://www.alpinesoft.co.uk
 *
 *  Based on original work in C by Christophe Devine, which is 
 *  Copyright (C) 2006, 2007, Christopher Devine, and is available from:
 *  http://xyssl.org/code/source/rsa/
 *
 *  Also requires Chew Keong TAN's most excellent BigInteger class:
 *  http://www.codeproject.com/csharp/BigInteger.asp
 *
 *	Support for SHA256 added by Pierre Arnaud, EPSITEC SA, Switzerland.
 *	Copyright (C) 2011, Pierre Arnaud
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License, version 2.1 as published by the Free Software Foundation.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 *  MA  02110-1301  USA
 *
 *  RSA was designed by Ron Rivest, Adi Shamir and Len Adleman, and is
 *  the cat's pyjamas as far as I am concerned:
 *  http://theory.lcs.mit.edu/~rivest/rsapaper.pdf
 *  http://www.cacr.math.uwaterloo.ca/hac/about/chap8.pdf
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable LocalVariableHidesMember
// ReSharper disable MemberCanBePrivate.Global

namespace TUtils.Common.Security.Asymmetric.OpenSource
{

	// ----------------------------------------------------------------------------
	// Class EZRSA starts here
	public class Ezrsa : RSA
	{

		public enum HASH_ALGORITHM
		{
			RSA_RAW = 0,
			RSA_SHA1 = 1,
			RSA_MD2 = 2,
			RSA_MD4 = 4,
			RSA_MD5 = 5,
			RSA_SHA256 = 6
		};

		// Constructor; keylen is in bits, and should be a multiple of 8
		public Ezrsa(int keylen)
		{
			this.keylen = keylen / 8;
			if (this.keylen * 8 != keylen)
				throw new ArgumentException("keylen must be a multiple of 8");
		}

		/// <summary>
		/// Initialise an EZRSA object from an XML string
		/// Uses the same markup scheme as RSACryptoServiceProvider.FromXmlString (qv)
		/// </summary>
		/// <param name="xmlString">
		/// example of generation: http://www.opinionatedgeek.com/DotNet/Snippets/Crypto/keygen.aspx
		/// private and public key pair:
		/// <code><![CDATA[
		/// <RSAKeyValue>
		///		<Modulus>ukvVJ7nFBxEQpmxNZRD2buzGgME5NVEZF9+0D7kcHTJiOltHfKl5TtwEQxvv3PRvwupacxK9LOFOCWJX6+tVhO43lFUxOon0mhT3dp8OMhQjYrkUu+zCUQ8/bkHKcAK3p6P1SOHxRH/Cv/WHWWr+HzEOfZY2kryg+rgPjnuOwvE=</Modulus>
		///		<Exponent>AQAB</Exponent>
		///		<P>/4aI9gDAmFvN1Yix3bAPidQWd/5ccvse1t760ODNYTEZAJlQnoxx0vOT9r3TKymJ2y5mV5GDGfWwQtUvqOGBvQ==</P>
		///		<Q>uqRjpIa5qYB6XCDKlKoHfyuzt2SVPzb254Zg7+etf+0imC8HE4ApJDV/CJJt0R8SB6nvIu9MlX4APBMgEL8nRQ==</Q>
		///		<DP>2Iw3PePdVEFY6wHxWqJ+SJwIfqB90KOouwg1Hxekdh2ZxrwnanYzcEckuhKdBxMo1Ss5aDTVGgbw3XK19TVHMQ==</DP>
		///		<DQ>TheMLYHFWxuLltKNkIhX3KjPaDNokuuPgS3jj11zZaw1plE+97TPAfx0K4UA99e1NomuqgJQG9h9hqVF7FvetQ==</DQ>
		///		<InverseQ>MMWcN970pQluUD7lJzr+0B2MmNYpLnSu9faVfsnCjCSUtRiKSeQAZ5oo5Oi2QCXtqPJrrHsv7M1tTmIkArc5AA==</InverseQ>
		///		<D>CBoms9LYPxGurfI9XnUhpprdGjntFUTI6NklkV59Wsa3b27LWeBcAoI+nDWRlcQ6vRkopGMO/63v0SgqBzxlrG5d04iie9aW2i7e1A/QKHHwUyC/7vnjjnTaex4JugHDVKfaxUWONhJXDqbpqAIemk6TO+TSn6BJ1TXNn2TbSF0=</D>
		/// </RSAKeyValue>
		/// ]]></code>
		/// or public key pair:
		/// <code><![CDATA[
		/// <RSAKeyValue>
		///		<Modulus>ukvVJ7nFBxEQpmxNZRD2buzGgME5NVEZF9+0D7kcHTJiOltHfKl5TtwEQxvv3PRvwupacxK9LOFOCWJX6+tVhO43lFUxOon0mhT3dp8OMhQjYrkUu+zCUQ8/bkHKcAK3p6P1SOHxRH/Cv/WHWWr+HzEOfZY2kryg+rgPjnuOwvE=</Modulus>
		///		<Exponent>AQAB</Exponent>
		/// </RSAKeyValue>
		/// ]]></code>
		/// </param>
		public override void FromXmlString(string xmlString)
		{
			StringReader sr = new StringReader(xmlString);

			//  XmlReaderSettings settings = new XmlReaderSettings ();
			//  settings.ConformanceLevel = ConformanceLevel.Fragment;
			//  settings.IgnoreWhitespace = true;
			//  settings.IgnoreComments = true;
			//  XmlReader reader = XmlReader.Create (sr, settings);

			XmlTextReader reader = new XmlTextReader(sr);
			N = E = P = Q = DP = DQ = QP = D = null;

			for (;;)
			{
				XmlNodeType node_type = reader.MoveToContent();
				switch (node_type)
				{
					case XmlNodeType.Element:
						{
							string element_name = reader.Name;
							if (!check_XML_element(reader, element_name, "Modulus", ref N) &&
								!check_XML_element(reader, element_name, "Exponent", ref E) &&
								!check_XML_element(reader, element_name, "P", ref P) &&
								!check_XML_element(reader, element_name, "Q", ref Q) &&
								!check_XML_element(reader, element_name, "DP", ref DP) &&
								!check_XML_element(reader, element_name, "DQ", ref DQ) &&
								!check_XML_element(reader, element_name, "InverseQ", ref QP) &&
								!check_XML_element(reader, element_name, "D", ref D))
							{
								reader.ReadString();
							}
							break;
						}

					case XmlNodeType.EndElement:
						reader.ReadEndElement();
						break;

					case XmlNodeType.None:
						return;

					default:
						throw new ArgumentException("something unexpected in xmlString");
				}
			}
		}


		// As RSACryptoServiceProvider.ToXmlString (qv)
		public override string ToXmlString(bool includePrivateParameters)
		{
			string result =
				"<RSAKeyValue>\n" +
				"    <Modulus>" + bigint_to_b64(N) + "</Modulus>\n" +
				"    <Exponent>" + bigint_to_b64(E) + "</Exponent>\n";

			if (includePrivateParameters)
				result +=
				"    <P>" + bigint_to_b64(P) + "</P>\n" +
				"    <Q>" + bigint_to_b64(Q) + "</Q>\n" +
				"    <DP>" + bigint_to_b64(DP) + "</DP>\n" +
				"    <DQ>" + bigint_to_b64(DQ) + "</DQ>\n" +
				"    <InverseQ>" + bigint_to_b64(QP) + "</InverseQ>\n" +
				"    <D>" + bigint_to_b64(D) + "</D>\n";

			result +=
				"</RSAKeyValue>\n";
			return result;
		}


		// As RSACryptoServiceProvider.ImportParameters (qv)
		public override void ImportParameters(RSAParameters parameters)
		{
			N = new BigInteger(parameters.Modulus);
			E = new BigInteger(parameters.Exponent);

			P = (!ReferenceEquals(parameters.P, null)) ?
				new BigInteger(parameters.P) : null;
			Q = (!ReferenceEquals(parameters.Q, null)) ?
				new BigInteger(parameters.Q) : null;
			DP = (!ReferenceEquals(parameters.DP, null)) ?
				new BigInteger(parameters.DP) : null;
			DQ = (!ReferenceEquals(parameters.DQ, null)) ?
				new BigInteger(parameters.DQ) : null;
			QP = (!ReferenceEquals(parameters.InverseQ, null)) ?
				new BigInteger(parameters.InverseQ) : null;
			D = (!ReferenceEquals(parameters.D, null)) ?
				new BigInteger(parameters.D) : null;
		}


		// As RSACryptoServiceProvider.ExportParameters (qv)
		public override RSAParameters ExportParameters(bool includePrivateParameters)
		{
			RSAParameters result = new RSAParameters
			{
				Modulus = N.getBytes(),
				Exponent = E.getBytes()
			};
			if (includePrivateParameters)
			{
				result.P = P.getBytes();
				result.Q = Q.getBytes();
				result.DP = DP.getBytes();
				result.DQ = DQ.getBytes();
				result.InverseQ = QP.getBytes();
				result.D = D.getBytes();
			}

			return result;
		}


		// Generate an RSA keypair
		// Popular exponents are 3, 17 and 65537; the bigger it is, the slower encryption becomes
		public void GenerateKeyPair(int exponent)
		{
			for (;;)
			{
				BigInteger E = exponent;
				Random rand = new Random();
				int nbits = keylen * 8 / 2;                // so that P * Q < N

				// Find primes P and Q with Q < P so that:
				//      GCD (E, (P - 1) * (Q - 1)) == 1

				BigInteger P;
				BigInteger Q;
				BigInteger Pmin1 = null;
				BigInteger Qmin1 = null;
				BigInteger H = null;
				BigInteger GCD = null;

				do
				{
					P = BigInteger.genPseudoPrime(nbits, 20, rand);
					Q = BigInteger.genPseudoPrime(nbits, 20, rand);
					if (P == Q)
						continue;
					if (P < Q)
					{
						BigInteger swap = P;
						P = Q;
						Q = swap;
					}

					Pmin1 = P - 1;
					Qmin1 = Q - 1;
					H = Pmin1 * Qmin1;
					GCD = H.gcd(E);
				}
				while (GCD != 1);

				// N = P * Q
				// D  = E^-1 mod ((P - 1) * (Q - 1))
				// DP = D mod (P - 1)
				// DQ = D mod (Q - 1)
				// QP = Q^-1 mod P

				N = P * Q;
				this.E = E;
				this.P = P;
				this.Q = Q;
				D = E.modInverse(H);
				DP = D.modPow(1, Pmin1);
				DQ = D.modPow(1, Qmin1);
				QP = Q.modInverse(P);

				// Check that this key actually works!
				// BigInteger.genPseudoPrime (rarely) returns a non-prime
				byte[] encrypt_me = new byte[keylen - 1];
				RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
				rng.GetBytes(encrypt_me);
				encrypt_me = pad_bytes(encrypt_me, keylen);       // ensure msg < modulus
				byte[] encrypted = DoPublic(encrypt_me);
				byte[] decrypted = DoPrivate(encrypted);
				if (compare_bytes(encrypt_me, 0, decrypted, 0, keylen))
					return;
			}
		}


		// Perform an RSA public key operation on input
		public byte[] DoPublic(byte[] input)
		{
			if (input.Length != keylen)
				throw new ArgumentException("input.Length does not match keylen");

			if (ReferenceEquals(N, null))
				throw new ArgumentException("no key set!");

			BigInteger T = new BigInteger(input);
			if (T >= N)
				throw new ArgumentException("input exceeds modulus");

			T = T.modPow(E, N);

			byte[] b = T.getBytes();
			return pad_bytes(b, keylen);
		}


		// Perform an RSA private key operation on input
		public byte[] DoPrivate(byte[] input)
		{
			if (input.Length != keylen)
				throw new ArgumentException("input.Length does not match keylen");

			if (ReferenceEquals(D, null))
				throw new ArgumentException("no private key set!");

			BigInteger T = new BigInteger(input);
			if (T >= N)
				throw new ArgumentException("input exceeds modulus");

			T = T.modPow(D, N);

			byte[] b = T.getBytes();
			return pad_bytes(b, keylen);
		}


		// Check if our private key is valid
		public bool CheckPrivateKey()
		{
			BigInteger TN = P * Q;
			if (TN != N)
				return false;

			BigInteger P1 = P - 1;
			BigInteger Q1 = Q - 1;
			BigInteger H = P1 * Q1;
			BigInteger GCD = E.gcd(H);
			if (GCD != 1)
				return false;

			return true;
		}


		// Encrypt a message and pack it up into PKCS#1 v1.5 format
		// Plug compatible with RSACryptoServiceProvider.Encrypt
		public byte[] Encrypt(byte[] input, bool fOAEP)
		{
			if (fOAEP)
				throw new ArgumentException("OAEP padding not supported, sorry");

			int input_len = input.Length;
			int n_pad = keylen - 3 - input_len;
			if (n_pad < 8)
				throw new ArgumentException("input too long");

			byte[] encrypt_me = new byte[keylen];
			encrypt_me[0] = 0;
			encrypt_me[1] = RSA_CRYPT;

			byte[] padding = new byte[n_pad];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(padding);

			for (int i = 0; i < n_pad; ++i)             // padding bytes must not be zero
			{
				if (padding[i] == 0)
					padding[i] = (byte)i;
				if ((byte)i == 0)
					padding[i] = 1;
			}

			Array.Copy(padding, 0, encrypt_me, 2, n_pad);
			encrypt_me[n_pad + 2] = 0;
			Array.Copy(input, 0, encrypt_me, n_pad + 3, input_len);

			return DoPublic(encrypt_me);
		}


		// Decrypt a message in PKCS#1 v1.5 format
		// Plug compatible with RSACryptoServiceProvider.Decrypt
		public byte[] Decrypt(byte[] input, bool fOAEP)
		{
			if (fOAEP)
				throw new ArgumentException("OAEP padding not supported, sorry");

			byte[] decrypted = DoPrivate(input);

			if (decrypted[0] != 0 || decrypted[1] != RSA_CRYPT)
				throw new ArgumentException("invalid signature bytes");

			int decrypted_len = decrypted.Length;               // = keylen
			for (int i = 2; i < decrypted_len - 1; ++i)
			{
				if (decrypted[i] == 0)
				{
					++i;
					int output_len = decrypted_len - i;
					byte[] output = new byte[output_len];
					Array.Copy(decrypted, i, output, 0, output_len);
					return output;
				}
			}

			throw new ArgumentException("invalid padding");
		}


		// Map a hash algorithm OID to a HASH_ALGORITHM
		// HASH_ALGORITHM knows about types of hash that CryptoConfig.MapNameToOID doesn't (and vice-versa)
		public HASH_ALGORITHM MapHashAlgorithmOID(string hash_algorithm_oid)
		{
			HASH_ALGORITHM ha;
			if (string.Compare(hash_algorithm_oid, CryptoConfig.MapNameToOID("MD5"), StringComparison.OrdinalIgnoreCase) == 0)
				ha = HASH_ALGORITHM.RSA_MD5;
			else if (string.Compare(hash_algorithm_oid, CryptoConfig.MapNameToOID("SHA1"), StringComparison.OrdinalIgnoreCase) == 0)
				ha = HASH_ALGORITHM.RSA_SHA1;
			else if (String.Compare(hash_algorithm_oid, CryptoConfig.MapNameToOID("SHA256"), StringComparison.OrdinalIgnoreCase) == 0)
				ha = HASH_ALGORITHM.RSA_SHA256;
			else
				throw new ArgumentException("unknown hash_algorithm_oid");
			return ha;
		}


		// SignHash - plug compatible with RSACryptoServiceProvider.SignHash
		public byte[] SignHash(byte[] sign_me, string hash_algorithm_oid)
		{
			HASH_ALGORITHM ha = MapHashAlgorithmOID(hash_algorithm_oid);
			return SignHash(sign_me, ha);
		}


		// Sign a message digest and pack it up into PKCS#1 format
		public byte[] SignHash(byte[] sign_me, HASH_ALGORITHM hash_algorithm)
		{
			int input_len = sign_me.Length;

			int n_pad = 0;
			switch (hash_algorithm)
			{
				case HASH_ALGORITHM.RSA_RAW:
					n_pad = keylen - 3 - input_len;
					break;

				case HASH_ALGORITHM.RSA_MD2:
				case HASH_ALGORITHM.RSA_MD4:
				case HASH_ALGORITHM.RSA_MD5:
					if (input_len != 16)
						throw new ArgumentException("MDx hashes must be 16 bytes long");
					n_pad = keylen - 3 - 16 - 18;
					break;

				case HASH_ALGORITHM.RSA_SHA1:
					if (input_len != 20)
						throw new ArgumentException("SHA1 hashes must be 20 bytes long");
					n_pad = keylen - 3 - 20 - 15;
					break;

				case HASH_ALGORITHM.RSA_SHA256:
					if (input_len != 32)
						throw new ArgumentException("SHA256 hashes must be 32 bytes long");
					n_pad = keylen - 3 - 32 - 19;
					break;
			}

			if (n_pad < 8)
				throw new ArgumentException("input too long");

			byte[] encrypt_me = new byte[keylen];
			encrypt_me[0] = 0;
			encrypt_me[1] = RSA_SIGN;

			for (int i = 0; i < n_pad; ++i)
				encrypt_me[i + 2] = 0xFF;

			encrypt_me[n_pad + 2] = 0;

			switch (hash_algorithm)
			{
				case HASH_ALGORITHM.RSA_RAW:
					Array.Copy(sign_me, 0, encrypt_me, n_pad + 3, input_len);
					break;

				case HASH_ALGORITHM.RSA_MD2:
				case HASH_ALGORITHM.RSA_MD4:
				case HASH_ALGORITHM.RSA_MD5:
					Array.Copy(ASN1_HASH_MDX, 0, encrypt_me, n_pad + 3, 18);
					encrypt_me[n_pad + 3 + 13] = (byte)hash_algorithm;
					Array.Copy(sign_me, 0, encrypt_me, n_pad + 3 + 18, input_len);
					break;

				case HASH_ALGORITHM.RSA_SHA1:
					Array.Copy(ASN1_HASH_SHA1, 0, encrypt_me, n_pad + 3, 15);
					Array.Copy(sign_me, 0, encrypt_me, n_pad + 3 + 15, input_len);
					break;

				case HASH_ALGORITHM.RSA_SHA256:
					Array.Copy(ASN1_HASH_SHA256, 0, encrypt_me, n_pad + 3, 19);
					Array.Copy(sign_me, 0, encrypt_me, n_pad + 3 + 19, input_len);
					break;
			}

			return DoPrivate(encrypt_me);
		}


		// VerifyHash - plug compatible with RSACryptoServiceProvider.VerifyHash
		public bool VerifyHash(byte[] hash, string hash_algorithm_oid, byte[] signature)
		{
			HASH_ALGORITHM ha = MapHashAlgorithmOID(hash_algorithm_oid);
			return VerifyHash(hash, signature, ha);
		}


		// Verify a signed PKCS#1 message digest
		public bool VerifyHash(byte[] hash, byte[] signature, HASH_ALGORITHM hash_algorithm)
		{
			int sig_len = signature.Length;
			if (sig_len != keylen)
				return false;

			byte[] decrypted = DoPublic(signature);
			if (decrypted[0] != 0 || decrypted[1] != RSA_SIGN)
				return false;

			int decrypted_len = decrypted.Length;           // = keylen

			for (int i = 2; i < decrypted_len - 1; ++i)
			{
				byte b = decrypted[i];
				if (b == 0)                                 // end of padding
				{
					++i;
					int bytes_left = decrypted_len - i;

					if (bytes_left == 34)                   // MDx
					{
						if (decrypted[i + 13] != (byte)hash_algorithm)
							return false;
						decrypted[i + 13] = 0;
						if (!compare_bytes(decrypted, i, ASN1_HASH_MDX, 0, 18))
							return false;
						return compare_bytes(decrypted, i + 18, hash, 0, 16);
					}

					if (bytes_left == 35 && hash_algorithm == HASH_ALGORITHM.RSA_SHA1)
					{
						if (!compare_bytes(decrypted, i, ASN1_HASH_SHA1, 0, 15))
							return false;
						return compare_bytes(decrypted, i + 15, hash, 0, 20);
					}

					if (bytes_left == hash.Length && hash_algorithm == HASH_ALGORITHM.RSA_RAW)
						return compare_bytes(decrypted, i, hash, 0, bytes_left);

					return false;
				}

				if (b != 0xFF)
					break;
			}

			return false;
		}


		// SignData - plug compatible with RSACryptoServiceProvider.SignData,
		// but only this one override provided
		public byte[] SignData(byte[] data, HashAlgorithm hasher)
		{
			HASH_ALGORITHM ha = map_hash_algorithm(hasher);
			byte[] hash = hasher.ComputeHash(data);
			byte[] signed_hash = SignHash(hash, ha);
			return signed_hash;
		}


		// VerifyData - plug compatible with RSACryptoServiceProvider.VerifyData
		public bool VerifyData(byte[] data, HashAlgorithm hasher, byte[] signature)
		{
			HASH_ALGORITHM ha = map_hash_algorithm(hasher);
			byte[] hash = hasher.ComputeHash(data);
			return VerifyHash(hash, signature, ha);
		}


		// Encrypt rgb (raw data) using our public key
		public override byte[] EncryptValue(byte[] rgb)
		{
			return DoPublic(rgb);
		}


		// Decrypt rgb (raw data) using our private key
		public override byte[] DecryptValue(byte[] rgb)
		{
			return DoPrivate(rgb);
		}


		// Required by AssymetricAlgorithm base class
		public override string KeyExchangeAlgorithm => "RSA-PKCS1-KeyEx";


		// Required by AssymetricAlgorithm base class
		public override string SignatureAlgorithm => "http://www.w3.org/2000/09/xmldsig#rsa-sha1";


		// For completeness; set is not implemented
		public override int KeySize
		{
			get { return keylen * 8; }
			set { throw new ArgumentException("please do this in the constructor!"); }
		}


		// ----------------------------------------------------------------------------
		// Protected methods

		// Required by AssymetricAlgorithm base class
		protected override void Dispose(bool disposing)
		{
			N = E = P = Q = DP = DQ = QP = D = null;
		}


		// ----------------------------------------------------------------------------
		// Private methods

		// Compare byte arrays
		private bool compare_bytes(byte[] b1, int i1, byte[] b2, int i2, int n)
		{
			for (int i = 0; i < n; ++i)
			{
				if (b1[i + i1] != b2[i + i2])
					return false;
			}

			return true;
		}


		// pad b with leading 0's to make it n bytes long
		private byte[] pad_bytes(byte[] b, int n)
		{
			int len = b.Length;
			if (len >= n)
				return b;

			byte[] result = new byte[n];
			int padding = n - len;
			for (int i = 0; i < padding; ++i)
				result[i] = 0;
			Array.Copy(b, 0, result, padding, len);
			return result;
		}


		// Convert a big integer to a base 64 string
		private string bigint_to_b64(BigInteger bi)
		{
			byte[] b = bi.getBytes();
			return Convert.ToBase64String(b);
		}


		// Initialise bi from XML element if element_name matches
		// Used by FromXmlString
		private bool check_XML_element(XmlReader reader, string element_name,
			string element_name_required, ref BigInteger bi_out)
		{
			if (String.Compare(element_name, element_name_required, StringComparison.OrdinalIgnoreCase) != 0)
				return false;

			string s = reader.ReadString();
			byte[] b = Convert.FromBase64String(s);
			BigInteger bi = new BigInteger(b);
			bi_out = bi;
			return true;
		}


		// Map a HashAlgorithm object to our HASH_ALGORITHM enumeration
		HASH_ALGORITHM map_hash_algorithm(HashAlgorithm hasher)
		{
			Type t = hasher.GetType();

			if (ReferenceEquals(t, typeof(MD5CryptoServiceProvider)))
				return HASH_ALGORITHM.RSA_MD5;
			if (ReferenceEquals(t, typeof(SHA1CryptoServiceProvider)))
				return HASH_ALGORITHM.RSA_SHA1;
			if (ReferenceEquals(t, typeof(SHA256Managed)))
				return HASH_ALGORITHM.RSA_SHA256;
			throw new ArgumentException("unknown HashAlgorithm");
		}


		// ----------------------------------------------------------------------------
		// Instance data and constants

		const int RSA_SIGN = 0x01;
		const int RSA_CRYPT = 0x02;

		static readonly byte[] ASN1_HASH_MDX =
			{ 0x30, 0x20, 0x30, 0x0C, 0x06, 0x08, 0x2A, 0x86, 0x48,
		  0x86, 0xF7, 0x0D, 0x02, 0x00, 0x05, 0x00, 0x04, 0x10 };
		static readonly byte[] ASN1_HASH_SHA1 =
			{ 0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03,
		  0x02, 0x1A, 0x05, 0x00, 0x04, 0x14 };
		static readonly byte[] ASN1_HASH_SHA256 =
			{ 0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48,
		  0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04,
		  0x20 };

		private readonly int keylen;             // in bytes
		private BigInteger N;
		private BigInteger E;
		private BigInteger P;
		private BigInteger Q;
		private BigInteger DP;
		private BigInteger DQ;
		private BigInteger QP;
		private BigInteger D;
	}                                   // end class EZRSA

}                                   // end namespace AlpineSoft