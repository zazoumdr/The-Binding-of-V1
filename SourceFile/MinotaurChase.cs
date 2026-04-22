using UnityEngine;

public class MinotaurChase : MonoBehaviour
{
	private Rigidbody rb;

	private EnemyIdentifier eid;

	private Animator anim;

	private bool gotValues;

	private bool trackTarget;

	public float movementRange;

	public Vector3 leashPosition;

	private float leashRandomizer = 0.1f;

	private float movementSpeed;

	private float currentAnimatorWeight;

	private float cooldown = 1f;

	private int previousAttack = -1;

	private int currentAttacks;

	[SerializeField]
	private GameObject[] trams;

	private bool attacking;

	private bool backAttacking;

	[SerializeField]
	private SwingCheck2 hammerSwingCheck;

	[SerializeField]
	private TrailRenderer hammerTrail;

	[SerializeField]
	private Transform hammerPoint;

	[SerializeField]
	private GameObject hammerExplosion;

	[SerializeField]
	private GameObject meatInHand;

	[SerializeField]
	private GameObject handBlood;

	[SerializeField]
	private GameObject handSwingStuff;

	[SerializeField]
	private SwingCheck2 handSwingCheck;

	[SerializeField]
	private GameObject fallEffect;

	[SerializeField]
	private AudioSource roar;

	[SerializeField]
	private AudioClip roarClip;

	[SerializeField]
	private AudioClip longGruntClip;

	[SerializeField]
	private AudioClip shortRoarClip;

	[SerializeField]
	private AudioClip squealClip;

	[Header("Intro")]
	public bool intro;

	public UltrakillEvent onIntroEnd;

	private Transform tempTarget;

	private int difficulty = -1;

	private bool dead;

	private bool dragging;

	public Material hurtMaterial;

	public Mesh hurtMesh;

	private void Start()
	{
		GetValues();
		trackTarget = !intro;
		if (intro)
		{
			eid.totalDamageTakenMultiplier = 0f;
			QuickHammer();
		}
		else
		{
			IntroEnd();
		}
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			rb = GetComponent<Rigidbody>();
			eid = GetComponent<EnemyIdentifier>();
			anim = GetComponent<Animator>();
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			SetSpeed();
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		GetValues();
		if (difficulty >= 4)
		{
			movementSpeed = 35f;
			anim.speed = 1.2f;
		}
		else if (difficulty == 3)
		{
			movementSpeed = 30f;
			anim.speed = 1f;
		}
		else if (difficulty == 2)
		{
			movementSpeed = 25f;
			anim.speed = 1f;
		}
		else if (difficulty == 1)
		{
			movementSpeed = 20f;
			anim.speed = 0.9f;
		}
		else
		{
			movementSpeed = 10f;
			anim.speed = 0.8f;
		}
		movementSpeed *= eid.totalSpeedModifier;
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
	}

	private void Update()
	{
		if (trackTarget && (eid.target != null || tempTarget != null))
		{
			Transform transform = (tempTarget ? tempTarget : eid.target.targetTransform);
			Quaternion quaternion = Quaternion.LookRotation(new Vector3(transform.position.x, base.transform.position.y, transform.position.z) - base.transform.position);
			rb.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, (20f + Quaternion.Angle(base.transform.rotation, quaternion)) * ((attacking && difficulty <= 2) ? 0.5f : 1f) * Time.deltaTime);
			float num = ((base.transform.localRotation.eulerAngles.y > 180f) ? (base.transform.localRotation.eulerAngles.y - 360f) : base.transform.localRotation.eulerAngles.y);
			if (Mathf.Abs(num) > 66f)
			{
				base.transform.localRotation = Quaternion.Euler(0f, Mathf.Clamp(num, -66f, 66f), 0f);
			}
		}
		if (!attacking && Mathf.Abs(base.transform.position.z - leashPosition.z) < 10f)
		{
			if (eid.target != null && ((eid.target.position.y > base.transform.position.y + 3f && Vector3.Distance(base.transform.position, new Vector3(eid.target.position.x, base.transform.position.y, eid.target.position.z)) < 8f) || eid.target.position.z > base.transform.position.z))
			{
				if (!backAttacking)
				{
					backAttacking = true;
					HandSwingStart();
					Invoke("HandSwinging", 0.5f);
					handSwingCheck.CanHitPlayerMultipleTimes(yes: true);
					anim.SetBool("BackAttack", true);
				}
			}
			else if (backAttacking)
			{
				backAttacking = false;
				HandSwingStop();
				handSwingCheck.CanHitPlayerMultipleTimes(yes: false);
				anim.SetBool("BackAttack", false);
			}
			if (cooldown <= 0f && !backAttacking)
			{
				if (currentAttacks >= 2)
				{
					HammerSwing();
					currentAttacks = 0;
					previousAttack = -1;
					cooldown = ((difficulty > 2) ? 1 : 2);
				}
				else
				{
					int num2 = Random.Range(0, 2);
					if (num2 == previousAttack)
					{
						num2++;
					}
					if (num2 >= 2)
					{
						num2 = 0;
					}
					switch (num2)
					{
					case 0:
						MeatThrow();
						break;
					case 1:
						HandSwing();
						break;
					}
					cooldown = ((difficulty > 2) ? 1 : 2);
					previousAttack = num2;
					currentAttacks++;
				}
			}
		}
		if (!attacking && cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		if (!dead)
		{
			currentAnimatorWeight = Mathf.MoveTowards(currentAnimatorWeight, (attacking || backAttacking) ? 1 : 0, Time.deltaTime * 3f);
			anim.SetLayerWeight(1, currentAnimatorWeight);
		}
	}

	private void FixedUpdate()
	{
		if (trackTarget && eid.target != null)
		{
			float num = Mathf.MoveTowards(base.transform.position.z, leashPosition.z + leashRandomizer, Mathf.Min(Mathf.Abs(base.transform.position.z - (leashPosition.z + leashRandomizer)) * 2f + 0.5f, movementSpeed) * 1.5f * Time.fixedDeltaTime);
			Vector3 target = new Vector3(Mathf.Clamp((base.transform.position + base.transform.forward * movementSpeed * Time.fixedDeltaTime).x, leashPosition.x - movementRange, leashPosition.x + movementRange), base.transform.position.y, num);
			rb.MovePosition(Vector3.MoveTowards(base.transform.position, target, movementSpeed * Time.fixedDeltaTime));
			anim.SetFloat("RunSpeed", 1f + Mathf.Min(Mathf.Abs(num - leashPosition.z) * 2f, movementSpeed) / movementSpeed / 3f);
			if (Mathf.Abs(base.transform.position.z - (leashPosition.z + leashRandomizer)) < 0.01f)
			{
				leashRandomizer *= -1f;
			}
		}
		if (dragging)
		{
			rb.MovePosition(Vector3.MoveTowards(base.transform.position, base.transform.position + Vector3.forward * 100f, movementSpeed * 1.5f * Time.fixedDeltaTime));
		}
	}

	private void MeatThrow()
	{
		anim.Play("MeatThrow", 1, 0f);
		Roar(longGruntClip);
		attacking = true;
	}

	private void HandBlood()
	{
		Object.Instantiate(handBlood, meatInHand.transform.position, Quaternion.identity);
	}

	private void MeatSpawn()
	{
		meatInHand.SetActive(value: true);
		HandBlood();
	}

	private void MeatThrowPickTarget()
	{
		tempTarget = GetClosestTram(eid.target.position).transform;
	}

	private void MeatThrowThrow()
	{
		meatInHand.SetActive(value: false);
		GameObject closestTram = GetClosestTram(base.transform.position);
		if (closestTram != null)
		{
			ObjectSpawner componentInChildren = closestTram.GetComponentInChildren<ObjectSpawner>(includeInactive: true);
			if ((bool)componentInChildren)
			{
				componentInChildren.SpawnObject(1);
			}
		}
	}

	private void HandSwing()
	{
		anim.Play("HandSwing", 1, 0f);
		attacking = true;
		Roar(shortRoarClip, 0.75f);
		Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, meatInHand.transform.position, Quaternion.identity).transform.localScale *= 25f;
	}

	private void HandSwingStart()
	{
		handSwingStuff.SetActive(value: true);
		if (difficulty == 0)
		{
			trackTarget = false;
		}
		HandBlood();
	}

	private void HandSwinging()
	{
		handSwingCheck.DamageStart();
	}

	private void HandSwingStop()
	{
		handSwingStuff.SetActive(value: false);
		handSwingCheck.DamageStop();
		trackTarget = true;
		HandBlood();
	}

	private void HammerSwing()
	{
		anim.Play("HammerSwing", 1, 0f);
		attacking = true;
		Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, hammerTrail.transform.position, Quaternion.identity).transform.localScale *= 25f;
		Roar();
	}

	private void QuickHammer()
	{
		anim.Play("HammerSwing", 1, 0.4f);
		attacking = true;
	}

	private void HammerSwingStart()
	{
		hammerSwingCheck.DamageStart();
		hammerTrail.emitting = true;
	}

	private void HammerImpact()
	{
		trackTarget = false;
		Explosion[] componentsInChildren = Object.Instantiate(hammerExplosion, new Vector3(hammerPoint.position.x, base.transform.position.y, hammerPoint.position.z), Quaternion.identity).GetComponentsInChildren<Explosion>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].toIgnore.Add(EnemyType.Minotaur);
		}
		MonoSingleton<CameraController>.Instance.CameraShake(1.5f);
		if (intro)
		{
			IntroEnd();
			return;
		}
		GameObject closestTram = GetClosestTram(hammerPoint.position, 10f);
		if (!(closestTram != null))
		{
			return;
		}
		Harpoon[] componentsInChildren2 = closestTram.GetComponentsInChildren<Harpoon>();
		if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
		{
			Harpoon[] array = componentsInChildren2;
			foreach (Harpoon harpoon in array)
			{
				if (harpoon.gameObject.activeInHierarchy)
				{
					TimeBomb componentInChildren = harpoon.GetComponentInChildren<TimeBomb>();
					if ((bool)componentInChildren)
					{
						componentInChildren.dontExplode = true;
					}
					Object.Destroy(harpoon.gameObject);
				}
			}
		}
		closestTram.SetActive(value: false);
		closestTram.transform.position += closestTram.transform.forward * 200f;
		if (difficulty >= 4)
		{
			MonoSingleton<DelayedActivationManager>.Instance.Add(closestTram, 7f);
		}
		else if (difficulty >= 2)
		{
			MonoSingleton<DelayedActivationManager>.Instance.Add(closestTram, 5f);
		}
		else if (difficulty == 1)
		{
			MonoSingleton<DelayedActivationManager>.Instance.Add(closestTram, 3f);
		}
		else
		{
			MonoSingleton<DelayedActivationManager>.Instance.Add(closestTram, 1f);
		}
		ObjectSpawner componentInChildren2 = closestTram.GetComponentInChildren<ObjectSpawner>(includeInactive: true);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.SpawnObject(0);
		}
	}

	private void HammerSwingStop()
	{
		hammerSwingCheck.DamageStop();
		hammerTrail.emitting = false;
		if (difficulty <= 1)
		{
			trackTarget = false;
		}
	}

	public void Death()
	{
		dead = true;
		HammerSwingStop();
		handSwingStuff.SetActive(value: false);
		handSwingCheck.DamageStop();
		meatInHand.SetActive(value: false);
		anim.SetBool("BackAttack", false);
		anim.SetFloat("RunSpeed", 2.5f);
		anim.SetLayerWeight(1, 1f);
		anim.Play("Death", 1, 0f);
		attacking = true;
		trackTarget = false;
		Roar(squealClip);
		MonoSingleton<TimeController>.Instance.SlowDown(0.001f);
	}

	private void GetDragged()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (dragging)
		{
			return;
		}
		dragging = true;
		Animator obj = anim;
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(1);
		obj.Play("Death", 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
		anim.SetLayerWeight(1, 0f);
		Object.Instantiate(fallEffect, base.transform.position, Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(3f);
		EnemySimplifier componentInChildren = GetComponentInChildren<EnemySimplifier>();
		if ((bool)componentInChildren)
		{
			componentInChildren.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, hurtMaterial);
		}
		else
		{
			ChangeMaterials componentInChildren2 = GetComponentInChildren<ChangeMaterials>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.Activate();
			}
		}
		GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = hurtMesh;
	}

	public void StopDragging()
	{
		dragging = false;
	}

	private void StartTracking()
	{
		trackTarget = true;
	}

	private void ResetTarget()
	{
		tempTarget = null;
	}

	private void StopAction()
	{
		attacking = false;
		trackTarget = true;
	}

	private void IntroEnd()
	{
		onIntroEnd?.Invoke();
		eid.totalDamageTakenMultiplier = 1f;
		intro = false;
	}

	public void DisableIntro()
	{
		intro = false;
	}

	private GameObject GetClosestTram(Vector3 position, float shortestDistance = float.PositiveInfinity)
	{
		GameObject result = null;
		for (int i = 0; i < trams.Length; i++)
		{
			float num = Vector3.Distance(trams[i].transform.position, position);
			if (num < shortestDistance)
			{
				shortestDistance = num;
				result = trams[i];
			}
		}
		return result;
	}

	private void Roar()
	{
		Roar(roarClip);
	}

	private void Roar(float pitch = 1f)
	{
		Roar(roarClip, pitch);
	}

	private void Roar(AudioClip clip, float pitch = 1f)
	{
		roar.clip = clip;
		roar.SetPitch(Random.Range(pitch - 0.1f, pitch + 0.1f));
		roar.Play(tracked: true);
	}
}
