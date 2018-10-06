﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
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

	// Barriers that surround a room. They trap enemies within the room and 
	// allow the player to load new rooms.
	[SerializeField] private RoomBarrier[] barriers;

	private RoomGenerator[] generators =
	{
		new GenPlains(),
		new GenDungeon()
	};

	// The active generator.
	private RoomGenerator generator;

	public SpawnPoint SpawnPoint { get; set; }

	private bool isDark;

	private GameCamera cam;

	private Dictionary<Vec2i, Room> loadedRooms = new Dictionary<Vec2i, Room>();

	// Stores the positions of each exit point for each room (key). This ensures rooms will
	// connect properly to each other.
	private Dictionary<Vec2i, List<Vec2i>> exitPoints = new Dictionary<Vec2i, List<Vec2i>>();

	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

	public static World Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		ColliderPool = new ColliderPool(transform);
		cam = Camera.main.GetComponent<GameCamera>();
		Array.Sort(entityPrefabs);
	}

	private void Start()
	{
		BeginNewSection(RoomType.Plains, true);
		cam.MoveToPlayer();
	}

	public int EntityPrefabCount() => entityPrefabs.Length;

	public Entity EntityPrefab(EntityType type) => entityPrefabs[(int)type];
	public Entity EntityPrefab(int i) => entityPrefabs[i];

	public bool RoomExists(Vec2i pos) => loadedRooms.ContainsKey(pos);

	public void AddExit(Vec2i room, Vec2i cell)
	{
		List<Vec2i> list;
		if (!exitPoints.TryGetValue(room, out list))
		{
			list = new List<Vec2i>();
			exitPoints[room] = list;
		}

		list.Add(cell);
	}

	public bool TryGetExit(Vec2i pos, out List<Vec2i> list) 
		=> exitPoints.TryGetValue(pos, out list);

	// Returns a random room position out of the last loaded rooms.
	public Vec2i GetRandomRoom()
	{
		int index = Random.Range(0, recentRooms.Count);
		return recentRooms.ElementAt(index);
	}

	public void NewRoom(Vec2i pos)
	{
		Room = new Room(pos);

		if (recentRooms.Count > MaxRecent)
			recentRooms.Dequeue();

		recentRooms.Enqueue(Room.Pos);
		loadedRooms.Add(Room.Pos, Room);
	}

	public void Update()
	{
		Room.Update();

		if (Input.GetKeyDown(KeyCode.Tab))
			SetLightMode(!isDark);
	}

	// Allows running a function after the given amount of seconds on this 
	// MonoBehaviour, for objects that aren't MonoBehaviours to call.
	public void Invoke(Action func, float seconds) => StartCoroutine(RunInvoke(func, seconds));

	private IEnumerator RunInvoke(Action func, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		func.Invoke();
	}

	public void LoadRoom(Vec2i pos, bool initial)
	{
		Assert.IsTrue(pos != Room.Pos);

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
			NewRoom(pos);
			generator.Generate(Room, pos, initial);
		}

		AdjustBarriers();
		cam.UpdateValues();

		StartCoroutine(TriggerRoomChanged(Vec2i.Zero));
		GC.Collect();
	}

	public void BeginNewSection(Vec2i dir, RoomType type)
	{
		Assert.IsTrue(dir != Vec2i.Zero);
		ChangeRoomType(type);
		LoadRoom(Room.Pos + dir, true);
		Room.Entities.MovePlayerTo(SpawnPoint.cell, SpawnPoint.facing);
	}

	public void BeginNewSection(RoomType type, bool spawnPlayer)
	{
		foreach (Room room in loadedRooms.Values)
			room.Destroy();

		loadedRooms.Clear();
		ChangeRoomType(type);
		NewRoom(Vec2i.Zero);
		generator.Generate(Room, Room.Pos, true);
		AdjustBarriers();
		cam.UpdateValues();

		if (spawnPlayer) Room.Entities.SpawnPlayer();
		else Room.Entities.MovePlayerTo(SpawnPoint.cell, SpawnPoint.facing);

		StartCoroutine(TriggerRoomChanged(Vec2i.Zero));
		GC.Collect();
	}

	private IEnumerator TriggerRoomChanged(Vec2i pos)
	{
		yield return wait;
		EventManager.Instance.TriggerEvent(GameEvent.RoomChanged, pos);
	}

	private void AdjustBarriers()
	{
		barriers[Direction.Left].Resize(-0.5f, Room.SizeY * 0.5f, 1.0f, Room.SizeY);
		barriers[Direction.Right].Resize(Room.SizeX + 0.5f, Room.SizeY * 0.5f, 1.0f, Room.SizeY);
		barriers[Direction.Back].Resize(Room.SizeX * 0.5f, -0.5f, Room.SizeX, 1.0f);
		barriers[Direction.Front].Resize(Room.SizeX * 0.5f, Room.SizeY + 0.5f, Room.SizeX, 1.0f);
	}

	public void ChangeRoomType(RoomType type)
		=> generator = generators[(int)type];

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
