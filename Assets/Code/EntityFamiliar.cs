//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

[RequireComponent(typeof(EntityTimer))]
public class EntityFamiliar : MonoBehaviour
{
	private Entity entity;
	private Entity player;
	private Entity followTarget;
	private EntityTimer timer;

	private bool wait = false;

	private void Start()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();

		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
		entity.SetFlag(EntityFlags.Rooted);

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.SetMove, SetMove);
	}

	private void SetMove()
	{
		Vec2i dir = Vec2i.Zero;

		if ((followTarget.Pos - entity.Pos).sqrMagnitude > 1.0f)
		{
			Vector2 targetDir = (followTarget.Pos - entity.Pos).normalized;
			int d = GetNumericDir(targetDir);
			dir = Vec2i.Directions[d];
			entity.facing = d;
		}
		else wait = true;

		CollideResult result;
		if (entity.Entities.UpdateTarget(entity, dir, out result) != CollideType.Collide)
		{
			if (wait)
			{
				wait = false;
				timer.SetValue(0.3f);
			}

			if (timer.Value > 0.0f)
				entity.movingDir = Vec2i.Zero;
		}
	}

	private void UpdateComponent()
	{
		if (followTarget == null)
		{
			if (!player.HasFlag(EntityFlags.Dead) && (player.Pos - entity.Pos).sqrMagnitude <= 9.0f)
				followTarget = player;
		}
		else
		{
			if (followTarget.HasFlag(EntityFlags.Dead))
				followTarget = null;
			else
			{
				if (!entity.IsMoving())
					SetMove();
			}
		}

		entity.Move();
	}
}
