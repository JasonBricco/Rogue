//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;

public delegate void EventHandler<T>(T arg);

public sealed class EventManager : MonoBehaviour
{
	private static EventManager instance;

	public static EventManager Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<EventManager>();

			return instance;
		}
	}

	private Delegate[] handlers = new Delegate[(int)GameEvent.Count];

	public void ListenForEvent<T>(GameEvent type, EventHandler<T> handler)
	{
		int i = (int)type;
		EventHandler<T> current = (EventHandler<T>)handlers[i];
		current += handler;
		handlers[i] = current;
	}

	public void TriggerEvent<T>(GameEvent type, T arg)
	{
		EventHandler<T> handler = (EventHandler<T>)handlers[(int)type];
		handler.Invoke(arg);
	}
}
