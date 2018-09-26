//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
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

	public Vec2i Pos { get; private set; }
	public Vector2 WorldPos { get; private set; }

	private int layers, mainLayer;
	private Tile[] tiles;

	// Stores box colliders being used by this chunk. These colliders come from the collision manager.
	// We store them here so that we can return them when the chunk doesn't need them anymore.
	private Queue<TileCollider> colliders = new Queue<TileCollider>(16);

	private Dictionary<int, SpriteMesh> meshes = new Dictionary<int, SpriteMesh>();

	// True if this chunk currently has meshes built.
	public bool built;

	private bool hasColliders;

	private List<Entity> entities = new List<Entity>();

	public Room(int pX, int pY, int layers, int mainLayer, int sizeX, int sizeY)
	{
		SizeX = sizeX;
		SizeY = sizeY;

		HalfX = sizeX / 2;
		HalfY = sizeY / 2;

		LimX = sizeX - 1;
		LimY = sizeY - 1;

		tiles = new Tile[sizeX * sizeY * layers];
		this.layers = layers;
		this.mainLayer = mainLayer;

		Pos = new Vec2i(pX, pY);
		WorldPos = new Vector2(pX * SizeX, pY * SizeY);
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
		return GetTile(x, y, mainLayer);
	}

	// Sets the given tile at the given location in this room. Fails if the location is out of bounds
	// of the room. Coordinates are specified in local room space between 0 and room size - 1.
	public void SetTile(int x, int y, int layer, Tile tile)
	{
		Assert.IsTrue(InBounds(x, y));
		tiles[x + SizeX * (y + SizeY * layer)] = tile;

		TileProperties data = tile.Properties;
		data.component?.OnSet(x, y);
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

	// Builds meshes for the room. Each visible tile will contribute to a mesh. 
	// One mesh will be built for each mesh index used by tiles in the room.
	public void BuildMeshes()
	{
		for (int layer = 0; layer < layers; layer++)
		{
			for (int y = 0; y < SizeY; y++)
			{
				for (int x = 0; x < SizeX; x++)
				{
					Tile tile = GetTile(x, y, layer);
					TileProperties data = tile.Properties;

					if (!data.invisible)
					{
						SpriteMesh mesh;
						if (!meshes.TryGetValue(data.index, out mesh))
						{
							mesh = new SpriteMesh(data.material);
							meshes.Add(data.index, mesh);
						}

						mesh.AddRect(x, y, layer, data.width, data.height, data.spriteIndex, data.align, data.color);
					}
				}
			}
		}

		foreach (SpriteMesh mesh in meshes.Values)
			mesh.BuildMesh();

		built = true;
	}

	// Draw all meshes comprising this room.
	public void Draw()
	{
		foreach (KeyValuePair<int, SpriteMesh> pair in meshes)
		{
			SpriteMesh m = pair.Value;
			Graphics.DrawMesh(m.Mesh, Matrix4x4.Translate(WorldPos), m.Material, 0);
		}
	}

	// Adds the given entity to this room.
	public void AddEntity(Entity entity)
	{
		Assert.IsTrue(!entities.Contains(entity));
		entities.Add(entity);
	}

	// Removes the entity from this room.
	public void RemoveEntity(Entity entity)
	{
		bool result = entities.Remove(entity);
		Assert.IsTrue(result);
	}

	// Adds all entities in this room to the given active entities list.
	public void GetActiveEntities(List<Entity> activeEntities)
	{
		for (int i = 0; i < entities.Count; i++)
			activeEntities.Add(entities[i]);
	}

	// Adds colliders for all tiles that require them in this room.
	public void GenerateColliders(TileCollision collision)
	{
		if (!hasColliders)
		{
			collision.Generate(this, colliders);
			hasColliders = true;
		}
	}

	// Removes all colliders for this room.
	public void RemoveColliders(TileCollision collision)
	{
		if (hasColliders)
		{
			collision.ReturnColliders(colliders);
			hasColliders = false;
		}
	}

	// Destroys all meshes comprising this room.
	public void Destroy(TileCollision collision)
	{
		foreach (SpriteMesh mesh in meshes.Values)
			mesh.Destroy();

		RemoveColliders(collision);

		// TODO: Save to disk.
	}

	// Returns true if the given coordinates are within the boundaries of this room.
	// Coordinates are specified in local room space between 0 and room size - 1.
	public bool InBounds(int x, int y)
	{
		return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
	}
}
