﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileComponent : MonoBehaviour
{
	[SerializeField] private bool enableOnSet;
	[SerializeField] private GameObject lightPrefab;

	public GameObject LightPrefab
	{
		get { return lightPrefab; }
	}

	public void OnSet(Room room, int x, int y)
	{
		if (!enableOnSet) return;

		ITileSet set = GetComponent<ITileSet>();
		set.OnSet(room, x, y, this);
	}
}
