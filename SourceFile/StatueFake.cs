using UnityEngine;

public class StatueFake : MonoBehaviour
{
	private Animator anim;

	private AudioSource aud;

	private ParticleSystem part;

	public AudioSource step;

	public bool quickSpawn;

	[HideInInspector]
	public bool beingActivated;

	public UltrakillEvent onFirstCrack;

	private bool hasCracked;

	public UltrakillEvent onComplete;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
		aud = GetComponentInChildren<AudioSource>();
		part = GetComponentInChildren<ParticleSystem>();
		StatueIntroChecker instance = MonoSingleton<StatueIntroChecker>.Instance;
		if (instance != null)
		{
			if (instance.beenSeen)
			{
				quickSpawn = true;
			}
			else if (!quickSpawn)
			{
				instance.beenSeen = true;
			}
		}
		if (quickSpawn)
		{
			anim.speed = 1.5f;
		}
		if (beingActivated)
		{
			Activate();
		}
	}

	public void Activate()
	{
		beingActivated = true;
		if ((Object)(object)anim == null)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (quickSpawn)
		{
			anim.Play("Awaken", -1, 0.33f);
		}
		else
		{
			Invoke("SlowStart", 3f);
		}
	}

	public void Crack()
	{
		if (!hasCracked)
		{
			hasCracked = true;
			onFirstCrack?.Invoke();
		}
		aud.Play(tracked: true);
		part.Play();
	}

	public void Step()
	{
		Object.Instantiate<AudioSource>(step, base.transform.position, Quaternion.identity);
	}

	public void Done()
	{
		onComplete?.Invoke();
		Object.Destroy(this);
	}

	private void SlowStart()
	{
		anim.Play("Awaken", -1, 0f);
	}
}
