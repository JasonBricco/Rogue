//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	private Entity entity;

	private void Start() => entity = GetComponentInParent<Entity>();

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
				room.Collision.TrackCollision(entity, gameObject.layer, tileCollider.inst, other.gameObject.layer);
			else
			{
				RoomBarrier barrier = other.GetComponent<RoomBarrier>();
				Assert.IsNotNull(barrier);
				room.Collision.HandleBarrier(entity, gameObject.layer, other.gameObject.layer, barrier.dir);
			}
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
				room.Collision.RemoveCollision(entity, gameObject.layer, tileCollider.inst, other.gameObject.layer);
		}
	}
}
