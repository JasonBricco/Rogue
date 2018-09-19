//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;
using static Utils;

public sealed class Engine : MonoBehaviour
{
	private void Awake()
	{
		Assert.raiseExceptions = true;
		Screen.SetResolution(1024, 576, false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		if (Input.GetKeyDown(KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;

		if (Input.GetKey(KeyCode.Equals))
		{
			int newWidth = Min(Screen.width + 16, Screen.currentResolution.width);
			int newHeight = Min(Screen.height + 9, Screen.currentResolution.height);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Minus))
		{
			int newWidth = Max(Screen.width - 16, 320);
			int newHeight = Max(Screen.height - 9, 180);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Backspace))
			Screen.SetResolution(1024, 576, false);
	}
}
