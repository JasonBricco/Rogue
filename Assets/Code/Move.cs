//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

public struct Move
{
	public Vector2 start, end;
	public Vec2i dir;
	public float speed;
	public int cellsLeft;

	public Move(Vector2 start, float speed, int cells, Vec2i dir)
	{
		this.start = start;
		this.dir = dir;
		this.speed = speed;
		cellsLeft = cells;
		end = start + dir.ToVector2();
	}

	public void ReachedNewCell()
	{
		cellsLeft--;
		end += dir.ToVector2();
	}

	public Vec2i EndCell
	{
		get { return TilePos(end); }
	}

	public bool Active
	{
		get { return cellsLeft > 0; }
	}

	public void ToTileCenter(Vector2 from, Vec2i to)
	{
		start = from;
		end = to.ToVector2();
		dir = new Vec2i((start - end).normalized);
		cellsLeft = 1;
	}

	public void SetStart(Vector2 newP)
	{
		start = newP;
		end = start + dir.ToVector2();
	}
}
