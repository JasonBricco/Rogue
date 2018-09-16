//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class LightProcessor : MonoBehaviour
{
	[SerializeField] private Material material;
	private Camera scanner;

	private void Awake()
	{
		scanner = transform.Find("Scanner").GetComponent<Camera>();

		RenderTexture lightmap = new RenderTexture(Screen.width, Screen.height, 0);
		scanner.targetTexture = lightmap;

		material.SetTexture("_Lightmap", lightmap);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}
}
