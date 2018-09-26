//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

public sealed class GameCamera : MonoBehaviour
{
	private Transform t;
	private Camera cam;
	private Entity player;

	private Vector3 velocity;
	private bool following;

	private void Awake()
	{
		t = GetComponent<Transform>();
		cam = GetComponent<Camera>();
		player = GameObject.FindWithTag("Player").GetComponent<Entity>();
	}

	/// <summary>
	/// Sets the camera position based on the camera mode. If the camera is following the player,
	/// the camera will move to the player's position. If it isn't, it will center itself within
	/// the room the player is in.
	/// </summary>
	public void SetPosition()
	{
		if (!following)
		{
			Room room = player.Room;
			Vec2i wPos = room.Pos * new Vec2i(Room.SizeX, Room.SizeY);
			t.position = new Vector3(wPos.x + Room.HalfSizeX, wPos.y + Room.HalfSizeY, t.position.z);
		}
	}

	/// <summary>
	/// Sets the camera follow mode. If true, the camera will follow the player. If false, it will
	/// be fixed in place and show only the room the player is in.
	/// </summary>
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

	/// <summary>
	/// Instantly move the camera to the player's location.
	/// </summary>
	public void MoveToPlayer()
	{
		Transform tPlayer = player.transform;
		t.position = new Vector3(tPlayer.position.x, tPlayer.position.y, t.position.z);
	}

	/// <summary>
	/// Returns a rectangle representing all rooms that intersect the camera's view frustum.
	/// The minimum value is the room coordinates of the bottom-left room and the maximum
	/// value is the room coordinates of the upper-right room.
	/// </summary>
	public RectInt GetIntersectingRooms(World level)
	{
		if (following)
		{
			Bounds visible = new Bounds();
			visible.min = cam.ScreenToWorldPoint(Vector3.zero);
			visible.max = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));

			visible.Expand(following ? 1.0f : -1.0f);

			Vec2i min = level.ClampRoomToLevel(ToRoomPos(visible.min));
			Vec2i max = level.ClampRoomToLevel(ToRoomPos(visible.max));

			RectInt bounds = new RectInt();
			bounds.min = new Vector2Int(min.x, min.y);
			bounds.max = new Vector2Int(max.x, max.y);

			return bounds;
		}
		else
		{
			Vec2i pRoom = player.Room.Pos;
			return new RectInt(pRoom.x, pRoom.y, 0, 0);
		}
	}
}
