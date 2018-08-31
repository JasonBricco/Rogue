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

	private void Start()
	{
		entity = GetComponent<Entity>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
		entity.SetFlag(EntityFlags.Rooted);
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	private void UpdateComponent()
	{
		float sqDist = (entity.Pos - player.Pos).sqrMagnitude;

		if (followTarget == null)
		{
			if (!player.HasFlag(EntityFlags.Dead) && sqDist <= 9.0f)
				followTarget = player;
		}
		else
		{
			if (followTarget.HasFlag(EntityFlags.Dead))
				followTarget = null;
			else
			{
				Vector2 targetDir = Vector2.zero;

				if (sqDist >= 2.0f)
				{
					targetDir = (followTarget.Pos - entity.Pos).normalized;
					entity.facing = GetNumericDir(targetDir);
				}

				entity.Move(targetDir);
			}
		}
	}
}
