//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public sealed class EntityCollisionHandler : MonoBehaviour
{
	private Entity entity;

	private void Start()
	{
		entity = GetComponent<Entity>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
		{
			if (entity.Entities.CollisionRuleExists(entity, target))
				return;

			entity.Entities.OnTriggerEntity(entity, target);
		}
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);
			entity.Entities.OnTriggerTile(entity, tileCollider.tile);
		}
	}
}
