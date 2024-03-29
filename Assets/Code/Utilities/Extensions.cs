﻿//
// Copyright (c) 2018 Jason Bricco
//

using UnityEngine;
using System.Collections.Generic;

public static class Extensions
{
	public static void SetX(this Transform t, float value)
	{
		Vector3 p = t.position;
		p.x = value;
		t.position = p;
	}

	public static void SetY(this Transform t, float value)
	{
		Vector3 p = t.position;
		p.y = value;
		t.position = p;
	}

	public static void SetZ(this Transform t, float value)
	{
		Vector3 p = t.position;
		p.z = value;
		t.position = p;
	}

	public static Color SetAlpha(this Color col, float alpha)
	{
		return new Color(col.r, col.g, col.b, alpha);
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
