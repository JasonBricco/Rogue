//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public struct FadeInfo
{
	public bool fadeIn;
	public float fadeR, fadeG, fadeB;
	public bool pauseDuring;

	public FadeInfo(bool fadeIn, float fadeR, float fadeG, float fadeB, bool pauseDuring)
	{
		this.fadeIn = fadeIn;
		this.fadeR = fadeR;
		this.fadeG = fadeG;
		this.fadeB = fadeB;
		this.pauseDuring = pauseDuring;
	}
}
