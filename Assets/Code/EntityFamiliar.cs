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
	private EntityTimer timer;
	private Entity followTarget;
	private bool wait;

	private void Start()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
		entity.SetFlag(EntityFlags.Rooted);
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	private void UpdateComponent()
	{
		if (followTarget == null)
		{
			if (!player.HasFlag(EntityFlags.Dead))
			{
				float sqDist = (entity.Pos - player.Pos).sqrMagnitude;
				if (sqDist <= 9.0f) followTarget = player;
			}
		}
		else
		{
			if (followTarget.HasFlag(EntityFlags.Dead))
				followTarget = null;
			else
			{
				if ((entity.Pos - followTarget.Pos).sqrMagnitude > 1.0f)
				{
					/*if (!entity.inTransition)
					{
						Vector2 targetDir = (followTarget.Pos - entity.Pos).normalized;
						int d = GetNumericDir(targetDir);
						Vec2i dir = Vec2i.Directions[d];
						entity.facing = d;

						CollideResult result;
						if (entity.TryUpdateTarget(dir, out result) != CollideType.Collide)
						{
							if (wait)
							{
								wait = false;
								timer.SetValue(0.3f);
							}

							if (timer.Value > 0.0f)
							{
								entity.inTransition = false;
								entity.moveDir = Vec2i.Zero;
							}
						}
					}*/
				}
				else wait = true;
			}
		}
	}
}
