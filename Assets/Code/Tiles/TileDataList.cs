﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using System.Collections.Generic;

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

	public void Refresh()
	{
		Dictionary<string, TileData> map = new Dictionary<string, TileData>(data.Length);

		for (int i = 0; i < data.Length; i++)
			map.Add(data[i].name, data[i]);

		string[] names = Enum.GetNames(typeof(TileType));
		TileType[] values = (TileType[])Enum.GetValues(typeof(TileType));

		List<TileData> newData = new List<TileData>(names.Length);

		for (int i = 0; i < names.Length; i++)
		{
			TileData td;
			if (map.TryGetValue(names[i], out td))
				newData.Add(td);
			else
			{
				td = new TileData();
				td.name = names[i];
				td.type = values[i];
				newData.Add(td);
			}
		}

		data = newData.ToArray();
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
