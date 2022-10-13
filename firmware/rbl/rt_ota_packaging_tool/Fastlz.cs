using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace rt_ota_packaging_tool
{

	public class Fastlz
	{
		private const string dlName = "fastlz.dll";

		[DllImport("fastlz.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int fastlz_compress_xx(byte[] source, int sourceLen, byte[] output);

		[DllImport("fastlz.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int fastlz_compress_level_(int level, byte[] input, int length, byte[] output);

		[DllImport("fastlz.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int fastlz_decompress_xx(byte[] input, int length, byte[] output, int maxout);

		public static byte[] Compress(byte[] bts)
		{
			int num = 4096;
			int num2 = 0;
			byte[] array = new byte[num];
			byte[] array2 = new byte[num];
			byte[] array3 = new byte[num + 270];
			try
			{
				Stream stream = new MemoryStream(bts, 0, bts.Length);
				StringBuilder stringBuilder = new StringBuilder();
				MemoryStream memoryStream = new MemoryStream();
				StringBuilder stringBuilder2 = new StringBuilder();
				int num3 = 1;
				for (int i = 0; i < bts.Length; i += num2)
				{
					stream.Seek(i, SeekOrigin.Begin);
					num2 = stream.Read(array2, 0, array2.Length);
					byte[] array4 = new byte[num2];
					Array.Copy(array2, array4, num2);
					int num4 = fastlz_compress_xx(array4, array4.Length, array3);
					array = new byte[num4];
					Array.Copy(array3, array, num4);
					byte[] array5 = new byte[4];
					array5[0] = (byte)(array.Length / 16777216);
					array5[1] = (byte)((array.Length - array5[0] * 16777216) / 65536);
					array5[2] = (byte)((array.Length - array5[0] * 16777216 - array5[1] * 65536) / 256);
					array5[3] = (byte)(array.Length % 256);
					memoryStream.Write(array5, 0, array5.Length);
					memoryStream.Write(array, 0, array.Length);
					stringBuilder2.AppendLine($"第{num3++}次压缩:\t压缩前大小:{array4.Length}\t压缩后大小:{array.Length}");
				}
				SaveCompInfo("FASTLZ", stringBuilder2.ToString());
				return memoryStream.ToArray();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static byte[] Decompress(byte[] source, int rawSize)
		{
			byte[] array = new byte[rawSize];
			int num = fastlz_decompress_xx(source, source.Length, array, array.Length);
			byte[] array2 = new byte[num];
			Array.Copy(array, array2, array2.Length);
			return array2;
		}

		public static void SaveCompInfo(string fileStart, string info)
		{
		}
	}
}