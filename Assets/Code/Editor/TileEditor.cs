//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEditor;

public class TileEditor : EditorWindow
{
	private TileDataList data;
	private Vector2 scroll;

	private GUIStyle boldStyle;

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
		// if (boldStyle == null)
			// boldStyle = new GUIStyle(G)

		if (data == null)
			LoadData();

		if (GUI.Button(new Rect(Screen.width - 140.0f, 15.0f, 100.0f, 30.0f), "Save"))
			SaveAsset();

		float y = 15.0f;

		scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width), GUILayout.Height(position.height));

		for (int i = 0; i < data.Count; i++)
		{
			TileData td = data[i];
			GUI.Label(new Rect(15.0f, y, 100.0f, 20.0f), td.name);

			y += 25.0f;
			td.type = (TileType)EditorGUI.EnumPopup(new Rect(15.0f, y, 120.0f, 20.0f), td.type);

			y += 25.0f;
			EditorGUI.LabelField(new Rect(15.0f, y, 60.0f, 20.0f), "Invisible");
			td.invisible = EditorGUI.Toggle(new Rect(75.0f, y, 15.0f, 25.0f), td.invisible);

			y += 30.0f;
		}

		GUILayout.Space(y - 15.0f);
		EditorGUILayout.EndScrollView();
	}
}
