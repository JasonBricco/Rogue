//
// Copyright (c) 2018 Jason Bricco
//

public struct SpawnPoint
{
	public Vec2i cell;
	public int facing;

	public SpawnPoint(int x, int y, int facing)
	{
		cell = new Vec2i(x, y);
		this.facing = facing;
	}
}
