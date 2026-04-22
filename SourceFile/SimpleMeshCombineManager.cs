using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SimpleMeshCombineManager : MonoSingleton<SimpleMeshCombineManager>
{
	public float waitTimeUntilProcess = 0.2f;

	private Queue<SimpleMeshCombiner> combinersQueue = new Queue<SimpleMeshCombiner>();

	private void Start()
	{
		SimpleMeshCombiner[] array = Resources.FindObjectsOfTypeAll<SimpleMeshCombiner>();
		foreach (SimpleMeshCombiner simpleMeshCombiner in array)
		{
			if (simpleMeshCombiner.gameObject.isStatic)
			{
				Debug.LogWarning("we can't process static meshes");
			}
			else
			{
				combinersQueue.Enqueue(simpleMeshCombiner);
			}
		}
		StartCoroutine(ProcessCombiners());
	}

	private IEnumerator ProcessCombiners()
	{
		WaitForSeconds waitTime = new WaitForSeconds(waitTimeUntilProcess);
		yield return waitTime;
		while (combinersQueue.Count > 0)
		{
			combinersQueue.Dequeue().CombineMeshes();
			yield return waitTime;
		}
	}
}
