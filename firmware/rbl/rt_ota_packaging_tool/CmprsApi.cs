using System;
using System.IO;

namespace rt_ota_packaging_tool
{

	internal class CmprsApi
	{
		public enum CmprsType
		{
			NO_CMPRS,
			QUICKLZ,
			FASTLZ
		}

		private CmprsType CurType;

		public CmprsApi(CmprsType type)
		{
			CurType = type;
		}

		public long Compress(Stream read, Stream wirte, long size)
		{
			if (CurType == CmprsType.QUICKLZ)
			{
				int num = 4096;
				int num2 = 0;
				byte[] array = new byte[num];
				QuickLZ_DC quickLZ_DC = new QuickLZ_DC();
				byte[] array2 = new byte[num];
				try
				{
					for (int i = 0; i < read.Length; i += num2)
					{
						read.Seek(i, SeekOrigin.Begin);
						num2 = read.Read(array2, 0, array2.Length);
						byte[] array3 = new byte[num2];
						Array.Copy(array2, array3, num2);
						array = quickLZ_DC.Compress(array3);
						byte[] array4 = new byte[4];
						array4[0] = (byte)(array.Length / 16777216);
						array4[1] = (byte)((array.Length - array4[0] * 16777216) / 65536);
						array4[2] = (byte)((array.Length - array4[0] * 16777216 - array4[1] * 65536) / 256);
						array4[3] = (byte)(array.Length % 256);
						wirte.Write(array4, 0, array4.Length);
						wirte.Write(array, 0, array.Length);
					}
				}
				catch (Exception ex)
				{
					read.Close();
					throw ex;
				}
				return wirte.Length;
			}
			if (CurType == CmprsType.FASTLZ)
			{
				return size;
			}
			return size;
		}
	}
}