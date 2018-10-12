//
// Copyright (c) 2018 Jason Bricco
//

using System.Collections.Generic;
using UnityEngine;

public sealed class ColliderPool
{
	private Queue<TileCollider> pool = new Queue<TileCollider>(64);
	private Transform parent;

	public ColliderPool(Transform parent)
		=> this.parent = parent;

	// Gets a new collider from the main collider pool and stores it in the given collider queue.
	// This queue can later be used to return the colliders back to the main pool.
	public TileCollider GetCollider(Tile tile, int x, int y, Queue<TileCollider> colliders)
	{
		TileCollider col;

		if (pool.Count > 0)
		{
			col = pool.Dequeue();
			col.Enable();
		}
		else
		{
			col = TileCollider.Create();
			col.transform.SetParent(parent);
		}

		col.inst = new TileInstance(tile, x, y);
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
			collider.scoreModifier = 0;
			collider.Disable();
			pool.Enqueue(collider);
		}
	}
}
