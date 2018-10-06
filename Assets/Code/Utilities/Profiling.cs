//
// Copyright (c) 2018 Jason Bricco
//

using System.Diagnostics;
using Debug = UnityEngine.Debug;

public enum ProfileSection
{
	General,
	Count
}

public static class Profiling
{
	private static Stopwatch[] watches = new Stopwatch[(int)ProfileSection.Count];

	public static void BeginTiming(ProfileSection section)
	{
		Stopwatch watch = new Stopwatch();
		watches[(int)section] = watch;
		watch.Start();
	}

	public static void EndTiming(ProfileSection section)
	{
		Stopwatch watch = watches[(int)section];
		watch.Stop();
		Debug.Log(section + ": " + watch.ElapsedTicks);
	}
}
