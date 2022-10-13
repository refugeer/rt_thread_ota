using System;
using System.IO;

namespace rt_ota_packaging_tool
{ 

	public class WriteLog
	{
		public static void WriteLogInfo(Exception ex)
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errlog.txt");
			StreamWriter streamWriter = File.AppendText(path);
			streamWriter.WriteLine();
			streamWriter.WriteLine(string.Format("   {0}Error 时间:{1}{0}", "---------------------------------", DateTime.Now));
			streamWriter.WriteLine("   导致错误信息 : " + ex.Message);
			streamWriter.WriteLine($"   导致错误的方法 :{ex.TargetSite.DeclaringType.FullName}.{ex.TargetSite.Name}");
			streamWriter.WriteLine();
			streamWriter.WriteLine(ex.StackTrace.ToString());
			streamWriter.Flush();
			streamWriter.Close();
		}
	}
}