using UnityEngine;

public class MixerVolumeDip : MonoBehaviour
{
	private bool dipped;

	public float targetAmount = -5f;

	private void OnEnable()
	{
		if (!dipped)
		{
			dipped = true;
			if ((bool)MonoSingleton<AudioMixerController>.Instance)
			{
				MonoSingleton<AudioMixerController>.Instance.TemporaryDip(targetAmount);
			}
		}
	}

	private void OnDisable()
	{
		if (dipped)
		{
			dipped = false;
			if ((bool)MonoSingleton<AudioMixerController>.Instance)
			{
				MonoSingleton<AudioMixerController>.Instance.TemporaryDip(0f);
			}
		}
	}
}
