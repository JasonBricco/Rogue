//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public struct SpriteArrayInfo : IEquatable<SpriteArrayInfo>
{
	public Material material;
	public int w, h;

	public SpriteArrayInfo(Material material, int w, int h)
	{
		this.material = material;
		this.w = w;
		this.h = h;
	}

	public bool Equals(SpriteArrayInfo other)
	{
		return material == other.material && w == other.w && h == other.h;
	}
}
