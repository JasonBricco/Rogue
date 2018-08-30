//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class GenPlains : LevelGenerator
{
	private const int MainLayer = 0, FloorLayer = 1;

	public override void Generate(Level level, LevelEntities entities, out Vec2i spawnRoom, out Vec2i spawnCell)
	{
		Vec2i rooms = new Vec2i(5, 7);

		for (int roomY = 0; roomY < rooms.y; roomY++)
		{
			for (int roomX = 0; roomX < rooms.x; roomX++)
				level.CreateRoom(roomX, roomY, 2, MainLayer).Fill(FloorLayer, TileType.PlainsGrass);
		}

		Vec2i start = new Vec2i(Room.SizeX, Room.SizeY);
		Vec2i end = start + new Vec2i(rooms.x - 2, rooms.y - 2) * start;

		for (int x = start.x + 3; x < end.x - 3; x++)
		{
			level.SetTile(x, start.y, MainLayer, new Tile(TileType.PlainsWall, Direction.Back));
			level.SetTile(x, end.y - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.Front));
		}

		for (int y = start.y + 3; y < end.y - 3; y++)
		{
			level.SetTile(start.x, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Left));
			level.SetTile(end.x - 3, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Right));
		}

		level.SetTile(start.x, end.y - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontLeft));
		level.SetTile(end.x - 3, end.y - 3, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontRight));
		level.SetTile(start.x, start.y, MainLayer, new Tile(TileType.PlainsWall, Direction.BackLeft));
		level.SetTile(end.x - 3, start.y, MainLayer, new Tile(TileType.PlainsWall, Direction.BackRight));

		int rX = Random.Range(2, rooms.x - 2), rY = Random.Range(2, rooms.y - 2);
		Room room = level.GetRoom(rX, rY);

		for (int x = 3; x <= Room.LimX - 3; x++)
		{
			room.SetTile(x, 0, MainLayer, new Tile(TileType.PlainsWall, Direction.Back));
			room.SetTile(x, Room.LimY - 2, MainLayer, new Tile(TileType.PlainsWall, Direction.Front));
		}

		for (int y = 3; y <= Room.LimY - 3; y++)
		{
			room.SetTile(0, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Left));
			room.SetTile(Room.LimX - 2, y, MainLayer, new Tile(TileType.PlainsWall, Direction.Right));
		}

		room.SetTile(0, Room.LimY - 2, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontLeft));
		room.SetTile(Room.LimX - 2, Room.LimY - 2, MainLayer, new Tile(TileType.PlainsWall, Direction.FrontRight));
		room.SetTile(0, 0, MainLayer, new Tile(TileType.PlainsWall, Direction.BackLeft));
		room.SetTile(Room.LimX - 2, 0, MainLayer, new Tile(TileType.PlainsWall, Direction.BackRight));

		int midX = Room.LimX / 2;
		room.SetTile(midX, 0, MainLayer, TileType.PlainsDoor);
		room.SetTile(midX - 1, 0, MainLayer, TileType.Barrier);
		room.SetTile(midX + 1, 0, MainLayer, TileType.Barrier);
		room.SetTile(midX, 1, MainLayer, TileType.Barrier);

		spawnRoom = new Vec2i(1, 1);
		spawnCell = new Vec2i(4, 4);

		Camera.main.GetComponent<GameCamera>().SetFollow(true);
	}
}
