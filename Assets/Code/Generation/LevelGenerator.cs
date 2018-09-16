//
// Copyright (c) 2018 Jason Bricco
//

public class LevelGenerator
{
	public virtual void Generate(Level level, LevelEntities entities, out Vec2i spawnRoom, out Vec2i spawnCell)
	{
		spawnRoom = Vec2i.Zero;
		spawnCell = Vec2i.Zero;
	}
}
