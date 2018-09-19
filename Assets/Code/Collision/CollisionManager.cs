//
// Copyright (c) 2018 Jason Bricco
//

using System.Collections.Generic;
using UnityEngine;

public sealed class TileCollision
{
	private Queue<TileCollider> pool = new Queue<TileCollider>(64);
	private Transform parent;

	public TileCollision(Transform parent)
	{
		this.parent = parent;
	}

	// Gets a new collider from the main collider pool and stores it in the given collider queue.
	// This queue can later be used to return the colliders back to the main pool.
	private TileCollider GetCollider(Tile tile, Queue<TileCollider> colliders)
	{
		TileCollider col;

		if (pool.Count > 0)
		{
			col = pool.Dequeue();
			col.Enable();
		}
		else
		{
			col = TileCollider.Create(tile);
			col.transform.SetParent(parent);
		}

		col.tile = tile;
		colliders.Enqueue(col);
		return col;
	}

	// All colliders in the given queue of colliders will be returned to the main collider pool.
	// In addition, they'll be reset in size and disabled.
	public void ReturnColliders(Queue<TileCollider> colliders)
	{
		while (colliders.Count > 0)
		{
			TileCollider collider = colliders.Dequeue();
			collider.Disable();
			pool.Enqueue(collider);
		}
	}

	// Generates colliders for the given room. Only main layer tiles are considered.
	public void Generate(Room room, Queue<TileCollider> colliders)
	{
		for (int y = 0; y < Room.SizeY; y++)
		{
			for (int x = 0; x < Room.SizeX; x++)
			{
				Tile tile = room.GetTile(x, y);
				TileData data = tile.Data;

				if (data.hasCollider)
				{
					TileCollider col = GetCollider(tile, colliders);
					Vector2 size = data.colliderSize;
					col.SetInfo(size, data.trigger, room.WorldPos, x, y, data.colliderOffset);
				}
			}
		}
	}
}
