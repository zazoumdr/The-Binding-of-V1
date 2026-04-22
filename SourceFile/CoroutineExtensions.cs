using System;
using System.Collections;
using UnityEngine;

public static class CoroutineExtensions
{
	public static Coroutine ContinueWith(this Coroutine coroutine, MonoBehaviour owner, Action action)
	{
		return owner.StartCoroutine(ContinueWithCoroutine(coroutine, action));
	}

	private static IEnumerator ContinueWithCoroutine(Coroutine coroutine, Action action)
	{
		yield return coroutine;
		action();
	}
}
