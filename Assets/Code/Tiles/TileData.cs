//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

[Serializable]
public sealed class TileData : IComparable<TileData>
{
	public string name;
	public TileType type;
	public bool invisible, hasCollider, trigger;
	public Vector2 colliderSize, colliderOffset;
	public Vector2 align;
	public Sprite sprite;
	public Material material;
	public Color32 color;
	public TileComponent component;

	public float spriteIndex;
	public int index;
	public int width, height;

	public int CompareTo(TileData other)
	{
		return type.CompareTo(other.type);
	}
}
