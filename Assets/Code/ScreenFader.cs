//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class ScreenFader : MonoBehaviour
{
	[SerializeField] private Image overlay;

	private float startAlpha, targetAlpha;

	private bool inProgress;
	private float duration, timeLeft;

	private Action callback;

	private void Awake()
	{
		EventManager.Instance.ListenForEvent<FadeInfo>(GameEvent.Fade, OnFade);
		overlay.color = Color.clear;
	}

	private void OnFade(FadeInfo info)
	{
		overlay.color = new Color(info.fadeR, info.fadeG, info.fadeB, info.fadeIn ? 1.0f : 0.0f);

		startAlpha = overlay.color.a;
		targetAlpha = 1.0f - startAlpha;

		duration = info.duration;
		timeLeft = duration;
		callback = info.callback;

		Engine.Paused = true;
		inProgress = true;
	}

	private void Update()
	{
		if (inProgress)
		{
			float t = 1.0f - (timeLeft / duration);
			float a = Mathf.Lerp(startAlpha, targetAlpha, t);
			overlay.color = overlay.color.SetAlpha(a);
			timeLeft -= Time.deltaTime;

			if (timeLeft <= 0.0f)
			{
				overlay.color = overlay.color.SetAlpha(targetAlpha);
				Engine.Paused = false;
				inProgress = false;
				callback?.Invoke();
			}
		}
	}
}
