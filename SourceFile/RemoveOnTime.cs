using ULTRAKILL.Cheats;
using UnityEngine;

public class RemoveOnTime : MonoBehaviour
{
	public bool useAudioLength;

	public float time;

	public float randomizer;

	public bool affectedByNoCooldowns;

	private void Start()
	{
		if (useAudioLength)
		{
			AudioSource component = GetComponent<AudioSource>();
			if (!(Object)(object)component)
			{
				Debug.LogError("useAudioLength is enabled, but an AudioSource was not found");
				Object.Destroy(this);
			}
			else if (!(Object)(object)component.clip)
			{
				Debug.LogError("useAudioLength is enabled without a clip");
				Object.Destroy(this);
			}
			else
			{
				Invoke("Remove", component.clip.length * component.GetPitch());
			}
		}
		else
		{
			Invoke("Remove", time + Random.Range(0f - randomizer, randomizer));
		}
	}

	private void Remove()
	{
		if (affectedByNoCooldowns && NoWeaponCooldown.NoCooldown)
		{
			Invoke("Remove", time / 2f + Random.Range(0f - randomizer, randomizer));
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
