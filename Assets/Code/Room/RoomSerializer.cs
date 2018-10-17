﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.IO;

public sealed class RoomSerializer
{
	private Room room;
	private RoomFileData data = new RoomFileData();
	private FileInfo path;

	public RoomSerializer(Room room)
	{
		this.room = room;
		path = new FileInfo(Application.persistentDataPath + "/World/");
		path.Directory.Create();
	}
	
	[Il2CppSetOptions(Option.NullChecks, false)]
	public void Serialize()
	{
		CompressTiles();

		data.roomType = (int)room.Type;
		data.entities = room.Entities.GetEntities();

		string json = JsonUtility.ToJson(data);
		File.WriteAllText(path.FullName + room.Pos.ToPathString(), json);

		void CompressTiles()
		{
			var tileData = data.tileData;

			int count = 1;
			int currentData = room.GetTile(0).GetInt();

			for (int i = 1; i < room.TileCount; i++)
			{
				int data = room.GetTile(i).GetInt();

				if (data != currentData)
				{
					tileData.Add(count);
					tileData.Add(currentData);
					count = 1;
					currentData = data;
				}
				else count++;

				if (i == room.TileCount - 1)
				{
					tileData.Add(count);
					tileData.Add(currentData);
				}
			}
		}
	}
}
