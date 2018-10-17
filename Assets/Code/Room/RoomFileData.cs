//
// Copyright (c) 2018 Jason Bricco
//

using System;
using System.Collections.Generic;

[Serializable]
public sealed class RoomFileData
{
	public List<int> tileData = new List<int>(256);
	public Entity[] entities;
	public int roomType;
}
