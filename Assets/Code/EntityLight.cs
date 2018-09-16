//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityLight : MonoBehaviour
{
	[SerializeField] private GameObject lightPrefab;
	private new Transform light;

	private Entity entity;

	private void Start()
	{
		entity = GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	private void UpdateComponent()
	{
		if (light == null)
			light = Instantiate(lightPrefab).GetComponent<Transform>();

		light.position = entity.Pos;
	}
}
