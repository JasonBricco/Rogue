//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

public sealed class World : MonoBehaviour
{
	private Entity[] entityPrefabs;

	public Room Room { get; private set; }
	private RoomEntities entities;

	private Queue<Entity>[] projectiles;

	private CollisionMatrix collisionMatrix = new CollisionMatrix();
	private CollisionMatrix exitMatrix = new CollisionMatrix();

	private const int MaxRecent = 10;
	private Queue<Vec2i> recentRooms = new Queue<Vec2i>(MaxRecent);

	// Stores all active over-time effects within the world.
	private OTEffects effects = new OTEffects();

	private TileCollision collision;

	private string dataPath;

	private RoomGenerator[] generators =
	{
		new GenPlains(),
		new GenDungeon()
	};

	// The active generator.
	private RoomGenerator generator;

	private void Start()
	{
		dataPath = Application.persistentDataPath + "/World/";
		collision = new TileCollision(transform);
		Array.Sort(entityPrefabs);
	}

	private SpawnPoint spawnPoint;

	public SpawnPoint SpawnPoint
	{
		get { return spawnPoint; }
	}

	private bool isDark;

	private GameCamera cam;

	public World()
	{
		entities = new RoomEntities(this);

		int projectileCount = 0;

		for (int i = 0; i < entityPrefabs.Length; i++)
		{
			if (entityPrefabs[i].GetComponent<EntityProjectile>() != null)
				projectileCount++;
		}

		projectiles = new Queue<Entity>[projectileCount];

		for (int i = 0; i < projectileCount; i++)
			projectiles[i] = new Queue<Entity>();

		BuildCollisionMatrices();

		generator.Generate(this, entities, out spawnPoint);
		entities.SpawnPlayer();

		cam = Camera.main.GetComponent<GameCamera>();
		cam.MoveToPlayer();
	}

	public Entity EntityPrefab(EntityType type)
	{
		return entityPrefabs[(int)type];
	}

	private void BuildCollisionMatrices()
	{
		int lPlayer = LayerMask.NameToLayer("Player");
		int lEnemy = LayerMask.NameToLayer("Enemy");
		int lProjectile = LayerMask.NameToLayer("Projectile");
		int lTerrain = LayerMask.NameToLayer("Terrain");
		int lTerrainTrigger = LayerMask.NameToLayer("Terrain Trigger");

		collisionMatrix.Add(lPlayer, lTerrainTrigger, null, entities.OnTriggerTile);
		collisionMatrix.Add(lEnemy, lTerrainTrigger, null, entities.OnTriggerTile);

		collisionMatrix.Add(lProjectile, lTerrain, null, entities.KillOnCollide);
		collisionMatrix.Add(lProjectile, lPlayer, entities.OnTriggerEntity, null);
		collisionMatrix.Add(lProjectile, lEnemy, entities.OnTriggerEntity, null);

		collisionMatrix.Add(lPlayer, lEnemy, entities.OnTriggerEntity, null);

		exitMatrix.Add(lPlayer, lTerrainTrigger, null, entities.TriggerTileExit);
		exitMatrix.Add(lEnemy, lTerrainTrigger, null, entities.TriggerTileExit);
	}

	// Returns a random room position out of the last loaded rooms.
	public Vec2i GetRandomRoom()
	{
		int index = Random.Range(0, recentRooms.Count);
		return recentRooms.ElementAt(index);
	}

	public Room CreateRoom(int x, int y, int layers, int mainLayer, int sizeX, int sizeY)
	{
		Room room = new Room(x, y, layers, mainLayer, sizeX, sizeY);
		this.Room = room;

		if (recentRooms.Count > MaxRecent)
			recentRooms.Dequeue();

		recentRooms.Enqueue(room.Pos);
		return room;
	}

	public void Update()
	{
		entities.Update(collision);
		cam.SetPosition();

		if (!Room.built)
			Room.BuildMeshes();

		Room.Draw();

		if (Input.GetKeyDown(KeyCode.Tab))
			SetLightMode(!isDark);
	}

	public void RemoveOTEffects(Entity entity)
	{
		effects.RemoveAll(entity);
	}

	public void LoadRoom(Vec2i pos)
	{
		Assert.IsTrue(pos != Room.Pos);

		Room.Destroy(collision);
		entities.Destroy();

		if (File.Exists(dataPath + pos.ToPathString()))
		{

		}
		else
		{
			generator.Generate();
		}

		GC.Collect();
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
