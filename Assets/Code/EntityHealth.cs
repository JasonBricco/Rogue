//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections;

public sealed class EntityHealth : MonoBehaviour
{
	[SerializeField] private int maxHealth;
	[SerializeField] private bool invincibleOnDamage;

	private Entity entity;
	private int health;

	private WaitForSeconds wait = new WaitForSeconds(0.1f);

	private void Start()
	{
		entity = GetComponent<Entity>();
		health = maxHealth;
	}

	public void ApplyDamage(int damage)
	{
		health -= damage;

		if (health <= 0)
			entity.SetFlag(EntityFlags.Dead);
		else
		{
			if (invincibleOnDamage)
			{
				entity.SetFlag(EntityFlags.Invincible);
				StartCoroutine(ResetInvincibility());
			}
		}
	}

	private IEnumerator ResetInvincibility()
	{
		yield return wait;
		entity.UnsetFlag(EntityFlags.Invincible);
	}
}
