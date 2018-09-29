//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine.Assertions;

public sealed class Room
{
	public int SizeX { get; private set; }
	public int SizeY { get; private set; }

	public int HalfX { get; private set; }
	public int HalfY { get; private set; }

	public int LimX { get; private set; }
	public int LimY { get; private set; }

	public int Layers { get; private set; }
	public int MainLayer { get; private set; }

	public Vec2i Pos { get; private set; }

	public bool cameraFollow;

	private Tile[] tiles;

	public SpawnPoint Spawn { get; set; }

	public RoomCollision Collision { get; private set; }
	public RoomRenderer Renderer { get; private set; }
	public RoomEntities Entities { get; private set; }

	private bool disabled;

	public Room(Vec2i pos)
	{
		Collision = new RoomCollision(this);
		Renderer = new RoomRenderer(this);
		Entities = new RoomEntities(this);

		Pos = pos;
	}

	public void Init(Vec2i pos, int layers, int mainLayer, int sizeX, int sizeY)
	{
		SizeX = sizeX;
		SizeY = sizeY;

		HalfX = sizeX / 2;
		HalfY = sizeY / 2;

		LimX = sizeX - 1;
		LimY = sizeY - 1;

		tiles = new Tile[sizeX * sizeY * layers];
		Layers = layers;
		MainLayer = mainLayer;
	}

	// Returns a tile at the given location from this room. Fails if the location is out of bounds of the room.
	// Coordinates are specified in local room space between 0 and room size - 1. 
	public Tile GetTile(int x, int y, int layer)
	{
		Assert.IsTrue(InBounds(x, y));
		return tiles[x + SizeX * (y + SizeY * layer)];
	}

	// Returns a tile from the main layer at the given location from this room. Fails if the location is out of 
	// bounds of the room. Coordinates are specified in local room space between 0 and room size - 1. 
	public Tile GetTile(int x, int y)
	{
		return GetTile(x, y, MainLayer);
	}

	// Sets the given tile at the given location in this room. Fails if the location is out of bounds
	// of the room. Coordinates are specified in local room space between 0 and room size - 1.
	public void SetTile(int x, int y, int layer, Tile tile)
	{
		Assert.IsTrue(InBounds(x, y));
		tiles[x + SizeX * (y + SizeY * layer)] = tile;

		TileProperties data = tile.Properties;
		data.component?.OnSet(this, x, y);
	}

	// Replaces every tile in the given layer with the given tile.
	public void Fill(int layer, Tile tile)
	{
		for (int y = 0; y < SizeY; y++)
		{
			for (int x = 0; x < SizeX; x++)
				SetTile(x, y, layer, tile);
		}
	}

	public void Update()
	{
		Assert.IsFalse(disabled);
		Collision.Update();
		Entities.Update();
		Renderer.Update();
		Renderer.Draw();
	}

	// Enables the room so it can be used as the active room.
	public void Enable()
	{
		Collision.Enable();
		Entities.Enable();
		disabled = false;
	}

	// Disables the room so it can exist in memory but not be the active room.
	public void Disable()
	{
		Entities.Disable();
		Collision.Disable();
		disabled = true;
	}

	// Destroys the room, freeing it from memory.
	public void Destroy()
	{
		Renderer.Destroy();
		Collision.RemoveColliders();
	}

	// Returns true if the given coordinates are within the boundaries of this room.
	// Coordinates are specified in local room space between 0 and room size - 1.
	public bool InBounds(int x, int y)
	{
		return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
	}
}
