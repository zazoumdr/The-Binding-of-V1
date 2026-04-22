using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class GhostDrone : MonoBehaviour
{
	private LayerMask avoidanceMask;

	public float detectionAngle = 45f;

	public float detectionDistance = 12f;

	public float idleSpeed = 2f;

	public float attackSpeed = 6f;

	private float variableAttackSpeed;

	[HideInInspector]
	public Vector3 vacuumVelocity;

	private Vector3 originalPos = Vector3.zero;

	private Animator animator;

	private PlayerTracker pt;

	private Coroutine crt;

	private bool isAttacking;

	public bool alwaysAggro;

	[SerializeField]
	private GameObject killZone;

	private Light aggroLight;

	private Color startLightColor;

	private float startLightIntensity;

	private AudioSource aud;

	[SerializeField]
	private AudioClip spottedSound;

	[SerializeField]
	private AudioClip lostSound;

	[SerializeField]
	private AudioClip[] idleSounds;

	private TimeSince lastIdleSound;

	private Rigidbody rb;

	public AudioMixerGroup audioGroup;

	public GameObject deathExplosion;

	[SerializeField]
	private AudioClip ghostDeathSound;

	private bool isSucked;

	private bool wasSuckedLastFrame;

	private static readonly int IsScared = Animator.StringToHash("IsScared");

	[SerializeField]
	private AudioSource scaredAudioSource;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		avoidanceMask = LayerMaskDefaults.Get(LMD.Environment);
		aggroLight = GetComponentInChildren<Light>();
		aud = GetComponent<AudioSource>();
		startLightColor = aggroLight.color;
		startLightIntensity = aggroLight.intensity;
		pt = MonoSingleton<PlayerTracker>.Instance;
		animator = GetComponent<Animator>();
		originalPos = base.transform.position;
		crt = StartCoroutine(Fly());
		variableAttackSpeed = attackSpeed + Random.Range((0f - attackSpeed) / 2f, attackSpeed / 2f);
	}

	private void Update()
	{
		TryFindPlayer();
		if (((float)lastIdleSound > Random.Range(3f, 7f) && isAttacking) || (float)lastIdleSound > Random.Range(8f, 12f))
		{
			if (!aud.isPlaying)
			{
				aud.clip = idleSounds[Random.Range(0, idleSounds.Length)];
				aud.SetPitch(Random.Range(0.9f, 1.1f));
				aud.Play(tracked: true);
			}
			lastIdleSound = 0f;
		}
	}

	private void LateUpdate()
	{
		isSucked = vacuumVelocity.magnitude > 1E-05f;
		if (isSucked)
		{
			animator.speed = 4f;
			isAttacking = false;
			if (!scaredAudioSource.isPlaying)
			{
				scaredAudioSource.SetPitch(Random.Range(0.8f, 1.2f));
				scaredAudioSource.Play(tracked: true);
			}
		}
		else
		{
			scaredAudioSource.Stop();
		}
		animator.SetBool(IsScared, isSucked);
		if (wasSuckedLastFrame != isSucked)
		{
			if (isSucked)
			{
				StopCoroutine(crt);
				aggroLight.color = startLightColor;
				aggroLight.intensity = startLightIntensity;
			}
			else
			{
				crt = StartCoroutine(Fly());
			}
		}
		if (isSucked)
		{
			Vector3 normalized = (pt.GetTarget().position - base.transform.position).normalized;
			base.transform.rotation = Quaternion.LookRotation(normalized);
		}
		rb.position += vacuumVelocity * Time.deltaTime;
		vacuumVelocity = Vector3.zero;
		wasSuckedLastFrame = isSucked;
	}

	private void TryFindPlayer()
	{
		if (isAttacking || isSucked)
		{
			return;
		}
		Vector3 position = pt.GetTarget().position;
		Vector3 normalized = (position - base.transform.position).normalized;
		float num = Vector3.Angle(base.transform.forward, normalized);
		float num2 = Vector3.Distance(position, base.transform.position);
		if (alwaysAggro || (num2 <= detectionDistance && num < detectionAngle))
		{
			if (crt != null)
			{
				StopCoroutine(crt);
			}
			aud.clip = spottedSound;
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.Play(tracked: true);
			killZone.SetActive(value: true);
			base.transform.rotation = Quaternion.LookRotation(normalized);
			isAttacking = true;
			crt = StartCoroutine(Attack());
		}
	}

	public void KillGhost()
	{
		GameObject obj = Object.Instantiate(deathExplosion, base.transform.position, base.transform.rotation);
		obj.transform.localScale = Vector3.one * 0.1f;
		obj.GetComponent<AudioSource>().volume = 0.1f;
		obj.transform.GetChild(0).GetComponent<AudioSource>().volume = 0.1f;
		ghostDeathSound.PlayClipAtPoint(audioGroup, base.transform.position, 128, 1f, 1f, Random.Range(0.8f, 1.1f), (AudioRolloffMode)1);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator Attack()
	{
		aggroLight.color = Color.red;
		aggroLight.intensity = startLightIntensity * 2f;
		animator.speed = 4f * (variableAttackSpeed / attackSpeed);
		while (alwaysAggro || Vector3.Distance(pt.GetTarget().position, base.transform.position) < 30f)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = pt.GetTarget().position;
			Quaternion rotation = base.transform.rotation;
			Quaternion b = Quaternion.LookRotation((position2 - position).normalized);
			float num = variableAttackSpeed * Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, num * 4f);
			base.transform.rotation = Quaternion.Slerp(rotation, b, t);
			base.transform.position = Vector3.MoveTowards(base.transform.position, position2, num);
			yield return null;
		}
		aud.clip = lostSound;
		aud.SetPitch(Random.Range(0.9f, 1.1f));
		aud.Play(tracked: true);
		killZone.SetActive(value: false);
		isAttacking = false;
		StopCoroutine(crt);
		crt = StartCoroutine(Fly());
	}

	private IEnumerator Fly()
	{
		aggroLight.color = startLightColor;
		aggroLight.intensity = startLightIntensity;
		animator.speed = 0.5f;
		while (true)
		{
			Vector3 startPos = base.transform.position;
			bool noTarget = true;
			Vector3 targetPos = Vector3.one;
			while (noTarget)
			{
				targetPos = RandomNavmeshLocation(60f);
				Vector3 normalized = (targetPos - startPos).normalized;
				if (!Physics.Raycast(startPos, normalized, (targetPos - startPos).magnitude, avoidanceMask))
				{
					noTarget = false;
				}
				yield return null;
			}
			if (targetPos != startPos)
			{
				targetPos.y += 5f;
				Quaternion startDir = base.transform.rotation;
				float num = Vector3.Distance(startPos, targetPos);
				Vector3 normalized2 = (targetPos - startPos).normalized;
				Quaternion lookDir = Quaternion.LookRotation(normalized2);
				_ = num / idleSpeed;
				float elapsed = 0f;
				animator.speed = 1f;
				while (Vector3.Distance(base.transform.position, targetPos) > 0.1f)
				{
					elapsed += Time.deltaTime;
					float t = Mathf.SmoothStep(0f, 1f, elapsed);
					base.transform.rotation = Quaternion.Slerp(startDir, lookDir, t);
					float maxDistanceDelta = idleSpeed * Time.deltaTime;
					base.transform.position = Vector3.MoveTowards(base.transform.position, targetPos, maxDistanceDelta);
					yield return null;
				}
				animator.speed = 0.5f;
				yield return new WaitForSeconds(1.5f);
			}
		}
	}

	public Vector3 RandomNavmeshLocation(float radius)
	{
		Vector3 vector = Random.insideUnitSphere * radius + originalPos;
		Vector3 result = Vector3.zero;
		NavMeshHit val = default(NavMeshHit);
		if (NavMesh.SamplePosition(vector, ref val, radius, 1))
		{
			result = ((NavMeshHit)(ref val)).position;
		}
		return result;
	}
}
