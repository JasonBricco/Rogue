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
		public int scoreModifier;

		public PathCell(bool passable, int scoreModifier)
		{
			this.passable = passable;
			this.scoreModifier = scoreModifier;
		}

		public override string ToString()
		{
			return "Passable: " + passable + ", Score: " + scoreModifier;
		}
	}

	private class PathNode : IEquatable<PathNode>
	{
		public Vec2i pos;
		public int g, f, h;
		public PathNode parent;
		private RoomPathfinding rP;

		public PathNode(RoomPathfinding rP)
		{
			this.rP = rP;
		}

		public bool Equals(PathNode other)
			=> pos == other.pos;

		public override string ToString()
		{
			return pos.ToString() + ", F: " + f;
		}

		~PathNode()
		{
			rP.ReturnNode(this);
			GC.ReRegisterForFinalize(this);
		}
	}

	private Room room;
	private PathCell[,] grid;

	private Queue<PathNode> nodePool = new Queue<PathNode>();

	private LinkedList<PathNode> openList = new LinkedList<PathNode>();
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
							grid[x, y] = new PathCell(false, 0);
						else
						{
							TileCollider tC = col.GetComponent<TileCollider>();
							grid[x, y] = new PathCell(true, tC.scoreModifier);
						}
					}
					else grid[x, y] = new PathCell(true, 0);
				}
			}

			generated = true;
		}
	}

	private PathNode GetNode(Vec2i p)
	{
		PathNode node;

		if (nodePool.Count > 0)
			node = nodePool.Dequeue();
		else node = new PathNode(this);

		node.pos = p;
		return node;
	}

	private void ReturnNode(PathNode node)
	{
		node.f = 0;
		node.g = 0;
		node.h = 0;
		nodePool.Enqueue(node);
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

	private void AddToOpenList(PathNode next)
	{
		Assert.IsTrue(grid[next.pos.x, next.pos.y].passable);

		if (openList.Count == 0)
			openList.AddFirst(next);
		else
		{
			LinkedListNode<PathNode> node = openList.First;
			while (true)
			{
				if (node.Value.f > next.f)
				{
					openList.AddBefore(node, next);
					break;
				}

				if (node.Next == null)
				{
					openList.AddAfter(node, next);
					break;
				}
				else node = node.Next;
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

		openList.Clear();
		closedList.Clear();

		Assert.IsTrue(room.InBounds(start));
		Assert.IsTrue(room.InBounds(target));
		Assert.IsTrue(start != target);
		Assert.IsTrue(grid[start.x, start.y].passable);
		Assert.IsTrue(grid[target.x, target.y].passable);

		openList.AddFirst(GetNode(start));

		while (openList.Count > 0)
		{
			if (openList.Count > 1000) return null;

			PathNode current = openList.First.Value;
			openList.RemoveFirst();
			closedList.Add(current);
			GetSuccessors(current, current.pos);

			for (int i = 0; i < successorCount; i++)
			{
				PathNode next = successors[i];
				Vec2i nP = next.pos;

				if (nP == target)
				{
					next.parent = current;
					TracePath(start, next);
					return path;
				}

				if (closedList.Contains(next))
					continue;

				LinkedListNode<PathNode> existingOpen = openList.Find(next);

				if (existingOpen == null || existingOpen.Value.f > next.f)
				{
					next.g = current.g + 1;
					next.h = ComputeHeuristic(nP, target);
					next.f = next.g + next.h + grid[nP.x, nP.y].scoreModifier;
					next.parent = current;
					AddToOpenList(next);
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

		GameObject.Destroy(line, 15.0f);
		Debug.Log("Path computed. Line will disappear in 15 seconds.");
	}
}
