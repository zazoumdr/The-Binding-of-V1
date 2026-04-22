using UnityEngine;

public class Pincer : MonoBehaviour
{
	[HideInInspector]
	public bool hasValues;

	public Vector3 direction;

	public bool randomInvertDirection;

	public float rotationSpeed = 180f;

	public float pincerSpeed = 45f;

	public float windup = 1f;

	public float delay = 0.5f;

	[HideInInspector]
	public float windupMax;

	[HideInInspector]
	public AudioSource aud;

	[HideInInspector]
	public float origVolume;

	[SerializeField]
	private SpriteRenderer insignia;

	private Vector3 originalScale;

	[SerializeField]
	private SpriteRenderer flash;

	[SerializeField]
	private ParticleSystem flashParticle;

	[SerializeField]
	private Color targetColor;

	[HideInInspector]
	public Color defaultColor;

	[HideInInspector]
	public bool detached;

	[HideInInspector]
	public bool activated;

	[SerializeField]
	private Transform[] beams;

	[HideInInspector]
	public LineRenderer[] lrs;

	[HideInInspector]
	public float[] lrWidths;

	[HideInInspector]
	public bool completed;

	public GameObject firedMessageReceiver;

	public UltrakillEvent onComplete;

	[HideInInspector]
	public int difficulty = -1;

	private TimeSince timeSincePincerStart;

	private TimeSince timeSinceCompletion;

	private void Start()
	{
		GetValues();
		if (difficulty <= 0)
		{
			Detach();
		}
	}

	private void GetValues()
	{
		if (!hasValues)
		{
			hasValues = true;
			windupMax = windup;
			if ((bool)insignia)
			{
				defaultColor = insignia.color;
			}
			lrs = GetComponentsInChildren<LineRenderer>(includeInactive: true);
			lrWidths = new float[lrs.Length];
			for (int i = 0; i < lrs.Length; i++)
			{
				lrWidths[i] = lrs[i].widthMultiplier;
			}
			aud = GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origVolume = aud.volume;
			}
			if (difficulty <= -1)
			{
				MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
			if (randomInvertDirection && Random.Range(0f, 1f) > 0.5f)
			{
				direction *= -1f;
			}
		}
	}

	private void Update()
	{
		base.transform.Rotate(direction, rotationSpeed * Time.deltaTime);
		if ((bool)flash && flash.gameObject.activeSelf)
		{
			flash.transform.Rotate(direction, rotationSpeed * 4f * Time.deltaTime);
		}
		if (windup > 0f)
		{
			windup = Mathf.MoveTowards(windup, 0f, Time.deltaTime);
			if ((bool)insignia)
			{
				insignia.color = Color.Lerp(defaultColor, targetColor, 1f - windup / windupMax);
			}
			if (!activated && windup <= 0f)
			{
				activated = true;
				if (difficulty == 1)
				{
					Detach();
				}
				Transform[] array = beams;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(value: true);
				}
				if ((bool)flash)
				{
					flash.gameObject.SetActive(value: true);
				}
				if ((bool)insignia)
				{
					insignia.transform.localScale *= 0.5f;
				}
				if ((bool)firedMessageReceiver)
				{
					firedMessageReceiver.SendMessage("PincerFired", SendMessageOptions.DontRequireReceiver);
				}
				if ((bool)(Object)(object)aud)
				{
					aud.Play(tracked: true);
				}
			}
		}
		else if (delay > 0f)
		{
			delay = Mathf.MoveTowards(delay, 0f, Time.deltaTime);
			if (delay == 0f)
			{
				if (difficulty <= 3)
				{
					Detach();
				}
				timeSincePincerStart = 0f;
			}
		}
		else if (!completed)
		{
			bool flag = true;
			Transform[] array = beams;
			foreach (Transform obj in array)
			{
				obj.forward = Vector3.MoveTowards(obj.forward, base.transform.forward, Time.deltaTime);
				if (obj.forward != base.transform.forward)
				{
					flag = false;
				}
			}
			if (!detached && difficulty >= 4 && (float)timeSincePincerStart > pincerSpeed / 90f)
			{
				Detach();
			}
			if ((bool)(Object)(object)aud)
			{
				aud.SetPitch(Mathf.MoveTowards(aud.GetPitch(), 2f, Time.deltaTime * (pincerSpeed / 90f)));
			}
			if (flag)
			{
				completed = true;
				onComplete.Invoke();
				timeSinceCompletion = 0f;
				originalScale = base.transform.localScale;
				if ((bool)(Object)(object)flashParticle)
				{
					flashParticle.Stop();
				}
			}
		}
		else
		{
			base.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, (float)timeSinceCompletion * 2f);
			if ((bool)(Object)(object)aud)
			{
				aud.volume = Mathf.Lerp(origVolume, 0f, (float)timeSinceCompletion * 2f);
			}
			for (int j = 0; j < lrs.Length; j++)
			{
				lrs[j].widthMultiplier = Mathf.Lerp(lrWidths[j], 0f, (float)timeSinceCompletion * 2f);
			}
			if ((float)timeSinceCompletion > 0.5f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void Detach()
	{
		detached = true;
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		base.transform.SetParent(componentInParent ? componentInParent.transform : null, worldPositionStays: true);
	}
}
