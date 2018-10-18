//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.IO;

public static class RoomSerializer
{
	[Il2CppSetOptions(Option.NullChecks, false)]
	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	public static void Serialize(FileInfo path, Room room)
	{
		RoomFileData data = new RoomFileData();
		data.roomType = (int)room.Type;

		if (World.Instance.TryGetExit(room.Pos, out var list))
			data.exitPoints = list;

		CompressTiles();

		Entity[] entities = room.Entities.GetEntities();
		data.entityIds = new int[entities.Length];
		data.entityPositions = new Vector2[entities.Length];

		for (int i = 0; i < entities.Length; i++)
		{
			data.entityIds[i] = (int)entities[i].Type;
			data.entityPositions[i] = entities[i].Pos;
		}

		string json = JsonUtility.ToJson(data);
		File.WriteAllText(path.FullName + room.Pos.ToPathString(), json);

		void CompressTiles()
		{
			var tileData = data.tileData;

			int count = 1;
			int currentData = room.GetTile(0).GetInt();

			for (int i = 1; i < room.TileCount; i++)
			{
				int tile = room.GetTile(i).GetInt();

				if (tile != currentData)
				{
					tileData.Add(count);
					tileData.Add(currentData);
					count = 1;
					currentData = tile;
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

	[Il2CppSetOptions(Option.NullChecks, false)]
	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	public static void Load(FileInfo path, Room room)
	{
		string json = File.ReadAllText(path.FullName + room.Pos.ToPathString());
		RoomFileData data = JsonUtility.FromJson<RoomFileData>(json);

		
	}

	public static bool Exists(FileInfo path, Vec2i pos)
		=> File.Exists(path.FullName + pos.ToPathString());
}
