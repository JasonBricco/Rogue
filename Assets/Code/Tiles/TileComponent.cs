//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileComponent : MonoBehaviour
{
	[SerializeField] private bool enableOnSet;
	[SerializeField] private bool enableOnCollider;

	public void OnSet(Room room, int x, int y)
	{
		if (!enableOnSet) return;

		ITileSet set = GetComponent<ITileSet>();
		set.OnSet(room, x, y, this);
	}

	public void OnCollider(TileCollider col)
	{
		if (!enableOnCollider) return;

		IColliderSet set = GetComponent<IColliderSet>();
		set.OnCollider(col);
	}
}
