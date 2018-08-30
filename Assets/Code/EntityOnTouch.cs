//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public enum KnockbackType
{
	ConstantDirection,
	VariableDirection
}

public sealed class EntityOnTouch : MonoBehaviour
{
	[SerializeField] private int damage;
	[SerializeField] private bool knockback;
	[SerializeField] private float force;
	[SerializeField] private KnockbackType type;

	public int Damage
	{
		get { return damage; }
	}

	public bool Knockback
	{
		get { return knockback; }
	}

	public float KnockbackForce
	{
		get { return force; }
	}

	public KnockbackType KnockbackType
	{
		get { return type; }
	}
}
