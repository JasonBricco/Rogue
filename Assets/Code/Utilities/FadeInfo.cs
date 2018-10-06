//
// Copyright (c) 2018 Jason Bricco
//

using System;

public struct FadeInfo
{
	public bool fadeIn;
	public float fadeR, fadeG, fadeB;
	public float duration;
	public Action callback;

	public FadeInfo(bool fadeIn, float fadeR, float fadeG, float fadeB, float duration, Action callback)
	{
		this.fadeIn = fadeIn;
		this.fadeR = fadeR;
		this.fadeG = fadeG;
		this.fadeB = fadeB;
		this.duration = duration;
		this.callback = callback;
	}
}
