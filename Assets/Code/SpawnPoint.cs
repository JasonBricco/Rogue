//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public struct SpawnPoint : IEquatable<SpawnPoint>
{
	public Vec2i room, pos;
	public int facing;
	public Vector2 offset;

	public SpawnPoint(Vec2i room, int x, int y, Vector2 offset, int facing)
	{
		this.room = room;
		this.facing = facing;
		this.offset = offset;
		pos = new Vec2i(x, y);
	}

	public bool Equals(SpawnPoint other)
		=> room == other.room && pos == other.pos;
}
