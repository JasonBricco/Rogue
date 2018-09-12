//
// Copyright (c) 2018 Jason Bricco
//

using static UnityEngine.Mathf;

public delegate void EntityCollisionResponse(Entity a, Entity b);
public delegate void TileCollisionResponse(Entity a, Tile tile);

public struct CollisionHandler
{
	public CollideType type;
	public EntityCollisionResponse ecr;
	public TileCollisionResponse tcr;

	public CollisionHandler(CollideType type, EntityCollisionResponse ecr, TileCollisionResponse tcr)
	{
		this.type = type;
		this.ecr = ecr;
		this.tcr = tcr;
	}
}

public sealed class CollisionMatrix
{
	public const int Layers = 32;

	private CollisionHandler[,] matrix = new CollisionHandler[Layers, Layers];

	public void Add(Layer layer0, Layer layer1, CollideType type, EntityCollisionResponse entityResponse, TileCollisionResponse tileResponse)
	{
		int a = Min((int)layer0, (int)layer1);
		int b = Max((int)layer0, (int)layer1);
		matrix[a, b] = new CollisionHandler(type, entityResponse, tileResponse);
	}

	public CollisionHandler Get(Layer layer0, Layer layer1)
	{
		int a = (int)layer0, b = (int)layer1;
		return matrix[Min(a, b), Max(a, b)];
	}
}
