//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class RoomRenderer
{
	private Room room;
	private Dictionary<int, SpriteMesh> meshes = new Dictionary<int, SpriteMesh>();

	// True if this chunk currently has meshes built.
	private bool built;

	public RoomRenderer(Room room)
	{
		this.room = room;
	}

	public void Update()
	{
		if (!built)
			BuildMeshes();
	}

	// Builds meshes for the room. Each visible tile will contribute to a mesh. 
	// One mesh will be built for each mesh index used by tiles in the room.
	public void BuildMeshes()
	{
		for (int layer = 0; layer < room.Layers; layer++)
		{
			for (int y = 0; y < room.SizeY; y++)
			{
				for (int x = 0; x < room.SizeX; x++)
				{
					Tile tile = room.GetTile(x, y, layer);
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
			Graphics.DrawMesh(m.Mesh, Matrix4x4.Translate(Vector3.zero), m.Material, 0);
		}
	}

	public void Destroy()
	{
		foreach (SpriteMesh mesh in meshes.Values)
			mesh.Destroy();
	}
}
