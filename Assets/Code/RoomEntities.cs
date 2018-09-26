//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class RoomEntities
{
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

	// Simulates OnTriggerStay() by adding to this list when OnTriggerEnter() is called
	// and removing from it when OnTriggerExit() is called.
	private List<TrackedCollision> entityCollisions = new List<TrackedCollision>();
	private List<TrackedCollision> tileCollisions = new List<TrackedCollision>();

	private Entity playerEntity;
	private EntityPlayer player;

	private World world;

	public RoomEntities(World world)
	{
		this.world = world;

		playerEntity = GameObject.FindWithTag("Player").GetComponent<Entity>();
		player = playerEntity.GetComponent<EntityPlayer>();
	}

	private void SpawnEntity(Entity entity, Vector2 pos, int facing = 0)
	{
		Room room = world.Room;
		room.AddEntity(entity);
		entity.Init(this, room);
		entity.MoveTo(pos);
		entity.facing = facing;
	}

	private void SpawnEntity(Entity entity, Vec2i cell, int facing = 0)
	{
		float cellX = cell.x + 0.5f, cellY = cell.y + 0.5f;
		Vector2 pos = new Vector2(cellX, cellY);
		SpawnEntity(entity, pos, facing);
	}

	public void SpawnEntity(EntityType type, Vec2i cell, int facing = 0)
	{
		Entity entity = Object.Instantiate(world.EntityPrefab(type), world.transform).GetComponent<Entity>();
		SpawnEntity(entity, cell, facing);
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

	private Vector2 GetKnockbackDir(Entity pusher, Entity other, KnockbackType type)
	{
		switch (type)
		{
			case KnockbackType.ConstantDirection:
				return Vec2i.Directions[pusher.facing].ToVector2();

			case KnockbackType.VariableDirection:
			{
				Entity moving = pusher.velocity.sqrMagnitude > other.velocity.sqrMagnitude ? pusher : other;

				if (moving == pusher)
					return pusher.FacingDir;
				else return -moving.FacingDir;
			}
		}

		return Vector2.zero;
	}

	private void ApplyOnTouchEffects(EntityOnTouch effects, Entity entity, Entity target)
	{
		if (target.HasFlag(EntityFlags.Invincible)) return;

		if (effects != null)
		{
			EntityHealth health = target.GetComponent<EntityHealth>();
			health?.ApplyDamage(effects.Damage);

			if (effects.Knockback)
				target.ApplyKnockback(GetKnockbackDir(entity, target, effects.KnockbackType), effects.KnockbackForce);

			if (effects.DieOnTouch)
				entity.SetFlag(EntityFlags.Dead);

			if (effects.AddCollisionRule)
				AddCollisionRule(entity, target);
		}
	}

	public void OnTriggerEntity(Entity a, Entity b)
	{
		if (CollisionRuleExists(a, b)) return;

		EntityOnTouch onTouchedA = a.GetComponent<EntityOnTouch>();
		EntityOnTouch onTouchedB = b.GetComponent<EntityOnTouch>();

		ApplyOnTouchEffects(onTouchedA, a, b);
		ApplyOnTouchEffects(onTouchedB, b, a);
	}

	public void OnTriggerTile(Entity entity, Tile tile)
	{
		switch (tile.id)
		{
			case TileType.Portal:
			{
				if (entity.Type == EntityType.Player)
					manager.ChangeLevel(LevelType.Plains);
			} break;

			case TileType.PlainsDoor:
			{
				if (entity.Type == EntityType.Player)
					manager.ChangeLevel(LevelType.Dungeon);
			} break;

			case TileType.DungeonDoor:
			{
				if (entity.Type == EntityType.Player)
					manager.ChangeLevel(LevelType.Plains);
			} break;

			case TileType.Spikes:
			{
				if (!entity.HasFlag(EntityFlags.Invincible) && !effects.Exists(entity, OTEffectType.Spikes))
				{
					OTEffect effect = new OTEffect(OTEffectType.Spikes, 0.0f);
					effects.Add(entity, effect);
				}
			} break;
		}
	}

	public void TriggerTileExit(Entity entity, Tile tile)
	{
		switch (tile.id)
		{
			case TileType.Spikes:
				effects.Remove(entity, OTEffectType.Spikes);
				break;
		}
	}

	private void TrackCollisionInternal(List<TrackedCollision> list, Entity a, int layerA, Entity b, int layerB, Tile tile, int tileLayer)
	{
		TrackedCollision col = new TrackedCollision(a, layerA, b, layerB, tile, tileLayer);
		int index = list.IndexOf(col);

		if (index == -1)
		{
			list.Add(col);
			index = list.Count - 1;
		}

		list[index] = list[index].Increment();
	}

	public void TrackCollision(Entity a, int layerA, Entity b, int layerB)
	{
		TrackCollisionInternal(entityCollisions, a, layerA, b, layerB,  default(Tile), 0);
	}

	public void TrackCollision(Entity a, int layerA, Tile tile, int tileLayer)
	{
		TrackCollisionInternal(tileCollisions, a, layerA, null, 0, tile, tileLayer);
	}

	private bool RemoveCollisionInternal(List<TrackedCollision> list, Entity a, int layerA, Entity b, int layerB, Tile tile, int tileLayer)
	{
		TrackedCollision col = new TrackedCollision(a, layerA, b, layerB, tile, tileLayer);
		int index = list.IndexOf(col);

		if (index != -1)
		{
			bool destroy;
			list[index] = list[index].Decrement(out destroy);

			if (destroy)
			{
				list.RemoveAt(index);
				return true;
			}
		}

		return false;
	}

	public void RemoveCollision(Entity a, int layerA, Entity b, int layerB)
	{
		if (RemoveCollisionInternal(entityCollisions, a, layerA, b, layerB, default(Tile), 0))
			HandleCollisionExit(a, layerA, b, layerB);
	}

	public void RemoveCollision(Entity a, int layerA, Tile tile, int tileLayer)
	{
		if (RemoveCollisionInternal(tileCollisions, a, layerA, null, 0, tile, tileLayer))
			HandleCollisionExit(a, layerA, tile, tileLayer);
	}

	public void KillOnCollide(Entity a, Tile tile)
	{
		a.SetFlag(EntityFlags.Dead);
	}

	public void HandleCollision(Entity a, int layerA, Entity b, int layerB)
	{
		collisionMatrix.GetEntityResponse(layerA, layerB)?.Invoke(a, b);
	}

	public void HandleCollision(Entity a, int layerA, Tile tile, int tileLayer)
	{
		collisionMatrix.GetTileResponse(layerA, tileLayer)?.Invoke(a, tile);
	}

	public void HandleCollisionExit(Entity a, int layerA, Entity b, int layerB)
	{
		exitMatrix.GetEntityResponse(layerA, layerB)?.Invoke(a, b);
	}

	public void HandleCollisionExit(Entity a, int layerA, Tile tile, int tileLayer)
	{
		exitMatrix.GetTileResponse(layerA, tileLayer)?.Invoke(a, tile);
	}

	private void RunCollisions()
	{
		for (int i = 0; i < entityCollisions.Count; i++)
		{
			TrackedCollision col = entityCollisions[i];
			HandleCollision(col.a, col.layerA, col.b, col.layerB);
		}

		for (int i = 0; i < tileCollisions.Count; i++)
		{
			TrackedCollision col = tileCollisions[i];
			HandleCollision(col.a, col.layerA, col.tile, col.tileLayer);
		}
	}

	private void ClearTrackedCollisions(List<TrackedCollision> list, Entity entity)
	{
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i].Involves(entity))
				list.RemoveAt(i);
		}
	}

	public void ClearTrackedCollisions(Entity entity)
	{
		ClearTrackedCollisions(entityCollisions, entity);
		ClearTrackedCollisions(tileCollisions, entity);
	}

	public Entity FireProjectile(Vector2 start, int facing, EntityType type)
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
		proj.transform.rotation = Quaternion.Euler(Vector3.forward * Direction.Rotations[facing]);

		start.y += 0.3f;
		Vec2i roomP = ToRoomPos(start);
		SpawnEntity(proj, roomP, start);

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
		SpawnPoint spawn = world.SpawnPoint;
		SpawnEntity(playerEntity, spawn.room, spawn.cell, spawn.facing);
		player.OnSpawn();
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
				Room room = world.GetRoom(x, y);
				room?.GenerateColliders(collision);
			}
		}

		// Update entities.
		for (int y = camRoomP.y - 1; y <= camRoomP.y + 1; y++)
		{
			for (int x = camRoomP.x - 1; x <= camRoomP.x + 1; x++)
			{
				Room room = world.GetRoom(x, y);
				room?.GetActiveEntities(activeEntities);
			}
		}

		RunCollisions();

		// Apply all over-time effects.
		effects.Apply(world);

		for (int i = 0; i < activeEntities.Count; i++)
		{
			Entity entity = activeEntities[i];
			entity.UpdateEntity(world);

			if (entity.HasFlag(EntityFlags.Dead))
				deadEntities.Add(entity);
		}

		for (int i = 0; i < deadEntities.Count; i++)
			deadEntities[i].KillEntity();

		activeEntities.Clear();
		deadEntities.Clear();

		if (playerEntity.HasFlag(EntityFlags.Dead))
		{
			// Update the player's respawn time here instead of in UpdateComponent() since
			// UpdateComponent() requires that the player is inside the room to be called.
			player.RespawnTime -= Time.deltaTime;

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
