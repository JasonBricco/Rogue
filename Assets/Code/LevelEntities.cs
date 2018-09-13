//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;
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
		entity.Init(this, room, pos);
	}

	private void SpawnEntity(Entity entity, Vec2i roomP, Vec2i cell)
	{
		float cellX = cell.x, cellY = cell.y;
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

	private Vec2i GetKnockbackDir(Entity pusher, Entity moving, bool variableKnockback)
	{
		if (variableKnockback)
		{
			if (moving == pusher)
				return Vec2i.Directions[pusher.facing];
			else return -Vec2i.Directions[moving.facing];
		}
		else return Vec2i.Directions[pusher.facing];
	}

	private void ApplyOnTouchEffects(EntityOnTouch onTouch, Entity affector, Entity moving, Entity target)
	{
		if (target.invincible) return;

		if (onTouch != null)
		{
			target.ApplyDamage(onTouch.Damage);

			if (onTouch.Knockback)
				target.ApplyKnockback(onTouch.KnockbackCells, GetKnockbackDir(affector, moving, onTouch.VariableKnockback));

			if (onTouch.DieOnTouch)
				affector.SetFlag(EntityFlags.Dead);

			if (onTouch.AddCollisionRule)
				AddCollisionRule(affector, target);
		}
	}

	private void OnTriggerObstacle(Entity entity, Tile tile)
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

			case TileType.Spikes:
			{
				if (!entity.invincible && !effects.Exists(entity, OTEffectType.Spikes))
				{
					OTEffect effect = new OTEffect(OTEffectType.Spikes, 0.0f);
					effects.Add(entity, effect);
				}
			} break;
		}
	}

	private void KillOnObstacle(Entity entity, Tile tile)
	{
		entity.SetFlag(EntityFlags.Dead);
	}

	private void OnTriggerEntity(Entity a, Entity b)
	{
		EntityOnTouch onTouchA = a.GetComponent<EntityOnTouch>();
		EntityOnTouch onTouchB = b.GetComponent<EntityOnTouch>();

		ApplyOnTouchEffects(onTouchA, a, a, b);
		ApplyOnTouchEffects(onTouchB, b, a, a);
	}

	private void BuildCollisionMatrix()
	{
		collisionMatrix.Add(Layer.TriggerObstacle, Layer.Player, CollideType.Trigger, null, OnTriggerObstacle);
		collisionMatrix.Add(Layer.TriggerObstacle, Layer.Enemy, CollideType.Trigger, null, OnTriggerObstacle);
		collisionMatrix.Add(Layer.Obstacle, Layer.Player, CollideType.Collide, null, null);
		collisionMatrix.Add(Layer.Obstacle, Layer.Familiar, CollideType.Collide, null, null);
		collisionMatrix.Add(Layer.Obstacle, Layer.Enemy, CollideType.Collide, null, null);
		collisionMatrix.Add(Layer.Obstacle, Layer.Projectile, CollideType.Collide, null, KillOnObstacle);
		collisionMatrix.Add(Layer.Obstacle, Layer.PiercingProjectile, CollideType.Collide, null, KillOnObstacle);
		collisionMatrix.Add(Layer.Projectile, Layer.Player, CollideType.Trigger, OnTriggerEntity, null);
		collisionMatrix.Add(Layer.Projectile, Layer.Enemy, CollideType.Trigger, OnTriggerEntity, null);
		collisionMatrix.Add(Layer.PiercingProjectile, Layer.Player, CollideType.Trigger, OnTriggerEntity, null);
		collisionMatrix.Add(Layer.PiercingProjectile, Layer.Enemy, CollideType.Trigger, OnTriggerEntity, null);
		collisionMatrix.Add(Layer.Player, Layer.Enemy, CollideType.Trigger, OnTriggerEntity, null);
		collisionMatrix.Add(Layer.Enemy, Layer.Enemy, CollideType.Collide, null, null);
	}

	private CollideType CanCollide(Entity entity, CollideResult target)
	{
		if (CollisionRuleExists(entity, target.entity))
			return CollideType.None;

		if (target.invalid) return CollideType.Collide;

		if (target.unloaded == true)
			return CollideType.Collide;

		CollideType tileType = collisionMatrix.Get(entity.Layer, target.tile.Data.layer).type;
		CollideType entityType = CollideType.None;

		if (target.entity != null)
			entityType = collisionMatrix.Get(entity.Layer, target.entity.Layer).type;

		return (CollideType)Max((int)tileType, (int)entityType);
	}

	public bool WillCollide(Entity entity, Vec2i cell)
	{
		CollideResult result = new CollideResult();
		Room room = level.GetRoom(ToRoomPos(cell));

		if (room == null) result.invalid = true;
		else
		{
			room.GetCollisionData(entity, ToLocalPos(cell), ref result);
			return CanCollide(entity, result) == CollideType.Collide;
		}

		return true;
	}

	private CollideResult GetCollisionData(Entity entity, Vec2i dir = default(Vec2i))
	{
		Vec2i tileP = entity.TilePos + dir;
		Room room = level.GetRoom(ToRoomPos(tileP));

		CollideResult result = new CollideResult();
		room.GetCollisionData(entity, ToLocalPos(tileP), ref result);

		return result;
	}

	public void HandleCollision(Entity entity, CollideResult target)
	{
		if (target.entity != null)
		{
			CollisionHandler entityHandler = collisionMatrix.Get(entity.Layer, target.entity.Layer);
			entityHandler.ecr?.Invoke(entity, target.entity);
		}

		CollisionHandler tileHandler = collisionMatrix.Get(entity.Layer, target.tile.Data.layer);
		tileHandler.tcr?.Invoke(entity, target.tile);
	}

	public void TestCollision(Entity entity)
	{
		CollideResult result = GetCollisionData(entity);

		if (CanCollide(entity, result) != CollideType.None)
			HandleCollision(entity, result);
	}

	public CollideType UpdateTarget(Entity entity, Vec2i dir, out CollideResult target)
	{
		CollideType type = CollideType.None;
		target = default(CollideResult);

		if (dir != Vec2i.Zero)
		{
			target = GetCollisionData(entity, dir);
			type = CanCollide(entity, target);

			if (type != CollideType.Collide)
			{
				Vec2i start = entity.TilePos;
				Vec2i end = start + dir;
				entity.NewMoveTarget(start, end, dir);
				return type;
			}
		}

		entity.movingDir = Vec2i.Zero;
		return type;
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

	public void Update()
	{
		Transform camera = Camera.main.transform;
		Vec2i camRoomP = ToRoomPos(camera.position);

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
			Object.Destroy(objects[i]);
	}
}
