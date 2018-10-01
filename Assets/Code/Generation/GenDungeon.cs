﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using static Utils;

public sealed class GenDungeon : RoomGenerator
{
	private const int MainLayer = 0, FloorLayer = 1;

	private bool familiarSpawned;

	public override void Generate(Room room, Vec2i roomP, bool initial)
	{
		World.Instance.SetLightMode(false);
		room.Init(roomP, 2, MainLayer, 32, 18);

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

		room.Fill(FloorLayer, TileType.DungeonFloor);

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
			room.Entities.SpawnEntity(EntityType.Mole, new Vec2i(pX, pY));
		}

		if (initial)
		{
			room.SetTile(25, 11, MainLayer, TileType.Torch);
			room.SetTile(room.HalfX, 0, MainLayer, new Tile(TileType.DungeonDoor, 0));

			for (int y = 0; y <= 1; y++)
			{
				room.SetTile(room.HalfX - 1, y, MainLayer, TileType.Barrier);
				room.SetTile(room.HalfX + 1, y, MainLayer, TileType.Barrier);
			}

			World.Instance.SpawnPoint = new SpawnPoint(roomP, room.HalfX, 1, Direction.Front);
		}

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
					AddConnection(room, cen, possibleRooms[i] - roomP);
				}
			}
		}

		List<Vec2i> exits;
		World.Instance.TryGetExit(roomP, out exits);

		for (int i = 0; i < exits.Count; i++)
		{
			Vec2i p = exits[i];
			room.SetTile(p.x, p.y, MainLayer, TileType.Air);
		}

		if (!familiarSpawned)
		{
			if (Random.value < 0.05f)
			{
				room.Entities.SpawnEntity(EntityType.Familiar, new Vec2i(27, 13));
				familiarSpawned = true;
			}
		}

		room.cameraFollow = false;
	}

	private void AddConnection(Room room, Vec2i pos, Vec2i dir)
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
