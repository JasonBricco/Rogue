//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class GenPlains : RoomGenerator
{
	private const int MainLayer = 0, FloorLayer = 1;

	private void SetFloor(Room room)
	{
		for (int y = 0; y < room.SizeY; y++)
		{
			for (int x = 0; x < room.SizeX; x++)
			{
				if (Random.Range(0.0f, 100.0f) > 5.0f)
					room.SetTile(x, y, FloorLayer, TileType.PlainsGrass);
				else
				{
					int variant = Random.Range(0, 6);
					room.SetTile(x, y, FloorLayer, new Tile(TileType.PlainsGrass, variant));
				}
			}
		}
	}

	private void CreatePlateau(Room room, int startX, int startY, int endX, int endY)
	{
		for (int x = startX + 3; x < endX - 3; x++)
		{
			room.SetTile(x, startY, MainLayer, new Tile(TileType.PlainsWall, Direction.Back));
			room.SetTile(x, endY - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.Front));
		}

		for (int y = startY + 3; y < endY - 3; y++)
		{
			room.SetTile(startX, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Left));
			room.SetTile(endX - 3, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Right));
		}

		room.SetTile(startX, endY - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontLeft));
		room.SetTile(endX - 3, endY - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontRight));
		room.SetTile(startX, startY, MainLayer, new Tile(TileType.PlainsWall, Direction.BackLeft));
		room.SetTile(endX - 3, startY, MainLayer, new Tile(TileType.PlainsWall, Direction.BackRight));
	}

	public override void Generate(Room room, Vec2i roomP, bool initial)
	{
		World.Instance.SetLightMode(false);

		room.Init(roomP, 2, MainLayer, 64, 36);
		SetFloor(room);

		CreatePlateau(room, 0, 0, room.SizeX, room.SizeY);

		// Raised plateau for the exit door.
		Vec2i start = new Vec2i(25, 18);
		Vec2i end = new Vec2i(38, 26);
		int midX = start.x + (end.x - start.x) / 2;

		CreatePlateau(room, start.x, start.y, end.x, end.y);

		room.SetTile(midX, start.y, MainLayer, TileType.PlainsDoor);
		room.SetTile(midX - 1, start.y, MainLayer, TileType.Barrier);
		room.SetTile(midX + 1, start.y, MainLayer, TileType.Barrier);
		room.SetTile(midX, start.y + 1, MainLayer, TileType.Barrier);

		int enemyCount = Random.Range(8, 14);

		for (int e = 0; e < enemyCount; e++)
		{
			int pX = Random.Range(10, 54);
			int pY = Random.Range(10, 26);

			if (pX >= start.x && pX < end.x && pY >= start.y && pY <= end.y)
				continue;

			room.Entities.SpawnEntity(EntityType.Wolf, new Vec2i(pX, pY));
		}

		if (firstRoom)
		{
			World.Instance.SpawnPoint = new SpawnPoint(4, 4, Direction.Front);
			firstRoom = false;
		}
		else if (initial)
			World.Instance.SpawnPoint = new SpawnPoint(64, 58, Direction.Back);

		room.cameraFollow = true;
	}
}
