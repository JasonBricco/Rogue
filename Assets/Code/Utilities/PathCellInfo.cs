//
// Copyright (c) 2018 Jason Bricco
//

public struct PathCellInfo
{
	public bool passable, trigger;
	public int cost;

	public PathCellInfo(bool passable, bool trigger, int cost)
	{
		this.passable = passable;
		this.trigger = trigger;
		this.cost = cost;
	}

	public override string ToString()
		=> "Passable: " + passable + ", Score: " + cost;
}
