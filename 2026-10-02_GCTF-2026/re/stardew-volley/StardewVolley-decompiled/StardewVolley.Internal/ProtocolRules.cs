using System.Text;

namespace StardewVolley.Internal;

internal static class ProtocolRules
{
	internal const int Major = 1;

	internal const int Minor = 4;

	public static string Label => $"SVP/{1}.{4}";

	public static byte[] Normalize()
	{
		byte[] bytes = Encoding.ASCII.GetBytes(ProtocolRules.Label);
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] = (byte)(bytes[i] ^ 0x14 ^ i);
		}
		return bytes;
	}
}
