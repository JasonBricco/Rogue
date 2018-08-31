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
	public int Health { get; private set; }

	private WaitForSeconds wait = new WaitForSeconds(0.1f);

	private void Start()
	{
		entity = GetComponent<Entity>();
		SetHealth(maxHealth);
	}

	public void SetHealth(int health)
	{
		Health = health;
		entity.InvokeEvent(EntityEvent.HealthChanged);
	}

	public void ApplyDamage(int damage)
	{
		Health -= damage;

		if (Health <= 0)
			entity.SetFlag(EntityFlags.Dead);
		else
		{
			if (invincibleOnDamage)
			{
				entity.SetFlag(EntityFlags.Invincible);
				StartCoroutine(ResetInvincibility());
			}
		}

		entity.InvokeEvent(EntityEvent.HealthChanged);
	}

	private IEnumerator ResetInvincibility()
	{
		yield return wait;
		entity.UnsetFlag(EntityFlags.Invincible);
	}
}
