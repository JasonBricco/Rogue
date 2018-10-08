//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

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
			int newWidth = Mathf.Min(Screen.width + 16, Screen.currentResolution.width);
			int newHeight = Mathf.Min(Screen.height + 9, Screen.currentResolution.height);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Minus))
		{
			int newWidth = Mathf.Max(Screen.width - 16, 480);
			int newHeight = Mathf.Max(Screen.height - 9, 270);
			Screen.SetResolution(newWidth, newHeight, false);
		}

		if (Input.GetKey(KeyCode.Backspace))
			Screen.SetResolution(1024, 576, false);
	}
}
