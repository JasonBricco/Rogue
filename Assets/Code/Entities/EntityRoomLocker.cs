//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public class EntityRoomLocker : MonoBehaviour
{
	private void Start()
		=> GetComponent<Entity>().ListenForEvent(EntityEvent.Kill, OnKill);
	
	private void OnKill()
		=> World.Instance.Room.Entities.LockerKilled();
}
