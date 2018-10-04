//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class EntityImage : MonoBehaviour
{
	[SerializeField] private Sprite[] sprites;
	[SerializeField] private bool directional;

	private Entity entity;
	private SpriteRenderer rend;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		rend = GetComponent<SpriteRenderer>();
		rend.sprite = sprites[0];
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
	}

	public void TintRed(WaitForSeconds wait)
		=> StartCoroutine(DoTintRed(wait));

	private IEnumerator DoTintRed(WaitForSeconds wait)
	{
		rend.color = Color.red;
		yield return wait;
		rend.color = Color.white;
	}

	private void UpdateComponent()
	{
		if (directional) rend.sprite = sprites[entity.facing];
	}
}
