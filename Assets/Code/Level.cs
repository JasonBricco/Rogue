//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static Utils;

public sealed class Level
{
	public const int RoomCount = 128;

	private Room[] rooms = new Room[RoomCount * RoomCount];
	private List<Room> loadedRooms = new List<Room>();

	private LevelEntities entities;

	private Vec2i spawnRoom, spawnCell;

	private TileCollision collision;

	/// <summary>
	/// The coordinates of the room the player will spawn in.
	/// </summary>
	public Vec2i SpawnRoom
	{
		get { return spawnRoom; }
	}

	/// <summary>
	/// The local coordinates of the cell within the spawn room the player
	/// will spawn at.
	/// </summary>
	public Vec2i SpawnCell
	{
		get { return spawnCell; }
	}

	private GameCamera cam;

	public Level(LevelGenerator generator, TileCollision collision)
	{
		entities = new LevelEntities(this);
		generator.Generate(this, entities, out spawnRoom, out spawnCell);
		entities.SpawnPlayer();

		cam = Camera.main.GetComponent<GameCamera>();
		cam.MoveToPlayer();

		this.collision = collision;
	}

	/// <summary>
	/// Clamps the given room position to the level, between 0 and room count.
	/// </summary>
	public Vec2i ClampRoomToLevel(Vec2i roomPos)
	{
		roomPos.x = Clamp(roomPos.x, 0, RoomCount - 1);
		roomPos.y = Clamp(roomPos.y, 0, RoomCount - 1);
		return roomPos;
	}

	/// <summary>
	/// Returns the room at the given location in room coordinates. Returns null
	/// if the room is out of bounds or hasn't been created.
	/// </summary>
	public Room GetRoom(int x, int y)
	{
		if (y >= 0 && y < RoomCount && x >= 0 && x < RoomCount)
			return rooms[y * RoomCount + x];

		return null;
	}

	/// <summary>
	/// Returns the room at the given location in room coordinates. Returns null
	/// if the room is out of bounds or hasn't been created.
	/// </summary>
	public Room GetRoom(Vec2i p)
	{
		return GetRoom(p.x, p.y);
	}

	/// <summary>
	/// Returns a random room out of all loaded rooms.
	/// </summary>
	public Room GetRandomRoom()
	{
		return loadedRooms[Random.Range(0, loadedRooms.Count)];
	}

	/// <summary>
	/// Creates a new room at the given location with the given number of layers
	/// and main layer. Fails if the room already exists. The room will be 
	/// considered a loaded room after this call.
	/// </summary>
	public Room CreateRoom(int x, int y, int layers, int mainLayer)
	{
		Assert.IsTrue(GetRoom(x, y) == null);
		Room room = new Room(x, y, layers, mainLayer);
		rooms[y * RoomCount + x] = room;
		loadedRooms.Add(room);
		return room;
	}

	/// <summary>
	/// Returns the tile at the given location. Location is specified in world 
	/// tile space.
	/// </summary>
	public Tile GetTile(int x, int y)
	{
		Vec2i rP = ToRoomPos(x, y), lP = ToLocalPos(x, y);
		Room room = rooms[rP.y * RoomCount + rP.x];
		return room.GetTile(lP.x, lP.y);
	}

	/// <summary>
	/// Returns the tile at the given location from the room's main layer. 
	/// Location is specified in world tile space.
	/// </summary>
	public Tile GetTile(Vec2i p)
	{
		return GetTile(p.x, p.y);
	}

	/// <summary>
	/// Sets the given tile at the given location. Location is specified in world 
	/// tile space.
	/// </summary>
	public void SetTile(int x, int y, int z, Tile tile)
	{
		Vec2i rP = ToRoomPos(x, y), lP = ToLocalPos(x, y);
		Room room = rooms[rP.y * RoomCount + rP.x];
		room.SetTile(lP.x, lP.y, z, tile);
	}

	public void Update()
	{
		entities.Update(collision);
		cam.SetPosition();
	}

	/// <summary>
	/// Draws all rooms that are visible to the game camera.
	/// </summary>
	public void Draw()
	{
		RectInt bounds = cam.GetIntersectingRooms(this);

		for (int y = bounds.min.y; y <= bounds.max.y; y++)
		{
			for (int x = bounds.min.x; x <= bounds.max.x; x++)
			{
				Room room = GetRoom(x, y);

				if (room != null)
				{
					if (!room.built)
						room.BuildMeshes();

					room.Draw();
				}
			}
		}
	}

	/// <summary>
	/// Destroys the level.
	/// </summary>
	public void Destroy()
	{
		entities.Destroy();

		for (int i = 0; i < loadedRooms.Count; i++)
			loadedRooms[i].Destroy(collision);
	}
}
