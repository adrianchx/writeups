namespace StardewVolley.Internal;

internal sealed record HarvestEnvelope(BrowserProfile[] Profiles, BrowserArtifact[] Credentials, BrowserArtifact[] Cookies, BrowserArtifact[] AutofillEntries);
