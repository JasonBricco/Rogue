//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;

public sealed class RoomPathfinding
{
	private struct PathCell
	{
		public bool passable;
		public int scoreModifier;

		public PathCell(bool passable, int scoreModifier)
		{
			this.passable = passable;
			this.scoreModifier = scoreModifier;
		}
	}

	private class PathStep
	{
		public Vec2i pos;
		public int moveScore, heuristic, scoreMod;
		public PathStep parent;

		public int Score => moveScore + heuristic + scoreMod;

		public PathStep(Vec2i pos, PathCell[,] grid)
		{
			this.pos = pos;
			scoreMod = grid[pos.x, pos.y].scoreModifier;
		}
	}

	private Room room;
	private PathCell[,] grid;

	private static LayerMask mask = LayerMask.NameToLayer("Terrain") | LayerMask.NameToLayer("TerrainTrigger");

	private Queue<PathStep> stepPool = new Queue<PathStep>();

	private List<PathStep> openList = new List<PathStep>();
	private List<PathStep> closedList = new List<PathStep>();

	public RoomPathfinding(Room room)
	{
		this.room = room;
		grid = new PathCell[room.SizeX, room.SizeY];
	}

	public void Generate()
	{
		for (int y = 0; y < grid.GetLength(1); y++)
		{
			for (int x = 0; x < grid.GetLength(0); x++)
			{
				Vector3 origin = new Vector3(x + 0.5f, y + 0.5f, -100.0f);
				Ray ray = new Ray(origin, Vector3.back);

				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, mask))
				{
					Collider col = hit.collider;

					if (!col.isTrigger)
						grid[x, y] = new PathCell(false, 0);
					else
					{
						TilePathScore scoreMod = col.GetComponent<TilePathScore>();
						grid[x, y] = new PathCell(true, scoreMod != null ? scoreMod.ScoreModifier : 0);
					}
				}
				else grid[x, y] = new PathCell(true, 0);
			}
		}
	}

	// Add the given step to the open list, sorting by the final score.
	private void AddToOpenList(PathStep step)
	{
		Vec2i p = step.pos;
		int score = step.Score;

		int count = openList.Count;

		int i = 0;

		while (i++ < count)
		{
			if (score <= openList[i].Score)
				break;
		}

		openList.Insert(i, step);
	}

	// Compute the estimated number of cells to reach the destination 
	// using Manhattan distance.
	private int ComputeHeuristic(Vec2i start, Vec2i end)
		=> Abs(end.x - start.x) + Abs(end.y - start.y);

	public void FindPath(Vec2i start, Vec2i target)
	{
		if (start == target) return;


	}
}
