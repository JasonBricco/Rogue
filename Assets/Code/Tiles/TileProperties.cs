//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

[Serializable]
public sealed class TileProperties
{
	// Filled in by the tile data editor.
	public bool invisible, hasCollider, trigger;
	public Vector2 colliderSize, colliderOffset;
	public Vector3 renderOffset;
	public Material baseMaterial;
	public Sprite sprite;
	public Color32 color;
	public TileComponent component;

	// Computed during runtime.
	public float spriteIndex;
	public int index;
	public int width, height;
	public Material material;
}
