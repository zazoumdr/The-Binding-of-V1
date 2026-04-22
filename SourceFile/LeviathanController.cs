using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanController : EnemyScript
{
	[HideInInspector]
	public bool active = true;

	public LeviathanHead head;

	[SerializeField]
	private Transform headWeakPoint;

	public LeviathanTail tail;

	[SerializeField]
	private Transform tailWeakPoint;

	[HideInInspector]
	public EnemyIdentifier eid;

	[HideInInspector]
	public Enemy stat;

	public float tailAddHealth;

	public float phaseChangeHealth;

	public bool stopTail;

	private float tailTimer;

	private bool tailAttacking;

	private bool inSubPhase;

	private bool tailAddPhase;

	public bool readyForSecondPhase;

	[HideInInspector]
	public bool secondPhase;

	private int currentAttacks;

	private int setDifficulty = -1;

	public UltrakillEvent onEnterSecondPhase;

	[SerializeField]
	private Transform tailPartsParent;

	[SerializeField]
	private Transform headPartsParent;

	private Transform[] tailParts;

	private Transform[] headParts;

	private int currentPart;

	private GoreZone gz;

	private bool shaking;

	private Vector3 defaultPosition;

	public UltrakillEvent onDeathEnd;

	public GameObject bigSplash;

	[HideInInspector]
	public int difficulty
	{
		get
		{
			return GetDifficulty();
		}
		set
		{
			setDifficulty = value;
		}
	}

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		stat = GetComponent<Enemy>();
		tail.lcon = this;
		head.lcon = this;
	}

	private void UpdateBuff()
	{
		head.SetSpeed();
	}

	private int GetDifficulty()
	{
		if (setDifficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		return setDifficulty;
	}

	private void OnDestroy()
	{
		if (eid.dead)
		{
			DeathEnd();
		}
	}

	private void Update()
	{
		if (shaking)
		{
			base.transform.localPosition = defaultPosition + Random.onUnitSphere * 0.5f;
		}
		if (!active)
		{
			return;
		}
		if (!tailAddPhase && stat.health <= tailAddHealth)
		{
			tailAddPhase = true;
			if (!inSubPhase)
			{
				BeginSubPhase();
			}
			else
			{
				BeginMainPhase();
			}
		}
		else if (!secondPhase && stat.health <= phaseChangeHealth)
		{
			readyForSecondPhase = true;
			stopTail = true;
			eid.totalDamageTakenMultiplier = 0.5f;
		}
		if (tailAddPhase && !tailAttacking)
		{
			tailTimer = Mathf.MoveTowards(tailTimer, 0f, Time.deltaTime);
			if (tailTimer <= 0f && !stopTail)
			{
				SubAttack();
			}
		}
	}

	private void BeginMainPhase()
	{
		if (!active)
		{
			return;
		}
		if (!tailAddPhase)
		{
			inSubPhase = false;
		}
		if (readyForSecondPhase || secondPhase)
		{
			if (readyForSecondPhase)
			{
				eid.totalDamageTakenMultiplier = 1f;
				readyForSecondPhase = false;
				secondPhase = true;
				onEnterSecondPhase?.Invoke();
			}
			head.CenterPosition();
		}
		else
		{
			head.ChangePosition();
		}
		eid.weakPoint = headWeakPoint.gameObject;
	}

	public void MainPhaseOver()
	{
		if (active)
		{
			if (!tailAddPhase)
			{
				BeginSubPhase();
			}
			else
			{
				BeginMainPhase();
			}
		}
	}

	public void BeginSubPhase()
	{
		if (!inSubPhase && active)
		{
			if (!tailAddPhase)
			{
				eid.weakPoint = tailWeakPoint.gameObject;
			}
			inSubPhase = true;
			currentAttacks = 2;
			SubAttack();
		}
	}

	private void SubAttack()
	{
		if (active)
		{
			tailAttacking = true;
			if (!BlindEnemies.Blind || tailAddPhase)
			{
				tail.ChangePosition();
			}
			else
			{
				BeginMainPhase();
			}
		}
	}

	public void SubAttackOver()
	{
		if (!active)
		{
			return;
		}
		if (BlindEnemies.Blind)
		{
			currentAttacks = 0;
		}
		tailAttacking = false;
		if (!tailAddPhase)
		{
			currentAttacks--;
			if (currentAttacks <= 0)
			{
				BeginMainPhase();
			}
			else
			{
				SubAttack();
			}
		}
		else if (difficulty <= 2)
		{
			tailTimer = (10f - (float)difficulty * 2.5f) / eid.totalSpeedModifier;
		}
	}

	public void SpecialDeath()
	{
		headParts = headPartsParent.GetComponentsInChildren<Transform>();
		tailParts = tailPartsParent.GetComponentsInChildren<Transform>();
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		foreach (Animator val in componentsInChildren)
		{
			if (((Component)(object)val).gameObject != head.gameObject && ((Component)(object)val).gameObject != tail.gameObject)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		tail.Death();
		head.Death();
		active = false;
		head.active = false;
		defaultPosition = base.transform.localPosition;
		shaking = true;
	}

	private void ExplodeTail()
	{
		if (tailParts[currentPart] == null)
		{
			currentPart--;
			ExplodeTail();
			return;
		}
		bool flag = true;
		if (currentPart >= 0)
		{
			flag = tailParts[currentPart].position.y > base.transform.position.y - 5f;
			tailParts[currentPart].localScale = Vector3.zero;
			tailParts[currentPart].localPosition = Vector3.zero;
			if (flag)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed, eid);
				gore.transform.position = tailParts[currentPart].position;
				gore.transform.localScale *= 2f;
				if (!gz)
				{
					gz = GoreZone.ResolveGoreZone(base.transform);
				}
				gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (gore.TryGetComponent<AudioSource>(out var component))
				{
					component.maxDistance = 500f;
				}
				gore.SetActive(value: true);
			}
		}
		if (currentPart > 0)
		{
			currentPart = Mathf.Max(0, currentPart - 2);
			if (flag)
			{
				Invoke("ExplodeTail", 0.125f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("ExplodeTail", 0.025f / eid.totalSpeedModifier);
			}
		}
		else
		{
			tail.gameObject.SetActive(value: false);
			currentPart = headParts.Length - 1;
			Invoke("ExplodeHead", 0.125f / eid.totalSpeedModifier);
		}
	}

	private void ExplodeHead()
	{
		if (headParts[currentPart] == null)
		{
			if (currentPart > 0)
			{
				currentPart--;
				ExplodeHead();
			}
			else
			{
				Invoke("FinalExplosion", 0.5f / eid.totalSpeedModifier);
			}
			return;
		}
		bool flag = true;
		if (currentPart >= 0)
		{
			flag = headParts[currentPart].position.y > base.transform.position.y - 5f;
			headParts[currentPart].localScale = Vector3.zero;
			headParts[currentPart].localPosition = Vector3.zero;
			if (flag)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed, eid);
				gore.transform.position = headParts[currentPart].position;
				gore.transform.localScale *= 2f;
				if (!gz)
				{
					gz = GoreZone.ResolveGoreZone(base.transform);
				}
				gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (gore.TryGetComponent<AudioSource>(out var component))
				{
					component.maxDistance = 500f;
				}
				gore.SetActive(value: true);
			}
		}
		if (currentPart > 0)
		{
			currentPart = Mathf.Max(0, currentPart - 2);
			if (flag)
			{
				Invoke("ExplodeHead", 0.125f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("ExplodeHead", 0.025f / eid.totalSpeedModifier);
			}
		}
		else
		{
			Invoke("FinalExplosion", 0.5f / eid.totalSpeedModifier);
		}
	}

	public void FinalExplosion()
	{
		MeshRenderer[] componentsInChildren = head.tracker.GetComponentsInChildren<MeshRenderer>();
		GameObject gameObject = null;
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] == null)
			{
				continue;
			}
			for (int j = 0; j < 3; j++)
			{
				switch (j)
				{
				case 0:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed, eid);
					break;
				case 1:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGib(BSType.gib);
					break;
				case 2:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGib((BSType)Random.Range(0, 5));
					break;
				}
				if (!(gameObject == null))
				{
					gameObject.transform.position = componentsInChildren[i].transform.position;
					gameObject.transform.localScale *= (float)((j == 0) ? 3 : 15);
					if (!gz)
					{
						gz = GoreZone.ResolveGoreZone(base.transform);
					}
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
					gameObject.SetActive(value: true);
					num++;
				}
			}
		}
		shaking = false;
		MonoSingleton<TimeController>.Instance.SlowDown(0.0001f);
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
	}

	public void DeathEnd()
	{
		onDeathEnd?.Invoke();
		head.gameObject.SetActive(value: false);
	}

	private void GotParried()
	{
		stat.GetHurt(GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, Vector3.zero, 20f, 0f, head.tracker.position);
		head.GotParried();
	}
}
