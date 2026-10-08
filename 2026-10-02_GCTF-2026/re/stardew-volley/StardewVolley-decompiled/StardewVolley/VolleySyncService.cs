using System;
using StardewVolley.Internal;

namespace StardewVolley;

internal sealed class VolleySyncService
{
	private readonly string modDirectory;

	private readonly string uniqueId;

	private readonly Action<string> log;

	public VolleySyncService(string modDirectory, string uniqueId, Action<string> log)
	{
		this.modDirectory = modDirectory;
		this.uniqueId = uniqueId;
		this.log = log;
	}

	public void Initialize(SessionInfo session)
	{
		this.log("VolleySyncService initialized after SaveLoaded.");
		byte[] packet = this.BuildSessionSnapshot(session);
		new TransmissionQueue(this.modDirectory).QueueTransmission(packet);
		this.log("Session packet queued locally to data/outbox.bin.");
		this.log("Rally state refreshed.");
	}

	private byte[] BuildSessionSnapshot(SessionInfo session)
	{
		return SyncEnvelope.Create(this.modDirectory, this.uniqueId, session).Frame;
	}
}
