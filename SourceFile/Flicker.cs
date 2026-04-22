using UnityEngine;

public class Flicker : MonoBehaviour
{
	private Light light;

	public float delay;

	private AudioSource aud;

	private float intensity;

	public bool onlyOnce;

	public bool quickFlicker;

	public float intensityRandomizer;

	public float timeRandomizer;

	public bool stopAudio;

	public bool forceOnAfterDisable;

	public bool dontGoOff;

	public GameObject[] flickerDisableObjects;

	private void Start()
	{
		light = GetComponent<Light>();
		aud = GetComponent<AudioSource>();
		intensity = light.intensity;
		light.intensity = 0f;
		GameObject[] array = flickerDisableObjects;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void OnDisable()
	{
		CancelInvoke();
		if (forceOnAfterDisable)
		{
			On();
		}
	}

	private void OnEnable()
	{
		if (timeRandomizer != 0f)
		{
			Invoke("Flickering", delay + Random.Range(0f - timeRandomizer, timeRandomizer));
		}
		else
		{
			Invoke("Flickering", delay);
		}
	}

	private void Flickering()
	{
		if (light.intensity == 0f || dontGoOff)
		{
			light.intensity = intensity + Random.Range(0f - intensityRandomizer, intensityRandomizer);
			if ((Object)(object)aud != null && base.gameObject.activeInHierarchy)
			{
				aud.Play(tracked: true);
			}
			if (quickFlicker)
			{
				Invoke("Off", 0.1f);
			}
		}
		else
		{
			light.intensity = 0f;
			if ((Object)(object)aud != null && stopAudio && base.gameObject.activeInHierarchy)
			{
				aud.Stop();
			}
		}
		if (!onlyOnce)
		{
			if (timeRandomizer != 0f)
			{
				Invoke("Flickering", delay + Random.Range(0f - timeRandomizer, timeRandomizer));
			}
			else
			{
				Invoke("Flickering", delay);
			}
		}
		GameObject[] array = flickerDisableObjects;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public void On()
	{
		light.intensity = intensity;
		if ((Object)(object)aud != null && base.gameObject.activeInHierarchy)
		{
			aud.Play(tracked: true);
		}
		GameObject[] array = flickerDisableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	public void Off()
	{
		light.intensity = 0f;
		if ((Object)(object)aud != null && stopAudio && base.gameObject.activeInHierarchy)
		{
			aud.Stop();
		}
		GameObject[] array = flickerDisableObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}
}
