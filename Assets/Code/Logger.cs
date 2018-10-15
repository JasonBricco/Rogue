//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using System.IO;
using System.Text;

public sealed class Logger : MonoBehaviour
{
	private string dataPath;

	private void Awake()
	{
		dataPath = Application.persistentDataPath;
		Application.logMessageReceived += HandleError;
	}

	private void HandleError(string logString, string stackTrace, LogType type)
	{
		if (type == LogType.Error)
		{
			Log(logString, stackTrace);
			Engine.SignalQuit = true;
		}
	}

	private void Log(params string[] items)
	{
		StringBuilder text = new StringBuilder(DateTime.Now.ToString() + Environment.NewLine);

		for (int i = 0; i < items.Length; i++)
			text.AppendLine(items[i]);

		File.AppendAllText(dataPath + "/Log.txt", text.ToString() + Environment.NewLine);
	}
}
