using UnityEngine;

public sealed class NoVirtualAudio : MonoBehaviour
{
	private void OnEnable()
	{
		if (MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.enabled = true;
		}
	}
}
