//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileCollider : MonoBehaviour
{
	public Tile tile;
	public BoxCollider col;

	public static TileCollider Create(Tile tile)
	{
		GameObject obj = new GameObject("Tile Collider");

		TileCollider tileCollider = obj.AddComponent<TileCollider>();
		tileCollider.col = obj.AddComponent<BoxCollider>();

		Rigidbody rb = obj.AddComponent<Rigidbody>();
		obj.layer = 8;
		rb.isKinematic = true;

		return tileCollider;
	}

	public TileCollider(Tile tile, BoxCollider col)
	{
		this.tile = tile;
		this.col = col;
	}

	public void Enable()
	{
		col.enabled = true;
	}

	public void Disable()
	{
		col.enabled = false;
	}

	public void SetInfo(Vector2 size, bool trigger, Vector2 roomWorldPos, int cellX, int cellY)
	{
		col.size = new Vector3(size.x, size.y, 1.0f);
		col.isTrigger = trigger;
		col.transform.position = roomWorldPos + new Vector2(cellX, cellY) + new Vector2(col.size.x * 0.5f, col.size.y * 0.5f);
	}
}
