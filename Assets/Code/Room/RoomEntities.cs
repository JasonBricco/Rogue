//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class RoomEntities
{
	// Stores all active over-time effects within the world.
	private OTEffects effects = new OTEffects();

	// Stores disposable objects that are disabled when the room shifts. 
	// This allows us to enable them again when the room is loaded again,
	// since FindGameObjectsWithTag() does not find inactive objects.
	private GameObject[] disposable;

	// If true, this room must be cleared before the player can move on.
	private bool requireClear;
	private int enemiesLeft;

	private Entity playerEntity;
	private EntityPlayer player;
	private Room room;

	public RoomEntities(Room room)
	{
		this.room = room;

		playerEntity = GameObject.FindWithTag("Player").GetComponent<Entity>();
		player = playerEntity.GetComponent<EntityPlayer>();
	}

	public void LockRoom(int enemies)
	{
		requireClear = true;
		enemiesLeft = enemies;
		World.Instance.LockBarriers();
		room.TriggerEvent(TileEvent.RoomLocked);
	}

	public void LockerKilled()
	{
		if (requireClear)
		{
			if (--enemiesLeft <= 0)
			{
				World.Instance.UnlockBarriers();
				room.TriggerEvent(TileEvent.RoomUnlocked);
			}
		}
	}

	public void MovePlayerTo(SpawnPoint p)
	{
		Vector2 pos = p.pos.ToVector2() + p.offset + new Vector2(0.5f, 0.5f);
		playerEntity.MoveTo(pos);
		playerEntity.facing = p.facing;
	}

	public void SpawnEntity(EntityType type, Vec2i cell, int facing = 0)
	{
		Entity entity = ObjectPool.Get(World.Instance.EntityPrefab(type), World.Instance.transform).GetComponent<Entity>();
		Vector2 pos = cell.ToVector2();
		entity.MoveTo(new Vector2(pos.x + 0.5f, pos.y + 0.5f));
		entity.facing = facing;
	}

	public void AddOTEffect(Entity entity, OTEffectType type)
	{
		if (!effects.Exists(entity, OTEffectType.Spikes))
		{
			OTEffect effect = new OTEffect(type, 0.0f);
			effects.Add(entity, effect);
		}
	}

	public void RemoveOTEffect(Entity entity, OTEffectType type)
		=> effects.FlagForRemoval(entity, type);

	public void RemoveOTEffects(Entity entity)
		=> effects.RemoveAll(entity);

	public void FireProjectile(Entity owner, Vector2 start, int facing, EntityType type)
	{
		Entity proj = ObjectPool.Get(World.Instance.EntityPrefab(type)).GetComponent<Entity>();

		proj.facing = facing;
		proj.transform.rotation = Quaternion.Euler(Vector3.forward * Direction.Rotations[facing]);

		room.Collision.AddCollisionRule(proj, owner);

		start.y += 0.3f;
		proj.MoveTo(start);
	}

	public void ReturnProjectile(Entity projectile)
		=> ObjectPool.Return(projectile.gameObject);

	public void Update()
	{
		// Apply all over-time effects.
		effects.Apply();
	}

	private void GetDisposable()
		=> disposable = GameObject.FindGameObjectsWithTag("Disposable");

	public void Enable()
	{
		for (int i = 0; i < disposable.Length; i++)
			disposable[i]?.SetActive(true);
	}

	public void Disable()
	{
		GetDisposable();

		for (int i = 0; i < disposable.Length; i++)
		{
			GameObject disp = disposable[i];
			disp.SetActive(false);

			Entity entity = disp.GetComponent<Entity>();

			if (entity != null && entity.Transient)
			{
				entity.Kill();
				disposable[i] = null;
			}
		}
	}

	public void Destroy()
	{
		GetDisposable();

		for (int i = 0; i < disposable.Length; i++)
			ObjectPool.Return(disposable[i]);
	}
}
