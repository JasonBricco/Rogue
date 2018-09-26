//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;

public sealed class GenDungeon : RoomGenerator
{
	private const int MainLayer = 0, FloorLayer = 1;

	private Dictionary<Vec2i, List<Vec2i>> exitPoints = new Dictionary<Vec2i, List<Vec2i>>();
	HashSet<Vec2i> invalid = new HashSet<Vec2i>();

	public override void Init(World world)
	{
		world.SetLightMode(false);
	}

	public override Vec2i SpawnCell()
	{
		
	}

	public override void Generate(World world, Vec2i roomP, RoomEntities entities)
	{
		invalid.Add(roomP);
		Room room = world.CreateRoom(roomP.x, roomP.y, 2, MainLayer, 32, 18);

		for (int x = 2; x <= room.LimX - 2; x++)
		{
			room.SetTile(x, room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.Front));
			room.SetTile(x, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.Back));
		}

		for (int y = 2; y <= room.LimY - 2; y++)
		{
			room.SetTile(0, y, MainLayer, new Tile(TileType.DungeonWall, Direction.Left));
			room.SetTile(room.LimX - 1, y, MainLayer, new Tile(TileType.DungeonWall, Direction.Right));
		}

		room.SetTile(0, room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.FrontLeft));
		room.SetTile(room.LimX - 1, room.LimY - 1, MainLayer, new Tile(TileType.DungeonWall, Direction.FrontRight));
		room.SetTile(0, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.BackLeft));
		room.SetTile(room.LimX - 1, 0, MainLayer, new Tile(TileType.DungeonWall, Direction.BackRight));

		for (int y = 2; y <= room.LimY - 2; y++)
		{
			for (int x = 2; x <= room.LimX - 2; x++)
				room.SetTile(x, y, FloorLayer, TileType.DungeonFloor);
		}

		int spikeCount = Random.Range(0, 6);

		for (int s = 0; s < spikeCount; s++)
		{
			int pX = Random.Range(4, room.LimX - 3);
			int pY = Random.Range(4, room.LimY - 3);

			for (int y = pY; y <= pY + 1; y++)
			{
				for (int x = pX; x <= pX + 1; x++)
					room.SetTile(x, y, MainLayer, TileType.Spikes);
			}
		}

		int enemyCount = Random.Range(2, 6);

		for (int e = 0; e < enemyCount; e++)
		{
			int pX = Random.Range(room.HalfX - 4, room.HalfX + 5);
			int pY = Random.Range(room.HalfY - 3, room.HalfY + 4);
			entities.SpawnEntity(EntityType.Mole, new Vec2i(pX, pY));
		}

		List<Vec2i> possibleRooms = new List<Vec2i>(4)
		{
			roomP + Vec2i.Directions[Direction.Front],
			roomP + Vec2i.Directions[Direction.Back],
			roomP + Vec2i.Directions[Direction.Left],
			roomP + Vec2i.Directions[Direction.Right]
		};

		// Add connections to already existing rooms.
		for (int i = 0; i < possibleRooms.Count; i++)
		{
			List<Vec2i> points;
			if (exitPoints.TryGetValue(possibleRooms[i], out points))
			{
				for (int p = 0; p < points.Count; p++)
					AddConnection(room, points[p], roomP - possibleRooms[i]);
			}
		}

		for (int i = possibleRooms.Count - 1; i >= 0; i--)
		{
			if (invalid.Contains(possibleRooms[i]))
				possibleRooms.RemoveAt(i);
		}

		// No possible ways to generate, exit with a portal.
		if (possibleRooms.Count == 0)
			room.SetTile(room.HalfX, room.HalfY, MainLayer, TileType.Portal);
		else
		{
			bool[] paths = new bool[possibleRooms.Count];

			possibleRooms.Shuffle();
			float chance = 1.0f;

			for (int i = 0; i < possibleRooms.Count; i++)
			{
				paths[i] = Random.value < chance;
				chance /= 2.0f;
			}

			for (int i = 0; i < paths.Length; i++)
			{
				if (paths[i])
				{
					Vec2i cen = new Vec2i(room.HalfX, room.HalfY);
					AddConnection(room, cen, roomP - possibleRooms[i]);
				}
			}
		}
	}

	private void AddConnection(Room room, Vec2i pos, Vec2i dir)
	{
		if (Abs(dir.x) > 0)
		{
			int startX = room.LimX - 1;

			for (int x = startX; x < startX + 2; x++)
			{
				room.SetTile(x, pos.y, MainLayer, TileType.Air);
				room.SetTile(x, pos.y, FloorLayer, TileType.DungeonFloor);
			}
		}
		else
		{
			int startY = room.LimY - 1;

			for (int y = startY; y < startY + 4; y++)
			{
				room.SetTile(pos.x, y, MainLayer, TileType.Air);
				room.SetTile(pos.x, y, FloorLayer, TileType.DungeonFloor);
			}
		}
	}

	public override void Generate(World level, RoomEntities entities, out SpawnPoint spawnPoint)
	{
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

			// TODO: depends on room.
		Camera.main.GetComponent<GameCamera>().SetFollow(false);
	}
}
