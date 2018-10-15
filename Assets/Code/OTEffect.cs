//
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

	// If true, this effect will be removed once the timer reaches 0.
	public bool remove;

	public OTEffect(OTEffectType type, float timer = 0.0f)
	{
		this.type = type;
		this.timer = timer;
		remove = false;
	}

	public bool Equals(OTEffect other) 
		=> type == other.type;

	public static implicit operator OTEffect(OTEffectType type)
		=> new OTEffect(type);
}
