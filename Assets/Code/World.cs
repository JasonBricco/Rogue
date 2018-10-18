//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using static Utils;

public sealed class World : MonoBehaviour
{
	[SerializeField] private Entity[] entityPrefabs;
	[SerializeField] private LayerMask raycastLayers;

	public LayerMask RaycastLayers => raycastLayers;

	public Room Room { get; private set; }

	// Barriers that surround a room. They trap enemies within the room and 
	// allow the player to load new rooms.
	[SerializeField] private RoomBarrier[] barriers;

	private RoomGenerator[] generators =
	{
		new GenPlains(),
		new GenDungeon(),
		new GenDarkDungeon()
	};

	// The active generator.
	private RoomGenerator generator;

	private GameCamera cam;

	private LinkedList<Room> loadedRooms = new LinkedList<Room>();

	// Stores the positions of each exit point for each room (key). This ensures rooms will
	// connect properly to each other.
	private Dictionary<Vec2i, List<Vec2i>> exitPoints = new Dictionary<Vec2i, List<Vec2i>>();

	// Stores links between doors in the world.
	private Dictionary<TileInstance, TileInstance?> teleports = new Dictionary<TileInstance, TileInstance?>();

	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

	private FileInfo worldPath;

	public static World Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		cam = Camera.main.GetComponent<GameCamera>();
		Array.Sort(entityPrefabs);

		worldPath = new FileInfo(Application.persistentDataPath + "/World/");
		worldPath.Directory.Create();
	}

	private void Start()
	{
		ChangeRoomType(RoomType.Plains);
		Room room = new Room(Vec2i.Zero);
		loadedRooms.AddFirst(Room);
		generator.Generate(Room, cam, Room.Pos, null, out _);
		AdjustBarriers();
		cam.UpdateValues();
		Room.Entities.MovePlayerTo(new SpawnPoint(Vec2i.Zero, 4, 4, Vector2.zero, Direction.Front));
		cam.MoveToPlayer();
	}

	public int EntityPrefabCount() => entityPrefabs.Length;

	public GameObject EntityPrefab(EntityType type) => entityPrefabs[(int)type].gameObject;
	public GameObject EntityPrefab(int i) => entityPrefabs[i].gameObject;

	private Room FindRoom(Vec2i p)
	{
		for (LinkedListNode<Room> it = loadedRooms.First; it != null; it = it.Next)
		{
			if (it.Value.Pos == p)
			{
				loadedRooms.Remove(it);
				return it.Value;
			}
		}

		return null;
	}

	public bool RoomExists(Vec2i p)
	{
		Room room = FindRoom(p);

		if (room == null)
			return RoomSerializer.Exists(worldPath, p);

		return true;
	}

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

	public void Update() => Room.Update();

	// Allows running a function after the given amount of seconds on this 
	// MonoBehaviour, for objects that aren't MonoBehaviours to call.
	public void Invoke(Action func, float seconds) => StartCoroutine(RunInvoke(func, seconds));

	private IEnumerator RunInvoke(Action func, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		func.Invoke();
	}

	public void AddTeleport(TileInstance inst, TileInstance? target = null)
	{
		teleports.Add(inst, target);

		if (target.HasValue)
			teleports[target.Value] = inst;
	}

	public SpawnPoint SpawnFromTileInstance(TileInstance inst)
	{
		TileProperties props = inst.tile.Properties;
		Vec2i facing = new Vec2i(props.facing);
		Vec2i spawnP = new Vec2i(inst.x, inst.y) + facing;
		return new SpawnPoint(Room.Pos, spawnP.x, spawnP.y, props.spawnOffset, GetNumericDir(facing));
	}

	public void LoadRoom(Vec2i pos, TileInstance? from, out SpawnPoint spawn)
	{
		Assert.IsTrue(pos != Room.Pos);
		Room.Disable();

		Room newRoom = FindRoom(pos);

		if (newRoom != null)
		{
			loadedRooms.AddFirst(newRoom);
			Room = newRoom;
			newRoom.Enable();
			ChangeRoomType(Room.Type);

			if (from.HasValue) spawn = SpawnFromTileInstance(teleports[from.Value].Value);
			else spawn = default(SpawnPoint);
		}
		else
		{
			Room room = new Room(pos);
			loadedRooms.AddFirst(Room);

			if (RoomSerializer.Exists(worldPath, pos))
			{
				RoomSerializer.Load(worldPath, room);
				spawn = default(SpawnPoint);
			}
			else generator.Generate(Room, cam, pos, from, out spawn);
		}

		AdjustBarriers();
		cam.UpdateValues();

		StartCoroutine(TriggerRoomChanged(Vec2i.Zero));
		GC.Collect();
	}

	public void BeginNewSection(Vec2i dir, RoomType type, TileInstance? from)
	{
		Assert.IsTrue(dir != Vec2i.Zero);
		EventManager.Instance.TriggerEvent(GameEvent.AreaChanging, type);
		ChangeRoomType(type);
		LoadRoom(Room.Pos + dir, from, out SpawnPoint spawn);
		generator.SetProperties(cam);
		Room.Entities.MovePlayerTo(spawn);
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

	public void LockBarriers()
	{
		for (int i = 0; i < barriers.Length; i++)
			barriers[i].Lock();
	}

	public void UnlockBarriers()
	{
		for (int i = 0; i < barriers.Length; i++)
			barriers[i].Unlock();
	}

	public void ChangeRoomType(RoomType type)
		=> generator = generators[(int)type];
}
