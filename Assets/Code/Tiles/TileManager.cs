//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class TileManager : MonoBehaviour
{
	[SerializeField] private TileDataList dataList;

	public static TileManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		InitTiles();
	}

	public TileProperties GetProperties(Tile tile)
	{
		return dataList.GetProperties(tile);
	}

	// Takes information provided in the inspector and stored in the tile group array and assigns the
	// appropriate tile data into the TileData arrays. This allows the code to access tile information.
	private void InitTiles()
	{
		var arrayData = new Dictionary<SpriteArrayInfo, List<TileProperties>>();

		for (int i = 0; i < dataList.Count; i++)
		{
			TileData data = dataList[i];

			for (int v = 0; v < data.variantCount; v++)
			{
				TileProperties props = data.GetProperties(v);

				if (props.invisible)
					continue;

				// Duplicate the material so tile groups that utilize the same material don't share it 
				// and overwrite the assigned texture information.
				props.material = new Material(props.baseMaterial);

				Sprite sprite = props.sprite;

				props.width = sprite.texture.width;
				props.height = sprite.texture.height;

				SpriteArrayInfo info = new SpriteArrayInfo(props.material, props.width, props.height);

				List<TileProperties> list;
				if (!arrayData.TryGetValue(info, out list))
				{
					list = new List<TileProperties>();
					arrayData[info] = list;
				}

				list.Add(props);
			}
		}

		TextureArrays arrays = new TextureArrays();
		int index = 0;

		foreach (KeyValuePair<SpriteArrayInfo, List<TileProperties>> pair in arrayData)
		{
			SpriteArrayInfo info = pair.Key;
			List<TileProperties> props = pair.Value;

			arrays.BuildTextureArray(props, info.material, info.w, info.h);

			for (int i = 0; i < props.Count; i++)
			{
				props[i].index = index;
				props[i].spriteIndex = i;
			}

			index++;
		}
	}
}
