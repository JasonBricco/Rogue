//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TilePathScore : MonoBehaviour, IColliderSet
{
	[SerializeField] private int scoreModifier;

	public void OnCollider(TileCollider col)
	{
		col.scoreModifier = scoreModifier;
	}
}
