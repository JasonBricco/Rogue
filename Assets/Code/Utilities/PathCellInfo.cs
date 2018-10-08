//
// Copyright (c) 2018 Jason Bricco
//

public struct PathCellInfo
{
	public bool passable;
	public int cost;

	public PathCellInfo(bool passable, int cost)
	{
		this.passable = passable;
		this.cost = cost;
	}

	public override string ToString()
	{
		return "Passable: " + passable + ", Score: " + cost;
	}
}
