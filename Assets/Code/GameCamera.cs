//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class GameCamera : MonoBehaviour
{
	private Transform t;
	private Entity player;

	private Vector3 velocity;
	private bool following;

	private void Awake()
	{
		t = GetComponent<Transform>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
	}

	// Sets the camera position based on the camera mode. If the camera is following the player,
	// the camera will move to the player's position. If it isn't, it will center itself within
	// the room the player is in.
	public void SetPosition()
	{
		if (!following)
		{
			Room room = player.Room;
			Vec2i wPos = room.Pos * new Vec2i(room.SizeX, room.SizeY);
			t.position = new Vector3(wPos.x + room.HalfX, wPos.y + room.HalfY, t.position.z);
		}
	}

	// Sets the camera follow mode. If true, the camera will follow the player. If false, it will
	// be fixed in place and show only the room the player is in.
	public void SetFollow(bool follow)
	{
		if (follow)
		{
			if (player == null)
				player = GameObject.FindWithTag("Player").GetComponent<Entity>();

			t.SetParent(player.transform, false);
			t.position = new Vector3(0.0f, 0.0f, t.position.z);
		}
		else t.SetParent(null);

		following = follow;
	}

	// Instantly move the camera to the player's location.
	public void MoveToPlayer()
	{
		Transform tPlayer = player.transform;
		t.position = new Vector3(tPlayer.position.x, tPlayer.position.y, t.position.z);
	}
}
