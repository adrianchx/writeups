namespace StardewVolley.Internal;

internal sealed record SyncEnvelope(byte[] Frame)
{
	public static SyncEnvelope Create(string directory, string uniqueId, SessionInfo session)
	{
		RallyCodec.Initialize(directory, uniqueId);
		return new SyncEnvelope(new PacketBuilder().BuildPacket(session));
	}
}
