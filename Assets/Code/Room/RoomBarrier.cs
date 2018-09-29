//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

public sealed class RoomBarrier : MonoBehaviour
{
	[SerializeField] private Vec2i dir;

	private void OnCollisionEnter(Collision collision)
	{
		Entity entity = collision.gameObject.GetComponent<Entity>();
		Assert.IsNotNull(entity);

		if (entity.Type == EntityType.Player)
		{
			World world = World.Instance;
			world.LoadRoom(world.Room.Pos + dir, false);
		}
	}
}
