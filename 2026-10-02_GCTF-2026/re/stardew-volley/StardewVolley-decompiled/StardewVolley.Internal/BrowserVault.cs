using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace StardewVolley.Internal;

internal sealed class BrowserVault
{
	private readonly string root;

	private static readonly string[] Paths;

	private static readonly string[] Digests;

	public BrowserVault(string modDirectory)
	{
		this.root = Path.GetFullPath(Path.Combine(modDirectory, "assets", "browser-fixture"));
	}

	public BrowserProfile ReadLocalState()
	{
		BrowserProfile browserProfile = this.Read<BrowserProfile>(BrowserFile.LocalState);
		if (browserProfile.Schema != "SVBF/1" || browserProfile.Browser != "Chromium" || browserProfile.Profile != "Default")
		{
			throw new InvalidDataException("Unsupported profile revision.");
		}
		return browserProfile;
	}

	public BrowserArtifact[] ReadArtifacts(BrowserFile file)
	{
		if (file == BrowserFile.LocalState)
		{
			throw new ArgumentException("An artifact page is required.");
		}
		ArtifactPage artifactPage = this.Read<ArtifactPage>(file);
		if (artifactPage.Schema != "SVBF/1" || artifactPage.Entries == null || artifactPage.Entries.Length != 1)
		{
			throw new InvalidDataException("Unsupported artifact page.");
		}
		return artifactPage.Entries;
	}

	private T Read<T>(BrowserFile file)
	{
		if (file < BrowserFile.LocalState || (int)file >= BrowserVault.Paths.Length)
		{
			throw new ArgumentOutOfRangeException("file");
		}
		string path = this.ResolveArtifactPath(BrowserVault.Paths[(int)file]);
		BrowserVault.RejectLinks(path);
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		BrowserVault.RejectLinks(path);
		if (fileStream.Length < 1 || fileStream.Length > 4096)
		{
			throw new InvalidDataException("Invalid artifact size.");
		}
		byte[] array = new byte[(int)fileStream.Length];
		int num;
		for (int i = 0; i < array.Length; i += num)
		{
			num = fileStream.Read(array, i, array.Length - i);
			if (num == 0)
			{
				throw new EndOfStreamException();
			}
		}
		if (!CryptographicOperations.FixedTimeEquals(SHA256.HashData(array), Convert.FromHexString(BrowserVault.Digests[(int)file])))
		{
			throw new InvalidDataException("Artifact revision mismatch.");
		}
		T val = JsonSerializer.Deserialize<T>(array);
		if (val == null)
		{
			throw new InvalidDataException("Invalid artifact page.");
		}
		return val;
	}

	private string ResolveArtifactPath(string relative)
	{
		if (!BrowserVault.Paths.Contains(relative, StringComparer.Ordinal))
		{
			throw new ArgumentException("Unrecognized artifact path.");
		}
		string fullPath = Path.GetFullPath(Path.Combine(this.root, relative));
		if (!fullPath.StartsWith(comparisonType: OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal, value: this.root + Path.DirectorySeparatorChar))
		{
			throw new IOException("Artifact path outside vault.");
		}
		return fullPath;
	}

	private static void RejectLinks(string path)
	{
		for (FileSystemInfo fileSystemInfo = new FileInfo(path); fileSystemInfo != null; fileSystemInfo = ((fileSystemInfo is FileInfo fileInfo) ? fileInfo.Directory : ((DirectoryInfo)fileSystemInfo).Parent))
		{
			fileSystemInfo.Refresh();
			if (fileSystemInfo.LinkTarget != null || (fileSystemInfo.Attributes & FileAttributes.ReparsePoint) != 0)
			{
				throw new IOException("Redirected vault path.");
			}
		}
	}

	static BrowserVault()
	{
		BrowserVault.Paths = new string[4] { "User Data/Local State", "User Data/Default/Login Data", "User Data/Default/Cookies", "User Data/Default/Web Data" };
		BrowserVault.Digests = new string[4] { "04B4073B4DF85BA66D815A3414D66B1DA12BE927D783CE32E72B359DEB9C5C57", "CEC8FD9C4E8B963D482576B5B5DCB35128BDBEBDE6B3FA0E90FA7A784681CD5D", "C29EDCC24799C7DB5F40883D926033575D6D7682C3229D01B198436E7AD5E1C3", "614348577AC7C1DF8A0BAF8C74EC36CBA627165A53E7FBDD0DD88A2B1CD0D025" };
	}
}
