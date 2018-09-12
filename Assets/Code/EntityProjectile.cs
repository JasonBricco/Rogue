//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

public class EntityProjectile : MonoBehaviour
{
	[SerializeField] private bool piercing;
	[SerializeField] private bool hasLifetime;
	[SerializeField] private int maxDistance;

	private int tilesLeft;

	public bool Piercing
	{
		get { return piercing; }
	}

	private Entity entity;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.SetMove, SetMove);
		entity.ListenForEvent(EntityEvent.ReachedNewCell, OnNewCell);

		tilesLeft = maxDistance;
	}

	private void OnNewCell()
	{
		if (hasLifetime)
		{
			tilesLeft--;
			Assert.IsTrue(tilesLeft >= 0);

			if (tilesLeft == 0)
				entity.SetFlag(EntityFlags.Dead);
		}
	}

	private void SetMove()
	{
		CollideResult target;
		entity.Entities.UpdateTarget(entity, Vec2i.Directions[entity.facing], out target);
		entity.Entities.HandleCollision(entity, target);

		if (target.unloaded == true)
			entity.SetFlag(EntityFlags.Dead);
	}

	private void UpdateComponent()
	{
		if (!entity.IsMoving())
			SetMove();

		entity.Move();
	}

	private void Kill()
	{
		entity.UnsetFlag(EntityFlags.Dead);
		tilesLeft = maxDistance;
		entity.Entities.ReturnProjectile(entity);
	}
}
