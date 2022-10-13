using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace rt_ota_packaging_tool
{

	public class CryptApi
	{
		public enum CryptType
		{
			NO_CRYPT,
			AES_256_CBC
		}

		private SymmetricAlgorithm des = Rijndael.Create();

		private CryptType CurType;

		public CryptApi(CryptType type, string key, string Iv)
		{
			CurType = type;
			if (CurType == CryptType.AES_256_CBC)
			{
				des.BlockSize = 128;
				des.KeySize = 256;
				des.FeedbackSize = 128;
				des.Padding = PaddingMode.None;
				des.Mode = CipherMode.CBC;
				des.Key = Encoding.ASCII.GetBytes(key);
				des.IV = Encoding.ASCII.GetBytes(Iv);
			}
		}

		public byte[] Encrypt(byte[] inputdata)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(des.Key, des.IV), CryptoStreamMode.Write);
			cryptoStream.Write(inputdata, 0, inputdata.Length);
			cryptoStream.FlushFinalBlock();
			byte[] result = memoryStream.ToArray();
			cryptoStream.Close();
			memoryStream.Close();
			return result;
		}

		public byte[] Decrypt(byte[] inputdata, int rawSize)
		{
			byte[] array = new byte[rawSize];
			using (MemoryStream memoryStream = new MemoryStream(inputdata))
			{
				using CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Read);
				cryptoStream.Read(array, 0, array.Length);
				cryptoStream.Close();
				memoryStream.Close();
			}
			return array;
		}
	}
}