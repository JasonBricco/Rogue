//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityTimer : MonoBehaviour
{
	public float Value { get; private set; }

	private void Start()
	{
		GetComponent<Entity>().ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	private void UpdateComponent()
	{
		Value -= Time.deltaTime;
	}

	public void SetValue(float value)
	{
		Value = value;
	}
}
