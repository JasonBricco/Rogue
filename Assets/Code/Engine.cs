//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using static Utils;

public sealed class Engine : MonoBehaviour
{
	public const int AspectX = 16, AspectY = 9;

	private Vec2i lastScreenSize;

	private void Awake()
	{
		Assert.raiseExceptions = true;
		Screen.SetResolution(1024, 576, false);
		lastScreenSize = new Vec2i(Screen.width, Screen.height);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		if (Input.GetKeyDown(KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;

		Vec2i screenSize = new Vec2i(Screen.width, Screen.height);

		if (screenSize != lastScreenSize)
		{
			int dstW = FloorToNearestMultiple(screenSize.x, AspectX);
			int dstH = (dstW / AspectX) * AspectY;

			if (dstH > screenSize.y)
			{
				dstH = FloorToNearestMultiple(screenSize.y, AspectY);
				dstW = (dstH / AspectY) * AspectX;
			}

			Screen.SetResolution(dstW, dstH, false);
			lastScreenSize = screenSize;
		}
	}
}
