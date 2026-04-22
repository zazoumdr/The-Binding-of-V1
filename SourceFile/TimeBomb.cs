using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;

public class TimeBomb : MonoBehaviour
{
	public bool dontStartOnAwake;

	private bool activated;

	public float timer;

	public float beeptimer;

	public bool freezeOnNoCooldown;

	private AudioSource aud;

	public GameObject beepLight;

	public float beeperSize;

	[HideInInspector]
	public float beeperSizeMultiplier = 1f;

	private GameObject beeper;

	private Vector3 origScale;

	public Color beeperColor = Color.white;

	private SpriteRenderer beeperSpriteRenderer;

	public float beeperPitch = 0.65f;

	public GameObject explosion;

	public bool dontExplode;

	private bool isActive;

	private void Start()
	{
		if (!dontStartOnAwake)
		{
			StartCountdown();
		}
	}

	private void OnEnable()
	{
		if (!isActive && (bool)MonoSingleton<GunControl>.Instance && MonoSingleton<GunControl>.Instance.gameObject.activeInHierarchy && MonoSingleton<GunControl>.Instance.enabled)
		{
			MonoSingleton<GunControl>.Instance.StartCoroutine(CheckDisabled());
		}
	}

	private IEnumerator CheckDisabled()
	{
		WaitForEndOfFrame waitForEnd = new WaitForEndOfFrame();
		isActive = true;
		while ((bool)this && base.gameObject.activeInHierarchy)
		{
			yield return waitForEnd;
		}
		isActive = false;
	}

	private void OnDestroy()
	{
		if (!dontExplode && explosion != null && isActive)
		{
			Object.Instantiate(explosion, base.transform.position, base.transform.rotation);
		}
	}

	private void Update()
	{
		if (!activated)
		{
			return;
		}
		if (!PauseTimedBombs.Paused && (!NoWeaponCooldown.NoCooldown || !freezeOnNoCooldown))
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
			beeptimer = Mathf.MoveTowards(beeptimer, 0f, Time.deltaTime);
			if (beeptimer == 0f)
			{
				Beep();
			}
		}
		if ((bool)beeperSpriteRenderer)
		{
			beeperSpriteRenderer.color = beeperColor;
		}
		if ((bool)(Object)(object)aud)
		{
			aud.SetPitch(beeperPitch);
		}
		if (timer != 0f && (bool)beeper)
		{
			beeper.transform.localScale = Vector3.Lerp(beeper.transform.localScale, Vector3.zero, Time.deltaTime * 5f);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void StartCountdown()
	{
		if (!activated)
		{
			activated = true;
		}
		Beep();
	}

	private void Beep()
	{
		if (beeper == null)
		{
			beeper = Object.Instantiate(beepLight, base.transform.position, base.transform.rotation);
			beeper.transform.SetParent(base.transform, worldPositionStays: true);
			origScale = new Vector3(beeperSize, beeperSize, 1f);
			beeperSpriteRenderer = beeper.GetComponent<SpriteRenderer>();
			beeper.layer = base.gameObject.layer;
			aud = beeper.GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				aud.SetPitch(beeperPitch);
			}
		}
		if ((bool)(Object)(object)aud)
		{
			aud.Play(tracked: true);
		}
		beeper.transform.localScale = origScale * beeperSizeMultiplier;
		beeptimer = timer / 6f;
	}
}
