using System.Collections;
using UnityEngine;

public class LampFlicker : MonoBehaviour
{
	public float randomSpeedMin;

	public float randomSpeedMax;

	public float randomMin;

	public float randomMax;

	private float baseIntensity;

	private Light thisLight;

	private void Awake()
	{
		thisLight = GetComponent<Light>();
		baseIntensity = thisLight.intensity;
	}

	private void OnEnable()
	{
		StartCoroutine(FlickerLamp());
	}

	private IEnumerator FlickerLamp()
	{
		while (true)
		{
			thisLight.intensity = baseIntensity * Random.Range(randomMin, randomMax);
			yield return new WaitForSeconds(Random.Range(randomSpeedMin, randomSpeedMax));
		}
	}
}
