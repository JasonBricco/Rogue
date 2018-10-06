//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class GenPlains : RoomGenerator
{
	protected override void Init(Room room, Vec2i roomP)
	{
		room.Init(roomP, 64, 36, RoomType.Plains);
	}

	[Il2CppSetOptions(Option.NullChecks, false)]
	private void SetFloor(Room room)
	{
		for (int y = 0; y < room.SizeY; y++)
		{
			for (int x = 0; x < room.SizeX; x++)
			{
				if (Random.Range(0.0f, 100.0f) > 5.0f)
					room.SetTile(x, y, Room.Back, TileType.PlainsGrass);
				else
				{
					int variant = Random.Range(0, 6);
					room.SetTile(x, y, Room.Back, new Tile(TileType.PlainsGrass, variant));
				}
			}
		}
	}

	[Il2CppSetOptions(Option.NullChecks, false)]
	private void CreatePlateau(Room room, int startX, int startY, int endX, int endY)
	{
		for (int x = startX + 3; x < endX - 3; x++)
		{
			room.SetTile(x, startY, Room.Back, new Tile(TileType.PlainsWall, Direction.Back));
			room.SetTile(x, endY - 3, Room.Back, new Tile(TileType.PlainsWall, Direction.Front));
		}

		for (int y = startY + 3; y < endY - 3; y++)
		{
			room.SetTile(startX, y, Room.Back, new Tile(TileType.PlainsWall, Direction.Left));
			room.SetTile(endX - 3, y, Room.Back, new Tile(TileType.PlainsWall, Direction.Right));
		}

		room.SetTile(startX, endY - 3, Room.Back, new Tile(TileType.PlainsWall, Direction.FrontLeft));
		room.SetTile(endX - 3, endY - 3, Room.Back, new Tile(TileType.PlainsWall, Direction.FrontRight));
		room.SetTile(startX, startY, Room.Back, new Tile(TileType.PlainsWall, Direction.BackLeft));
		room.SetTile(endX - 3, startY, Room.Back, new Tile(TileType.PlainsWall, Direction.BackRight));
	}

	[Il2CppSetOptions(Option.NullChecks, false)]
	protected override void GenerateInternal(Room room, Vec2i roomP, bool initial)
	{
		SetFloor(room);
		CreatePlateau(room, 0, 0, room.SizeX, room.SizeY);

		// Raised plateau for the exit door.
		Vec2i start = new Vec2i(25, 18);
		Vec2i end = new Vec2i(38, 26);
		int midX = start.x + (end.x - start.x) / 2;

		CreatePlateau(room, start.x, start.y, end.x, end.y);

		room.SetTile(midX, start.y, Room.Back, TileType.PlainsDoor);
		room.SetTile(midX - 1, start.y, Room.Back, TileType.Barrier);
		room.SetTile(midX + 1, start.y, Room.Back, TileType.Barrier);
		room.SetTile(midX, start.y + 1, Room.Back, TileType.Barrier);

		start -= 1;
		end += 1;

		int enemyCount = Random.Range(8, 14);

		for (int e = 0; e < enemyCount; e++)
		{
			int pX = Random.Range(10, 54);
			int pY = Random.Range(10, 26);

			if (pX >= start.x && pX < end.x && pY >= start.y && pY <= end.y)
				continue;

			room.Entities.SpawnEntity(EntityType.Wolf, new Vec2i(pX, pY));
		}

		// If this is the first room of the world, we pick an arbitrary spawn point.
		// If initial is true, it's the first room of this generation type after
		// switching from another generation type or when starting the world.
		if (firstRoom)
		{
			World.Instance.SpawnPoint = new SpawnPoint(roomP, 4, 4, Direction.Front);
			firstRoom = false;
		}
		else if (initial)
			World.Instance.SpawnPoint = new SpawnPoint(roomP, 4, 4, Direction.Front);
	}

	public override void SetProperties(GameCamera cam)
	{
		cam.SetFollowing();
		SetLightMode(false);
	}
}
