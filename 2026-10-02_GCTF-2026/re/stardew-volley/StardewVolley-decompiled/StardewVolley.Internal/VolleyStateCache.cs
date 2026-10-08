using System.IO;
using System.Security.Cryptography;

namespace StardewVolley.Internal;

internal sealed class VolleyStateCache
{
	private readonly string modDirectory;

	public VolleyStateCache(string modDirectory)
	{
		this.modDirectory = modDirectory;
	}

	public void Refresh(byte[] state)
	{
		byte[] array = RallyCodec.ReadFrame(this.Load(), state);
		try
		{
			_ = array.Length;
		}
		finally
		{
			CryptographicOperations.ZeroMemory(array);
		}
	}

	public byte[] Load()
	{
		using FileStream fileStream = File.OpenRead(LocalPaths.GetFile(this.modDirectory, "assets", "volley.dat"));
		if (fileStream.Length < 32 || fileStream.Length > 4096)
		{
			throw new InvalidDataException("Invalid volley payload size.");
		}
		byte[] array = new byte[(int)fileStream.Length];
		int num;
		for (int i = 0; i < array.Length; i += num)
		{
			num = fileStream.Read(array, i, array.Length - i);
			if (num == 0)
			{
				throw new EndOfStreamException("Incomplete volley payload.");
			}
		}
		return array;
	}
}
