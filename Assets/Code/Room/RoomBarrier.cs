//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class RoomBarrier : MonoBehaviour
{
	[SerializeField] private Vec2i dir;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<Entity>().Type == EntityType.Player)
		{
			World world = World.Instance;
			world.LoadRoom(world.Room.Pos + dir, false);
		}
	}
}
