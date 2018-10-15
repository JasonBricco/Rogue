//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public class EntityProjectile : MonoBehaviour
{
	[SerializeField] private bool piercing;
	[SerializeField] private bool hasMaxDistance;
	[SerializeField] private float maxDistance;

	public bool Piercing => piercing;

	private Entity entity;
	private float distRemaining;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		distRemaining = maxDistance;

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
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
		World.Instance.Room.Entities.ReturnProjectile(entity);
	}
}
