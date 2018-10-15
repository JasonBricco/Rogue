//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public sealed class Engine : MonoBehaviour
{
	public static bool Paused { get; set; }
	public static bool SignalQuit { get; set; }

	private void Awake()
	{
		Assert.raiseExceptions = true;
		Screen.SetResolution(1024, 576, false);
		Paused = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		if (Input.GetKeyDown(KeyCode.End))
			SceneManager.LoadScene("Game");

		if (SignalQuit)
			Application.Quit();
	}

	private void OnDestroy()
		=> ObjectPool.Clear();
}
