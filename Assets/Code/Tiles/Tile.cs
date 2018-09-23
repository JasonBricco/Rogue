//
// Copyright (c) 2018 Jason Bricco
//

using System;

public struct Tile : IEquatable<Tile>
{
	public TileType id;
	public ushort variant;

	public Tile(TileType id)
	{
		this.id = id;
		variant = 0;
	}

	public Tile(TileType id, int variant)
	{
		this.id = id;
		this.variant = (ushort)variant;
	}

	// Returns the tile data for this tile. This contains all information about
	// this particular tile.
	public TileProperties Properties
	{
		get { return TileManager.Instance.GetProperties(this); }
	}

	public static implicit operator Tile(TileType id)
	{
		return new Tile(id);
	}

	public static bool operator ==(Tile a, TileType b)
	{
		return a.id == b;
	}

	public static bool operator ==(Tile a, Tile b)
	{
		return a.id == b.id;
	}

	public static bool operator !=(Tile a, Tile b)
	{
		return a.id != b.id;
	}

	public static bool operator !=(Tile a, TileType b)
	{
		return a.id != b;
	}

	public bool Equals(Tile other)
	{
		return id == other.id;
	}

	public override bool Equals(object obj)
	{
		return Equals((Tile)obj);
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}
}
