using ScriptableObjects;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlayerFootsteps : MonoSingleton<PlayerFootsteps>
{
	[SerializeField]
	private FootstepSet footstepSet;

	public bool onGround;

	public AudioClip[] footsteps;

	private AudioSource aud;

	private float footstepTimer = 1f;

	private int lastFootstep = -1;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (MonoSingleton<NewMovement>.Instance.walking && (!MonoSingleton<NewMovement>.Instance.groundProperties || MonoSingleton<NewMovement>.Instance.groundProperties.friction > 0f))
		{
			footstepTimer = Mathf.MoveTowards(footstepTimer, 0f, Mathf.Min(MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude, 15f) / 15f * Time.deltaTime * 3f);
		}
		if (footstepTimer <= 0f)
		{
			Footstep();
		}
	}

	public void Footstep(float volume = 0.25f, bool force = false, float delay = 0f)
	{
		if (!(onGround || force))
		{
			return;
		}
		footstepTimer = 1f;
		if ((Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if (!(Object)(object)aud)
		{
			return;
		}
		aud.volume = volume;
		if ((bool)MonoSingleton<NewMovement>.Instance && MonoSingleton<NewMovement>.Instance.touchingWaters.Count > 0 && !MonoSingleton<UnderwaterController>.Instance.inWater)
		{
			if (footstepSet != null && footstepSet.TryGetFootstepClips(SurfaceType.Wet, out var clips))
			{
				PlayRandomFootstepClip(clips, delay);
			}
			return;
		}
		if (!MonoSingleton<NewMovement>.Instance || !MonoSingleton<NewMovement>.Instance.groundProperties || (!MonoSingleton<NewMovement>.Instance.groundProperties.overrideFootsteps && !MonoSingleton<NewMovement>.Instance.groundProperties.overrideSurfaceType))
		{
			if (footstepSet != null && MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position, out var hitSurfaceData) && footstepSet.TryGetFootstepClips(hitSurfaceData.surfaceType, out var clips2))
			{
				PlayRandomFootstepClip(clips2, delay);
			}
			else if (footsteps.Length != 0)
			{
				PlayRandomFootstepClip(footsteps, delay);
			}
			return;
		}
		CustomGroundProperties groundProperties = MonoSingleton<NewMovement>.Instance.groundProperties;
		if (footstepSet != null && groundProperties.overrideSurfaceType && footstepSet.TryGetFootstepClips(groundProperties.surfaceType, out var clips3))
		{
			PlayRandomFootstepClip(clips3, delay);
		}
		else if ((Object)(object)groundProperties.newFootstepSound != null)
		{
			PlayFootstepClip(groundProperties.newFootstepSound, delay);
		}
	}

	public void WallJump(Vector3 position)
	{
		if ((Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if ((bool)(Object)(object)aud)
		{
			aud.volume = 0.5f;
			if (footstepSet != null && MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position, position - base.transform.position, Vector3.Distance(base.transform.position, position) + 1f, out var hitSurfaceData) && footstepSet.TryGetFootstepClips(hitSurfaceData.surfaceType, out var clips))
			{
				PlayRandomFootstepClip(clips);
			}
			else if (footsteps.Length != 0)
			{
				PlayRandomFootstepClip(footsteps);
			}
		}
	}

	public void WallJump(CustomGroundProperties cgp)
	{
		if ((Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if ((bool)(Object)(object)aud)
		{
			aud.volume = 0.5f;
			if (cgp.overrideFootsteps)
			{
				PlayFootstepClip(cgp.newFootstepSound);
				return;
			}
			footstepSet.TryGetFootstepClips(cgp.surfaceType, out var clips);
			PlayRandomFootstepClip(clips);
		}
	}

	private void PlayRandomFootstepClip(AudioClip[] clips, float delay = 0f)
	{
		if (clips != null && clips.Length != 0)
		{
			int num = Random.Range(0, clips.Length);
			if (clips.Length > 1 && num == lastFootstep)
			{
				num = (num + 1) % clips.Length;
			}
			lastFootstep = num;
			PlayFootstepClip(clips[num], delay);
		}
	}

	private void PlayFootstepClip(AudioClip clip, float delay = 0f)
	{
		if (!((Object)(object)clip == null))
		{
			aud.clip = clip;
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			if (delay == 0f)
			{
				aud.Play(tracked: true);
			}
			else
			{
				aud.PlayDelayed(delay, tracked: true);
			}
		}
	}
}
