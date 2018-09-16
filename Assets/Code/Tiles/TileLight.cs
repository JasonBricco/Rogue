//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileLight : MonoBehaviour, ITileSet
{
	public void OnSet(Vec2i tPos, TileComponent tc)
	{
		Instantiate(tc.LightPrefab, new Vector3(tPos.x + 0.5f, tPos.y + 0.5f), Quaternion.identity);
	}
}
