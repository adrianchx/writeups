using System.IO;

namespace StardewVolley.Internal;

internal static class LocalPaths
{
	public static string GetFile(string root, string directory, string file)
	{
		string fullPath = Path.GetFullPath(root);
		string text = Path.Combine(fullPath, directory);
		string text2 = Path.Combine(text, file);
		string[] array = new string[3] { fullPath, text, text2 };
		foreach (string path in array)
		{
			if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
			{
				throw new IOException("Challenge paths must not be symbolic links or junctions.");
			}
		}
		return text2;
	}
}
