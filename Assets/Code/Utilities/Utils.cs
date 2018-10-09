//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

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

	public static Vec2i TilePos(float x, float y)
	{
		return new Vec2i(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
	}

	public static Vec2i TilePos(Vector2 p)
	{
		return TilePos(p.x, p.y);
	}

	public static int GetNumericDir(Vec2i dir)
	{
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			return dir.x > 0 ? Direction.Right : Direction.Left;

		return dir.y > 0 ? Direction.Front : Direction.Back;
	}

	public static int GetNumericDir(Vector2 dir)
	{
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			return dir.x > 0.0f ? Direction.Right : Direction.Left;

		return dir.y > 0.0f ? Direction.Front : Direction.Back;
	}

	// When Abs(dir.x) and Abs(dir.y) are the same, biases the result
	// to the x-axis for stability. Otherwise, the result will rapidly
	// change between the two axes.
	public static int GetStableNumericDir(Vector2 dir)
	{
		return GetNumericDir(new Vector2(dir.x, dir.y - 0.1f));
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
