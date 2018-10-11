//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameOver : MonoBehaviour
{
	public void OnRestart()
	{
		SceneManager.LoadScene("Game");
	}

	public void OnQuit()
	{
		Application.Quit();
	}
}
