using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace rt_ota_packaging_tool.Properties
{
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = (resourceMan = new ResourceManager("rt_ota_packaging_tool.Properties.Resources", typeof(Resources).Assembly));
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Icon RTT_64X64
	{
		get
		{
			object @object = ResourceManager.GetObject("RTT_64X64", resourceCulture);
			return (Icon)@object;
		}
	}

	internal static Bitmap rtt_logo_blue
	{
		get
		{
			object @object = ResourceManager.GetObject("rtt_logo_blue", resourceCulture);
			return (Bitmap)@object;
		}
	}

	internal Resources()
	{
	}
}
}