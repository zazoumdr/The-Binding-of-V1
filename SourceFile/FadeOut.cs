using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
	public bool fadeIn;

	public bool distance;

	private List<float> origVol = new List<float>();

	public AudioSource[] auds;

	private bool fading;

	public float speed;

	public float maxDistance;

	public bool activateOnEnable;

	public bool dontStopOnZero;

	private GameObject player;

	public bool fadeEvenIfNotPlaying;

	private void Start()
	{
		if (auds == null || auds.Length == 0)
		{
			auds = GetComponents<AudioSource>();
		}
		if (fadeIn || distance)
		{
			AudioSource[] array = auds;
			foreach (AudioSource val in array)
			{
				origVol.Add(val.volume);
				val.volume = 0f;
			}
		}
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		if (activateOnEnable)
		{
			BeginFade();
		}
	}

	private void Update()
	{
		if (fading)
		{
			if (fadeIn)
			{
				for (int i = 0; i < auds.Length; i++)
				{
					if (auds[i].isPlaying)
					{
						if (auds[i].volume == origVol[i])
						{
							fading = false;
						}
						else
						{
							auds[i].volume = Mathf.MoveTowards(auds[i].volume, origVol[i], Time.deltaTime * speed);
						}
					}
				}
				return;
			}
			AudioSource[] array = auds;
			foreach (AudioSource val in array)
			{
				if (!val.isPlaying && !fadeEvenIfNotPlaying)
				{
					continue;
				}
				if (val.volume <= 0f)
				{
					if (!dontStopOnZero)
					{
						val.Stop();
					}
					else
					{
						fading = false;
					}
				}
				else
				{
					val.volume -= Time.deltaTime * speed;
				}
			}
		}
		else
		{
			if (!distance)
			{
				return;
			}
			if (fadeIn)
			{
				for (int k = 0; k < auds.Length; k++)
				{
					if (Vector3.Distance(base.transform.position, player.transform.position) > maxDistance)
					{
						auds[k].volume = 0f;
					}
					else
					{
						auds[k].volume = Mathf.Min(origVol[k], Mathf.Pow((Mathf.Sqrt(maxDistance) - Mathf.Sqrt(Vector3.Distance(base.transform.position, player.transform.position))) / Mathf.Sqrt(maxDistance), 2f) * origVol[k]);
					}
				}
			}
			else
			{
				for (int l = 0; l < auds.Length; l++)
				{
					auds[l].volume = Mathf.Min(origVol[l], Vector3.Distance(base.transform.position, player.transform.position) / maxDistance * origVol[l]);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			BeginFade();
		}
	}

	public void BeginFade()
	{
		fading = true;
		AudioSource[] array = auds;
		for (int i = 0; i < array.Length; i++)
		{
			GetMusicVolume component = ((Component)(object)array[i]).GetComponent<GetMusicVolume>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
		}
	}
}
