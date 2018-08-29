//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class TextureArrays
{
	public void BuildTextureArray(List<TileData> data, Material material, int w, int h)
	{
		if (data.Count == 0) return;

		Texture2DArray texArray = new Texture2DArray(w, h, data.Count, TextureFormat.RGBA32, false);
		texArray.filterMode = FilterMode.Point;

		for (int i = 0; i < data.Count; i++)
		{
			Texture2D tex = data[i].sprite.texture;
			Graphics.CopyTexture(tex, 0, 0, texArray, i, 0);
		}

		material.SetTexture("_TexArray", texArray);
	}
}
