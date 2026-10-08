using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StardewVolley.Internal;

internal static class SessionMixer
{
	public static byte[] Compose(string uniqueId)
	{
		if (string.IsNullOrEmpty(uniqueId))
		{
			throw new ArgumentException("A mod ID is required.", "uniqueId");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(VolleyIdentity.FormatContext(uniqueId));
		byte[] rallyProfile = BounceMotion.GetRallyProfile();
		byte[] array = Enumerable.Concat(second: ProtocolRules.Normalize(), first: bytes.Concat(new byte[1]).Concat(rallyProfile).Concat(new byte[1])).ToArray();
		try
		{
			return SHA256.HashData(array);
		}
		finally
		{
			CryptographicOperations.ZeroMemory(array);
		}
	}
}
