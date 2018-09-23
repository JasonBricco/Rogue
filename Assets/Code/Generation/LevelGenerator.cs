//
// Copyright (c) 2018 Jason Bricco
//

public class LevelGenerator
{
	public virtual void Generate(Level level, LevelEntities entities, out SpawnPoint spawnPoint)
	{
		spawnPoint = new SpawnPoint();
	}
}
