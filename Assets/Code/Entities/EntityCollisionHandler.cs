//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	[SerializeField] private bool allowOnTriggerStay;

	private Entity entity;

	private void Start()
	{
		entity = GetComponentInParent<Entity>();
	}

	private void HandleCollision(Collider other, Action<Entity, int, Entity, int> entityHandler, Action<Entity, int, Tile, int> tileHandler)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			entityHandler(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);
			tileHandler(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		HandleCollision(other, entity.Entities.HandleCollision, entity.Entities.HandleCollision);
	}

	private void OnTriggerExit(Collider other)
	{
		HandleCollision(other, entity.Entities.HandleCollisionExit, entity.Entities.HandleCollisionExit);
	}

	private void OnTriggerStay(Collider other)
	{
		if (allowOnTriggerStay)
			OnTriggerEnter(other);
	}
}
