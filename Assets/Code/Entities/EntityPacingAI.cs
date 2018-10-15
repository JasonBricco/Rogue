//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

[RequireComponent(typeof(EntityTimer))]
public sealed class EntityPacingAI : MonoBehaviour
{
	private Entity entity;
	private EntityTimer timer;
	private Vector2 dir;
	private float distRemaining;

	private void Start()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.Reset, OnReset);

		timer.SetValue(2.0f);
	}

	private void UpdateComponent()
	{
		if (timer.Value <= 0.0f)
		{
			int d = Random.Range(0, 4);
			entity.facing = d;

			dir = Vec2i.Directions[d].ToVector2();
			distRemaining = Random.Range(1.0f, 1.2f);

			timer.SetValue(Random.Range(4.0f, 8.0f));
		}

		entity.Move(dir, ref distRemaining);

		if (distRemaining <= 0.0f)
			dir = Vector2.zero;
	}

	private void Kill()
	{
		if (entity != null)
			ObjectPool.Return(entity.gameObject);
	}

	private void OnReset()
	{
		entity.UnsetFlag(EntityFlags.Invincible);
		timer.SetValue(2.0f);
	}
}
