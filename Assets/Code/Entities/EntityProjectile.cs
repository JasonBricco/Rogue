﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static UnityEngine.Mathf;
using static Utils;

public class EntityProjectile : MonoBehaviour
{
	[SerializeField] private bool piercing;
	[SerializeField] private bool hasMaxDistance;
	[SerializeField] private float maxDistance;

	public bool Piercing
	{
		get { return piercing; }
	}

	private Entity entity;
	private float distRemaining;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		distRemaining = maxDistance;
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.RoomChanged, OnRoomChanged);
	}

	// Ensures the projectile doesn't get stuck in unloaded rooms.
	private void OnRoomChanged()
	{
		Vec2i roomP = ToRoomPos(entity.TilePos);
		Vec2i camRoomP = ToRoomPos(Camera.main.transform.position);

		if (Abs(camRoomP.x - roomP.x) > 1 || Abs(camRoomP.y - roomP.y) > 1)
			entity.KillEntity();
	}

	private void UpdateComponent()
	{
		Vector2 dir = Vec2i.Directions[entity.facing].ToVector2();

		if (hasMaxDistance)
			entity.SimpleMove(dir, ref distRemaining, () => entity.SetFlag(EntityFlags.Dead));
		else entity.SimpleMove(dir);
	}

	private void Kill()
	{
		entity.UnsetFlag(EntityFlags.Dead);
		distRemaining = maxDistance;
		entity.Entities.ReturnProjectile(entity);
	}
}