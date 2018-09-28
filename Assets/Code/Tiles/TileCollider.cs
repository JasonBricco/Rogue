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

	public void SetInfo(Vector2 size, bool trigger, int cellX, int cellY, Vector2 offset)
	{
		col.size = new Vector3(size.x, size.y, 1.0f);
		col.isTrigger = trigger;

		if (col.size.x < 1.0f || col.size.y < 1.0f)
			offset += new Vector2(0.5f, 0.5f);
		else offset += new Vector2(col.size.x * 0.5f, col.size.y * 0.5f);

		col.transform.position = new Vector2(cellX, cellY) + offset;
		gameObject.layer = trigger ? 14 : 8;
	}
}
