//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class RoomBarrier : MonoBehaviour
{
	public Vec2i dir;
	private BoxCollider barrier;

	private void Awake()
	{
		barrier = GetComponent<BoxCollider>();
	}

	public void Resize(float x, float y, float width, float height)
	{
		barrier.center = new Vector3(x, y);
		barrier.size = new Vector3(width, height, 10.0f);
	}
}
