﻿//
// Copyright (c) 2018 Jason Bricco
//

using System;

public enum OTEffectType
{
	None,
	Spikes
};

public struct OTEffect : IEquatable<OTEffect>
{
	public OTEffectType type;
	public float timer;

	public OTEffect(OTEffectType type, float timer)
	{
		this.type = type;
		this.timer = timer;
	}

	public bool Equals(OTEffect other)
	{
		return type == other.type;
	}
}