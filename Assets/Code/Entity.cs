//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using System.Collections;
using static UnityEngine.Mathf;
using static Utils;

public enum EntityType
{
	Player, Mole, Familiar, Arrow, PiercingArrow, Fireball, Wolf
}

[Flags]
public enum EntityFlags
{
	None = 0,
	Dead = 1,
	Rooted = 2,
	EmitsLight = 4,
	Invincible = 8,
	KnockedBack = 16
}

public enum CollideType
{
	None, Trigger, Collide
}

public enum EntityEvent
{
	Update, Kill, HealthChanged, ReachedNewCell, SetMove, Count
}

public sealed class Entity : MonoBehaviour, IComparable<Entity>
{
	[SerializeField] private EntityType type;
	[SerializeField] private Layer layer;
	[SerializeField] private float defaultSpeed;
	[SerializeField] private int maxHealth;
	[SerializeField] private bool invincibleOnDamage;
	[SerializeField] private Sprite[] sprites;
	[SerializeField] private bool directional;

	public EntityType Type
	{
		get { return type; }
	}

	public Layer Layer
	{
		get { return layer; }
	}

	// Component-modified fields.
	[HideInInspector] public float speed;
	[HideInInspector] public int facing;
	[HideInInspector] public Vector2 start, end;
	[HideInInspector] public Vec2i movingDir;

	public int Health { get; private set; }

	private Action[] events = new Action[(int)EntityEvent.Count];

	public Room Room { get; private set; }

	private EntityFlags flags;

	private Transform t;

	public LevelEntities Entities { get; private set; }

	private WaitForSeconds wait = new WaitForSeconds(0.1f);

	private SpriteRenderer rend;

	public Vector2 Pos
	{
		get { return t.position; }
	}

	public Vec2i TilePos
	{
		get { return TilePos(t.position); }
	}

	public void Init(LevelEntities entities, Room room)
	{
		t = GetComponent<Transform>();

		Entities = entities;

		speed = defaultSpeed;
		Room = room;

		FullHeal();

		rend = GetComponent<SpriteRenderer>();
		rend.sprite = sprites[0];
	}

	public void ListenForEvent(EntityEvent type, Action func)
	{
		events[(int)type] += func;
	}

	public void InvokeEvent(EntityEvent type)
	{
		events[(int)type]?.Invoke();
	}

	public void SetFlag(EntityFlags flag)
	{
		flags |= flag;
	}

	public void UnsetFlag(EntityFlags flag)
	{
		flags &= ~flag;
	}

	public bool HasFlag(EntityFlags flag)
	{
		return (flags & flag) != 0;
	}

	public bool IsMoving()
	{
		return movingDir != Vec2i.Zero;
	}

	/// <summary>
	/// Sets the entity's speed to its default speed.
	/// </summary>
	public void ResetSpeed()
	{
		speed = defaultSpeed;
	}

	public void MoveTo(Vector2 pos)
	{
		t.position = pos;
	}

	public void NewMoveTarget(Vec2i start, Vec2i end, Vec2i dir)
	{
		if (start != end)
		{
			this.start = new Vector2(start.x, start.y);
			this.end = new Vector2(end.x, end.y);
			movingDir = dir;
		}
	}

	/// <summary>
	/// Updates all updatable entity components and ensures the entity is in the correct room.
	/// </summary>
	public void UpdateEntity(Level level)
	{
		if (directional) rend.sprite = sprites[facing];

		InvokeEvent(EntityEvent.Update);

		Vec2i roomP = ToRoomPos(TilePos);

		if (roomP != Room.Pos)
		{
			Room.RemoveEntity(this);
			Room newRoom = level.GetRoom(roomP);
			newRoom.AddEntity(this);
			Room = newRoom;
		}
	}

	/// <summary>
	/// Destroys the entity. The kill behavior is defined by the entity's components.
	/// Removes the entity from its room and clears its collision rules.
	/// </summary>
	public void KillEntity()
	{
		Room.RemoveEntity(this);
		Entities.RemoveCollisionRules(this);
		Entities.RemoveOTEffects(this);

		InvokeEvent(EntityEvent.Kill);
	}

	private void ReachedNewCell()
	{
		if (HasFlag(EntityFlags.KnockedBack))
			UnsetFlag(EntityFlags.KnockedBack);

		InvokeEvent(EntityEvent.ReachedNewCell);

		Vec2i prevDir = movingDir;
		Vector2 prevEnd = end;

		InvokeEvent(EntityEvent.SetMove);

		Vec2i next = movingDir;
		Vector2 target = Pos;

		if (prevDir.x != next.x)
			target.x = prevEnd.x;

		if (prevDir.y != next.y)
			target.y = prevEnd.y;

		MoveTo(target);
		Entities.TestCollision(this);
	}

	/// <summary>
	/// Moves the entity using the given accel. Accel represents the move direcction and should 
	/// have values in the range -1 to 1.
	/// </summary>
	public void Move()
	{
		if (IsMoving())
		{
			float vel = HasFlag(EntityFlags.KnockedBack) ? 12.0f : speed;

			Vector2 move = new Vector2(movingDir.x, movingDir.y) * vel * Time.deltaTime;

			MoveTo(Pos + move);
			float t = InverseLerp2(start, end, Pos);

			if (t >= 1.0f)
				ReachedNewCell();
		}
	}

	/// <summary>
	/// Performs a simple translation by amount.
	/// </summary>
	public Vector2 SimpleMove(Vector2 dir)
	{
		float moveLength = dir.sqrMagnitude;

		// Correct diagonal movement speed so that it isn't too fast.
		if (moveLength > 1.0f)
			dir *= (1.0f / Sqrt(moveLength));

		Vector2 move = dir * speed * Time.deltaTime;
		t.Translate(move, Space.World);

		return move;
	}

	/// <summary>
	/// Performs a simple translation by amount. When the distRemaining value becomes 0, the callback
	/// 'onDistReached' will be invoked.
	/// </summary>
	public void SimpleMove(Vector2 dir, ref float distRemaining, Action onDistReached)
	{
		Vector2 move = SimpleMove(dir);
		distRemaining -= move.magnitude;

		if (distRemaining <= 0.0f)
			onDistReached.Invoke();
	}

	public void SetHealth(int health)
	{
		this.Health = health;
		InvokeEvent(EntityEvent.HealthChanged);
	}

	public void FullHeal()
	{
		SetHealth(maxHealth);
	}

	public void ApplyDamage(int damage)
	{
		Health -= damage;

		if (Health <= 0)
			SetFlag(EntityFlags.Dead);
		else
		{
			if (invincibleOnDamage)
			{
				SetFlag(EntityFlags.Invincible);
				StartCoroutine(ResetInvincibility());
			}
		}

		InvokeEvent(EntityEvent.HealthChanged);
	}

	private IEnumerator ResetInvincibility()
	{
		yield return wait;
		UnsetFlag(EntityFlags.Invincible);
	}

	public void ApplyKnockback(int cells, Vec2i dir)
	{
		if (HasFlag(EntityFlags.Rooted)) return;

		float t = InverseLerp2(start, end, Pos);

		Vec2i kbStart = t < 0.5f ? TilePos(start) : TilePos(end);

		Vec2i newEnd = Vec2i.MaxValue;

		for (int i = 1; i <= cells; i++)
		{
			Vec2i next = kbStart + (dir * i);

			if (!Entities.WillCollide(this, next))
				newEnd = next;
			else break;
		}

		Vec2i target = newEnd != Vec2i.MaxValue ? newEnd : kbStart;
		NewMoveTarget(kbStart, target, dir);
		SetFlag(EntityFlags.KnockedBack);
	}

	public void MakeVisible()
	{
		rend.enabled = true;
	}

	public void MakeInvisible()
	{
		rend.enabled = false;
	}

	public int CompareTo(Entity other)
	{
		return type.CompareTo(other.type);
	}

	public static bool operator <(Entity a, Entity b)
	{
		return a.type < b.type;
	}

	public static bool operator >(Entity a, Entity b)
	{
		return a.type > b.type;
	}
}
