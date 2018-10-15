//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class UIHealth : MonoBehaviour
{
	[SerializeField] private GameObject heartPrefab;

	private Transform t;

	private Stack<GameObject> pool = new Stack<GameObject>();
	private List<GameObject> active = new List<GameObject>();

	private void Awake()
	{
		t = GetComponent<Transform>();
		EventManager.Instance.ListenForEvent<int>(GameEvent.PlayerHealthModifed, UpdateDisplay);
	}

	private GameObject GetHeart()
	{
		if (pool.Count > 0)
		{
			GameObject heart = pool.Pop();
			heart.SetActive(true);
			return heart;
		}
		else return ObjectPool.Get(heartPrefab);
	}

	private void ReturnHearts()
	{
		for (int i = active.Count - 1; i >= 0; i--)
		{
			GameObject heart = active[i];
			heart.SetActive(false);
			active.RemoveAt(i);
			pool.Push(heart);
		}
	}

	private void AddHearts(int health)
	{
		for (int i = 0; i < health; i++)
		{
			GameObject heart = GetHeart();
			heart.transform.SetParent(t, false);
			active.Add(heart);
		}
	}

	private void UpdateDisplay(int health)
	{
		ReturnHearts();
		AddHearts(health);
	}
}
