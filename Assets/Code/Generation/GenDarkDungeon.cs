//
// Copyright (c) 2018 Jason Bricco
//

public sealed class GenDarkDungeon : GenDungeon
{
	protected override void Init(Room room, Vec2i roomP)
		=> room.Init(roomP, 32, 18, RoomType.DarkDungeon);

	protected override void GenerateInternal(Room room, Vec2i roomP, TileInstance? from, out SpawnPoint spawn)
	{
		base.GenerateInternal(room, roomP, from, out spawn);
		room.SetTile(25, 11, Room.Main, TileType.Torch);
	}

	public override void SetProperties(GameCamera cam)
	{
		cam.SetFixed();
		SetLightMode(true);
	}
}
