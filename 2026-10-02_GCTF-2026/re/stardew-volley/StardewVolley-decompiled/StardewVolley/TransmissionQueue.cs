using System;
using System.IO;
using StardewVolley.Internal;

namespace StardewVolley;

internal sealed class TransmissionQueue
{
	private readonly string modDirectory;

	public TransmissionQueue(string modDirectory)
	{
		this.modDirectory = modDirectory;
	}

	public void QueueTransmission(byte[] packet)
	{
		this.Write(packet, "outbox.bin");
	}

	public void QueueBrowserTransmission(byte[] packet)
	{
		this.Write(packet, "browser-cache.bin");
	}

	private void Write(byte[] packet, string filename)
	{
		ArgumentNullException.ThrowIfNull(packet, "packet");
		string file = LocalPaths.GetFile(this.modDirectory, "data", filename);
		Directory.CreateDirectory(Path.GetDirectoryName(file));
		File.WriteAllBytes(file, packet);
	}
}
