namespace rt_ota_packaging_tool
{ 

	public class fnv1a
	{
		public const uint FNV_SEED = 2166136261u;

		public static uint fnv1a_r(byte oneByte, uint hash)
		{
			return (oneByte ^ hash) * 16777619;
		}

		public static uint calc(byte[] data, uint hash)
		{
			for (int i = 0; i < data.Length; i++)
			{
				hash = fnv1a_r(data[i], hash);
			}
			return hash;
		}
	}
}