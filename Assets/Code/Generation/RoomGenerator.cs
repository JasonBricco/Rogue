//
// Copyright (c) 2018 Jason Bricco
//

public abstract class RoomGenerator
{
	public abstract Vec2i SpawnCell();
	public abstract void Init(World world);
	public abstract void Generate(World world, Vec2i roomP, RoomEntities entities);
}
