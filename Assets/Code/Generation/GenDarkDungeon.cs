//
// Copyright (c) 2018 Jason Bricco
//

public sealed class GenDarkDungeon : GenDungeon
{
	protected override void Init(Room room, Vec2i roomP)
		=> room.Init(roomP, 32, 18, RoomType.DarkDungeon);

	protected override void GenerateInternal(Room room, Vec2i roomP, bool initial)
	{
		base.GenerateInternal(room, roomP, initial);
		room.SetTile(25, 11, Room.Main, TileType.Torch);
	}

	public override void SetProperties(GameCamera cam)
	{
		cam.SetFixed();
		SetLightMode(true);
	}
}
