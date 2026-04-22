using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class FireObjectPool : MonoSingleton<FireObjectPool>
{
	public GameObject firePrefab;

	public GameObject simpleFirePrefab;

	public int poolSize = 100;

	private Queue<GameObject> firePool;

	private Queue<GameObject> simpleFirePool;

	private void Awake()
	{
		firePool = new Queue<GameObject>();
		simpleFirePool = new Queue<GameObject>();
		for (int i = 0; i < poolSize; i++)
		{
			GameObject gameObject = Object.Instantiate(firePrefab, base.transform);
			gameObject.SetActive(value: false);
			firePool.Enqueue(gameObject);
			GameObject gameObject2 = Object.Instantiate(simpleFirePrefab, base.transform);
			gameObject2.SetActive(value: false);
			simpleFirePool.Enqueue(gameObject2);
		}
	}

	public GameObject GetFire(bool isSimple)
	{
		Queue<GameObject> queue = (isSimple ? simpleFirePool : firePool);
		if (queue.Count > 0)
		{
			GameObject gameObject = queue.Dequeue();
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
				return gameObject;
			}
		}
		return Object.Instantiate(isSimple ? simpleFirePrefab : firePrefab);
	}

	public void ReturnFire(GameObject fireObject, bool isSimple)
	{
		fireObject.transform.SetParent(base.transform);
		fireObject.SetActive(value: false);
		(isSimple ? simpleFirePool : firePool).Enqueue(fireObject);
	}

	public void RemoveAllFiresFromObject(GameObject objectToSearch)
	{
		Flammable[] componentsInChildren = objectToSearch.GetComponentsInChildren<Flammable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].MarkForDestroy();
		}
	}
}
