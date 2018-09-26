//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static UnityEngine.Mathf;

public static class Utils
{
	public static float Square(float v)
	{
		return v * v;
	}

	public static int Square(int v)
	{
		return v * v;
	}

	public static int GetNumericDir(Vec2i dir)
	{
		if (Abs(dir.x) > Abs(dir.y))
			return dir.x > 0 ? Direction.Right : Direction.Left;

		return dir.y > 0 ? Direction.Front : Direction.Back;
	}

	public static int GetNumericDir(Vector2 dir)
	{
		if (Abs(dir.x) > Abs(dir.y))
			return dir.x > 0.0f ? Direction.Right : Direction.Left;

		return dir.y > 0.0f ? Direction.Front : Direction.Back;
	}

	public static int GetNumericDirFull(Vec2i dir)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Vec2i.Directions[i] == dir)
				return i;
		}

		return -1;
	}

	public static int FloorToNearestMultiple(int value, int multipleOf)
	{
		return (value / multipleOf) * multipleOf;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T temp = a;
		a = b;
		b = temp;
	}
}
