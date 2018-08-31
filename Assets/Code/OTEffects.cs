//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class OTEffects
{
	private Dictionary<Entity, List<OTEffect>> effects = new Dictionary<Entity, List<OTEffect>>();

	public void Add(Entity key, OTEffect value)
	{
		List<OTEffect> list;
		if (effects.TryGetValue(key, out list))
			list.Add(value);
		else
		{
			list = new List<OTEffect>();
			list.Add(value);
			effects[key] = list;
		}
	}

	public void Remove(Entity key)
	{
		effects.Remove(key);
	}

	public bool Exists(Entity key, OTEffectType type)
	{
		List<OTEffect> list;
		if (effects.TryGetValue(key, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == type)
					return true;
			}
		}

		return false;
	}

	private bool ApplyEffect(Entity entity, Level level, OTEffectType type, ref float timer)
	{
		switch (type)
		{
			case OTEffectType.Spikes:
			{
				if (level.GetTile(entity.TilePos) != TileType.Spikes)
					return false;

				entity.GetComponent<EntityHealth>()?.ApplyDamage(1);
				timer += 0.5f;
				break;
			}
		}

		return true;
	}

	private void DecrementTimer(ref OTEffect effect)
	{
		effect.timer -= Time.deltaTime;
	}

	public void Apply(Level level)
	{
		foreach (var pair in effects)
		{
			Entity entity = pair.Key;

			for (int i = 0; i < pair.Value.Count; i++)
			{
				OTEffect effect = pair.Value[i];
				DecrementTimer(ref effect);

				bool success = ApplyEffect(entity, level, effect.type, ref effect.timer);

				if (!success)
				{

				}
			}
		}



		/*auto it = state->effects.begin();

		while (it != state->effects.end())
		{
			OTEffect & effect = it->second;

			effect.timer -= deltaTime;

			if (effect.timer <= 0.0f)
			{
				bool success = ApplyEffectOverTime(effect.entity, level, effect.type, effect.timer);

				if (!success)
				{
					it = state->effects.erase(it);
					continue;
				}
			}

			it++;*/
	}
}
