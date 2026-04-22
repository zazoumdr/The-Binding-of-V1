using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PooledWaterStore : MonoSingleton<PooledWaterStore>
{
	private GameObject smallSplash;

	private GameObject bigSplash;

	private GameObject continuousSplash;

	private GameObject bubblePrefab;

	private GameObject wetParticle;

	private Dictionary<Water.WaterGOType, Queue<GameObject>> waterGOQueues = new Dictionary<Water.WaterGOType, Queue<GameObject>>();

	private Transform thisTrans;

	private void Start()
	{
		thisTrans = base.transform;
		InitPools();
	}

	private void InitPools()
	{
		DefaultReferenceManager defaultReferenceManager = MonoSingleton<DefaultReferenceManager>.Instance;
		continuousSplash = defaultReferenceManager.continuousSplash;
		bigSplash = defaultReferenceManager.splash;
		smallSplash = defaultReferenceManager.smallSplash;
		bubblePrefab = defaultReferenceManager.bubbles;
		wetParticle = defaultReferenceManager.wetParticle;
		StartCoroutine(InitPool(Water.WaterGOType.small));
		StartCoroutine(InitPool(Water.WaterGOType.big));
		StartCoroutine(InitPool(Water.WaterGOType.continuous));
		StartCoroutine(InitPool(Water.WaterGOType.bubble));
		StartCoroutine(InitPool(Water.WaterGOType.wetparticle));
	}

	private GameObject GetPrefabByWaterType(Water.WaterGOType waterType)
	{
		return waterType switch
		{
			Water.WaterGOType.small => smallSplash, 
			Water.WaterGOType.big => bigSplash, 
			Water.WaterGOType.continuous => continuousSplash, 
			Water.WaterGOType.bubble => bubblePrefab, 
			Water.WaterGOType.wetparticle => wetParticle, 
			_ => null, 
		};
	}

	private IEnumerator InitPool(Water.WaterGOType type)
	{
		Queue<GameObject> queue = new Queue<GameObject>();
		waterGOQueues.Add(type, queue);
		GameObject prefabByWaterType = GetPrefabByWaterType(type);
		prefabByWaterType.SetActive(value: false);
		AsyncInstantiateOperation<GameObject> asyncOp = Object.InstantiateAsync(prefabByWaterType, 50, thisTrans);
		while (!asyncOp.isDone)
		{
			yield return null;
		}
		GameObject[] result = asyncOp.Result;
		for (int i = 0; i < 50; i++)
		{
			GameObject gameObject = result[i];
			gameObject.SetActive(value: false);
			queue.Enqueue(gameObject);
		}
	}

	public GameObject GetFromQueue(Water.WaterGOType type)
	{
		GameObject gameObject = null;
		Queue<GameObject> queue = waterGOQueues[type];
		while (gameObject == null && queue.Count > 0)
		{
			gameObject = queue.Dequeue();
		}
		if (gameObject == null)
		{
			gameObject = Object.Instantiate(GetPrefabByWaterType(type), thisTrans);
		}
		if (gameObject == null)
		{
			return null;
		}
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public void ReturnToQueue(GameObject go, Water.WaterGOType type)
	{
		if (type == Water.WaterGOType.none)
		{
			Object.Destroy(go);
			return;
		}
		waterGOQueues[type].Enqueue(go);
		if (type == Water.WaterGOType.bubble || type == Water.WaterGOType.wetparticle)
		{
			go.transform.SetParent(thisTrans);
		}
		go.transform.localScale = Vector3.one;
		go.SetActive(value: false);
	}
}
