//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

public class TileSpawnSetter : MonoBehaviour, ITileSet
{
	[SerializeField] private Vec2i facing;

	public void OnSet(Room room, int x, int y, TileComponent tc)
	{
		Vec2i p = new Vec2i(x, y) + facing;
		room.Spawn = new SpawnPoint(p.x, p.y, GetNumericDir(facing));
	}
}
