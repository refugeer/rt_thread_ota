using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace rt_ota_packaging_tool
{
		internal class QuickLZ_DC
	{
		private byte[] state_compress;

		private byte[] state_decompress;

		public uint QLZ_COMPRESSION_LEVEL => (uint)qlz_get_setting(0);

		public uint QLZ_SCRATCH_COMPRESS => (uint)qlz_get_setting(1);

		public uint QLZ_SCRATCH_DECOMPRESS => (uint)qlz_get_setting(2);

		public uint QLZ_VERSION_MAJOR => (uint)qlz_get_setting(7);

		public uint QLZ_VERSION_MINOR => (uint)qlz_get_setting(8);

		public int QLZ_VERSION_REVISION => qlz_get_setting(9);

		public uint QLZ_STREAMING_BUFFER => (uint)qlz_get_setting(3);

		public bool QLZ_MEMORY_SAFE => qlz_get_setting(6) == 1;

		[DllImport("quicklz150_32_3.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern IntPtr qlz_compress(byte[] source, byte[] destination, IntPtr size, byte[] scratch);

		[DllImport("quicklz150_32_3.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern IntPtr qlz_decompress(byte[] source, byte[] destination, byte[] scratch);

		[DllImport("quicklz150_32_3.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern IntPtr qlz_size_compressed(byte[] source);

		[DllImport("quicklz150_32_3.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern IntPtr qlz_size_decompressed(byte[] source);

		[DllImport("quicklz150_32_3.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern int qlz_get_setting(int setting);

		public QuickLZ_DC()
		{
			state_compress = new byte[qlz_get_setting(1)];
			if (QLZ_STREAMING_BUFFER == 0)
			{
				state_decompress = state_compress;
			}
			else
			{
				state_decompress = new byte[qlz_get_setting(2)];
			}
		}

		public byte[] Compress(byte[] Source)
		{
			byte[] array = new byte[Source.Length + 400];
			uint num = (uint)(int)qlz_compress(Source, array, (IntPtr)Source.Length, state_compress);
			byte[] array2 = new byte[num];
			Array.Copy(array, array2, num);
			return array2;
		}

		public byte[] Decompress(byte[] Source)
		{
			byte[] array = new byte[(int)qlz_size_decompressed(Source)];
			uint num = (uint)(int)qlz_decompress(Source, array, state_decompress);
			return array;
		}

		public long Compress(Stream read, Stream wirte, long size)
		{
			int num = 4096;
			int num2 = 0;
			byte[] array = new byte[num];
			byte[] array2 = new byte[num];
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num3 = 1;
				for (int i = 0; i < read.Length; i += num2)
				{
					read.Seek(i, SeekOrigin.Begin);
					num2 = read.Read(array2, 0, array2.Length);
					byte[] array3 = new byte[num2];
					Array.Copy(array2, array3, num2);
					array = Compress(array3);
					byte[] array4 = new byte[4];
					array4[0] = (byte)(array.Length / 16777216);
					array4[1] = (byte)((array.Length - array4[0] * 16777216) / 65536);
					array4[2] = (byte)((array.Length - array4[0] * 16777216 - array4[1] * 65536) / 256);
					array4[3] = (byte)(array.Length % 256);
					wirte.Write(array4, 0, array4.Length);
					wirte.Write(array, 0, array.Length);
					stringBuilder.AppendLine($"第{num3++}次压缩:\t压缩前大小:{array3.Length}\t压缩后大小:{array.Length}");
				}
				Fastlz.SaveCompInfo("QUICKLZ", stringBuilder.ToString());
			}
			catch (Exception ex)
			{
				read.Close();
				throw ex;
			}
			return wirte.Length;
		}

		public uint SizeCompressed(byte[] Source)
		{
			return (uint)(int)qlz_size_compressed(Source);
		}

		public uint SizeDecompressed(byte[] Source)
		{
			return (uint)(int)qlz_size_decompressed(Source);
		}
	}
}