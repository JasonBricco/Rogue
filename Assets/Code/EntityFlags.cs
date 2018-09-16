//
// Copyright (c) 2018 Jason Bricco
//

using System;

[Flags]
public enum EntityFlags
{
	None = 0,
	Dead = 1,
	Rooted = 2,
	EmitsLight = 4,
	InvincibleFrames = 8
}
