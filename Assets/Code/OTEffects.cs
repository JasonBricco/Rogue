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
		if (effects.TryGetValue(key, out List<OTEffect> list))
			list.Add(value);
		else
		{
			list = new List<OTEffect>();
			list.Add(value);
			effects[key] = list;
		}
	}

	public void RemoveAll(Entity key)
		=> effects.Remove(key);

	public void FlagForRemoval(Entity key, OTEffectType effect)
	{
		if (effects.TryGetValue(key, out List<OTEffect> list))
		{
			int index = list.IndexOf(effect);

			if (index != -1)
			{
				OTEffect e = list[index];
				e.remove = true;
				list[index] = e;
			}
		}
	}

	public bool Exists(Entity key, OTEffectType type)
	{
		if (effects.TryGetValue(key, out List<OTEffect> list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == type)
					return true;
			}
		}

		return false;
	}

	private bool ApplyEffect(Entity entity, OTEffectType type, ref float timer)
	{
		switch (type)
		{
			case OTEffectType.Spikes:
			{
				entity.GetComponent<EntityHealth>()?.ApplyDamage(1);
				timer += 0.5f;
			} break;
		}

		return true;
	}

	public void Apply()
	{
		foreach (var pair in effects)
		{
			Entity entity = pair.Key;
			List<OTEffect> effects = pair.Value;

			for (int i = effects.Count - 1; i >= 0; i--)
			{
				OTEffect effect = effects[i];
				effect.timer -= Time.deltaTime;

				if (effect.timer <= 0.0f)
				{
					if (effect.remove)
						effects.RemoveAt(i);
					else
					{
						if (!ApplyEffect(entity, effect.type, ref effect.timer))
							effects.RemoveAt(i);
						else effects[i] = effect;
					}
				}
				else effects[i] = effect;
			}
		}
	}
}
