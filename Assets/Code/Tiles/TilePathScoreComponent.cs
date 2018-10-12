//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TilePathScoreComponent : MonoBehaviour, IColliderSet
{
	[SerializeField] private int scoreModifier;

	public void OnCollider(TileCollider col)
	{
		col.scoreModifier = scoreModifier;
	}
}
