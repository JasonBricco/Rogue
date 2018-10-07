//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;
using static UnityEngine.Mathf;

public sealed class RoomPathfinding
{
	private struct PathCell
	{
		public bool passable;
		public int cost;

		public PathCell(bool passable, int cost)
		{
			this.passable = passable;
			this.cost = cost;
		}

		public override string ToString()
		{
			return "Passable: " + passable + ", Score: " + cost;
		}
	}

	private class PathNode : IEquatable<PathNode>, IComparable<PathNode>
	{
		public Vec2i pos;
		public int g, f, h;
		public PathNode parent;

		public int CompareTo(PathNode other)
		{
			return f.CompareTo(other.f);
		}

		public bool Equals(PathNode other)
			=> pos == other.pos;

		public override string ToString()
		{
			return pos.ToString() + ", F: " + f;
		}
	}

	private Room room;

	private PathCell[,] grid;
	private PathNode[,] nodes;

	private SortedList<Vec2i, PathNode> openList = new SortedList<Vec2i, PathNode>();
	private HashSet<PathNode> closedList = new HashSet<PathNode>();

	private int successorCount = 0;
	private PathNode[] successors = new PathNode[8];

	// Stores the final path. 
	private List<Vec2i> path = new List<Vec2i>();

	private bool generated = false;

	public RoomPathfinding(Room room)
	{
		this.room = room;
	}

	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOptions(Option.NullChecks, false)]
	public void Update()
	{
		if (Engine.Paused) return;

		if (!generated)
		{
			grid = new PathCell[room.SizeX, room.SizeY];
			nodes = new PathNode[room.SizeX, room.SizeY];

			for (int y = 0; y < grid.GetLength(1); y++)
			{
				for (int x = 0; x < grid.GetLength(0); x++)
				{
					Vector3 origin = new Vector3(x + 0.5f, y + 0.5f, -100.0f);
					Ray ray = new Ray(origin, Vector3.forward);

					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 200.0f, World.Instance.RaycastLayers, QueryTriggerInteraction.Collide))
					{
						Collider col = hit.collider;

						if (!col.isTrigger)
							grid[x, y] = new PathCell(false, 1);
						else
						{
							TileCollider tC = col.GetComponent<TileCollider>();
							grid[x, y] = new PathCell(true, tC.scoreModifier);
						}
					}
					else grid[x, y] = new PathCell(true, 1);
				}
			}

			generated = true;
		}
	}

	private PathNode GetNode(Vec2i p)
	{
		PathNode node = nodes[p.x, p.y];

		if (node == null)
		{
			node = new PathNode();
			nodes[p.x, p.y] = node;
		}

		node.pos = p;
		return node;
	}

	// Compute the estimated number of cells to reach the destination 
	// using Manhattan distance.
	private int ComputeHeuristic(Vec2i start, Vec2i end)
		=> Abs(end.x - start.x) + Abs(end.y - start.y);

	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOptions(Option.NullChecks, false)]
	private void GetSuccessors(PathNode current, Vec2i pos)
	{
		successorCount = 0;

		for (int i = 0; i < 4; i++)
		{
			Vec2i next = pos + Vec2i.Directions[i];

			if (room.InBounds(next) && grid[next.x, next.y].passable)
				successors[successorCount++] = GetNode(next);
		}

		for (int i = 4; i < 8; i++)
		{
			Vec2i dir = Vec2i.Directions[i];
			Vec2i next = pos + dir;

			if (room.InBounds(next) && grid[next.x, next.y].passable)
			{
				Vec2i adjX = new Vec2i(pos.x + dir.x, pos.y);
				Vec2i adjY = new Vec2i(pos.x, pos.y + dir.y);

				if (grid[adjX.x, adjX.y].passable && grid[adjY.x, adjY.y].passable)
					successors[successorCount++] = GetNode(next);
			}
		}
	}

	private void TracePath(Vec2i start, PathNode dest)
	{
		PathNode current = dest;

		while (current.pos != start)
		{
			path.Add(current.pos);
			current = current.parent;
			Assert.IsNotNull(current);
		}

		path.Reverse();
	}

	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOptions(Option.NullChecks, false)]
	public List<Vec2i> FindPath(Vec2i start, Vec2i target)
	{
		if (!generated) return null;

		path.Clear();
		openList.Clear();
		closedList.Clear();

		Assert.IsTrue(room.InBounds(start));
		Assert.IsTrue(room.InBounds(target));
		Assert.IsTrue(start != target);
		Assert.IsTrue(grid[start.x, start.y].passable);
		Assert.IsTrue(grid[target.x, target.y].passable);

		openList.Add(start, GetNode(start));

		while (openList.Count > 0)
		{
			PathNode current = openList.First;
			openList.RemoveFirst(current.pos, current);
			closedList.Add(current);

			if (current.pos == target)
			{
				Debug.Log("Ended with " + openList.Count + " items in the open list.");
				TracePath(start, current);
				return path;
			}

			GetSuccessors(current, current.pos);

			for (int i = 0; i < successorCount; i++)
			{
				PathNode next = successors[i];

				if (closedList.Contains(next))
					continue;

				Vec2i nP = next.pos;
				int cost = grid[nP.x, nP.y].cost;
				int newG = current.g + cost;

				PathNode node;
				if (!openList.TryGetValue(next.pos, out node))
				{
					next.g = newG;
					next.h = ComputeHeuristic(nP, target);
					next.f = next.g + next.h;
					next.parent = current;
					openList.Add(next.pos, next);
				}
				else 
				{
					if (newG < node.g)
					{
						openList.Remove(node);
						node.g = newG;
						node.f = node.g + node.h;
						node.parent = current;
						openList.Add(node);
					}
				}
			}
		}

		return null;
	}

	[Conditional("DEBUG")]
	public void TestUpdate()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
		{
			Debug.Log("Testing pathfinding. Click two cells to compute the path.");
			World.Instance.StopAllCoroutines();
			World.Instance.StartCoroutine(GetTestInput());
		}
	}

	private IEnumerator GetTestInput()
	{
		Vec2i start = Vec2i.MinValue;
		Vec2i end = Vec2i.MinValue;

		while (start == Vec2i.MinValue)
		{
			if (Input.GetMouseButtonDown(0))
				start = new Vec2i(Camera.main.ScreenToWorldPoint(Input.mousePosition));

			yield return null;
		}

		while (end == Vec2i.MinValue)
		{
			if (Input.GetMouseButtonDown(0))
				end = new Vec2i(Camera.main.ScreenToWorldPoint(Input.mousePosition));

			yield return null;
		}

		List<Vec2i> path = FindPath(start, end);

		if (path == null)
		{
			Debug.Log("Path could not be found.");
			yield break;
		}

		Vector3[] v3 = new Vector3[path.Count];

		for (int i = 0; i < v3.Length; i++)
		{
			Vector3 v = path[i].ToVector3();
			v.x += 0.5f;
			v.y += 0.5f;
			v3[i] = v;
		}

		GameObject line = new GameObject("Debug Path");
		LineRenderer lR = line.AddComponent<LineRenderer>();
		lR.widthMultiplier = 0.2f;
		lR.positionCount = v3.Length;
		lR.SetPositions(v3);

		start = Vec2i.MinValue;
		end = Vec2i.MinValue;

		GameObject.Destroy(line, 8.0f);
		Debug.Log("Path computed. Line will disappear in 15 seconds.");
	}
}
