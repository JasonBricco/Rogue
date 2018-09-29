//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static UnityEngine.Mathf;

public sealed class GameCamera : MonoBehaviour
{
	private Transform t;
	private Entity player;

	private Vector3 velocity;
	private bool following;

	// Boundary values for clamping the camera within the room.
	private float minX, maxX, minY, maxY;

	private Camera cam;

	private void Awake()
	{
		t = GetComponent<Transform>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
		cam = GetComponent<Camera>();
	}

	public void SetBoundaries()
	{
		Room room = World.Instance.Room;
		float width = cam.aspect * cam.orthographicSize * 2.0f;
		minX = width / 2.0f;
		maxX = room.SizeX - minX;
		minY = cam.orthographicSize;
		maxY = room.SizeY - minY;
	}

	// Sets the camera position based on the camera mode. If the camera is following the player,
	// the camera will move to the player's position. If it isn't, it will center itself within
	// the room the player is in.
	public void SetPosition()
	{
		if (following)
		{
			Vector3 target = player.Pos;
			t.position = Vector3.Slerp(t.position, target, 5.0f * Time.deltaTime);
			t.position = new Vector3(Clamp(t.position.x, minX, maxX), Clamp(t.position.y, minY, maxY), -10.0f);
		}
		else
		{
			Room room = World.Instance.Room;
			Vec2i wPos = room.Pos * new Vec2i(room.SizeX, room.SizeY);
			t.position = new Vector3(wPos.x + room.HalfX, wPos.y + room.HalfY, t.position.z);
		}
	}

	// Sets the camera follow mode. If true, the camera will follow the player. 
	// If false, it will be fixed in place.
	public void SetFollow(bool follow)
	{
		following = follow;
	}

	// Instantly move the camera to the player's location.
	public void MoveToPlayer()
	{
		Transform tPlayer = player.transform;
		t.position = new Vector3(tPlayer.position.x, tPlayer.position.y, t.position.z);
	}
}
