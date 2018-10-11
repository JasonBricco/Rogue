//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class RoomBarrier : MonoBehaviour
{
	public Vec2i dir;
	private BoxCollider barrier;

	private int terrainLayer, triggerLayer;

	private void Awake()
	{
		terrainLayer = LayerMask.NameToLayer("Terrain");
		triggerLayer = LayerMask.NameToLayer("Terrain Trigger");
		barrier = GetComponent<BoxCollider>();
	}

	public void Resize(float x, float y, float width, float height)
	{
		barrier.center = new Vector3(x, y);
		barrier.size = new Vector3(width, height, 10.0f);
	}

	public void Lock()
	{
		barrier.isTrigger = false;
		barrier.gameObject.layer = terrainLayer;
	}

	public void Unlock()
	{
		barrier.isTrigger = true;
		barrier.gameObject.layer = triggerLayer;
	}
}
