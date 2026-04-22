using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public sealed class PlayOnAwakeTracker : MonoBehaviour
{
	private void OnEnable()
	{
		List<AudioSource> value;
		using (CollectionPool<List<AudioSource>, AudioSource>.Get(out value))
		{
			GetComponents(value);
			foreach (AudioSource item in value)
			{
				if (item.playOnAwake)
				{
					MonoSingleton<VirtualAudioManager>.Instance.AddAudioSource(item);
				}
			}
		}
	}
}
