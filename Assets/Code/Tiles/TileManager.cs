//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class TileManager : MonoBehaviour
{
	[SerializeField] private TileDataList dataList;

	public static TileManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		InitTiles();
	}

	public TileData GetData(TileType type)
	{
		return dataList.Get(type);
	}

	// Takes information provided in the inspector and stored in the tile group array and assigns the
	// appropriate tile data into the TileData arrays. This allows the code to access tile information.
	private void InitTiles()
	{
		var arrayData = new Dictionary<SpriteArrayInfo, List<TileData>>();

		for (int i = 0; i < dataList.Count; i++)
		{
			TileData data = dataList[i];

			if (data.invisible)
				continue;

			// Duplicate the material so tile groups that utilize the same material don't share it 
			// and overwrite the assigned texture information.
			data.material = new Material(data.material);

			Sprite sprite = data.sprite;

			data.width = sprite.texture.width;
			data.height = sprite.texture.height;

			SpriteArrayInfo info = new SpriteArrayInfo(data.material, data.width, data.height);

			List<TileData> list;
			if (!arrayData.TryGetValue(info, out list))
			{
				list = new List<TileData>();
				arrayData[info] = list;
			}

			list.Add(data);
		}

		TextureArrays arrays = new TextureArrays();
		int index = 0;

		foreach (KeyValuePair<SpriteArrayInfo, List<TileData>> pair in arrayData)
		{
			SpriteArrayInfo info = pair.Key;
			List<TileData> data = pair.Value;

			arrays.BuildTextureArray(data, info.material, info.w, info.h);

			for (int i = 0; i < data.Count; i++)
			{
				data[i].index = index;
				data[i].spriteIndex = i;
			}

			index++;
		}

		dataList.Sort();
	}
}
