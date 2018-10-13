//
// Copyright (c) 2018 Jason Bricco
//

using System;

public struct TileInstance : IEquatable<TileInstance>
{
	public Vec2i room;
	public int x, y;
	public Tile tile;

	public TileInstance(Tile tile, Vec2i room, int x, int y)
	{
		this.room = room;
		this.x = x;
		this.y = y;
		this.tile = tile;
	}

	public bool Equals(TileInstance other)
	{
		return room == other.room && x == other.x && y == other.y && tile == other.tile;
	}

	public override bool Equals(object obj)
	{
		return Equals((TileInstance)obj);
	}

	public static bool operator ==(TileInstance a, TileInstance b) => a.Equals(b);
	public static bool operator !=(TileInstance a, TileInstance b) => !a.Equals(b);

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + x.GetHashCode();
		hash = hash * 31 + y.GetHashCode();
		hash = hash * 31 + room.GetHashCode();
		return hash;
	}

	public override string ToString()
	{
		return "Room: " + room.ToString() + ", Tile: " + tile.ToString();
	}
}
