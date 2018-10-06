//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TilePathScore : MonoBehaviour
{
	[SerializeField] private int scoreModifier;

	public int ScoreModifier => scoreModifier;
}
