//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

[RequireComponent(typeof(EntityTimer))]
public class EntityFamiliar : MonoBehaviour
{
	[SerializeField] private bool boundToArea;

	private Entity entity;
	private Entity player;
	private Entity followTarget;

	private void Start()
	{
		entity = GetComponent<Entity>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);

		if (boundToArea)
			EventManager.Instance.ListenForEvent<RoomType>(GameEvent.AreaChanging, OnAreaChange);
	}

	private void OnAreaChange(RoomType type) => StopFollowing();
	private void OnRoomChanged(Vec2i roomP) => entity.MoveTo(followTarget.Pos);

	private void StopFollowing()
	{
		followTarget = null;
		gameObject.tag = "Disposable";
		EventManager.Instance.StopListening<Vec2i>(GameEvent.RoomChanged, OnRoomChanged);
	}

	private void UpdateComponent()
	{
		float sqDist = (entity.Pos - player.Pos).sqrMagnitude;

		if (followTarget == null)
			TryFollowPlayer();
		else
		{
			if (followTarget.HasFlag(EntityFlags.Dead))
				StopFollowing();
			else MoveTowardTarget();
		}

		void MoveTowardTarget()
		{
			Vector2 targetDir = Vector2.zero;

			if (sqDist >= 2.0f)
			{
				targetDir = (followTarget.Pos - entity.Pos).normalized;
				entity.facing = GetNumericDir(targetDir);
			}

			entity.Move(targetDir);
		}

		void TryFollowPlayer()
		{
			if (!player.HasFlag(EntityFlags.Dead) && sqDist <= 9.0f)
			{
				followTarget = player;
				gameObject.tag = "Untagged";
				EventManager.Instance.ListenForEvent<Vec2i>(GameEvent.RoomChanged, OnRoomChanged);
			}
		}
	}
}
