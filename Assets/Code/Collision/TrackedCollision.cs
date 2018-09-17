//
// Copyright (c) 2018 Jason Bricco
//

using System;

public struct TrackedCollision : IEquatable<TrackedCollision>
{
	public Entity a;
	public Entity b;
	public Tile tile;
	private int count;

	public TrackedCollision(Entity a, Entity b, Tile tile)
	{
		this.a = a;
		this.b = b;
		this.tile = tile;
		count = 0;
	}

	public void Increment()
	{
		count++;
	}

	public bool Decrement()
	{
		return --count == 0;
	}

	public bool Equals(TrackedCollision other)
	{
		return a == other.a && b == other.b && tile == other.tile;
	}
}
