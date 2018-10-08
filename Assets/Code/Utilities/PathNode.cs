//
// Copyright (c) 2018 Jason Bricco
//

using System;

public sealed class PathNode : IEquatable<PathNode>, IComparable<PathNode>
{
	public Vec2i pos;
	public int g, f, h;
	public PathNode parent;

	public int CompareTo(PathNode other)
	{
		return f.CompareTo(other.f);
	}

	public bool Equals(PathNode other)
		=> pos == other.pos;

	public override string ToString()
	{
		return pos.ToString() + ", F: " + f;
	}
}
