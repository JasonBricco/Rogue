//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using static Utils;

public sealed class GenDungeon : LevelGenerator
{
	private const int MainLayer = 0, FloorLayer = 1;

	private struct Connection
	{
		public Vec2i a, b;
		public bool xAxis;

		public Connection(Vec2i a, Vec2i b, bool xAxis)
		{
			this.a = a;
			this.b = b;
			this.xAxis = xAxis;
		}
	}

	public override void Generate(Level level, LevelEntities entities, out SpawnPoint spawnPoint)
	{
		int roomCount = Random.Range(3, 21);
		Vec2i roomP = new Vec2i(25, 25);

		List<Connection> connections = new List<Connection>(roomCount);
		List<Vec2i> invalid = new List<Vec2i>();

		spawnPoint = new SpawnPoint();

		spawnPoint.room = new Vec2i(25, 25);
		invalid.Add(new Vec2i(spawnPoint.room.x, spawnPoint.room.y - 1));

		int i = 0;
		while (i < roomCount)
		{
			Room room = level.CreateRoom(roomP.x, roomP.y, 2, MainLayer);

			for (int x = 2; x <= Room.LimX - 2; x++)
			{
				room.SetTile(x, Room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.Front));
				room.SetTile(x, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.Back));
			}

			for (int y = 2; y <= Room.LimY - 2; y++)
			{
				room.SetTile(0, y, MainLayer, new Tile(TileType.DungeonWall, Direction.Left));
				room.SetTile(Room.LimX - 1, y, MainLayer, new Tile(TileType.DungeonWall, Direction.Right));
			}

			room.SetTile(0, Room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.FrontLeft));
			room.SetTile(Room.LimX - 1, Room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.FrontRight));
			room.SetTile(0, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.BackLeft));
			room.SetTile(Room.LimX - 1, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.BackRight));

			for (int y = 2; y <= Room.LimY - 2; y++)
			{
				for (int x = 2; x <= Room.LimX - 2; x++)
					room.SetTile(x, y, FloorLayer, TileType.DungeonFloor);
			}

			int spikeCount = Random.Range(0, 6);

			for (int s = 0; s < spikeCount; s++)
			{
				int pX = Random.Range(4, Room.LimX - 3);
				int pY = Random.Range(4, Room.LimY - 3);

				for (int y = pY; y <= pY + 1; y++)
				{
					for (int x = pX; x <= pX + 1; x++)
						room.SetTile(x, y, MainLayer, TileType.Spikes);
				}
			}

			int enemyCount = Random.Range(2, 6);

			for (int e = 0; e < enemyCount; e++)
			{
				int pX = Random.Range(Room.HalfSizeX - 4, Room.HalfSizeX + 5);
				int pY = Random.Range(Room.HalfSizeY - 3, Room.HalfSizeY + 4);
				entities.SpawnEntity(EntityType.Mole, roomP, new Vec2i(pX, pY));
			}

			while (true)
			{
				if (++i == roomCount) break;
				Vec2i next = roomP + Vec2i.Directions[Random.Range(0, 4)];

				if (level.GetRoom(next) != null || invalid.Contains(next))
					continue;

				connections.Add(new Connection(roomP, next, roomP.x != next.x));
				roomP = next;
				break;
			}
		}

		for (int c = 0; c < connections.Count; c++)
		{
			Connection info = connections[c];

			Vec2i a = info.a * new Vec2i(Room.SizeX, Room.SizeY);
			Vec2i b = info.b * new Vec2i(Room.SizeX, Room.SizeY);

			if (b.LengthSq < a.LengthSq)
				Swap(ref a, ref b);

			if (info.xAxis)
			{
				int startX = a.x + (Room.LimX - 1), y = a.y + Room.HalfSizeY;

				for (int x = startX; x < startX + 4; x++)
				{
					level.SetTile(x, y, MainLayer, TileType.Air);
					level.SetTile(x, y, FloorLayer, TileType.DungeonFloor);
				}
			}
			else
			{
				int startY = a.y + (Room.LimY - 1), x = a.x + Room.HalfSizeX;

				for (int y = startY; y < startY + 4; y++)
				{
					level.SetTile(x, y, MainLayer, TileType.Air);
					level.SetTile(x, y, FloorLayer, TileType.DungeonFloor);
				}
			}
		}

		Room portalRoom = level.GetRandomRoom();

		for (int y = Room.HalfSizeY - 4; y <= Room.HalfSizeY + 4; y++)
		{
			for (int x = Room.HalfSizeX - 4; x <= Room.HalfSizeX + 4; x++)
				portalRoom.SetTile(x, y, MainLayer, TileType.Air);
		}

		portalRoom.SetTile(Room.HalfSizeX, Room.HalfSizeY, MainLayer, TileType.Portal);

		Room familiarRoom = level.GetRandomRoom();
		entities.SpawnEntity(EntityType.Familiar, familiarRoom.Pos, new Vec2i(27, 13));

		Room spawn = level.GetRoom(spawnPoint.room);
		spawn.SetTile(25, 11, MainLayer, TileType.Torch);
		spawn.SetTile(Room.HalfSizeX, 0, MainLayer, new Tile(TileType.DungeonDoor, 0));

		for (int y = 0; y <= 1; y++)
		{
			spawn.SetTile(Room.HalfSizeX - 1, y, MainLayer, TileType.Barrier);
			spawn.SetTile(Room.HalfSizeX + 1, y, MainLayer, TileType.Barrier);
		}

		spawnPoint.cell = new Vec2i(Room.HalfSizeX, 1);
		spawnPoint.facing = Direction.Front;

		Camera.main.GetComponent<GameCamera>().SetFollow(false);
		level.SetLightMode(false);
	}
}
