﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public sealed class RoomEntities
{
	// Entities are flagged to be removed. Their removal is deferred to the 
	// end of the frame using this list, to prevent issues while iterating the entities.
	private List<Entity> deadEntities = new List<Entity>();

	// All entities belonging to this room.
	private List<Entity> entities = new List<Entity>();

	// Stores all active over-time effects within the world.
	private OTEffects effects = new OTEffects();

	private static Queue<Entity>[] projectiles;

	private Entity playerEntity;
	private EntityPlayer player;
	private Room room;

	public RoomEntities(Room room)
	{
		this.room = room;

		playerEntity = GameObject.FindWithTag("Player").GetComponent<Entity>();
		player = playerEntity.GetComponent<EntityPlayer>();

		if (projectiles == null)
		{
			int projectileCount = 0;

			for (int i = 0; i < World.Instance.EntityPrefabCount(); i++)
			{
				if (World.Instance.EntityPrefab(i).GetComponent<EntityProjectile>() != null)
					projectileCount++;
			}

			projectiles = new Queue<Entity>[projectileCount];

			for (int i = 0; i < projectileCount; i++)
				projectiles[i] = new Queue<Entity>();
		}
	}

	// Adds the given entity to this room.
	public void Add(Entity entity)
	{
		Assert.IsTrue(!entities.Contains(entity));
		entities.Add(entity);
	}

	// Removes the entity from this room.
	public void Remove(Entity entity)
	{
		bool result = entities.Remove(entity);
		Assert.IsTrue(result);
	}

	public void RemovePlayer()
	{
		Remove(playerEntity);
	}

	public void AddPlayer()
	{
		Add(playerEntity);
	}

	public void MovePlayerTo(Vec2i cell, int facing)
	{
		Assert.IsNotNull(playerEntity);
		playerEntity.MoveTo(new Vector2(cell.x + 0.5f, cell.y + 0.5f));
	}

	private void SpawnEntity(Entity entity, Vector2 pos, int facing = 0)
	{
		room.Entities.Add(entity);
		entity.Init(room);
		entity.MoveTo(pos);
		entity.facing = facing;
	}

	private void SpawnEntity(Entity entity, Vec2i cell, int facing = 0)
	{
		Vector2 pos = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
		SpawnEntity(entity, pos, facing);
	}

	public void SpawnEntity(EntityType type, Vec2i cell, int facing = 0)
	{
		Entity entity = Object.Instantiate(World.Instance.EntityPrefab(type), World.Instance.transform).GetComponent<Entity>();
		SpawnEntity(entity, cell, facing);
	}

	public void SpawnPlayer()
	{
		SpawnPoint spawn = World.Instance.SpawnPoint;
		SpawnEntity(playerEntity, spawn.cell, spawn.facing);
		player.OnSpawn();
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
	{
		effects.Remove(entity, type);
	}

	public void RemoveOTEffects(Entity entity)
	{
		effects.RemoveAll(entity);
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
		else proj = Object.Instantiate(World.Instance.EntityPrefab(type).GetComponent<Entity>());

		proj.facing = facing;
		proj.transform.rotation = Quaternion.Euler(Vector3.forward * Direction.Rotations[facing]);

		start.y += 0.3f;
		SpawnEntity(proj, start, facing);

		return proj;
	}

	public void ReturnProjectile(Entity projectile)
	{
		Queue<Entity> queue = projectiles[(int)projectile.Type % projectiles.Length];
		projectile.gameObject.SetActive(false);
		queue.Enqueue(projectile);
	}

	public void Update()
	{
		// Apply all over-time effects.
		effects.Apply();

		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			entity.UpdateEntity();

			if (entity.HasFlag(EntityFlags.Dead))
				deadEntities.Add(entity);
		}

		for (int i = 0; i < deadEntities.Count; i++)
			deadEntities[i].KillEntity();

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

	public void Enable()
	{
		for (int i = 0; i < entities.Count; i++)
			entities[i].gameObject.SetActive(true);
	}

	public void Disable()
	{
		for (int i = 0; i < entities.Count; i++)
			entities[i].gameObject.SetActive(false);
	}

	public void Destroy()
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Disposable");

		for (int i = 0; i < objects.Length; i++)
			GameObject.Destroy(objects[i]);
	}
}
