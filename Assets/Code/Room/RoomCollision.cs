//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class RoomCollision
{
	private Room room;
	private bool hasColliders;

	private Queue<GameObject> activeColliders = new Queue<GameObject>();

	// Stores collisions that have occurred within the level between two entities. 
	// Not all collisions are stored here; this structure is used when an entity
	// requires this information, such as to avoid repeated collisions with the same entity.
	private static CollisionRules collisionRules = new CollisionRules();

	// Simulates OnTriggerStay() by adding to this list when OnTriggerEnter() is called
	// and removing from it when OnTriggerExit() is called.
	private List<TrackedCollision> entityCollisions = new List<TrackedCollision>();
	private List<TrackedCollision> tileCollisions = new List<TrackedCollision>();

	// The collision matrices should be shared among all rooms and only defined once.
	private static CollisionMatrix collisionMatrix;
	private static CollisionMatrix exitMatrix;

	private Transform colliderParent;

	public RoomCollision(Room room)
	{
		this.room = room;

		if (collisionMatrix == null)
			BuildCollisionMatrices();

		colliderParent = World.Instance.transform;
	}

	public void Update()
	{
		if (Engine.Paused) return;
		RunCollisions();
	}

	private void BuildCollisionMatrices()
	{
		collisionMatrix = new CollisionMatrix();
		exitMatrix = new CollisionMatrix();

		int lPlayer = LayerMask.NameToLayer("Player");
		int lEnemy = LayerMask.NameToLayer("Enemy");
		int lProjectile = LayerMask.NameToLayer("Projectile");
		int lTerrain = LayerMask.NameToLayer("Terrain");
		int lTerrainTrigger = LayerMask.NameToLayer("Terrain Trigger");

		collisionMatrix.Add(lPlayer, lTerrainTrigger, tcr: OnTriggerTile, bcr: ShiftRoom);
		collisionMatrix.Add(lEnemy, lTerrainTrigger, tcr: OnTriggerTile, bcr: BarrierKnockback);

		collisionMatrix.Add(lProjectile, lTerrain, tcr: KillOnCollide, dcr: KillOnDefault);
		collisionMatrix.Add(lProjectile, lPlayer, ecr: OnTriggerEntity);
		collisionMatrix.Add(lProjectile, lEnemy, ecr: OnTriggerEntity);
		collisionMatrix.Add(lProjectile, lTerrainTrigger, bcr: KillOnBarrier);

		collisionMatrix.Add(lPlayer, lEnemy, ecr: OnTriggerEntity);

		exitMatrix.Add(lPlayer, lTerrainTrigger, tcr: TriggerTileExit);
		exitMatrix.Add(lEnemy, lTerrainTrigger, tcr: TriggerTileExit);
	}

	public void AddCollisionRule(Entity a, Entity b) => collisionRules.Add(a, b);
	public bool CollisionRuleExists(Entity a, Entity b) => collisionRules.Exists(a, b);
	public void RemoveCollisionRules(Entity entity) => collisionRules.Remove(entity);

	// Adds colliders for all tiles that require them in this room.
	[Il2CppSetOptions(Option.NullChecks, false)]
	public void Generate()
	{
		if (hasColliders)
			RemoveColliders();

		for (int z = 0; z < Room.Layers; z++)
		{
			for (int y = 0; y < room.SizeY; y++)
			{
				for (int x = 0; x < room.SizeX; x++)
				{
					Tile tile = room.GetTile(x, y, z);
					TileProperties data = tile.Properties;

					if (data.hasCollider)
					{
						TileCollider col = ObjectPool.Get(data.collider.gameObject, colliderParent).GetComponent<TileCollider>();
						col.inst = new TileInstance(tile, room.Pos, x, y);
						col.SetPosition(x, y);
						activeColliders.Enqueue(col.gameObject);
					}
				}
			}
		}

		hasColliders = true;
	}

	// Removes all colliders for this room.
	public void RemoveColliders()
	{
		if (hasColliders)
		{
			while (activeColliders.Count > 0)
				ObjectPool.Return(activeColliders.Dequeue());

			hasColliders = false;
		}
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
				entity.Kill();
			else
			{
				if (effects.AddCollisionRule)
					AddCollisionRule(entity, target);
			}
		}
	}

	private void ShiftRoom(Entity a, Vec2i dir)
	{
		World world = World.Instance;
		world.LoadRoom(world.Room.Pos + dir, null, out _);
		a.ShiftPosition(dir);
	}

	private void BarrierKnockback(Entity a, Vec2i dir)
		=> a.ApplyKnockback(-dir.ToVector2(), 10.0f);

	private void OnTriggerEntity(Entity a, Entity b)
	{
		if (CollisionRuleExists(a, b)) return;

		EntityOnTouch onTouchedA = a.GetComponent<EntityOnTouch>();
		EntityOnTouch onTouchedB = b.GetComponent<EntityOnTouch>();

		ApplyOnTouchEffects(onTouchedA, a, b);
		ApplyOnTouchEffects(onTouchedB, b, a);
	}

	private void OnTriggerTile(Entity entity, TileInstance inst)
	{
		Tile tile = inst.tile;

		switch (tile.id)
		{
			case TileType.PlainsDoor:
			{
				if (entity.Type == EntityType.Player)
				{
					RoomType target = Random.value < 0.15f ? RoomType.DarkDungeon : RoomType.Dungeon;
					BeginNewSection(target, -new Vec2i(tile.Properties.facing));
				}
			} break;

			case TileType.DungeonDoor:
			{
				if (entity.Type == EntityType.Player)
					BeginNewSection(RoomType.Plains, -new Vec2i(tile.Properties.facing));
			} break;

			case TileType.Spikes:
			{
				// Because the collision matrices are shared, we cannot rely on our stored room reference
				// in this method. We must always get it directly from the world.
				if (!entity.HasFlag(EntityFlags.Invincible))
					World.Instance.Room.Entities.AddOTEffect(entity, OTEffectType.Spikes);
			} break;
		}

		void BeginNewSection(RoomType type, Vec2i dir)
		{
			Action callback = () =>
			{
				World.Instance.BeginNewSection(dir, type, inst);

				FadeInfo fadeIn = new FadeInfo(true, 0.0f, 0.0f, 0.0f, 0.2f, null);
				EventManager.Instance.TriggerEvent(GameEvent.Fade, fadeIn);
			};

			FadeInfo fadeOut = new FadeInfo(false, 0.0f, 0.0f, 0.0f, 0.2f, callback);
			EventManager.Instance.TriggerEvent(GameEvent.Fade, fadeOut);
		}
	}

	private void TriggerTileExit(Entity entity, TileInstance inst)
	{
		switch (inst.tile.id)
		{
			case TileType.Spikes:
				World.Instance.Room.Entities.RemoveOTEffect(entity, OTEffectType.Spikes);
				break;
		}
	}

	private void TrackCollisionInternal(List<TrackedCollision> list, Entity a, int layerA, Entity b, int layerB, TileInstance inst, int tileLayer)
	{
		TrackedCollision col = new TrackedCollision(a, layerA, b, layerB, inst, tileLayer);
		int index = list.IndexOf(col);

		if (index == -1)
		{
			list.Add(col);
			index = list.Count - 1;
		}

		list[index] = list[index].Increment();
	}

	public void TrackCollision(Entity a, int layerA, Entity b, int layerB)
		=> TrackCollisionInternal(entityCollisions, a, layerA, b, layerB, default(TileInstance), 0);

	public void TrackCollision(Entity a, int layerA, TileInstance inst, int tileLayer)
		=> TrackCollisionInternal(tileCollisions, a, layerA, null, 0, inst, tileLayer);

	private bool RemoveCollisionInternal(List<TrackedCollision> list, Entity a, int layerA, Entity b, int layerB, TileInstance inst, int tileLayer)
	{
		TrackedCollision col = new TrackedCollision(a, layerA, b, layerB, inst, tileLayer);
		int index = list.IndexOf(col);

		if (index != -1)
		{
			list[index] = list[index].Decrement(out bool destroy);

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
		if (RemoveCollisionInternal(entityCollisions, a, layerA, b, layerB, default(TileInstance), 0))
			HandleCollisionExit(a, layerA, b, layerB);
	}

	public void RemoveCollision(Entity a, int layerA, TileInstance inst, int tileLayer)
	{
		if (RemoveCollisionInternal(tileCollisions, a, layerA, null, 0, inst, tileLayer))
			HandleCollisionExit(a, layerA, inst, tileLayer);
	}

	public void KillOnBarrier(Entity a, Vec2i dir) => a.SetFlag(EntityFlags.Dead);
	public void KillOnCollide(Entity a, TileInstance inst) => a.SetFlag(EntityFlags.Dead);
	public void KillOnDefault(Entity a) => a.SetFlag(EntityFlags.Dead);

	public void HandleCollision(Entity a, int layerA, Entity b, int layerB)
		=> collisionMatrix.GetEntityResponse(layerA, layerB)?.Invoke(a, b);

	public void HandleCollision(Entity a, int layerA, TileInstance inst, int tileLayer)
		=> collisionMatrix.GetTileResponse(layerA, tileLayer)?.Invoke(a, inst);

	public void HandleBarrier(Entity a, int layerA, int barrierLayer, Vec2i dir)
		=> collisionMatrix.GetBarrierResponse(layerA, barrierLayer)?.Invoke(a, dir);

	public void HandleDefault(Entity a, int layerA, int layerB)
		=> collisionMatrix.GetDefaultResponse(layerA, layerB)?.Invoke(a);

	public void HandleCollisionExit(Entity a, int layerA, Entity b, int layerB)
		=> exitMatrix.GetEntityResponse(layerA, layerB)?.Invoke(a, b);

	public void HandleCollisionExit(Entity a, int layerA, TileInstance inst, int tileLayer)
		=> exitMatrix.GetTileResponse(layerA, tileLayer)?.Invoke(a, inst);

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
			HandleCollision(col.a, col.layerA, col.inst, col.tileLayer);
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

	public void Enable() => Generate();

	public void Disable()
	{
		RemoveColliders();
		entityCollisions.Clear();
		tileCollisions.Clear();
	}
}
