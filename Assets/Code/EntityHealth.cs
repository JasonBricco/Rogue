//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityHealth : MonoBehaviour
{
	[SerializeField] private int maxHealth;

	private Entity entity;
	private int health;

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
	}
}
