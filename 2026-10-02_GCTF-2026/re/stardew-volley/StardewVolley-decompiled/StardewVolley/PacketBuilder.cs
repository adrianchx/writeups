using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StardewVolley.Internal;

namespace StardewVolley;

internal sealed class PacketBuilder
{
	internal const string Channel = "pelican-recreation";

	public int GetProtocolVersion()
	{
		return 1;
	}

	public string GetProtocolLabel()
	{
		return ProtocolRules.Label;
	}

	public string[] GetEventCatalog()
	{
		return new string[4] { "rally_start", "bounce", "serve", "session_end" };
	}

	public string BuildClientId(SessionInfo session)
	{
		return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new string[3] { "pelican-recreation", session.FarmerName, session.FarmName })));
	}

	public string DecodeEndpoint()
	{
		return Encoding.ASCII.GetString(Convert.FromBase64String("c3luYy5zdGFyZGV3dm9sbGV5LmludmFsaWQ="));
	}

	public byte[] BuildPacket(SessionInfo session)
	{
		return JsonSerializer.SerializeToUtf8Bytes(new
		{
			ProtocolVersion = this.GetProtocolVersion(),
			ProtocolLabel = this.GetProtocolLabel(),
			EventCatalog = this.GetEventCatalog(),
			ClientId = this.BuildClientId(session),
			Endpoint = this.DecodeEndpoint(),
			Session = session
		});
	}
}
