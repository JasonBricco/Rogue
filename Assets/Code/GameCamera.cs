//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class GameCamera : MonoBehaviour
{
	private Transform t;
	private Entity player;

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

	public void UpdateValues()
	{
		Room room = World.Instance.Room;
		float width = cam.aspect * cam.orthographicSize * 2.0f;
		minX = width / 2.0f;
		maxX = room.SizeX - minX;
		minY = cam.orthographicSize;
		maxY = room.SizeY - minY;
	}

	public void SetFollowing()
	{
		following = true;
		t.position = player.Pos;
		t.SetZ(-10.0f);
	}

	public void SetFixed() => following = false;

	// Sets the camera position based on the camera mode. If the camera is following the player,
	// the camera will move to the player's position. If it isn't, it will center itself within
	// the room the player is in.
	public void LateUpdate()
	{
		if (following)
		{
			Vector3 target = player.Pos;
			t.position = target;
			t.position = new Vector3(Mathf.Clamp(t.position.x, minX, maxX), Mathf.Clamp(t.position.y, minY, maxY), -10.0f);
		}
		else
		{
			Room room = World.Instance.Room;
			t.position = new Vector3(room.HalfX, room.HalfY, t.position.z);
		}
	}

	// Instantly move the camera to the player's location.
	public void MoveToPlayer()
	{
		Transform tPlayer = player.transform;
		t.position = new Vector3(tPlayer.position.x, tPlayer.position.y, t.position.z);
	}
}
