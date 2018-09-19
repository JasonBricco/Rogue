//
// Copyright (c) 2018 Jason Bricco
//

using System;
using UnityEngine.Assertions;

public struct TrackedCollision : IEquatable<TrackedCollision>
{
	public Entity a, b;
	public Tile tile;
	public int layerA, layerB, tileLayer;
	public int count;

	public TrackedCollision(Entity a, int layerA, Entity b, int layerB, Tile tile, int tileLayer, int count = 0)
	{
		this.a = a;
		this.b = b;
		this.tile = tile;
		this.layerA = layerA;
		this.layerB = layerB;
		this.tileLayer = tileLayer;
		this.count = count;
	}

	public TrackedCollision Increment()
	{
		return new TrackedCollision(a, layerA, b, layerB, tile, tileLayer, count + 1);
	}

	public TrackedCollision Decrement(out bool destroy)
	{
		Assert.IsTrue(count > 0);
		int newCount = count - 1;
		destroy = newCount == 0;
		return new TrackedCollision(a, layerA, b, layerB, tile, tileLayer, newCount);
	}

	public bool Involves(Entity entity)
	{
		return a == entity || b == entity;
	}

	public bool Equals(TrackedCollision other)
	{
		return a == other.a && b == other.b && tile == other.tile;
	}
}
