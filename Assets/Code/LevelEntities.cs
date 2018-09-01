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

	// Stores all active over-time effects within this level.
	private OTEffects effects = new OTEffects();

	private Queue<Entity>[] projectiles;

	private CollisionMatrix collisionMatrix = new CollisionMatrix();

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

		BuildCollisionMatrix();
	}

	private void SpawnEntity(Entity entity, Vec2i roomP, Vector2 pos)
	{
		Room room = level.GetRoom(roomP.x, roomP.y);
		room.AddEntity(entity);
		entity.Init(this, room);
		entity.MoveTo(pos);
	}

	private void SpawnEntity(Entity entity, Vec2i roomP, Vec2i cell)
	{
		float cellX = cell.x + 0.5f, cellY = cell.y + 0.5f;
		Vector2 pos = new Vector2(roomP.x * Room.SizeX + cellX, roomP.y * Room.SizeY + cellY);
		SpawnEntity(entity, roomP, pos);
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

	public void RemoveOTEffects(Entity entity)
	{
		effects.Remove(entity);
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
		}
	}

	private void OnTriggerEntity(Entity a, Entity b)
	{
		if (CollisionRuleExists(a, b)) return;

		EntityOnTouch onTouchedA = a.GetComponent<EntityOnTouch>();
		EntityOnTouch onTouchedB = b.GetComponent<EntityOnTouch>();

		ApplyOnTouchEffects(onTouchedA, a, b);
		ApplyOnTouchEffects(onTouchedB, b, a);
	}

	public void OnTriggerProjectile(Entity proj, Entity b)
	{
		EntityProjectile projInfo = proj.GetComponent<EntityProjectile>();

		if (projInfo == null || CollisionRuleExists(proj, b)) return;

		EntityOnTouch onTouchProj = proj.GetComponent<EntityOnTouch>();
		ApplyOnTouchEffects(onTouchProj, proj, b);

		if (projInfo.Piercing) AddCollisionRule(proj, b);
		else proj.SetFlag(EntityFlags.Dead);
	}

	private void OnTriggerTile(Entity entity, Tile tile)
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

			case TileType.Spikes:
			{
				if (!entity.HasFlag(EntityFlags.Invincible) && !effects.Exists(entity, OTEffectType.Spikes))
				{
					OTEffect effect = new OTEffect(OTEffectType.Spikes, 0.0f);
					effects.Add(entity, effect);
				}

				break;
			}
		}
	}

	private void KillOnCollide(Entity a, Tile tile)
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

	private void BuildCollisionMatrix()
	{
		int lPlayer = LayerMask.NameToLayer("Player");
		int lEnemy = LayerMask.NameToLayer("Enemy");
		int lProjectile = LayerMask.NameToLayer("Projectile");
		int lTerrain = LayerMask.NameToLayer("Terrain");
		int lTerrainTrigger = LayerMask.NameToLayer("Terrain Trigger");

		collisionMatrix.Add(lPlayer, lTerrainTrigger, null, OnTriggerTile);
		collisionMatrix.Add(lEnemy, lTerrainTrigger, null, OnTriggerTile);

		collisionMatrix.Add(lProjectile, lTerrain, null, KillOnCollide);
		collisionMatrix.Add(lProjectile, lPlayer, OnTriggerProjectile, null);
		collisionMatrix.Add(lProjectile, lEnemy, OnTriggerProjectile, null);

		collisionMatrix.Add(lPlayer, lEnemy, OnTriggerEntity, null);
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
		SpawnEntity(playerEntity, level.SpawnRoom, level.SpawnCell);
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

		// Apply all over-time effects.
		effects.Apply(level);

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
