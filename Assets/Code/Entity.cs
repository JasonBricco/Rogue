//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using System.Collections;
using static Utils;

public sealed class Entity : MonoBehaviour, IComparable<Entity>
{
	[SerializeField] private EntityType type;
	[SerializeField] private Layer layer;
	[SerializeField] private float defaultSpeed;
	[SerializeField] private int maxHealth;
	[SerializeField] private Sprite[] sprites;
	[SerializeField] private bool directional;

	public bool invincible;

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

	private Move normalMove;
	private Move forcedMove;

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

	public Vec2i EndCell
	{
		get { return forcedMove.Active ? forcedMove.EndCell : normalMove.Active ? normalMove.EndCell : TilePos; }
	}

	public void Init(LevelEntities entities, Room room, Vector2 pos)
	{
		t = GetComponent<Transform>();
		rend = GetComponent<SpriteRenderer>();

		rend.sprite = sprites[0];
		Entities = entities;

		speed = defaultSpeed;
		Room = room;

		FullHeal();

		MoveTo(pos);
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
		return normalMove.Active || forcedMove.Active;
	}

	public void ResetSpeed()
	{
		speed = defaultSpeed;
	}

	public void MoveTo(Vector2 pos)
	{
		t.position = pos;
	}

	public void SetMove(Vector2 start, int cells, Vec2i dir)
	{
		normalMove = new Move(start, speed, cells, dir);
	}

	public void SetMove(Vec2i start, int cells, Vec2i dir)
	{
		SetMove(new Vector2(start.x, start.y), cells, dir);
	}

	public void SetForcedMove(Vector2 start, float speed, int cells, Vec2i dir)
	{
		forcedMove = new Move(start, speed, cells, dir);
	}

	// Updates all updatable entity components and ensures the entity is in the correct room.
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

	// Destroys the entity. The kill behavior is defined by the entity's components.
	// Removes the entity from its room and clears its collision rules.
	public void KillEntity()
	{
		Room.RemoveEntity(this);
		Entities.RemoveCollisionRules(this);
		Entities.RemoveOTEffects(this);

		InvokeEvent(EntityEvent.Kill);
	}

	private void UpdateMoves()
	{
		if (forcedMove.Active)
		{
			forcedMove.ReachedNewCell();

			if (forcedMove.cellsLeft > 0)
			{
				if (Entities.WillCollide(this, forcedMove.EndCell))
					forcedMove.ToTileCenter(Pos, TilePos);
			}
			else
			{
				if (normalMove.Active)
					normalMove.SetStart(Pos);
			}
		}

		if (normalMove.Active)
		{
			normalMove.ReachedNewCell();

			if (Entities.WillCollide(this, normalMove.EndCell))
				normalMove.cellsLeft = 0;
			else return;
		}
	}

	private void ReachedNewCell()
	{
		InvokeEvent(EntityEvent.ReachedNewCell);
		Entities.TestCollision(this);
		UpdateMoves();
		InvokeEvent(EntityEvent.SetMove);
	}

	private bool ProcessMove(ref Move move)
	{
		if (move.Active)
		{
			Vector2 v = new Vector2(move.dir.x, move.dir.y) * move.speed * Time.deltaTime;

			MoveTo(Pos + v);
			float t = InverseLerp2(move.start, move.end, Pos);

			if (t >= 1.0f)
				ReachedNewCell();

			return true;
		}

		return false;
	}

	public void Move()
	{
		if (!ProcessMove(ref forcedMove))
		{
			if (!ProcessMove(ref normalMove))
				MoveTo(normalMove.end);
		}
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
			if (HasFlag(EntityFlags.InvincibleFrames))
			{
				invincible = true;
				StartCoroutine(ResetInvincibility());
			}
		}

		InvokeEvent(EntityEvent.HealthChanged);
	}

	private IEnumerator ResetInvincibility()
	{
		yield return wait;
		invincible = false;
	}

	public void ApplyKnockback(int cells, Vec2i dir)
	{
		if (HasFlag(EntityFlags.Rooted)) return;
		Entities.SetForcedMove(this, dir, cells, 12.0f);
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
