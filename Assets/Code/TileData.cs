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
	public bool invisible;
	public Vector2 align;
	public Sprite sprite;
	public Material material;
	public Color32 color;
	public Layer layer;

	[HideInInspector] public float spriteIndex;
	[HideInInspector] public int index;
	[HideInInspector] public int width, height;

	public int CompareTo(TileData other)
	{
		return type.CompareTo(other.type);
	}
}
