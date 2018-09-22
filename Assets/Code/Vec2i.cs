//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public struct Vec2i : IEquatable<Vec2i>
{
	public int x, y;

	public static readonly Vec2i MaxValue = new Vec2i(int.MaxValue, int.MaxValue);

	public static readonly Vec2i[] Directions =
	{
		new Vec2i(0, -1), new Vec2i(0, 1), new Vec2i(-1, 0), new Vec2i(1, 0),
		new Vec2i(-1, 1), new Vec2i(1, 1), new Vec2i(-1, -1), new Vec2i(1, -1)
	};

	public static readonly Vec2i Zero = new Vec2i(0, 0);

	public Vec2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vec2i(Vector3 p)
	{
		x = (int)p.x;
		y = (int)p.y;
	}

	// The squared length of the vector.
	public int LengthSq
	{
		get { return Dot(this, this); }
	}

	// Convert this vector to a floating-point Vector2.
	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	// Convert this vector to a floating-point Vector3.
	public Vector3 ToVector3()
	{
		return new Vector3(x, y);
	}

	// Returns the absolute value of this vector.
	public Vec2i Abs()
	{
		return new Vec2i(Mathf.Abs(x), Mathf.Abs(y));
	}

	public bool Equals(Vec2i other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		return Equals((Vec2i)obj);
	}

	public override int GetHashCode()
	{
		return 29 * x + 17 * y;
	}

	// Returns the dot product between the two given vectors.
	public static int Dot(Vec2i a, Vec2i b)
	{
		Vec2i tmp = a * b;
		return tmp.x + tmp.y;
	}

	public static bool operator ==(Vec2i a, Vec2i b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(Vec2i a, Vec2i b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public static Vec2i operator +(Vec2i a, Vec2i b)
	{
		return new Vec2i(a.x + b.x, a.y + b.y);
	}

	public static Vec2i operator -(Vec2i a, Vec2i b)
	{
		return new Vec2i(a.x - b.x, a.y - b.y);
	}

	public static Vec2i operator *(Vec2i a, Vec2i b)
	{
		return new Vec2i(a.x * b.x, a.y * b.y);
	}

	public static Vec2i operator *(Vec2i a, int v)
	{
		return new Vec2i(a.x * v, a.y * v);
	}

	public static Vec2i operator -(Vec2i v)
	{
		return new Vec2i(-v.x, -v.y);
	}
}
