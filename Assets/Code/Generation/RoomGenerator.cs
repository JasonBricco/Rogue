//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public class RoomGenerator
{
	public void Generate(Room room, GameCamera cam, Vec2i roomP, TileInstance? from, out SpawnPoint spawn)
	{
		Init(room, roomP);
		GenerateInternal(room, roomP, from, out spawn);
		room.OnGenerate();
		SpawnEntities(room);
		SetProperties(cam);
	}

	protected virtual void GenerateInternal(Room room, Vec2i roomP, TileInstance? from, out SpawnPoint spawn)
		=> throw new NotImplementedException();

	protected virtual void SpawnEntities(Room room) { }

	protected virtual void Init(Room room, Vec2i roomP)
		=> throw new NotImplementedException();

	public virtual void SetProperties(GameCamera cam)
		=> throw new NotImplementedException();

	protected void SetLightMode(bool dark)
	{
		if (dark)
		{
			Color col = new Color(0.02f, 0.02f, 0.02f, 1.0f);
			RenderSettings.ambientLight = col;
			Camera.main.backgroundColor = Color.black;
		}
		else
		{
			RenderSettings.ambientLight = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			Camera.main.backgroundColor = new Color(0.58f, 0.8f, 1.0f, 1.0f);
		}
	}
}
