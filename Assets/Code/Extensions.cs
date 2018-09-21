//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public static class Extensions
{
	public static Color SetAlpha(this Color col, float alpha)
	{
		return new Color(col.r, col.g, col.b, alpha);
	}
}
