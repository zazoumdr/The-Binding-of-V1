using System.Collections;
using UnityEngine;

public class ObjectActivateInSequence : MonoBehaviour
{
	public GameObject[] objectsToActivate;

	private Coroutine coroutine;

	public float delay;

	private AudioSource aud;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		GameObject[] array = objectsToActivate;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: false);
			}
		}
		coroutine = StartCoroutine(activationCoroutine());
	}

	private void OnDisable()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
	}

	private IEnumerator activationCoroutine()
	{
		int i = 0;
		while (i < objectsToActivate.Length)
		{
			if (objectsToActivate[i] == null)
			{
				i++;
				continue;
			}
			objectsToActivate[i].SetActive(value: true);
			i++;
			if ((bool)(Object)(object)aud)
			{
				aud.Play(tracked: true);
			}
			yield return new WaitForSeconds(delay);
		}
	}
}
