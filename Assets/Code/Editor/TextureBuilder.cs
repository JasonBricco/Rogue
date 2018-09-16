// Adventure
// Jason Bricco

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using static Utils;
using static UnityEngine.Mathf;

public sealed class TextureBuilder : EditorWindow
{
	private string lightName;
	private int radius;
	private float intensity;

	private string flatName;
	private Color flatColor;
	private int flatWidth, flatHeight;

	[MenuItem("Tools/Texture Builder")]
	public static void OpenLightBuilder()
	{
		GetWindow(typeof(TextureBuilder), false, "Light Builder", true);
	}

	private void DrawDivider()
	{
		GUILayout.Box(String.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(4));
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Light Texture");
		EditorGUILayout.Space();

		lightName = EditorGUILayout.TextField("Name", lightName);
		EditorGUILayout.Space();

		radius = EditorGUILayout.IntField("Radius (Tiles)", radius);
		EditorGUILayout.Space();

		intensity = EditorGUILayout.FloatField("Intensity (0-1)", intensity);
		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
			BuildLight();

		EditorGUILayout.Space();
		DrawDivider();
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Flat Texture");
		EditorGUILayout.Space();

		flatName = EditorGUILayout.TextField("Name", flatName);
		EditorGUILayout.Space();

		flatWidth = EditorGUILayout.IntField("Width", flatWidth);
		flatHeight = EditorGUILayout.IntField("Height", flatHeight);
		EditorGUILayout.Space();

		flatColor = EditorGUILayout.ColorField("Color", flatColor);
		EditorGUILayout.Space();

		if (GUILayout.Button("Generate"))
			BuildFlat();
	}

	private void BuildLight()
	{
		radius *= 32;

		Texture2D tex = new Texture2D(radius * 2, radius * 2);
		Vector2Int center = new Vector2Int(tex.width / 2, tex.height / 2);

		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
			{
				int valueInCircle = Square(x - center.x) + Square(y - center.y);

				if (valueInCircle < Square(radius))
				{
					float dist = Vector2Int.Distance(new Vector2Int(x, y), center);
					float col = (1.0f - (dist / radius)) * intensity;
					col = Pow(col, 2.0f);

					Color final = new Color(col, col, col, 1.0f);
					tex.SetPixel(x, y, final);
				}
				else tex.SetPixel(x, y, Color.black);
			}
		}

		string path = Application.dataPath + "/Sprites/" + lightName + ".png";
		File.WriteAllBytes(path, tex.EncodeToPNG());
		AssetDatabase.Refresh();
	}

	private void BuildFlat()
	{
		Texture2D tex = new Texture2D(flatWidth, flatHeight);

		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
				tex.SetPixel(x, y, flatColor);
		}

		string path = Application.dataPath + "/Sprites/" + flatName + ".png";
		File.WriteAllBytes(path, tex.EncodeToPNG());
		AssetDatabase.Refresh();
	}
}
