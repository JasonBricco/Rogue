//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public class TileDataList : ScriptableObject
{
	private TileData[] data;

	public void Init()
	{
		TileType[] values = (TileType[])Enum.GetValues(typeof(TileType));
		string[] names = Enum.GetNames(typeof(TileType));

		data = new TileData[names.Length];

		for (int i = 0; i < Count; i++)
		{
			data[i] = new TileData();
			data[i].name = names[i];
			data[i].type = values[i];
		}
	}

	public int Count
	{
		get { return data.Length - 1; }
	}
	
	public TileData Get(TileType type)
	{
		return data[(int)type];
	}

	public void Sort()
	{
		Array.Sort(data);
	}

	public TileData this[int i]
	{
		get { return data[i]; }
	}
}
