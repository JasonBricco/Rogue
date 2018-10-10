//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class EntityLight : MonoBehaviour
{
	[SerializeField] private GameObject lightPrefab;
	[SerializeField] private float radius;
	[SerializeField] private Vector2 offset;

	private Transform tLight;
	private Entity entity;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		tLight = Instantiate(lightPrefab).GetComponent<Transform>();
		tLight.SetParent(entity.transform);
		UpdateSize();
	}

	private void UpdateSize()
		=> tLight.localScale = new Vector3(radius * 2, radius * 2);

	public void MakePersist()
		=> tLight.gameObject.tag = "Untagged";

	private void UpdateComponent()
		=> tLight.position = entity.Pos + offset;

	private void OnEnable()
		=> tLight.gameObject.SetActive(true);

	public void OnDisable()
	{
		if (tLight != null)
			tLight.gameObject.SetActive(false);
	}

	// Called whenever values in the inspector change.
	private void OnValidate()
	{
		if (tLight != null)
			UpdateSize();
	}
}
