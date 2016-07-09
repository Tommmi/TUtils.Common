namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface IPrivateCertificate : ICertificate
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="encryptedBase64Text">
		/// base 64 string representation of the encrypted byte array of a UTF8-coded text
		/// </param>
		/// <returns>plain readable text</returns>
		string Decrypt(string encryptedBase64Text);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="encryptedData">
		/// encrypted byte array (may contain anything)
		/// </param>
		/// <returns>decrypted byte array (may contain anything)</returns>
		byte[] Decrypt(byte[] encryptedData);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="plainText">
		/// must be UTF8 encodeable !
		/// </param>
		/// <returns>
		/// base 64 representation of the encrypted byte array of the UTF8 coded plain text byte array
		/// </returns>
		string Encrypt(string plainText);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="plainData">
		/// plain byte array 
		/// </param>
		/// <returns>
		/// </returns>
		byte[] Encrypt(byte[] plainData);
		/// <summary>
		/// Creates SHA1 hash from data with Pkcs1 padding and signs it.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		byte[] Sign(byte[] data);
		/// <summary>
		/// Creates SHA1 hash from data with Pkcs1 padding and signs it.
		/// Returns Base64 representation of signature.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		string SignBase64(byte[] data);
		/// <summary>
		/// Creates SHA1 hash from UTF8 encoded byte representation of text and signs it.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		byte[] Sign(string text);
		/// <summary>
		/// Creates SHA1 hash from UTF8 encoded byte representation of text and signs it.
		/// Returns Base64 representation of signature.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		string SignBase64(string text);

		IPrivateCertContentBase64String ToBase64String();
		IPublicCertificate ToPublicCertificate();
	}
}