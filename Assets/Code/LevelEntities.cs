//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using static Utils;

public sealed class LevelEntities
{
	private Entity[] entityPrefabs;

	// Entities in this list are updated. Entity updates are deferred to this list since, if we
	// update them in place, they may mutate the lists they reside within during updating.
	private List<Entity> activeEntities = new List<Entity>();

	// Entities are flagged to be removed. Their removal is deferred to the 
	// end of the frame using this list, to prevent issues while iterating the entities.
	private List<Entity> deadEntities = new List<Entity>();

	// Stores collisions that have occurred within the level between two entities. 
	// Not all collisions are stored here; this structure is used when an entity
	// requires this information, such as to avoid repeated collisions with the same entity.
	private CollisionRules collisionRules = new CollisionRules();

	private Queue<Entity>[] projectiles;

	private Entity playerEntity;
	private EntityPlayer player;

	private LevelManager manager;
	private Transform tManager;
	private Level level;

	public LevelEntities(Level level)
	{
		manager = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>();
		tManager = manager.GetComponent<Transform>();
		entityPrefabs = manager.EntityPrefabs;
		
		this.level = level;

		int projectileCount = 0;
		
		for (int i = 0; i < entityPrefabs.Length; i++)
		{
			if (entityPrefabs[i].GetComponent<EntityProjectile>() != null)
				projectileCount++;
		}

		projectiles = new Queue<Entity>[projectileCount];

		for (int i = 0; i < projectileCount; i++)
			projectiles[i] = new Queue<Entity>();

		playerEntity = GameObject.FindWithTag("Player").GetComponent<Entity>();
		player = playerEntity.GetComponent<EntityPlayer>();
	}

	private void SpawnEntity(Entity entity, Vec2i roomP, Vec2i cell)
	{
		float cellX = cell.x + 0.5f, cellY = cell.y + 0.5f;
		Vector2 pos = new Vector2(roomP.x * Room.SizeX + cellX, roomP.y * Room.SizeY + cellY);
		Room room = level.GetRoom(roomP.x, roomP.y);
		room.AddEntity(entity);
		entity.Init(this, room);
		entity.MoveTo(pos);
	}

	public void SpawnEntity(EntityType type, Vec2i room, Vec2i cell)
	{
		Entity entity = Object.Instantiate(entityPrefabs[(int)type], tManager).GetComponent<Entity>();
		SpawnEntity(entity, room, cell);
	}

	public void AddCollisionRule(Entity a, Entity b)
	{
		collisionRules.Add(a, b);
	}

	public bool CollisionRuleExists(Entity a, Entity b)
	{
		return collisionRules.Exists(a, b);
	}

	public void RemoveCollisionRules(Entity entity)
	{
		collisionRules.Remove(entity);
	}

	private Vec2i GetKnockbackDir(Entity pusher, Entity moving, KnockbackType type)
	{
		switch (type)
		{
			case KnockbackType.ConstantDirection:
				return Vec2i.Directions[pusher.facing];

			case KnockbackType.VariableDirection:
			{
				if (moving == pusher)
					return Vec2i.Directions[pusher.facing];
				else return -Vec2i.Directions[moving.facing];
			}
		}

		return Vec2i.Zero;
	}

	public void OnTriggerEntity(Entity a, Entity b)
	{
		EntityOnTouch onTouchedA = a.GetComponent<EntityOnTouch>();
		EntityOnTouch onTouchedB = b.GetComponent<EntityOnTouch>();

		if (onTouchedA != null)
		{
			EntityHealth healthB = b.GetComponent<EntityHealth>();
			healthB?.ApplyDamage(onTouchedA.Damage);

			if (onTouchedA.Knockback)
				b.ApplyKnockback(onTouchedA.KnockbackAmount, GetKnockbackDir(a, b, onTouchedA.KnockbackType));
		}

		if (onTouchedB != null)
		{
			EntityHealth healthA = a.GetComponent<EntityHealth>();
			healthA?.ApplyDamage(onTouchedB.Damage);

			if (onTouchedB.Knockback)
				a.ApplyKnockback(onTouchedB.KnockbackAmount, GetKnockbackDir(b, a, onTouchedB.KnockbackType));
		}
	}

	public void OnTriggerTile(Entity entity, Tile tile)
	{
		switch (tile.id)
		{
			case TileType.Portal:
			{
				if (entity.Type == EntityType.Player)
					manager.ChangeLevel(LevelType.Plains);

				break;
			}

			case TileType.PlainsDoor:
			{
				if (entity.Type == EntityType.Player)
					manager.ChangeLevel(LevelType.Dungeon);

				break;
			}
		}
	}

	public Entity FireProjectile(Vec2i start, int facing, EntityType type)
	{
		Queue<Entity> queue = projectiles[(int)type % projectiles.Length];

		Entity proj;

		if (queue.Count > 0)
		{
			proj = queue.Dequeue();
			proj.gameObject.SetActive(true);
		}
		else proj = Object.Instantiate(entityPrefabs[(int)type]).GetComponent<Entity>();

		proj.facing = facing;
		proj.transform.rotation = Quaternion.Euler(Direction.Rotation(facing));

		Vec2i roomP = ToRoomPos(start.x, start.y);
		SpawnEntity(proj, roomP, ToLocalPos(start));

		return proj;
	}

	public void ReturnProjectile(Entity projectile)
	{
		Queue<Entity> queue = projectiles[(int)projectile.Type % projectiles.Length];
		projectile.gameObject.SetActive(false);
		queue.Enqueue(projectile);
	}

	public void SpawnPlayer()
	{
		SpawnEntity(playerEntity, level.SpawnRoom, level.SpawnCell);
	}

	public void Update(TileCollision collision)
	{
		Transform camera = Camera.main.transform;
		Vec2i camRoomP = ToRoomPos(camera.position);

		// Generate colliders.
		for (int y = camRoomP.y - 2; y <= camRoomP.y + 2; y++)
		{
			for (int x = camRoomP.x - 2; x <= camRoomP.x + 2; x++)
			{
				Room room = level.GetRoom(x, y);
				room?.GenerateColliders(collision);
			}
		}

		// Update entities.
		for (int y = camRoomP.y - 1; y <= camRoomP.y + 1; y++)
		{
			for (int x = camRoomP.x - 1; x <= camRoomP.x + 1; x++)
			{
				Room room = level.GetRoom(x, y);
				room?.GetActiveEntities(activeEntities);
			}
		}

		for (int i = 0; i < activeEntities.Count; i++)
		{
			Entity entity = activeEntities[i];
			entity.UpdateEntity(level);

			if (entity.HasFlag(EntityFlags.Dead))
				deadEntities.Add(entity);
		}

		for (int i = 0; i < deadEntities.Count; i++)
			deadEntities[i].KillEntity();

		activeEntities.Clear();
		deadEntities.Clear();

		if (playerEntity.HasFlag(EntityFlags.Dead))
		{
			if (player.RespawnTime <= 0.0f)
			{
				SpawnPlayer();
				playerEntity.UnsetFlag(EntityFlags.Dead);
			}
		}
	}

	public void Destroy()
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Disposable");

		for (int i = 0; i < objects.Length; i++)
			GameObject.Destroy(objects[i]);
	}
}
