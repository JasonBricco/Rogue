//
// Copyright (c) 2018 Jason Bricco
//

using System;

public struct Tile : IEquatable<Tile>
{
	public TileType id;

	public Tile(TileType id)
	{
		this.id = id;
	}

	public Tile(TileType id, int offset)
	{
		this.id = id + offset;
	}

	/// <summary>
	/// Returns the tile data for this tile. This contains all information about
	/// this particular tile.
	/// </summary>
	public TileData Data
	{
		get { return TileManager.Instance.GetData(id); }
	}

	public static implicit operator Tile(TileType id)
	{
		return new Tile(id);
	}

	public static bool operator ==(Tile a, TileType b)
	{
		return a.id == b;
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
