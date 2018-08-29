//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public static class Direction
{
	public const int Front = 0;
	public const int Back = 1;
	public const int Left = 2;
	public const int Right = 3;
	public const int FrontLeft = 4;
	public const int FrontRight = 5;
	public const int BackLeft = 6;
	public const int BackRight = 7;

	public static Vector3 Rotation(int dir)
	{
		switch (dir)
		{
			case Front:
				return new Vector3(0.0f, 0.0f, 0.0f);

			case Back:
				return new Vector3(0.0f, 0.0f, 180.0f);

			case Left:
				return new Vector3(0.0f, 0.0f, 90.0f);

			case Right:
				return new Vector3(0.0f, 0.0f, 270.0f);

			default:
				return Vector3.zero;
		}
	}
};
