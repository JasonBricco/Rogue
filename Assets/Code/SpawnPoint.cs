//
// Copyright (c) 2018 Jason Bricco
//

public struct SpawnPoint
{
	public Vec2i room, cell;
	public int facing;

	public SpawnPoint(Vec2i room, int x, int y, int facing)
	{
		this.room = room;
		this.facing = facing;
		cell = new Vec2i(x, y);
	}
}
