//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using static Utils;

public class GenDungeon : RoomGenerator
{
	private bool familiarSpawned;

	protected override void Init(Room room, Vec2i roomP)
		=> room.Init(roomP, 32, 18, RoomType.Dungeon);

	[Il2CppSetOptions(Option.NullChecks, false)]
	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	protected override void GenerateInternal(Room room, Vec2i roomP, TileInstance? from)
	{
		room.Fill(Room.Back, TileType.DungeonFloor);

		AddWalls();
		AddSpikes();

		if (from.HasValue)
			AddDoor(room.HalfX, 0, 1, true);

		List<Vec2i> possibleRooms = new List<Vec2i>(4)
		{
			roomP + Vec2i.Directions[Direction.Front],
			roomP + Vec2i.Directions[Direction.Back],
			roomP + Vec2i.Directions[Direction.Left],
			roomP + Vec2i.Directions[Direction.Right]
		};

		for (int i = possibleRooms.Count - 1; i >= 0; i--)
		{
			if (World.Instance.RoomExists(possibleRooms[i]))
				possibleRooms.RemoveAt(i);
		}

		// No possible ways to generate, exit with a portal.
		if (Random.value < 0.3f || possibleRooms.Count == 0)
			AddDoor(room.HalfX, room.LimY - 1, 0, false);
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
					AddConnection(cen, possibleRooms[i] - roomP);
				}
			}
		}

		if (World.Instance.TryGetExit(roomP, out List<Vec2i> exits))
		{
			for (int i = 0; i < exits.Count; i++)
			{
				Vec2i p = exits[i];
				room.SetTile(p.x, p.y, Room.Back, TileType.DungeonFloor);
			}
		}

		void AddDoor(int doorX, int doorY, int variant, bool setSpawn)
		{
			for (int y = 0; y <= 1; y++)
			{
				room.SetTile(doorX - 1, doorY + y, Room.Back, TileType.Barrier);
				room.SetTile(doorX + 1, doorY + y, Room.Back, TileType.Barrier);
			}

			TileInstance door = new TileInstance(new Tile(TileType.DungeonDoor, variant), doorX, doorY);
			room.SetTile(doorX, doorY, Room.Back, door.tile);
			World.Instance.AddTeleport(door, from);

			if (setSpawn) World.Instance.SpawnFromTileInstance(door);
		}

		void AddWalls()
		{
			for (int x = 2; x <= room.LimX - 2; x++)
			{
				room.SetTile(x, room.LimY - 1, Room.Back, new Tile(TileType.DungeonWall, Direction.Front));
				room.SetTile(x, 0, Room.Back, new Tile(TileType.DungeonWall, Direction.Back));
			}

			for (int y = 2; y <= room.LimY - 2; y++)
			{
				room.SetTile(0, y, Room.Back, new Tile(TileType.DungeonWall, Direction.Left));
				room.SetTile(room.LimX - 1, y, Room.Back, new Tile(TileType.DungeonWall, Direction.Right));
			}

			room.SetTile(0, room.LimY - 1, Room.Back, new Tile(TileType.DungeonWall, Direction.FrontLeft));
			room.SetTile(room.LimX - 1, room.LimY - 1, Room.Back, new Tile(TileType.DungeonWall, Direction.FrontRight));
			room.SetTile(0, 0, Room.Back, new Tile(TileType.DungeonWall, Direction.BackLeft));
			room.SetTile(room.LimX - 1, 0, Room.Back, new Tile(TileType.DungeonWall, Direction.BackRight));
		}

		void AddSpikes()
		{
			int spikeCount = Random.Range(0, 11);

			for (int s = 0; s < spikeCount; s++)
			{
				int pX = Random.Range(4, room.LimX - 3);
				int pY = Random.Range(4, room.LimY - 3);

				for (int y = pY; y <= pY + 1; y++)
				{
					for (int x = pX; x <= pX + 1; x++)
						room.SetTile(x, y, Room.Back, TileType.Spikes);
				}
			}
		}

		void AddConnection(Vec2i pos, Vec2i dir)
		{
			Assert.IsTrue(dir != Vec2i.Zero);
			World world = World.Instance;

			int d = GetNumericDir(dir);

			switch (d)
			{
				case Direction.Left:
					world.AddExit(room.Pos, new Vec2i(0, pos.y));
					world.AddExit(room.Pos + dir, new Vec2i(room.LimX - 1, pos.y));
					break;

				case Direction.Right:
					world.AddExit(room.Pos, new Vec2i(room.LimX - 1, pos.y));
					world.AddExit(room.Pos + dir, new Vec2i(0, pos.y));
					break;

				case Direction.Back:
					world.AddExit(room.Pos, new Vec2i(pos.x, 0));
					world.AddExit(room.Pos + dir, new Vec2i(pos.x, room.LimY - 1));
					break;

				case Direction.Front:
					world.AddExit(room.Pos, new Vec2i(pos.x, room.LimY - 1));
					world.AddExit(room.Pos + dir, new Vec2i(pos.x, 0));
					break;
			}
		}
	}

	protected override void SpawnEntities(Room room)
	{
		/*
		int enemyCount = Random.Range(3, 7);
		int spawned = 0, tries = 0, maxTries = 10;

		while (spawned < enemyCount)
		{
			int pX = Random.Range(room.HalfX - 4, room.HalfX + 5);
			int pY = Random.Range(room.HalfY - 3, room.HalfY + 4);

			if (room.Pathfinding.EmptyCell(pX, pY))
			{
				room.Entities.SpawnEntity(EntityType.Mole, new Vec2i(pX, pY));
				spawned++;
			}
			else
			{
				if (++tries == maxTries)
					break;
			}
		}

		if (!familiarSpawned)
		{
			room.Entities.SpawnEntity(EntityType.Familiar, new Vec2i(26, 11));
			familiarSpawned = true;
		}

		room.Entities.SetRequireClear(spawned);*/
	}

	public override void SetProperties(GameCamera cam)
	{
		cam.SetFixed();
		SetLightMode(false);
	}
}
