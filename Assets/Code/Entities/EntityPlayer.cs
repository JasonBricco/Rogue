//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

[RequireComponent(typeof(EntityTimer))]
public sealed class EntityPlayer : MonoBehaviour
{
	private Entity entity;
	private EntityHealth entityHealth;
	private EntityTimer timer;

	private EntityType projectile = EntityType.Arrow;

	private void Awake()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();
		entityHealth = GetComponent<EntityHealth>();
		entity.SetFlag(EntityFlags.InvincibleFrames);

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.HealthChanged, HealthChanged);
	}

	public void OnSpawn()
	{
		entity.velocity = Vector2.zero;
		entityHealth.FullHeal();

		EntityLight light = GetComponent<EntityLight>();
		light.MakePersist();
	}

	private void HealthChanged()
	{
		EventManager.Instance.TriggerEvent(GameEvent.PlayerHealthModifed, entityHealth.Health);
	}

	private void Respawn()
	{
		World.Instance.Room.Entities.SpawnPlayer();
		entity.gameObject.SetActive(true);
		entity.UnsetFlag(EntityFlags.Dead);
	}

	private void UpdateComponent()
	{
		Room room = World.Instance.Room;

		if (entity.HasFlag(EntityFlags.Dead))
			World.Instance.Invoke(Respawn, 2.0f);
		else
		{
			if (Input.GetKey(KeyCode.K))
				entity.SetFlag(EntityFlags.Dead);

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
					Entity proj = room.Entities.FireProjectile(entity.Pos + fireDir.ToVector2(), GetNumericDirFull(fireDir), projectile);
					room.Collision.AddCollisionRule(proj, entity);
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

			entity.Move(accel.ToVector2());
		}
	}

	private void Kill()
	{
		gameObject.SetActive(false);
	}
}
