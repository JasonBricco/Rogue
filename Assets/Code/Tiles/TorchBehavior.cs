//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TorchBehavior : TileBehavior
{
	public override void OnSet(Room room, int x, int y, Tile tile)
	{
		GameObject lightPrefab = tile.Properties.light;
		GameObject obj = ObjectPool.Get(lightPrefab);
		obj.transform.position = new Vector3(x + 0.5f, y + 0.5f);
	}
}
