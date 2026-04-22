using System.Collections.Generic;
using UnityEngine;

public class FleshPrison : EnemyScript
{
	public Transform rotationBone;

	private Collider col;

	private Animator anim;

	public bool altVersion;

	private Texture currentIdleTexture;

	private Texture defaultTexture;

	public Texture[] idleTextures;

	private float idleTimer = 0.5f;

	public Texture hurtTexture;

	public Texture attackTexture;

	[SerializeField]
	private EnemySimplifier mainSimplifier;

	private AudioSource aud;

	private BossHealthBar bossHealth;

	private float secondaryBarValue;

	private bool started;

	private bool inAction;

	private float health;

	private EnemyIdentifier eid;

	private Enemy stat;

	private bool hurting;

	private bool shakingCamera;

	private Vector3 origPos;

	public GameObject fleshDrone;

	public GameObject skullDrone;

	private float fleshDroneCooldown = 3f;

	private int droneAmount = 10;

	private int currentDrone;

	private GameObject targeter;

	private bool healing;

	public List<DroneFlesh> currentDrones = new List<DroneFlesh>();

	public GameObject healingTargetEffect;

	public GameObject healingEffect;

	private float rotationSpeed = 45f;

	private float rotationSpeedTarget = 45f;

	private float attackCooldown = 5f;

	private int previousAttack = 666;

	public GameObject insignia;

	private float maxHealth;

	public GameObject homingProjectile;

	private int projectileAmount = 40;

	private int currentProjectile = 40;

	private float homingProjectileCooldown;

	public GameObject attackWindUp;

	public GameObject blackHole;

	private BlackHoleProjectile currentBlackHole;

	private int difficulty = -1;

	public UltrakillEvent onFirstHeal;

	private int timesHealed;

	private bool noDrones;

	private MaterialPropertyBlock texOverride;

	private float maxDroneCooldown => (!started) ? 3 : ((difficulty == 2) ? 25 : 30);

	private void Awake()
	{
		if (!mainSimplifier)
		{
			mainSimplifier = GetComponentInChildren<EnemySimplifier>();
		}
		eid = GetComponent<EnemyIdentifier>();
		stat = GetComponent<Enemy>();
		aud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		bossHealth = GetComponent<BossHealthBar>();
	}

	private void Start()
	{
		if ((bool)mainSimplifier && !eid.puppet)
		{
			defaultTexture = mainSimplifier.originalMaterial.mainTexture;
		}
		maxHealth = stat.health;
		health = stat.health;
		origPos = rotationBone.localPosition;
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		col = rotationBone.GetComponentInChildren<EnemyIdentifierIdentifier>().GetComponent<Collider>();
		stat.isMassDeath = true;
	}

	private void Update()
	{
		float num = Mathf.Abs(rotationSpeed);
		if (num < 45f)
		{
			num = 45f;
		}
		if (rotationSpeed != rotationSpeedTarget)
		{
			rotationSpeed = Mathf.MoveTowards(rotationSpeed, rotationSpeedTarget, Time.deltaTime * (num / 2f + 5f));
		}
		rotationBone.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed * eid.totalSpeedModifier, Space.Self);
		if (eid.target == null)
		{
			return;
		}
		CheckHealth();
		UpdateTexture();
		UpdateCooldowns();
		if ((bool)(Object)(object)anim && !noDrones)
		{
			anim.speed = ((!inAction) ? 1 : 5);
		}
		if (shakingCamera)
		{
			MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
		}
		if (!inAction)
		{
			if (fleshDroneCooldown <= 0f)
			{
				StartSpawningFleshDrones();
			}
			else if (fleshDroneCooldown > 3f && attackCooldown <= 0f)
			{
				ChooseAttack();
			}
		}
		else
		{
			ChangeTexture(attackTexture);
			idleTimer = 0f;
			if (currentProjectile < projectileAmount)
			{
				ProjectileBurstUpdate();
			}
		}
	}

	private void CheckHealth()
	{
		if (health < stat.health)
		{
			health = stat.health;
		}
		else if (health > stat.health)
		{
			float num = health - stat.health;
			if (currentDrones.Count > 0)
			{
				for (int num2 = currentDrones.Count - 1; num2 >= 0; num2--)
				{
					if (currentDrones[num2] == null)
					{
						currentDrones.RemoveAt(num2);
					}
				}
			}
			float num3 = 1.5f;
			switch (currentDrones.Count)
			{
			default:
				num3 = 1.5f;
				break;
			case 6:
			case 7:
			case 8:
				num3 = 0.4f;
				break;
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
				num3 = 0.2f;
				break;
			case 0:
				num3 = 0.133f;
				break;
			}
			fleshDroneCooldown -= num * num3;
			health = stat.health;
		}
		if ((bool)bossHealth)
		{
			if (!healing)
			{
				secondaryBarValue = Mathf.MoveTowards(secondaryBarValue, fleshDroneCooldown / maxDroneCooldown, (Mathf.Abs(secondaryBarValue - fleshDroneCooldown / maxDroneCooldown) + 1f) * Time.deltaTime);
			}
			bossHealth.UpdateSecondaryBar(secondaryBarValue);
		}
	}

	private void UpdateTexture()
	{
		if (inAction)
		{
			return;
		}
		if (health > stat.health)
		{
			idleTimer = 0.15f;
			ChangeTexture(hurtTexture);
			hurting = true;
			return;
		}
		idleTimer = Mathf.MoveTowards(idleTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
		if (hurting)
		{
			if (idleTimer > 0f)
			{
				rotationBone.transform.localPosition = new Vector3(origPos.x + Random.Range(0f - idleTimer, idleTimer), origPos.y, origPos.z + Random.Range(0f - idleTimer, idleTimer));
			}
			else
			{
				rotationBone.transform.localPosition = origPos;
				hurting = false;
			}
		}
		if (idleTimer == 0f)
		{
			Texture tex = defaultTexture;
			if (currentIdleTexture == defaultTexture)
			{
				idleTimer = 0.25f;
				tex = idleTextures[Random.Range(0, idleTextures.Length)];
			}
			else
			{
				idleTimer = Random.Range(0.5f, 1f);
			}
			currentIdleTexture = tex;
			ChangeTexture(tex);
		}
	}

	private void ChangeTexture(Texture tex)
	{
		if (!eid.puppet)
		{
			mainSimplifier.ChangeTexture(EnemySimplifier.MaterialState.normal, tex);
		}
	}

	private void UpdateCooldowns()
	{
		if (fleshDroneCooldown > 0f)
		{
			fleshDroneCooldown = Mathf.MoveTowards(fleshDroneCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (attackCooldown > 0f)
		{
			float num = 1f;
			if (difficulty == 1)
			{
				num = 0.9f;
			}
			else if (difficulty == 0)
			{
				num = 0.75f;
			}
			attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * num * eid.totalSpeedModifier);
		}
	}

	private void ChooseAttack()
	{
		int num = ((!currentBlackHole && difficulty > 0) ? 3 : 2);
		int num2 = Random.Range(0, num);
		if (num2 == previousAttack)
		{
			num2 = ((num2 + 1 < num) ? (num2 + 1) : 0);
		}
		inAction = true;
		Color color = Color.white;
		float time = 1f / eid.totalSpeedModifier;
		switch (num2)
		{
		case 0:
			Invoke("SpawnInsignia", time);
			color = Color.white;
			attackCooldown = 4f;
			break;
		case 1:
			Invoke("HomingProjectileAttack", time);
			color = (altVersion ? new Color(1f, 0.75f, 0f) : new Color(0f, 1f, 0.9f));
			attackCooldown = 1f;
			break;
		case 2:
			Invoke("SpawnBlackHole", time);
			color = new Color(1f, 0f, 1f);
			attackCooldown = 2f;
			break;
		}
		GameObject obj = Object.Instantiate(attackWindUp, rotationBone.position, Quaternion.LookRotation(eid.target.position - rotationBone.position));
		if (obj.TryGetComponent<Light>(out var component))
		{
			component.color = color;
		}
		if (obj.TryGetComponent<SpriteRenderer>(out var component2))
		{
			component2.color = color;
		}
		previousAttack = num2;
	}

	private void ProjectileBurstUpdate()
	{
		homingProjectileCooldown = Mathf.MoveTowards(homingProjectileCooldown, 0f, Time.deltaTime * (Mathf.Abs(rotationSpeed) / 10f) * eid.totalSpeedModifier);
		if (homingProjectileCooldown <= 0f)
		{
			GameObject gameObject = Object.Instantiate(homingProjectile, rotationBone.position + rotationBone.up * 8f, rotationBone.rotation);
			Projectile component = gameObject.GetComponent<Projectile>();
			component.target = eid.target;
			component.safeEnemyType = (altVersion ? EnemyType.FleshPanopticon : EnemyType.FleshPrison);
			switch (difficulty)
			{
			case 4:
			case 5:
				component.turningSpeedMultiplier = 0.66f;
				break;
			case 2:
			case 3:
				component.turningSpeedMultiplier = 0.5f;
				break;
			case 1:
				component.turningSpeedMultiplier = 0.45f;
				break;
			case 0:
				component.turningSpeedMultiplier = 0.4f;
				break;
			}
			if (altVersion)
			{
				component.turnSpeed *= 4f;
				component.turningSpeedMultiplier *= 4f;
				component.predictiveHomingMultiplier = 1.25f;
				if (gameObject.TryGetComponent<Rigidbody>(out var component2))
				{
					component2.AddForce(Vector3.up * 50f, ForceMode.VelocityChange);
				}
			}
			component.damage *= eid.totalDamageModifier;
			homingProjectileCooldown = 1f;
			currentProjectile++;
			gameObject.transform.SetParent(base.transform, worldPositionStays: true);
		}
		if (currentProjectile >= projectileAmount)
		{
			inAction = false;
			Animator obj = anim;
			if (obj != null)
			{
				obj.SetBool("Shooting", false);
			}
			rotationSpeedTarget = ((rotationSpeed >= 0f) ? 45 : (-45));
			if (fleshDroneCooldown < 1f)
			{
				fleshDroneCooldown = 1f;
			}
		}
	}

	private void StartSpawningFleshDrones()
	{
		started = true;
		for (int num = currentDrones.Count - 1; num >= 0; num--)
		{
			if (currentDrones[num] == null)
			{
				currentDrones.RemoveAt(num);
			}
		}
		ChangeTexture(attackTexture);
		idleTimer = 0f;
		fleshDroneCooldown = maxDroneCooldown;
		attackCooldown = 3f;
		inAction = true;
		rotationSpeed = 0f;
		rotationSpeedTarget = 0f;
		droneAmount = ((stat.health > maxHealth / 2f) ? 10 : 12);
		if (difficulty <= 1)
		{
			droneAmount = Mathf.RoundToInt((float)droneAmount / ((difficulty == 0) ? 2f : 1.5f));
		}
		if (altVersion)
		{
			droneAmount /= 2;
		}
		if (droneAmount < 3)
		{
			droneAmount = 3;
		}
		timesHealed++;
		if (timesHealed == 2)
		{
			onFirstHeal?.Invoke();
		}
		if (currentDrones.Count <= 0)
		{
			healing = true;
			secondaryBarValue = 0f;
			Invoke("SpawnFleshDrones", 1f / eid.totalSpeedModifier);
		}
		else
		{
			StartHealing();
		}
		shakingCamera = true;
		aud.Play(tracked: true);
	}

	private void SpawnFleshDrones()
	{
		if (eid.target == null)
		{
			return;
		}
		float num = 360f / (float)droneAmount;
		if (currentDrone == 0)
		{
			targeter = new GameObject("Targeter");
			targeter.transform.position = rotationBone.position;
			Vector3 vector = (altVersion ? Vector3.up : (new Vector3(eid.target.position.x, targeter.transform.position.y, eid.target.position.z) - targeter.transform.position));
			Quaternion rotation = (altVersion ? Quaternion.LookRotation(vector.normalized) : Quaternion.LookRotation(vector.normalized, Vector3.up));
			targeter.transform.rotation = rotation;
			targeter.transform.Rotate(Vector3.forward * num / 2f);
		}
		if (currentDrone < droneAmount)
		{
			secondaryBarValue = (float)currentDrone / (float)droneAmount;
			GameObject obj = Object.Instantiate(((difficulty == 3 && currentDrone % 5 == 0) || (difficulty == 4 && currentDrone % 3 == 0) || difficulty == 5) ? skullDrone : fleshDrone, targeter.transform.position + targeter.transform.up * (altVersion ? 50 : 20), targeter.transform.rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: true);
			if (obj.TryGetComponent<EnemyIdentifier>(out var component))
			{
				component.dontCountAsKills = true;
				component.damageBuff = eid.damageBuff;
				component.healthBuff = eid.healthBuff;
				component.speedBuff = eid.speedBuff;
			}
			if (obj.TryGetComponent<DroneFlesh>(out var component2))
			{
				currentDrones.Add(component2);
			}
			targeter.transform.Rotate(Vector3.forward * num);
			currentDrone++;
			Invoke("SpawnFleshDrones", 0.1f / eid.totalSpeedModifier);
		}
		else
		{
			inAction = false;
			rotationSpeedTarget = ((Random.Range(0, 2) == 0) ? 45 : (-45));
			aud.Stop();
			shakingCamera = false;
			currentDrone = 0;
			Object.Destroy(targeter);
			fleshDroneCooldown = (altVersion ? 30 : 25);
			healing = false;
		}
	}

	private void StartHealing()
	{
		healing = true;
		secondaryBarValue = 0f;
		for (int num = currentDrones.Count - 1; num >= 0; num--)
		{
			if (currentDrones[num] == null)
			{
				currentDrones.RemoveAt(num);
			}
			else
			{
				if (Object.Instantiate(healingTargetEffect, currentDrones[num].transform).TryGetComponent<LineToPoint>(out var component))
				{
					component.targets[1] = rotationBone;
				}
				if (currentDrones[num].TryGetComponent<Rigidbody>(out var component2))
				{
					component2.isKinematic = true;
				}
			}
		}
		if (difficulty >= 1)
		{
			eid.totalDamageTakenMultiplier = 0.1f;
		}
		if (currentDrones.Count > 0)
		{
			Invoke("HealFromDrone", 5f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("SpawnFleshDrones", 1f / eid.totalSpeedModifier);
		}
	}

	private void HealFromDrone()
	{
		if (stat.health <= 0f)
		{
			return;
		}
		if (currentDrones.Count <= 0)
		{
			eid.totalDamageTakenMultiplier = 1f;
			SpawnFleshDrones();
			return;
		}
		if (currentDrones[0] == null)
		{
			currentDrones.RemoveAt(0);
			HealFromDrone();
			return;
		}
		float num = 1f;
		if (difficulty <= 1)
		{
			num = ((difficulty == 0) ? 0.35f : 0.75f);
		}
		num /= eid.totalHealthModifier;
		if (altVersion)
		{
			num *= 2f;
		}
		if (!Physics.Raycast(rotationBone.position, currentDrones[0].transform.position - rotationBone.position, Vector3.Distance(rotationBone.position, currentDrones[0].transform.position), LayerMaskDefaults.Get(LMD.Environment)))
		{
			stat.health += 10f * num;
			if (stat.health > maxHealth)
			{
				stat.health = maxHealth;
			}
			eid.health = stat.health;
			Object.Instantiate(healingEffect, rotationBone);
		}
		currentDrones[0].Explode();
		currentDrones.RemoveAt(0);
		Invoke("HealFromDrone", 0.25f / eid.totalSpeedModifier);
	}

	private void HomingProjectileAttack()
	{
		inAction = true;
		ChangeTexture(attackTexture);
		idleTimer = 0f;
		homingProjectileCooldown = 1f;
		currentProjectile = 0;
		Animator obj = anim;
		if (obj != null)
		{
			obj.SetBool("Shooting", true);
		}
		rotationSpeedTarget = ((Random.Range(0, 2) == 0) ? 360 : (-360));
		if (altVersion)
		{
			rotationSpeedTarget /= 8f;
		}
		if ((rotationSpeedTarget > 0f && rotationSpeed < 0f) || (rotationSpeedTarget < 0f && rotationSpeed > 0f))
		{
			rotationSpeed = 0f;
		}
		if (difficulty >= 2)
		{
			projectileAmount = ((stat.health > maxHealth / 2f) ? 50 : 75);
		}
		else
		{
			projectileAmount = ((stat.health > maxHealth / 2f) ? 35 : 50);
		}
		if (altVersion)
		{
			projectileAmount /= 3;
		}
	}

	private void SpawnInsignia()
	{
		if (eid.target == null)
		{
			return;
		}
		inAction = false;
		GameObject gameObject = Object.Instantiate(insignia, eid.target.position, Quaternion.identity);
		if (altVersion)
		{
			Vector3 velocity = eid.target.GetVelocity();
			velocity.y = 0f;
			if (velocity.magnitude > 0f)
			{
				gameObject.transform.LookAt(eid.target.position + velocity);
			}
			else
			{
				gameObject.transform.Rotate(Vector3.up * Random.Range(0f, 360f), Space.Self);
			}
			gameObject.transform.Rotate(Vector3.right * 90f, Space.Self);
		}
		if (gameObject.TryGetComponent<VirtueInsignia>(out var component))
		{
			component.predictive = true;
			component.noTracking = true;
			component.otherParent = base.transform;
			component.charges = ((stat.health > maxHealth / 2f) ? 2 : 3);
			if (difficulty >= 3)
			{
				component.charges += difficulty - 2;
			}
			component.windUpSpeedMultiplier = 0.5f;
			component.windUpSpeedMultiplier *= eid.totalSpeedModifier;
			component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
			component.target = eid.target;
			component.predictiveVersion = null;
			Light light = gameObject.AddComponent<Light>();
			light.range = 30f;
			light.intensity = 50f;
		}
		float num = 8f;
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			num = 8f;
			break;
		case 1:
			num = 7f;
			break;
		case 0:
			num = 5f;
			break;
		}
		gameObject.transform.localScale = new Vector3(num, 2f, num);
		gameObject.transform.SetParent(GoreZone.ResolveGoreZone(base.transform).transform, worldPositionStays: true);
		if (fleshDroneCooldown < 1f)
		{
			fleshDroneCooldown = 1f;
		}
	}

	private void SpawnBlackHole()
	{
		GameObject obj = Object.Instantiate(blackHole, base.transform);
		obj.transform.position = rotationBone.position;
		if (obj.TryGetComponent<BlackHoleProjectile>(out currentBlackHole))
		{
			currentBlackHole.target = eid.target;
			currentBlackHole.safeType = EnemyType.FleshPrison;
			currentBlackHole.Activate();
		}
		inAction = false;
		ChangeTexture(attackTexture);
		idleTimer = 0.5f;
		if (fleshDroneCooldown < 1f)
		{
			fleshDroneCooldown = 1f;
		}
	}

	public void ForceDronesOff()
	{
		noDrones = true;
		CancelInvoke("HealFromDrone");
		CancelInvoke("SpawnFleshDrones");
		Animator obj = anim;
		if (obj != null)
		{
			obj.SetBool("Shooting", false);
		}
		if (currentDrones.Count > 0)
		{
			foreach (DroneFlesh currentDrone in currentDrones)
			{
				currentDrone.Explode();
			}
		}
		if ((bool)currentBlackHole)
		{
			currentBlackHole.Explode();
		}
		VirtueInsignia[] array = Object.FindObjectsOfType<VirtueInsignia>();
		foreach (VirtueInsignia virtueInsignia in array)
		{
			if (virtueInsignia.otherParent == base.transform)
			{
				Object.Destroy(virtueInsignia.gameObject);
			}
		}
		Projectile[] componentsInChildren = GetComponentsInChildren<Projectile>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		if ((bool)(Object)(object)anim)
		{
			anim.speed = 20f;
		}
	}

	public void HasBossBar(BossHealthBar bhb)
	{
		bossHealth = bhb;
	}

	public override void OnGoLimp(bool fromExplosion)
	{
	}
}
