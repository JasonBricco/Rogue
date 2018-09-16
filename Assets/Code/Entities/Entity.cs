//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System;
using static UnityEngine.Mathf;
using static Utils;

public sealed class Entity : MonoBehaviour, IComparable<Entity>
{
	[SerializeField] private EntityType type;
	[SerializeField] private float defaultSpeed;
	[SerializeField] private float friction;
	[SerializeField] private bool rooted;

	public EntityType Type
	{
		get { return type; }
	}

	// Component-modified fields.
	[HideInInspector] public float speed;
	[HideInInspector] public Vector2 velocity;
	[HideInInspector] public int facing;

	private Action[] events = new Action[(int)EntityEvent.Count];

	private EntityFlags flags;

	private CharacterController controller;

	public Room Room { get; private set; }

	private Transform t;

	public LevelEntities Entities { get; private set; }

	public Vector2 Pos
	{
		get { return t.position; }
	}

	public Vec2i TilePos
	{
		get { return TilePos(t.position); }
	}

	public Vector2 FacingDir
	{
		get { return velocity.normalized; }
	}

	public void Init(LevelEntities entities, Room room)
	{
		t = GetComponent<Transform>();
		controller = GetComponent<CharacterController>();

		Entities = entities;

		speed = defaultSpeed;
		Room = room;
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

	public void ListenForEvent(EntityEvent type, Action func)
	{
		events[(int)type] += func;
	}

	public void InvokeEvent(EntityEvent type)
	{
		events[(int)type]?.Invoke();
	}

	// Sets the entity's speed to its default speed.
	public void ResetSpeed()
	{
		speed = defaultSpeed;
	}

	public void MoveTo(Vector2 pos)
	{
		t.position = pos;
	}

	// Updates all updatable entity components and ensures the entity is in the correct room.
	public void UpdateEntity(Level level)
	{
		InvokeEvent(EntityEvent.Update);

		Vec2i roomP = ToRoomPos(TilePos);

		if (roomP != Room.Pos)
		{
			Room.RemoveEntity(this);
			Room newRoom = level.GetRoom(roomP);
			newRoom.AddEntity(this);
			Room = newRoom;
			InvokeEvent(EntityEvent.RoomChanged);
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

	// Moves the entity using the given accel. Accel represents the move direcction and should 
	// have values in the range -1 to 1.
	public Vector2 Move(Vector2 accel)
	{
		float moveLength = accel.sqrMagnitude;

		// Correct diagonal movement speed so that it isn't too fast.
		if (moveLength > 1.0f)
			accel *= (1.0f / Sqrt(moveLength));

		accel *= speed;
		accel += (velocity * friction);

		// Using the following equations of motion:

		// - p' = 1/2at^2 + vt + p.
		// - v' = at + v.
		// - a = specified by input.

		// Where a = acceleration, v = velocity, and p = position.
		// v' and p' denote new versions, while non-prime denotes old.

		// These are found by integrating up from acceleration to velocity. Use derivation
		// to go from position down to velocity and then down to acceleration to see how 
		// we can integrate back up.
		Vector2 delta = accel * 0.5f * Square(Time.deltaTime) + velocity * Time.deltaTime;
		velocity = accel * Time.deltaTime + velocity;

		controller.Move(delta);
		return delta;
	}

	// Moves the entity using the given accel. Accel represents the move direcction and should 
	// have values in the range -1 to 1. When the distRemaining value becomes 0, the callback
	// 'onDistReached' will be invoked.
	public void Move(Vector2 accel, ref float distRemaining, Action onDistReached)
	{
		Vector2 delta = Move(accel);
		distRemaining -= delta.magnitude;

		if (distRemaining <= 0.0f)
			onDistReached.Invoke();
	}


	// Performs a simple translation by amount.
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

	// Performs a simple translation by amount. When the distRemaining value becomes 0, the callback
	// 'onDistReached' will be invoked.
	public void SimpleMove(Vector2 dir, ref float distRemaining, Action onDistReached)
	{
		Vector2 move = SimpleMove(dir);
		distRemaining -= move.magnitude;

		if (distRemaining <= 0.0f)
			onDistReached.Invoke();
	}

	public void ApplyKnockback(Vector2 dir, float force)
	{
		if (rooted) return;
		velocity = dir * force;
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
