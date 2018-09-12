//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public enum LevelType
{
	Plains, Dungeon, Count
}

public enum Layer
{
	None,
	Player,
	Enemy,
	Familiar,
	Projectile,
	PiercingProjectile,
	Obstacle,
	TriggerObstacle,
	Count
};

public sealed class LevelManager : MonoBehaviour
{
	[SerializeField] private Entity[] entityPrefabs;

	private LevelGenerator[] generators =
	{
		new GenPlains(),
		new GenDungeon()
	};

	private Level level;

	public Entity[] EntityPrefabs
	{
		get { return entityPrefabs; }
	}

	private void Start()
	{
		Array.Sort(entityPrefabs);
		level = new Level(generators[1]);
	}

	private void Update()
	{
		level.Update();
		level.Draw();
	}

	/// <summary>
	/// Changes the level to the given level type. The previous level is destroyed.
	/// </summary>
	public void ChangeLevel(LevelType type)
	{
		level.Destroy();
		level = new Level(generators[(int)type]);
		GC.Collect();
	}
}
