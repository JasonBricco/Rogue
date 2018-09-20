//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEditor;

public class TileEditor : EditorWindow
{
	private TileDataList data;
	private Vector2 scroll;

	[MenuItem("Window/Tile Editor")]
	public static void ShowWindow()
	{
		GetWindow<TileEditor>();
	}

	private void LoadData()
	{
		data = AssetDatabase.LoadAssetAtPath<TileDataList>("Assets/Data/TileDataList.asset");

		if (data == null)
		{
			Debug.Log("TileDataList not found, creating a new one.");
			data = CreateInstance<TileDataList>();
			data.Init();
		}
		else Debug.Log("Successfully loaded the TileDataList.");
	}

	private void SaveAsset()
	{
		AssetDatabase.CreateAsset(data, "Assets/Data/TileData.asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void OnGUI()
	{
		if (data == null)
			LoadData();

		if (GUI.Button(new Rect(position.width - 140.0f, 15.0f, 100.0f, 30.0f), "Save"))
			SaveAsset();

		if (GUI.Button(new Rect(position.width - 140.0f, 50.0f, 100.0f, 30.0f), "Reset"))
		{
			data = null;
			return;
		}

		float y = 15.0f;

		scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width), GUILayout.Height(position.height));

		for (int i = 0; i < data.Count; i++)
		{
			TileData td = data[i];

			GUI.contentColor = Color.green;

			EditorGUI.LabelField(new Rect(15.0f, y, 100.0f, 20.0f), td.name, EditorStyles.boldLabel);

			GUI.contentColor = Color.white;

			y += 25.0f;
			td.type = (TileType)EditorGUI.EnumPopup(new Rect(15.0f, y, 120.0f, 20.0f), td.type);

			y += 25.0f;
			EditorGUI.LabelField(new Rect(15.0f, y, 60.0f, 20.0f), "Invisible");
			td.invisible = EditorGUI.Toggle(new Rect(70.0f, y, 15.0f, 25.0f), td.invisible);

			EditorGUI.LabelField(new Rect(100.0f, y, 80.0f, 20.0f), "Has Collider");
			td.hasCollider = EditorGUI.Toggle(new Rect(175.0f, y, 15.0f, 25.0f), td.hasCollider);

			if (td.hasCollider)
			{
				EditorGUI.LabelField(new Rect(205.0f, y, 60.0f, 20.0f), "Trigger");
				td.trigger = EditorGUI.Toggle(new Rect(255.0f, y, 15.0f, 25.0f), td.trigger);

				y += 25.0f;
				td.colliderSize = EditorGUI.Vector2Field(new Rect(15.0f, y, 120.0f, 20.0f), "Collider Size", td.colliderSize);
				td.colliderOffset = EditorGUI.Vector2Field(new Rect(150.0f, y, 120.0f, 20.0f), "Collider Offset", td.colliderOffset);
				y += 15.0f;
			}

			if (!td.invisible)
			{
				y += 25.0f;
				td.align = EditorGUI.Vector2Field(new Rect(15.0f, y, 120.0f, 20.0f), "Align", td.align);

				y += 45.0f;
				td.sprite = (Sprite)EditorGUI.ObjectField(new Rect(15.0f, y, 150.0f, 15.0f), td.sprite, typeof(Sprite), false);
				td.color = EditorGUI.ColorField(new Rect(180.0f, y, 150.0f, 15.0f), td.color);
				y += 20.0f;

				td.material = (Material)EditorGUI.ObjectField(new Rect(15.0f, y, 150.0f, 15.0f), td.material, typeof(Material), false);
			}

			y += 25.0f;
			td.component = (TileComponent)EditorGUI.ObjectField(new Rect(15.0f, y, 160.0f, 15.0f), td.component, typeof(TileComponent), false);

			y += 30.0f;
		}

		GUILayout.Space(y - 15.0f);
		EditorGUILayout.EndScrollView();
	}
}
