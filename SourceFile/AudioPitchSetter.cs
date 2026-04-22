using UnityEngine;

public class AudioPitchSetter : MonoBehaviour
{
	private AudioSource aud;

	public void SetPitch(float pitch)
	{
		if ((Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		aud.SetPitch(pitch);
	}
}
