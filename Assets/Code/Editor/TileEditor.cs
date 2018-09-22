//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

public class TileEditor : EditorWindow
{
	private TileDataList data;
	private Vector2 scroll;
	private Material defaultMat;

	private bool[] collapsed = new bool[0];

	[MenuItem("Window/Tile Editor")]
	public static void ShowWindow()
	{
		GetWindow<TileEditor>("Tile Editor");
	}

	private void LoadData()
	{
		data = AssetDatabase.LoadAssetAtPath<TileDataList>("Assets/Data/TileData.asset");

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
		if (!AssetDatabase.Contains(data))
			AssetDatabase.CreateAsset(data, "Assets/Data/TileData.asset");

		EditorUtility.SetDirty(data);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void ResetData()
	{
		AssetDatabase.DeleteAsset("Assets/Data/TileData.asset");
		data = null;
	}

	private void OnGUI()
	{
		if (!defaultMat)
		{
			defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteArray.mat");
			Assert.IsNotNull(defaultMat);
		}

		if (data == null)
		{
			LoadData();
			Assert.IsNotNull(data);
		}

		if (data.Count == -1)
		{
			Debug.LogWarning("TileData asset exists, but contains no data!");
			ResetData();
			return;
		}

		if (collapsed.Length != data.Count)
		{
			collapsed = new bool[data.Count];

			for (int i = 0; i < collapsed.Length; i++)
				collapsed[i] = true;
		}

		if (GUI.Button(new Rect(position.width - 140.0f, 15.0f, 100.0f, 30.0f), "Save"))
			SaveAsset();

		if (GUI.Button(new Rect(position.width - 140.0f, 50.0f, 100.0f, 30.0f), "Refresh"))
		{
			if (!data.Refresh())
			{
				Debug.LogWarning("Something went wrong. Resetting the tile data.");
				ResetData();
				return;
			}
		}

		float y = 15.0f;

		scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width), GUILayout.Height(position.height));

		for (int i = 0; i < data.Count; i++)
		{
			TileData td = data[i];

			if (collapsed[i])
			{
				GUI.contentColor = Color.gray;
				EditorGUI.LabelField(new Rect(15.0f, y, 100.0f, 20.0f), td.name, EditorStyles.boldLabel);
				GUI.contentColor = Color.white;

				GUI.color = GUI.color.SetAlpha(0.0f);

				if (GUI.Button(new Rect(15.0f, y, 100.0f, 20.0f), GUIContent.none))
					collapsed[i] = false;

				GUI.color = GUI.color.SetAlpha(1.0f);
				y += 20.0f;
				continue;
			}
			else
			{
				GUI.contentColor = Color.green;
				EditorGUI.LabelField(new Rect(15.0f, y, 100.0f, 20.0f), td.name, EditorStyles.boldLabel);
				GUI.contentColor = Color.white;

				GUI.color = GUI.color.SetAlpha(0.0f);

				if (GUI.Button(new Rect(15.0f, y, 100.0f, 20.0f), GUIContent.none))
					collapsed[i] = true;

				GUI.color = GUI.color.SetAlpha(1.0f);
			}

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

				EditorGUI.BeginChangeCheck();
				td.sprite = (Sprite)EditorGUI.ObjectField(new Rect(15.0f, y, 150.0f, 15.0f), td.sprite, typeof(Sprite), false);

				if (EditorGUI.EndChangeCheck())
				{
					if (td.hasCollider)
					{
						td.colliderSize.x = td.sprite.rect.width / td.sprite.pixelsPerUnit;
						td.colliderSize.y = td.sprite.rect.height / td.sprite.pixelsPerUnit;
					}

					td.baseMaterial = defaultMat;
				}

				td.color = EditorGUI.ColorField(new Rect(180.0f, y, 150.0f, 15.0f), td.color);
				y += 20.0f;

				td.baseMaterial = (Material)EditorGUI.ObjectField(new Rect(15.0f, y, 150.0f, 15.0f), td.baseMaterial, typeof(Material), false);
			}

			y += 25.0f;
			td.component = (TileComponent)EditorGUI.ObjectField(new Rect(15.0f, y, 160.0f, 15.0f), td.component, typeof(TileComponent), false);

			y += 30.0f;
		}

		GUILayout.Space(y - 15.0f);
		EditorGUILayout.EndScrollView();
	}
}
