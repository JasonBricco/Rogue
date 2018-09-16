//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using static Utils;

[RequireComponent(typeof(EntityTimer))]
public sealed class EntityPlayer : MonoBehaviour
{
	private Entity entity;
	private Vec2i prevInput;
	private EntityTimer timer;
	private bool yPriority;
	private EntityType projectile = EntityType.Arrow;
	public float RespawnTime { get; set; }

	private void Awake()
	{
		entity = GetComponent<Entity>();
		timer = GetComponent<EntityTimer>();

		entity.ListenForEvent(EntityEvent.Update, UpdateComponent);
		entity.ListenForEvent(EntityEvent.Kill, Kill);
		entity.ListenForEvent(EntityEvent.HealthChanged, HealthChanged);
		entity.ListenForEvent(EntityEvent.SetMove, SetMove);

		entity.SetFlag(EntityFlags.InvincibleFrames);
	}

	public void OnSpawn()
	{
		entity.FullHeal();
		entity.MakeVisible();
	}

	private void HealthChanged()
	{
		EventManager.Instance.TriggerEvent(GameEvent.PlayerHealthModifed, entity.Health);
	}

	private Vec2i SetDirectionY(Entity entity, int input, bool priority)
	{
		if (priority) yPriority = true;
		else yPriority = false;

		if (input == -1) entity.facing = Direction.Back;
		else entity.facing = Direction.Front;

		return new Vec2i(0, input);
	}

	private Vec2i SetDirectionX(Entity entity, int input, bool removePriority)
	{
		if (removePriority)
			yPriority = false;

		if (input == -1) entity.facing = Direction.Left;
		else entity.facing = Direction.Right;

		return new Vec2i(input, 0);
	}

	private void SetMove()
	{
		Vec2i dir = new Vec2i(0, 0);
		Vec2i move = new Vec2i((int)Input.GetAxisRaw("MoveX"), (int)Input.GetAxisRaw("MoveY"));

		if (move.x != 0 && move.y != 0)
		{
			if (yPriority || (move.y != prevInput.y))
				dir = SetDirectionY(entity, move.y, true);
			else dir = SetDirectionX(entity, move.x, false);
		}
		else if (move.x != 0) dir = SetDirectionX(entity, move.x, true);
		else if (move.y != 0) dir = SetDirectionY(entity, move.y, false);
		else yPriority = false;

		prevInput = move;

		CollideResult result;
		entity.Entities.SetMove(entity, dir, 1, out result);
	}

	private void UpdateComponent()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
			projectile = EntityType.Arrow;

		if (Input.GetKeyDown(KeyCode.Alpha2))
			projectile = EntityType.PiercingArrow;

		if (Input.GetKeyDown(KeyCode.Alpha3))
			projectile = EntityType.Fireball;

		if (!entity.IsMoving())
		{
			SetMove();

			if (timer.Value <= 0.0f && !entity.IsMoving())
			{
				Vec2i fireDir = new Vec2i((int)Input.GetAxisRaw("FireX"), 0);

				if (fireDir.x == 0)
					fireDir.y = (int)Input.GetAxisRaw("FireY");

				if (fireDir != Vec2i.Zero)
				{
					entity.facing = GetNumericDir(fireDir);
					entity.Entities.FireProjectile((entity.TilePos + fireDir).ToVector2(), entity.facing, projectile);
					timer.SetValue(0.25f);
				}
			}
		}

		if (Input.GetKey(KeyCode.LeftShift))
			entity.speed = 15.0f;
		else entity.ResetSpeed();

		entity.Move();
	}

	private void Kill()
	{
		entity.MakeInvisible();
		RespawnTime = 2.0f;
	}
}
