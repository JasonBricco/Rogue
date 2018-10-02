//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections;

public sealed class EntityHealth : MonoBehaviour
{
	[SerializeField] private int maxHealth;

	private Entity entity;
	private EntityImage image;
	public int Health { get; private set; }

	private WaitForSeconds wait = new WaitForSeconds(0.1f);

	private void Awake()
	{
		entity = GetComponent<Entity>();
		image = GetComponent<EntityImage>();

		FullHeal();
	}

	public void SetHealth(int health)
	{
		Health = health;
		entity.InvokeEvent(EntityEvent.HealthChanged);
	}

	public void FullHeal()
	{
		SetHealth(maxHealth);
	}

	public void ApplyDamage(int damage)
	{
		Health -= damage;

		if (Health <= 0)
			entity.SetFlag(EntityFlags.Dead);
		else
		{
			image.TintRed(wait);

			if (entity.HasFlag(EntityFlags.InvincibleFrames))
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
