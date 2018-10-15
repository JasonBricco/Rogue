//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public static class ObjectPool
{
	private static Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

	public static GameObject Get(GameObject prefab, Transform parent = null)
	{
		GameObject result;
		IPoolable p = prefab.GetComponent<IPoolable>();

		if (p != null)
		{
			if (!pools.TryGetValue(prefab.name, out Queue<GameObject> pool))
			{
				pool = new Queue<GameObject>();
				pools[prefab.name] = pool;
			}

			if (pool.Count > 0)
			{
				result = pool.Dequeue();
				result.GetComponent<IPoolable>().Enable();
			}
			else result = Instantiate();
		}
		else result = Instantiate();

		return result;

		GameObject Instantiate()
		{
			GameObject obj = Object.Instantiate(prefab);
			obj.name = prefab.name;

			if (parent != null)
				obj.transform.SetParent(parent);

			return obj;
		}
	}

	public static void Return(GameObject obj)
	{
		IPoolable p = obj.GetComponent<IPoolable>();

		if (p != null)
		{
			p.ResetObject();
			p.Disable();
			pools[obj.name].Enqueue(obj);
		}
		else Object.Destroy(obj);
	}

	public static void Clear() => pools.Clear();
}
