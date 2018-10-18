//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine.Assertions;
using System.Collections.Generic;

public sealed class Room
{
	public int SizeX { get; private set; }
	public int SizeY { get; private set; }

	public int HalfX { get; private set; }
	public int HalfY { get; private set; }

	public int LimX { get; private set; }
	public int LimY { get; private set; }

	public const int Layers = 3;
	public const int Back = 0, Main = 1, Front = 2;

	public Vec2i Pos { get; private set; }

	private Tile[] tiles;

	public int TileCount => tiles.Length;

	private List<TileInstance> onTileEvent = new List<TileInstance>();

	public RoomCollision Collision { get; private set; }
	public RoomRenderer Renderer { get; private set; }
	public RoomEntities Entities { get; private set; }
	public RoomPathfinding Pathfinding { get; private set; }

	public RoomType Type { get; private set; }

	public Room(Vec2i pos)
	{
		Collision = new RoomCollision(this);
		Renderer = new RoomRenderer(this);
		Entities = new RoomEntities(this);
		Pathfinding = new RoomPathfinding(this);

		Pos = pos;
	}

	public void Init(Vec2i pos, int sizeX, int sizeY, RoomType type)
	{
		Type = type;

		SizeX = sizeX;
		SizeY = sizeY;

		HalfX = sizeX / 2;
		HalfY = sizeY / 2;

		LimX = sizeX - 1;
		LimY = sizeY - 1;

		tiles = new Tile[sizeX * sizeY * Layers];
	}

	public void ListenForEvent(int x, int y, Tile tile)
		=> onTileEvent.Add(new TileInstance(tile, Pos, x, y));

	// Returns a tile at the given location from this room. Fails if the location is out of bounds of the room.
	// Coordinates are specified in local room space between 0 and room size - 1.
	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	public Tile GetTile(int x, int y, int layer)
	{
		Assert.IsTrue(InBounds(x, y));
		return tiles[x + SizeX * (y + SizeY * layer)];
	}

	// Returns a tile from the main layer at the given location from this room. Fails if the location is out of 
	// bounds of the room. Coordinates are specified in local room space between 0 and room size - 1. 
	public Tile GetTile(int x, int y) => GetTile(x, y, Main);

	public Tile GetTile(int i) => tiles[i];

	// Sets the given tile at the given location in this room. Fails if the location is out of bounds
	// of the room. Coordinates are specified in local room space between 0 and room size - 1.
	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	public void SetTile(int x, int y, int layer, Tile tile)
	{
		Assert.IsTrue(InBounds(x, y));
		tiles[x + SizeX * (y + SizeY * layer)] = tile;
		tile.Behavior?.OnSet(this, x, y, tile);
	}

	public void SetTile(Vec2i p, int layer, Tile tile)
		=> SetTile(p.x, p.y, layer, tile);

	public void SetVariant(int x, int y, int layer, int variant)
		=> tiles[x + SizeX * (y + SizeY * layer)].variant = (ushort)variant;

	// Replaces every tile in the given layer with the given tile.
	public void Fill(int layer, Tile tile)
	{
		for (int y = 0; y < SizeY; y++)
		{
			for (int x = 0; x < SizeX; x++)
				SetTile(x, y, layer, tile);
		}
	}

	public void OnGenerate()
	{
		Collision.Generate();
		Pathfinding.Generate();
	}

	public void Update()
	{
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
	}

	// Disables the room so it can exist in memory but not be the active room.
	public void Disable()
	{
		Entities.Disable();
		Collision.Disable();
	}

	// Destroys the room, freeing it from memory.
	public void Destroy()
	{
		Entities.Destroy();
		Renderer.Destroy();
		Collision.RemoveColliders();
	}

	public void TriggerEvent(TileEvent e)
	{
		for (int i = 0; i < onTileEvent.Count; i++)
		{
			TileInstance inst = onTileEvent[i];
			inst.tile.Behavior?.OnEvent(this, inst.x, inst.y, e);
		}
	}

	// Returns true if the given coordinates are within the boundaries of this room.
	// Coordinates are specified in local room space between 0 and room size - 1.
	public bool InBounds(int x, int y) 
		=> x >= 0 && x < SizeX && y >= 0 && y < SizeY;

	public bool InBounds(Vec2i p) => InBounds(p.x, p.y);
}
