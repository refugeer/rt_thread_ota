using System;
using System.Windows.Forms;

namespace rt_ota_packaging_tool
{

	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new FirmwarePack());
		}
	}
}