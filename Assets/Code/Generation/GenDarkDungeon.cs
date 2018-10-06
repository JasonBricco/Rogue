//
// Copyright (c) 2018 Jason Bricco
//

public sealed class GenDarkDungeon : GenDungeon
{
	protected override void Init(Room room, Vec2i roomP)
	{
		room.Init(roomP, 32, 18, RoomType.DarkDungeon);
	}

	public override void SetProperties(GameCamera cam)
	{
		cam.SetFixed();
		SetLightMode(true);
	}
}
