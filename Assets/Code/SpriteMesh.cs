//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public sealed class SpriteMesh
{
	public Mesh Mesh { get; private set; }
	public Material Material { get; private set; }

	private List<Vector3> vertices;
	private List<Vector3> uvs;
	private List<int> indices;
	private List<Color32> colors;

	public SpriteMesh(Material material)
	{
		vertices = new List<Vector3>(1024);
		uvs = new List<Vector3>(1024);
		indices = new List<int>(1536);
		colors = new List<Color32>(1024);

		Material = material;
	}

	[Il2CppSetOptions(Option.NullChecks, false)]
	public void AddRect(float x, float y, float z, int width, int height, float tex, Vector3 renderOffset, Color32 color)
	{
		x += renderOffset.x;
		y += renderOffset.y;

		int offset = vertices.Count;

		indices.Add(offset + 2);
		indices.Add(offset + 1);
		indices.Add(offset);

		indices.Add(offset + 3);
		indices.Add(offset + 2);
		indices.Add(offset);

		float w = width / 32.0f;
		float h = height / 32.0f;

		vertices.Add(new Vector3(x + w, y, z));
		vertices.Add(new Vector3(x + w, y + h, z));
		vertices.Add(new Vector3(x, y + h, z));
		vertices.Add(new Vector3(x, y, z));

		uvs.Add(new Vector3(1.0f, 0.0f, tex));
		uvs.Add(new Vector3(1.0f, 1.0f, tex));
		uvs.Add(new Vector3(0.0f, 1.0f, tex));
		uvs.Add(new Vector3(0.0f, 0.0f, tex));

		for (int i = 0; i < 4; i++)
			colors.Add(color);
	}

	public void BuildMesh()
	{
		Mesh = new Mesh();

		Mesh.SetVertices(vertices);
		Mesh.SetUVs(0, uvs);
		Mesh.SetTriangles(indices, 0);
		Mesh.SetColors(colors);

		vertices = null;
		uvs = null;
		indices = null;
		colors = null;
	}

	public void Destroy() => Object.Destroy(Mesh);
}
