using System;
using System.Runtime.InteropServices;
using System.Text;

namespace rt_ota_packaging_tool
{
		public class structSerializable
	{
		public struct MyStruct
		{
			public byte[] _int;

			public byte[] _long;

			public byte[] _byte;
		}

		public static byte[] StructToBytes(object structure)
		{
			int num = Marshal.SizeOf(structure);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
				byte[] array = new byte[num];
				Marshal.Copy(intPtr, array, 0, num);
				return array;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		public static object BytesToStruct(byte[] bytes, Type strcutType)
		{
			int num = Marshal.SizeOf(strcutType);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				Marshal.Copy(bytes, 0, intPtr, num);
				return Marshal.PtrToStructure(intPtr, strcutType);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		public void strcut()
		{
			byte[] bytes = Encoding.ASCII.GetBytes("fengxinxinTest1234567890");
			byte[] bytes2 = Encoding.ASCII.GetBytes("zhangsan");
			byte[] bytes3 = Encoding.ASCII.GetBytes("lisi");
			MyStruct myStruct = default(MyStruct);
			myStruct._int = bytes;
			myStruct._long = bytes2;
			myStruct._byte = bytes3;
			byte[] array = StructToBytes(myStruct);
			int num = 0;
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				Console.WriteLine(num + " : " + b);
				num++;
			}
			Console.ReadKey();
		}
	}
}