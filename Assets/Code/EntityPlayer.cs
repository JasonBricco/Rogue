//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static UnityEngine.Mathf;
using static Utils;

[RequireComponent(typeof(EntityTimer))]
public sealed class EntityPlayer : MonoBehaviour
{
	private Entity entity;
	private EntityType projectile = EntityType.Arrow;
	private EntityTimer timer;
	public float RespawnTime { get; private set; }

	private void Start()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();
		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
	}

	private void UpdateComponent()
	{
		RespawnTime -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.Alpha1))
			projectile = EntityType.Arrow;

		if (Input.GetKeyDown(KeyCode.Alpha2))
			projectile = EntityType.PiercingArrow;

		if (Input.GetKeyDown(KeyCode.Alpha3))
			projectile = EntityType.Fireball;

		if (timer.Value <= 0.0f)
		{
			Vec2i fireDir = new Vec2i((int)Input.GetAxisRaw("FireX"), (int)Input.GetAxisRaw("FireY"));

			if (fireDir != Vec2i.Zero)
			{
				entity.facing = GetNumericDir(fireDir);
				Entity proj = entity.Entities.FireProjectile(entity.TilePos + fireDir, entity.facing, projectile);
				entity.Entities.AddCollisionRule(proj, entity);
				timer.SetValue(0.25f);
			}
		}

		Vec2i accel = new Vec2i((int)Input.GetAxisRaw("MoveX"), (int)Input.GetAxisRaw("MoveY"));

		if (accel != Vec2i.Zero)
		{
			if (Input.GetKey(KeyCode.LeftShift))
				entity.speed = 200.0f;
			else entity.ResetSpeed();

			Vec2i facing = Vec2i.Directions[entity.facing];

			if ((facing.x != 0 && facing.x != accel.x) || (facing.y != 0 && facing.y != accel.y))
				entity.facing = GetNumericDir(accel);
		}

		Vector2 move = accel.ToVector2();

		float moveLength = move.sqrMagnitude;

		if (moveLength > 1.0f)
			move *= (1.0f / Sqrt(moveLength));

		entity.Move(move);
	}

	private void Kill()
	{
		RespawnTime = 2.0f;
	}
}
