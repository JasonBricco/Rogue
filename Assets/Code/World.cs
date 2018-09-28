//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public sealed class World : MonoBehaviour
{
	[SerializeField] private Entity[] entityPrefabs;

	public Room Room { get; private set; }

	private const int MaxRecent = 10;
	private Queue<Vec2i> recentRooms = new Queue<Vec2i>(MaxRecent);

	public ColliderPool ColliderPool { get; private set; }

	private RoomGenerator[] generators =
	{
		new GenPlains(),
		new GenDungeon()
	};

	// The active generator.
	private RoomGenerator generator;

	private void Start()
	{
		ColliderPool = new ColliderPool(transform);
		Array.Sort(entityPrefabs);
	}

	public SpawnPoint SpawnPoint { get; set; }

	private bool isDark;

	private GameCamera cam;

	private Dictionary<Vec2i, Room> loadedRooms = new Dictionary<Vec2i, Room>();

	// Stores the positions of each exit point for each room (key). This ensures rooms will
	// connect properly to each other.
	private Dictionary<Vec2i, List<Vec2i>> exitPoints = new Dictionary<Vec2i, List<Vec2i>>();

	public static World Instance { get; private set; }

	public World()
	{
		Instance = this;

		BeginNewSection(RoomType.Plains);
		Room.Entities.SpawnPlayer();

		cam = Camera.main.GetComponent<GameCamera>();
		cam.MoveToPlayer();
	}

	public int EntityPrefabCount()
	{
		return entityPrefabs.Length;
	}

	public Entity EntityPrefab(EntityType type)
	{
		return entityPrefabs[(int)type];
	}

	public Entity EntityPrefab(int i)
	{
		return entityPrefabs[i];
	}

	public bool RoomExists(Vec2i pos)
	{
		return loadedRooms.ContainsKey(pos);
	}

	public bool TryGetExit(Vec2i pos, out List<Vec2i> list)
	{
		return exitPoints.TryGetValue(pos, out list);
	}

	// Returns a random room position out of the last loaded rooms.
	public Vec2i GetRandomRoom()
	{
		int index = Random.Range(0, recentRooms.Count);
		return recentRooms.ElementAt(index);
	}

	public void NewRoom()
	{
		Room = new Room();

		if (recentRooms.Count > MaxRecent)
			recentRooms.Dequeue();

		recentRooms.Enqueue(Room.Pos);
		loadedRooms.Add(Room.Pos, Room);
	}

	public void Update()
	{
		cam.SetPosition();
		Room.Update();

		if (Input.GetKeyDown(KeyCode.Tab))
			SetLightMode(!isDark);
	}

	public void LoadRoom(Vec2i pos, bool initial)
	{
		Assert.IsTrue(pos != Room.Pos);

		Room.Entities.RemovePlayer();
		Room.Disable();

		Room newRoom;
		if (loadedRooms.TryGetValue(pos, out newRoom))
		{
			Room = newRoom;
			newRoom.Enable();

			if (initial)
				SpawnPoint = Room.Spawn;
		}
		else
		{
			NewRoom();
			generator.Generate(Room, pos, initial);
		}

		newRoom.Entities.AddPlayer();
	}

	public void ChangeRoomType(RoomType type)
	{
		generator = generators[(int)type];
	}

	public void BeginNewSection(Vec2i dir, RoomType type)
	{
		ChangeRoomType(type);
		LoadRoom(Room.Pos + dir, true);
		Room.Entities.MovePlayerTo(SpawnPoint.cell, SpawnPoint.facing);
	}

	public void BeginNewSection(RoomType type)
	{
		Room.Entities.RemovePlayer();

		foreach (Room room in loadedRooms.Values)
			room.Destroy();

		ChangeRoomType(type);
		NewRoom();
		generator.Generate(Room, Vec2i.Zero, true);
		Room.Entities.MovePlayerTo(SpawnPoint.cell, SpawnPoint.facing);
	}

	public void SetLightMode(bool dark)
	{
		if (dark)
		{
			Color col = new Color(0.02f, 0.02f, 0.02f, 1.0f);
			RenderSettings.ambientLight = col;
			Camera.main.backgroundColor = Color.black;
		}
		else
		{
			RenderSettings.ambientLight = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			Camera.main.backgroundColor = new Color(0.58f, 0.8f, 1.0f, 1.0f);
		}

		isDark = dark;
	}
}
