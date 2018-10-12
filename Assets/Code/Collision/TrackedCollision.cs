//
// Copyright (c) 2018 Jason Bricco
//

using System;
using UnityEngine.Assertions;

public struct TrackedCollision : IEquatable<TrackedCollision>
{
	public Entity a, b;
	public TileInstance inst;
	public int layerA, layerB, tileLayer;
	public int count;

	public TrackedCollision(Entity a, int layerA, Entity b, int layerB, TileInstance inst, int tileLayer, int count = 0)
	{
		this.a = a;
		this.b = b;
		this.inst = inst;
		this.layerA = layerA;
		this.layerB = layerB;
		this.tileLayer = tileLayer;
		this.count = count;
	}

	public TrackedCollision Increment()
		=> new TrackedCollision(a, layerA, b, layerB, inst, tileLayer, count + 1);

	public TrackedCollision Decrement(out bool destroy)
	{
		Assert.IsTrue(count > 0);
		int newCount = count - 1;
		destroy = newCount == 0;
		return new TrackedCollision(a, layerA, b, layerB, inst, tileLayer, newCount);
	}

	public bool Involves(Entity entity)
		=> a == entity || b == entity;

	public bool Equals(TrackedCollision other)
		=> a == other.a && b == other.b && inst == other.inst;
}
