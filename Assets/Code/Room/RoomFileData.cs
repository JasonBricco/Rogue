//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public sealed class RoomFileData
{
	public List<int> tileData = new List<int>(256);
	public int[] entityIds;
	public Vector2[] entityPositions;
	public int roomType;
}
