using ScriptableObjects;
using UnityEngine;

public class PowerIntro : MonoBehaviour
{
	[SerializeField]
	private PowerPersistentData persistentData;

	[SerializeField]
	private AudioClip introOverride;

	public float introAudioDelay = 0.5f;

	[SerializeField]
	private Animator animator;

	public float animationSpeedMultiplier = 1f;

	private void Start()
	{
		bool flag = persistentData != null && persistentData.PerformedIntro;
		if ((bool)(Object)(object)animator)
		{
			animator.speed = (flag ? (animationSpeedMultiplier * persistentData.RepeatedIntroSpeedFactor) : animationSpeedMultiplier);
		}
		Invoke("Activate", introAudioDelay);
	}

	private void Activate()
	{
		AudioClip val = introOverride;
		if ((Object)(object)introOverride == null)
		{
			val = MonoSingleton<PowerVoiceController>.Instance.Intro();
		}
		else if (persistentData != null && persistentData.PerformedIntro && persistentData.RepeatedIntroOverrideClip)
		{
			val = ((persistentData.RepeatedIntroClips.Length != 0) ? persistentData.RepeatedIntroClips[Random.Range(0, persistentData.RepeatedIntroClips.Length)] : null);
		}
		MonoSingleton<PowerVoiceController>.Instance.sinceLastIntro = 0f;
		float num = Random.Range(0.95f, 1.05f);
		if ((Object)(object)val != null)
		{
			if (TryGetComponent<AudioSource>(out var component))
			{
				component.SetPitch(num);
				component.clip = val;
				component.Play(tracked: true);
			}
			else
			{
				val.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.allGroup, base.transform.position, 20, 0.8f, 1f, Random.Range(0.9f, 1.1f), (AudioRolloffMode)1, 1f, 150f);
			}
		}
		Power componentInChildren = GetComponentInChildren<Power>();
		if ((bool)componentInChildren)
		{
			componentInChildren.voicePitch = num;
		}
		if (persistentData != null)
		{
			persistentData.PerformedIntro = true;
			if (MonoSingleton<SceneHelper>.TryGetInstance(out SceneHelper instance))
			{
				instance.enemyPersistentData.Add(persistentData);
			}
		}
		Object.Destroy(this);
	}
}
