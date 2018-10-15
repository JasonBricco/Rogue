//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityCollisionHandler : MonoBehaviour
{
	private Entity entity;

	private void Start() => entity = GetComponentInParent<Entity>();

	private void OnTriggerEnter(Collider other)
	{
		Room room = World.Instance.Room;

		if (CheckEntity()) return;
		if (CheckTile()) return;
		if (CheckBarrier()) return;

		room.Collision.HandleDefault(entity, gameObject.layer, other.gameObject.layer);
	
		bool CheckEntity()
		{
			Entity target = other.GetComponentInParent<Entity>();

			if (target != null)
			{
				room.Collision.TrackCollision(entity, gameObject.layer, target, other.gameObject.layer);
				return true;
			}

			return false;
		}

		bool CheckTile()
		{
			TileCollider target = other.GetComponent<TileCollider>();

			if (target != null)
			{
				room.Collision.TrackCollision(entity, gameObject.layer, target.inst, other.gameObject.layer);
				return true;
			}

			return false;
		}

		bool CheckBarrier()
		{
			RoomBarrier target = other.GetComponent<RoomBarrier>();

			if (target != null)
			{
				room.Collision.HandleBarrier(entity, gameObject.layer, other.gameObject.layer, target.dir);
				return true;
			}

			return false;
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
