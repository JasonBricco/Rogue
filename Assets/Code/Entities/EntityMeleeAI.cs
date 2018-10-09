//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using static Utils;

public sealed class EntityMeleeAI : MonoBehaviour
{
	[SerializeField] private float detectRange;

	private Entity entity;
	private EntityTimer timer;
	private Stack<Vector2> path = new Stack<Vector2>();

	private bool followingPath, pathDrawn;
	private Vector2? nextCell;

	private static Entity player;
	private Vector2 lastPlayerPos;

	private void Start()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").GetComponent<Entity>();

		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);

		timer.SetValue(Random.Range(1.5f, 2.5f));
	}

	private void PathFinished()
	{
		nextCell = path.Pop();
		pathDrawn = false;
	}

	private void GetPath()
	{
		Room room = World.Instance.Room;
		room.Pathfinding.FindPath(TilePos(entity.Pos), TilePos(player.Pos), path, PathFinished);
		lastPlayerPos = player.Pos;
		followingPath = true;
	}

	private void UpdateComponent()
	{
		DrawPath();

		if (nextCell.HasValue)
		{
			if (timer.Value <= 0.0f)
			{
				GetPath();
				timer.SetValue(1.0f);
			}

			Vector2 next = nextCell.Value;
			Vector2 dir = (next - entity.Pos).normalized;
			entity.facing = GetStableNumericDir(dir);
			entity.Move(dir);

			if ((next - entity.Pos).sqrMagnitude < 0.09f)
			{
				if (path.Count > 0)
					nextCell = path.Pop();
				else
				{
					nextCell = null;
					followingPath = false;
				}
			}
		}
		else
		{
			float dist = Vector2.Distance(entity.Pos, player.Pos);

			if (dist <= 1.0f)
			{
				Vector2 dir = (player.Pos - entity.Pos).normalized;
				entity.Move(dir);
			}
			else
			{
				if (!followingPath && dist <= detectRange)
					GetPath();
			}
		}
	}

	[Conditional("DRAW_PATH")]
	private void DrawPath()
	{
		if (!pathDrawn && followingPath && path.Count > 0)
		{
			Vector3[] v3 = new Vector3[path.Count];

			int i = 0;
			foreach (Vector2 v in path)
				v3[i++] = v;

			GameObject line = new GameObject("Debug Path");
			LineRenderer lR = line.AddComponent<LineRenderer>();
			lR.widthMultiplier = 0.2f;
			lR.positionCount = v3.Length;
			lR.SetPositions(v3);

			GameObject.Destroy(line, 10.0f);
			pathDrawn = true;
		}
	}

	private void Kill()
	{
		if (entity != null) Destroy(entity.gameObject);
	}
}
