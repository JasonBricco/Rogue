//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class ScreenFader : MonoBehaviour
{
	[SerializeField] private Material fadeMaterial;
	private Color fadeColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

	private void Awake()
	{
		EventManager.Instance.ListenForEvent<FadeInfo>(GameEvent.Fade, OnFade);
	}

	private void OnFade(FadeInfo info)
	{
		fadeColor = new Color(info.fadeR, info.fadeG, info.fadeB, info.fadeIn ? 0.0f : 1.0f);


	}

	private void OnPostRender()
	{
		if (fadeColor.a == 0.0f) return;

		fadeMaterial.SetColor("_Color", fadeColor);

		GL.PushMatrix();
		GL.LoadOrtho();

		fadeMaterial.SetPass(0);
		GL.Begin(GL.QUADS);

		GL.Vertex3(0.0f, 0.0f, 0.1f);
		GL.Vertex3(1.0f, 0.0f, 0.1f);
		GL.Vertex3(1.0f, 1.0f, 0.1f);
		GL.Vertex3(0.0f, 1.0f, 0.1f);

		GL.End();
		GL.PopMatrix();
	}
}
