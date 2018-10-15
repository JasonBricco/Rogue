//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public sealed class TileCollider : MonoBehaviour, IPoolable
{
	[HideInInspector] public TileInstance inst;
	
	// Score modifier for use in pathfinding. A higher score makes
	// it less likely that AI will walk on this tile.
	public int scoreModifier;

	private Transform t;

	private void Awake()
		=> t = GetComponent<Transform>();

	public void Enable() => gameObject.SetActive(true);
	public void Disable() => gameObject.SetActive(false);

	public void SetPosition(int x, int y)
		=> t.position = new Vector3(x, y);

	public void SetTileInstance(TileInstance inst)
		=> this.inst = inst;

	public void ResetObject() { }
}
