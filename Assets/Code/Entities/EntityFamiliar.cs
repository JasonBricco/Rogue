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
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	private void OnRoomChanged(Vec2i roomP)
	{
		entity.MoveTo(followTarget.Pos);
	}

	private void UpdateComponent()
	{
		float sqDist = (entity.Pos - player.Pos).sqrMagnitude;

		if (followTarget == null)
		{
			if (!player.HasFlag(EntityFlags.Dead) && sqDist <= 9.0f)
			{
				followTarget = player;
				gameObject.tag = "Untagged";
				EventManager.Instance.ListenForEvent<Vec2i>(GameEvent.RoomChanged, OnRoomChanged);
			}
		}
		else
		{
			if (followTarget.HasFlag(EntityFlags.Dead))
			{
				followTarget = null;
				gameObject.tag = "Disposable";
				EventManager.Instance.StopListening<Vec2i>(GameEvent.RoomChanged, OnRoomChanged);
			}
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
