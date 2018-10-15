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
		=> TileManager.Instance.GetProperties(this);

	public TileBehavior Behavior
		=> TileManager.Instance.GetBehavior(id);

	public static implicit operator Tile(TileType id) => new Tile(id);
	public static bool operator ==(Tile a, TileType b) => a.id == b;
	public static bool operator ==(Tile a, Tile b) => a.id == b.id;
	public static bool operator !=(Tile a, Tile b) => a.id != b.id;
	public static bool operator !=(Tile a, TileType b) => a.id != b;

	public bool Equals(Tile other) => id == other.id;
	public override bool Equals(object obj) => Equals((Tile)obj);
	public override int GetHashCode() => id.GetHashCode();
	public override string ToString() => id.ToString();
}
