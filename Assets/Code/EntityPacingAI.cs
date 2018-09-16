//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

[RequireComponent(typeof(EntityTimer))]
public sealed class EntityPacingAI : MonoBehaviour
{
	private Entity entity;
	private EntityTimer timer;
	private float distRemaining;

	private void Start()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();
		timer.SetValue(3.0f);

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.SetMove, SetMove);
	}

	int d = 0;

	private void SetMove()
	{
		Vec2i dir = Vec2i.Zero;

		if (timer.Value <= 0.0f)
		{
			d = d == 0 ? 1 : 0;// Random.Range(0, 4);
			entity.facing = d;
			dir = Vec2i.Directions[d];
			timer.SetValue(1.0f);
		}

		CollideResult target;
		entity.Entities.SetMove(entity, dir, 1, out target);
	}

	private void UpdateComponent()
	{
		if (!entity.IsMoving())
			SetMove();

		entity.Move();
	}

	private void Kill()
	{
		if (entity != null) Destroy(entity.gameObject);
	}
}
