//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

[Serializable]
public sealed class TileData : IComparable<TileData>
{
	// Filled in by the tile data editor.
	public string name;
	public TileType type;
	public bool invisible, hasCollider, trigger;
	public Vector2 colliderSize, colliderOffset;
	public Vector2 align;
	public Material baseMaterial;
	public Sprite sprite;
	public Color32 color;
	public TileComponent component;

	// Computed during runtime.
	public float spriteIndex;
	public int index;
	public int width, height;
	public Material material;

	public int CompareTo(TileData other)
	{
		return type.CompareTo(other.type);
	}

	public override int GetHashCode()
	{
		return type.GetHashCode();
	}
}
