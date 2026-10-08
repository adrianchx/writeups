using System.Linq;

namespace StardewVolley.Internal;

internal static class BounceMotion
{
	private static readonly byte[] BounceProfile;

	public static float GetFeedbackDuration()
	{
		return (float)(int)BounceMotion.BounceProfile.Max() * 40f;
	}

	public static byte[] GetRallyProfile()
	{
		return new int[6] { 2, 7, 11, 14, 5, 9 }.Select((int index, int step) => (byte)(BounceMotion.BounceProfile[index] ^ (49 + step * 7))).ToArray();
	}

	static BounceMotion()
	{
		BounceMotion.BounceProfile = new byte[16]
		{
			0, 18, 37, 58, 72, 65, 48, 29, 12, 4,
			16, 33, 52, 61, 43, 21
		};
	}
}
