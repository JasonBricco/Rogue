//
// Copyright (c) 2018 Jason Bricco
//

public struct Tile
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
}
