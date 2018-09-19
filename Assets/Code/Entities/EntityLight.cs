//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityLight : MonoBehaviour
{
	[SerializeField] private GameObject lightPrefab;
	private Transform tLight;

	private Entity entity;

	private void Start()
	{
		entity = GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		tLight = Instantiate(lightPrefab).GetComponent<Transform>();
	}

	public void MakePersist()
	{
		tLight.gameObject.tag = "Untagged";
	}

	private void UpdateComponent()
	{
		tLight.position = entity.Pos;
	}

	public void Enable()
	{
		tLight.position = entity.Pos;
		tLight.gameObject.SetActive(true);
	}

	public void Disable()
	{
		tLight.gameObject.SetActive(false);
	}
}
