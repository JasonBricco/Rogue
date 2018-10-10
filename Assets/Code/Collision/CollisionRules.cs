//
// Copyright (c) 2018 Jason Bricco
//

using System.Collections.Generic;

public sealed class CollisionRules
{
	private Dictionary<Entity, List<Entity>> rules = new Dictionary<Entity, List<Entity>>();

	private void AddInternal(Entity key, Entity value)
	{
		if (rules.TryGetValue(key, out List<Entity> list))
			list.Add(value);
		else
		{
			list = new List<Entity>();
			list.Add(value);
			rules[key] = list;
		}
	}

	public void Add(Entity key, Entity value)
	{
		AddInternal(key, value);
		AddInternal(value, key);
	}

	public void Remove(Entity key)
	{
		if (rules.TryGetValue(key, out List<Entity> values))
		{
			for (int i = 0; i < values.Count; i++)
				rules[values[i]].Remove(key);
		}

		rules.Remove(key);
	}

	public bool Exists(Entity key, Entity value)
	{
		if (rules.TryGetValue(key, out List<Entity> list))
			return list.Contains(value);

		return false;
	}

	public void Clear()
	{
		rules.Clear();
	}
}
