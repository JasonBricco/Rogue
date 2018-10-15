// Adventure
// Jason Bricco

using UnityEngine;
using UnityEditor;

public static class MenuTools
{
	[MenuItem("Tools/Open Save Folder")]
	private static void OpenSaveFolder()
		=> EditorUtility.RevealInFinder(Application.persistentDataPath);

	[MenuItem("GameObject/Create Other/Tile Collider")]
	private static void CreateTileCollider()
	{
		GameObject obj = new GameObject("Tile Collider");
		obj.AddComponent<TileCollider>();
		BoxCollider col = obj.AddComponent<BoxCollider>();
		col.size = new Vector3(1.0f, 1.0f, 64.0f);
		obj.layer = LayerMask.NameToLayer("Terrain");
	}
}
