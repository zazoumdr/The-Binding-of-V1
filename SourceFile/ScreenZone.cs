using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScreenZone : MonoBehaviour
{
	protected bool inZone;

	protected bool touchMode;

	private GunControl gc;

	private FistControl pun;

	[SerializeField]
	private AudioSource music;

	private float originalVolume;

	[SerializeField]
	private AudioSource jingleMusic;

	[SerializeField]
	private float jingleEndTime = 1f;

	private float originalJingleVolume;

	public bool muteMusic;

	[SerializeField]
	private float angleLimit;

	[SerializeField]
	private Transform angleSourceOverride;

	[Space]
	[SerializeField]
	protected UnityEvent onEnterZone = new UnityEvent();

	[SerializeField]
	protected UnityEvent onExitZone = new UnityEvent();

	protected GraphicRaycaster raycaster;

	private bool hasEntered;

	private Coroutine musicRoutine;

	private void Awake()
	{
		if ((bool)(Object)(object)music)
		{
			originalVolume = music.volume;
		}
		if ((bool)(Object)(object)jingleMusic)
		{
			originalJingleVolume = jingleMusic.volume;
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded && inZone)
		{
			UpdatePlayerState(active: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		if (gc == null)
		{
			gc = other.GetComponentInChildren<GunControl>();
		}
		if (pun == null)
		{
			pun = other.GetComponentInChildren<FistControl>();
		}
		inZone = true;
		if ((Object)(object)raycaster == null)
		{
			raycaster = GetComponentInChildren<GraphicRaycaster>(includeInactive: true);
		}
		if ((Object)(object)raycaster == null)
		{
			raycaster = base.transform.parent.GetComponentInChildren<GraphicRaycaster>(includeInactive: true);
		}
		if ((bool)(Object)(object)raycaster)
		{
			if (ControllerPointer.raycasters.Contains(raycaster))
			{
				ControllerPointer.raycasters.Remove(raycaster);
			}
			ControllerPointer.raycasters.Add(raycaster);
		}
		onEnterZone?.Invoke();
		PlayMusic();
		hasEntered = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			if (gc == null)
			{
				gc = other.GetComponentInChildren<GunControl>();
			}
			if ((bool)(Object)(object)raycaster && ControllerPointer.raycasters.Contains(raycaster))
			{
				ControllerPointer.raycasters.Remove(raycaster);
			}
			onExitZone?.Invoke();
			inZone = false;
			UpdatePlayerState(active: false);
			StopMusic();
		}
	}

	public virtual void UpdatePlayerState(bool active)
	{
		if (touchMode == active)
		{
			return;
		}
		if (active)
		{
			if (gc != null)
			{
				gc.NoWeapon();
			}
			if (pun != null)
			{
				pun.ShopMode();
			}
		}
		else
		{
			if (gc != null)
			{
				gc.YesWeapon();
			}
			if (pun != null)
			{
				pun.StopShop();
			}
		}
		touchMode = active;
	}

	protected virtual void Update()
	{
		if ((Object)(object)jingleMusic != null)
		{
			if (MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume == 0f)
			{
				jingleMusic.volume = 0f;
			}
			else
			{
				jingleMusic.volume = originalJingleVolume;
			}
		}
		if ((Object)(object)music != null)
		{
			if (MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume == 0f)
			{
				music.volume = 0f;
			}
			else
			{
				music.volume = originalVolume;
			}
		}
		if (inZone)
		{
			bool flag = Mathf.Approximately(angleLimit, 0f);
			if (!flag)
			{
				Transform obj = MonoSingleton<CameraController>.Instance.transform;
				Vector3 position = obj.position;
				Vector3 forward = obj.forward;
				Vector3 position2 = base.transform.position;
				Vector3 lhs = ((angleSourceOverride == null) ? base.transform.forward : angleSourceOverride.forward);
				Vector3 normalized = (position2 - position).normalized;
				float num = Vector3.Dot(lhs, normalized);
				float num2 = Vector3.Angle(forward, normalized);
				flag |= num > 0f && num2 <= angleLimit;
			}
			UpdatePlayerState(flag);
		}
	}

	private void PlayMusic()
	{
		if (musicRoutine != null)
		{
			StopCoroutine(musicRoutine);
		}
		musicRoutine = StartCoroutine(PlayMusicRoutine());
	}

	private void StopMusic()
	{
		if (musicRoutine != null)
		{
			StopCoroutine(musicRoutine);
		}
		musicRoutine = StartCoroutine(StopMusicRoutine());
	}

	private IEnumerator PlayMusicRoutine()
	{
		if (!muteMusic)
		{
			yield break;
		}
		if ((Object)(object)jingleMusic != null && !hasEntered)
		{
			jingleMusic.Play(tracked: true);
			yield return new WaitUntil(() => jingleMusic.time >= jingleEndTime);
		}
		if ((Object)(object)music != null)
		{
			if (!music.isPlaying)
			{
				music.Play(tracked: true);
			}
			while (music.GetPitch() < 1f)
			{
				music.SetPitch(Mathf.MoveTowards(music.GetPitch(), 1f, Time.deltaTime));
				yield return null;
			}
			music.SetPitch(1f);
		}
		musicRoutine = null;
	}

	private IEnumerator StopMusicRoutine()
	{
		if (!muteMusic)
		{
			yield break;
		}
		if ((Object)(object)jingleMusic != null)
		{
			jingleMusic.Stop();
		}
		if ((Object)(object)music != null)
		{
			while (music.GetPitch() > 0f)
			{
				music.SetPitch(Mathf.MoveTowards(music.GetPitch(), 0f, Time.deltaTime));
				yield return null;
			}
			music.SetPitch(0f);
		}
		musicRoutine = null;
	}
}
