//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

public sealed class RoomPathfinding
{
	private Room room;

	private PathCellInfo[,] grid;
	private Queue<PathComputer> data = new Queue<PathComputer>();

	private bool generated = false;

	public RoomPathfinding(Room room)
		=> this.room = room;

	[Il2CppSetOptions(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOptions(Option.NullChecks, false)]
	public void Generate()
	{
		Assert.IsFalse(generated);
		grid = new PathCellInfo[room.SizeX, room.SizeY];

		for (int y = 0; y < grid.GetLength(1); y++)
		{
			for (int x = 0; x < grid.GetLength(0); x++)
			{
				Vector3 origin = new Vector3(x + 0.5f, y + 0.5f, -100.0f);
				Ray ray = new Ray(origin, Vector3.forward);

				if (Physics.Raycast(ray, out RaycastHit hit, 200.0f, World.Instance.RaycastLayers, QueryTriggerInteraction.Collide))
				{
					Collider col = hit.collider;

					if (!col.isTrigger)
						grid[x, y] = new PathCellInfo(false, false, 1);
					else
					{
						TileCollider tC = col.GetComponent<TileCollider>();

						if (tC != null)
							grid[x, y] = new PathCellInfo(true, true, tC.scoreModifier);
					}
				}
				else grid[x, y] = new PathCellInfo(true, false, 1);
			}
		}

		generated = true;
	}

	private PathComputer GetData()
	{
		if (data.Count > 0)
			return data.Dequeue();

		return new PathComputer(room, grid);
	}

	public void FindPath(Vec2i start, Vec2i target, Stack<Vector2> path, Action callback)
	{
		if (generated && start != target && room.InBounds(target))
		{
			path.Clear();

			Assert.IsTrue(room.InBounds(start));
			Assert.IsTrue(grid[start.x, start.y].passable);
			Assert.IsTrue(grid[target.x, target.y].passable);

			PathComputer d = GetData();
			d.SetInfo(start, target, path);
			d.FindPath(callback);
		}
	}

	public bool EmptyCell(int x, int y)
		=> grid[x, y].passable && !grid[x, y].trigger;
}
