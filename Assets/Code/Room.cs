//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static Utils;

public sealed class Room
{
	public const int SizeX = 32, SizeY = 18;
	public const int HalfSizeX = SizeX / 2, HalfSizeY = SizeY / 2;
	public const int LimX = SizeX - 1, LimY = SizeY - 1;
	public const int ShiftX = 5, MaskX = SizeX - 1;

	public Vec2i Pos { get; private set; }
	public Vector2 WorldPos { get; private set; }

	private int layers, mainLayer;
	private Tile[] tiles;

	private Dictionary<int, SpriteMesh> meshes = new Dictionary<int, SpriteMesh>();

	// True if this chunk currently has meshes built.
	public bool built;

	private List<Entity> entities = new List<Entity>();

	public Room(int pX, int pY, int layers, int mainLayer)
	{
		tiles = new Tile[SizeX * SizeY * layers];
		this.layers = layers;
		this.mainLayer = mainLayer;

		Pos = new Vec2i(pX, pY);
		WorldPos = new Vector2(pX * SizeX, pY * SizeY);
	}

	/// <summary>
	/// Returns a tile at the given location from this room. Fails if the location is out of bounds of the room.
	/// Coordinates are specified in local room space between 0 and room size - 1. 
	/// </summary>
	public Tile GetTile(int x, int y, int layer)
	{
		Assert.IsTrue(InBounds(x, y));
		return tiles[x + SizeX * (y + SizeY * layer)];
	}

	/// <summary>
	/// Returns a tile from the main layer at the given location from this room. Fails if the location is out of 
	/// bounds of the room. Coordinates are specified in local room space between 0 and room size - 1. 
	/// </summary>
	public Tile GetTile(int x, int y)
	{
		return GetTile(x, y, mainLayer);
	}

	/// <summary>
	/// Sets the given tile at the given location in this room. Fails if the location is out of bounds
	/// of the room. Coordinates are specified in local room space between 0 and room size - 1.
	/// </summary>
	public void SetTile(int x, int y, int layer, Tile tile)
	{
		Assert.IsTrue(InBounds(x, y));
		tiles[x + SizeX * (y + SizeY * layer)] = tile;
	}

	/// <summary>
	/// Replaces every tile in the given layer with the given tile.
	/// </summary>
	public void Fill(int layer, Tile tile)
	{
		for (int y = 0; y < SizeY; y++)
		{
			for (int x = 0; x < SizeX; x++)
				SetTile(x, y, layer, tile);
		}
	}

	public void GetCollisionData(Entity entity, Vec2i cell, ref CollideResult result)
	{
		result.tile = GetTile(cell.x, cell.y, mainLayer);

		for (int i = 0; i < entities.Count; i++)
		{
			Entity target = entities[i];

			if (entity == target) continue;

			if (ToLocalPos(target.TilePos) == cell || ToLocalPos(TilePos(target.end)) == cell)
				result.entity = target;
		}

		Vec2i diff = Pos - ToRoomPos(Camera.main.transform.position);

		if (Abs(diff.x) > 1 || Abs(diff.y) > 1)
			result.unloaded = true;
	}

	/// <summary>
	/// Builds meshes for the room. Each visible tile will contribute to a mesh. 
	/// One mesh will be built for each mesh index used by tiles in the room.
	/// </summary>
	public void BuildMeshes()
	{
		for (int layer = 0; layer < layers; layer++)
		{
			for (int y = 0; y < SizeY; y++)
			{
				for (int x = 0; x < SizeX; x++)
				{
					Tile tile = GetTile(x, y, layer);
					TileData data = tile.Data;

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

	/// <summary>
	/// Draw all meshes comprising this room.
	/// </summary>
	public void Draw()
	{
		foreach (KeyValuePair<int, SpriteMesh> pair in meshes)
		{
			SpriteMesh m = pair.Value;
			Graphics.DrawMesh(m.Mesh, Matrix4x4.Translate(WorldPos), m.Material, 0);
		}
	}

	/// <summary>
	/// Adds the given entity to this room.
	/// </summary>
	public void AddEntity(Entity entity)
	{
		Assert.IsTrue(!entities.Contains(entity));
		entities.Add(entity);
	}

	/// <summary>
	/// Removes the entity from this room.
	/// </summary>
	public void RemoveEntity(Entity entity)
	{
		bool result = entities.Remove(entity);
		Assert.IsTrue(result);
	}

	/// <summary>
	/// Adds all entities in this room to the given active entities list.
	/// </summary>
	public void GetActiveEntities(List<Entity> activeEntities)
	{
		activeEntities.AddRange(entities);
	}

	/// <summary>
	/// Destroys all meshes comprising this room.
	/// </summary>
	public void Destroy()
	{
		foreach (SpriteMesh mesh in meshes.Values)
			mesh.Destroy();
	}

	/// <summary>
	/// Returns true if the given coordinates are within the boundaries of this room.
	/// Coordinates are specified in local room space between 0 and room size - 1.
	/// </summary>
	public static bool InBounds(int x, int y)
	{
		return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
	}
}
