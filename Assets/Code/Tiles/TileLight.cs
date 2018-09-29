//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileLight : MonoBehaviour, ITileSet
{
	[SerializeField] private GameObject lightPrefab;

	public void OnSet(Room room, int x, int y, TileComponent tc)
	{
		Instantiate(lightPrefab, new Vector3(x + 0.5f, y + 0.5f), Quaternion.identity);
	}
}
