//
// Copyright (c) 2018 Jason Bricco
//

using static UnityEngine.Mathf;

public delegate void EntityCollisionResponse(Entity a, Entity b);
public delegate void TileCollisionResponse(Entity a, Tile tile);

public sealed class CollisionMatrix
{
	public const int Layers = 32;

	private struct CollisionHandler
	{
		public EntityCollisionResponse ecr;
		public TileCollisionResponse tcr;

		public CollisionHandler(EntityCollisionResponse ecr, TileCollisionResponse tcr)
		{
			this.ecr = ecr;
			this.tcr = tcr;
		}
	}

	private CollisionHandler[,] matrix = new CollisionHandler[Layers, Layers];

	public void Add(int layer0, int layer1, EntityCollisionResponse entityResponse, TileCollisionResponse tileResponse)
	{
		int a = Min(layer0, layer1);
		int b = Max(layer0, layer1);
		matrix[a, b] = new CollisionHandler(entityResponse, tileResponse);
	}

	public EntityCollisionResponse GetEntityResponse(int layer0, int layer1)
	{
		return matrix[Min(layer0, layer1), Max(layer0, layer1)].ecr;
	}

	public TileCollisionResponse GetTileResponse(int layer0, int layer1)
	{
		return matrix[Min(layer0, layer1), Max(layer0, layer1)].tcr;
	}
}
