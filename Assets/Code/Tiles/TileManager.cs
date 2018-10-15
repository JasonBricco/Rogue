//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using System;

public sealed class TileManager : MonoBehaviour
{
	[SerializeField] private TileDataList dataList;

	private TileBehavior[] behaviors = new TileBehavior[(int)TileType.Count];

	public static TileManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		InitTiles();

		// Fill the behaviors array. If a behavior class has been created it will be loaded
		// into its appropriate spot in the array. If not, it will remain null.
		string[] names = Enum.GetNames(typeof(TileType));

		for (int i = 0; i < names.Length - 1; i++)
		{
			Type t = Type.GetType(names[i] + "Behavior");

			if (t != null)
			{
				object obj = Activator.CreateInstance(t);
				behaviors[i] = (TileBehavior)obj;
			}
		}
	}

	public TileProperties GetProperties(Tile tile)
		=> dataList.GetProperties(tile);

	public TileBehavior GetBehavior(TileType id)
		=> behaviors[(int)id];

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
