using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeamHitInterpolation;

public static class ArrayPool
{
	private static readonly Queue<RaycastHit[]> RaycastHitPool = new Queue<RaycastHit[]>();

	private const int DefaultRaycastSize = 32;

	private static readonly Queue<Collider[]> ColliderPool = new Queue<Collider[]>();

	private const int DefaultColliderSize = 50;

	public static RaycastHit[] GetRaycastHits()
	{
		if (RaycastHitPool.Count <= 0)
		{
			return new RaycastHit[32];
		}
		return RaycastHitPool.Dequeue();
	}

	public static void ReturnRaycastHits(RaycastHit[] array)
	{
		if (array != null && array.Length == 32)
		{
			Array.Clear(array, 0, array.Length);
			RaycastHitPool.Enqueue(array);
		}
	}

	public static Collider[] GetColliders(int minSize = 50)
	{
		if (ColliderPool.Count <= 0)
		{
			return new Collider[Mathf.Max(minSize, 50)];
		}
		Collider[] array = ColliderPool.Dequeue();
		if (array.Length >= minSize)
		{
			return array;
		}
		ColliderPool.Enqueue(array);
		return new Collider[Mathf.Max(minSize, 50)];
	}

	public static void ReturnColliders(Collider[] array)
	{
		if (array != null)
		{
			Array.Clear(array, 0, array.Length);
			ColliderPool.Enqueue(array);
		}
	}
}
