//
// Copyright (c) 2018 Jason Bricco
//

public sealed class DungeonDoorBehavior : TileBehavior
{
	public override void OnSet(Room room, int x, int y, Tile tile)
		=> room.ListenForEvent(x, y, tile);

	public override void OnEvent(Room room, int x, int y, TileEvent e)
	{
		switch (e)
		{
			case TileEvent.RoomLocked:
			{
				Tile tile = room.GetTile(x, y, Room.Back);
				room.SetVariant(x, y, Room.Back, tile.variant + 2);
			} break;

			case TileEvent.RoomUnlocked:
			{
				Tile tile = room.GetTile(x, y, Room.Back);
				room.SetVariant(x, y, Room.Back, tile.variant - 2);
			} break;
		}

		room.Renderer.FlagForRebuild();
		room.Collision.Generate();
	}
}
