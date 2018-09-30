//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	private Entity entity;

	private void Start()
	{
		entity = GetComponentInParent<Entity>();
	}

	private void OnTriggerEnter(Collider other)
	{
		Room room = World.Instance.Room;
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			room.Collision.TrackCollision(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			
			if (tileCollider != null)
				room.Collision.TrackCollision(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Room room = World.Instance.Room;
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
			room.Collision.RemoveCollision(entity, gameObject.layer, target, other.gameObject.layer);
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			
			if (tileCollider != null)
				room.Collision.RemoveCollision(entity, gameObject.layer, tileCollider.tile, other.gameObject.layer);
		}
	}
}
