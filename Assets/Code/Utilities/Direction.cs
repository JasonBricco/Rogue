﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public static class Direction
{
	public const int Back = 0;
	public const int Front = 1;
	public const int Left = 2;
	public const int Right = 3;
	public const int FrontLeft = 4;
	public const int FrontRight = 5;
	public const int BackLeft = 6;
	public const int BackRight = 7;

	public static readonly float[] Rotations =
	{
		180.0f, 0.0f, 90.0f, 270.0f,
		45.0f, 315.0f, 135.0f, 225.0f
	};
};
