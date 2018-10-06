//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityLight : MonoBehaviour
{
	[SerializeField] private GameObject lightPrefab;
	private Transform tLight;

	private Entity entity;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		tLight = Instantiate(lightPrefab).GetComponent<Transform>();
		tLight.SetParent(entity.transform);
	}

	public void MakePersist()
	{
		tLight.gameObject.tag = "Untagged";
	}

	private void UpdateComponent()
	{
		tLight.position = entity.Pos;
	}

	private void OnEnable()
	{
		tLight.gameObject.SetActive(true);
	}

	public void OnDisable()
	{
		if (tLight != null)
			tLight.gameObject.SetActive(false);
	}
}
