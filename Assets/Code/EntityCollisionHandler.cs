//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	[SerializeField] private bool allowOnTriggerStay;

	private Entity entity;

	private void Start()
	{
		entity = GetComponentInParent<Entity>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			entity.Entities.HandleCollision(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);
			entity.Entities.HandleCollision(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (allowOnTriggerStay)
			OnTriggerEnter(other);
	}
}
