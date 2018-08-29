//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody))]
public sealed class EntityProjectileCollision : MonoBehaviour
{
	[SerializeField] private bool piercing;

	private Entity entity;
	private int terrainLayer;

	private void Start()
	{
		entity = GetComponent<Entity>();
		terrainLayer = LayerMask.NameToLayer("Terrain");
	}

	private void OnTriggerEnter(Collider other)
	{
		Entity target = other.GetComponentInParent<Entity>();

		if (target != null)
		{
			if (entity.Entities.CollisionRuleExists(entity, target))
				return;

			entity.Entities.OnTriggerEntity(entity, target);

			if (!piercing) entity.SetFlag(EntityFlags.Dead);
		}
		else
		{
			TileCollider tileCollider = other.GetComponent<TileCollider>();
			Assert.IsNotNull(tileCollider);

			Tile tile = tileCollider.tile;

			if (!tile.Data.trigger && other.gameObject.layer == terrainLayer)
				entity.SetFlag(EntityFlags.Dead);
			else entity.Entities.OnTriggerTile(entity, tileCollider.tile);
		}
	}
}
