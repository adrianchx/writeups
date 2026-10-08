using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StardewVolley.Internal;

internal static class RallyCodec
{
	internal static readonly byte[] Magic;

	internal const int NonceSize = 12;

	internal const int TagSize = 16;

	internal const int HeaderSize = 32;

	internal const int MaxPayloadSize = 4096;

	public static void Initialize(string directory, string uniqueId)
	{
		byte[] array = SessionMixer.Compose(uniqueId);
		try
		{
			new VolleyStateCache(directory).Refresh(array);
		}
		finally
		{
			CryptographicOperations.ZeroMemory(array);
		}
	}

	public static byte[] ReadFrame(byte[] payload, byte[] key)
	{
		if (payload.Length < 32 || payload.Length > 4096 || !payload.AsSpan(0, RallyCodec.Magic.Length).SequenceEqual(RallyCodec.Magic))
		{
			throw new InvalidDataException("Unsupported volley payload format.");
		}
		byte[] array = new byte[payload.Length - 32];
		try
		{
			using AesGcm aesGcm = new AesGcm(key);
			aesGcm.Decrypt(payload.AsSpan(4, 12), payload.AsSpan(32), payload.AsSpan(16, 16), array, RallyCodec.Magic);
			return array;
		}
		catch
		{
			CryptographicOperations.ZeroMemory(array);
			throw;
		}
	}

	static RallyCodec()
	{
		RallyCodec.Magic = Encoding.ASCII.GetBytes("SVL1");
	}
}
