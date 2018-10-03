//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.Mathf;

public sealed class Engine : MonoBehaviour
{
	public static bool Paused { get; set; }

	private void Awake()
	{
		Assert.raiseExceptions = true;
		Screen.SetResolution(1024, 576, false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		if (Input.GetKey(KeyCode.Equals))
		{
			int newWidth = Min(Screen.width + 16, Screen.currentResolution.width);
			int newHeight = Min(Screen.height + 9, Screen.currentResolution.height);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Minus))
		{
			int newWidth = Max(Screen.width - 16, 480);
			int newHeight = Max(Screen.height - 9, 270);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Backspace))
			Screen.SetResolution(1024, 576, false);
	}
}
