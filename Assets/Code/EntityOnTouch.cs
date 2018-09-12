//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityOnTouch : MonoBehaviour
{
	[SerializeField] private int damage;
	[SerializeField] private bool knockback;
	[SerializeField] private int knockbackDistance;
	[SerializeField] private bool variableKnockback;
	[SerializeField] private bool dieOnTouch;
	[SerializeField] private bool addCollisionRule;

	public int Damage
	{
		get { return damage; }
	}

	public bool Knockback
	{
		get { return knockback; }
	}

	public int KnockbackCells
	{
		get { return knockbackDistance; }
	}

	public bool VariableKnockback
	{
		get { return variableKnockback; }
	}

	public bool DieOnTouch
	{
		get { return dieOnTouch; }
	}

	public bool AddCollisionRule
	{
		get { return addCollisionRule; }
	}
}
