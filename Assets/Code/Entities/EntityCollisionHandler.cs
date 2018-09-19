//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	private Entity entity;

	private void Start()
	{
		entity = GetComponentInParent<Entity>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			entity.Entities.TrackCollision(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);
			entity.Entities.TrackCollision(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			entity.Entities.RemoveCollision(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);
			entity.Entities.RemoveCollision(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}
}
