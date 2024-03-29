﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;

public delegate void EntityColFunc(Entity a, Entity b);
public delegate void TileColFunc(Entity a, TileInstance inst);
public delegate void BarrierColFunc(Entity a, Vec2i dir);
public delegate void DefaultColFunc(Entity a);

public sealed class CollisionMatrix
{
	public const int Layers = 32;

	private struct CollisionHandler
	{
		public EntityColFunc ecr;
		public TileColFunc tcr;
		public BarrierColFunc bcr;
		public DefaultColFunc dcr;

		public CollisionHandler(EntityColFunc ecr, TileColFunc tcr, BarrierColFunc bcr, DefaultColFunc dcr)
		{
			this.ecr = ecr;
			this.tcr = tcr;
			this.bcr = bcr;
			this.dcr = dcr;
		}
	}

	private CollisionHandler[,] matrix = new CollisionHandler[Layers, Layers];

	public void Add(int layer0, int layer1, EntityColFunc ecr = null, TileColFunc tcr = null, BarrierColFunc bcr = null, DefaultColFunc dcr = null)
	{
		int a = Mathf.Min(layer0, layer1);
		int b = Mathf.Max(layer0, layer1);
		matrix[a, b] = new CollisionHandler(ecr, tcr, bcr, dcr);
	}

	public EntityColFunc GetEntityResponse(int layer0, int layer1)
		=> matrix[Mathf.Min(layer0, layer1), Mathf.Max(layer0, layer1)].ecr;

	public TileColFunc GetTileResponse(int layer0, int layer1)
		=> matrix[Mathf.Min(layer0, layer1), Mathf.Max(layer0, layer1)].tcr;

	public BarrierColFunc GetBarrierResponse(int layer0, int layer1)
		=> matrix[Mathf.Min(layer0, layer1), Mathf.Max(layer0, layer1)].bcr;

	public DefaultColFunc GetDefaultResponse(int layer0, int layer1)
		=> matrix[Mathf.Min(layer0, layer1), Mathf.Max(layer0, layer1)].dcr;
}
