//
// Copyright (c) 2018 Jason Bricco
//

public class TileBehavior
{
	public virtual void OnSet(Room room, int x, int y, Tile tile) { }
	public virtual void OnEvent(Room room, int x, int y, TileEvent e) { }
}
