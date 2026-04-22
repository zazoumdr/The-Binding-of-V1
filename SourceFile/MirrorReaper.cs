using System.Collections.Generic;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Utilities;

public class MirrorReaper : EnemyScript
{
	public bool stationary;

	public float speed = 5f;

	private NavMeshAgent nma;

	private EnemyIdentifier eid;

	private Animator anim;

	private Enemy mach;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	public FootController fC;

	public Rig rig;

	[SerializeField]
	private SwingCheck2 highSwingCheck;

	[SerializeField]
	private SwingCheck2 lowSwingCheck;

	[SerializeField]
	private SwingCheck2 verticalSwingCheck;

	private MirrorReaperMelee previousMeleeAttack;

	[SerializeField]
	private TrailRenderer scytheTrail;

	[SerializeField]
	private GameObject teleportEffect;

	[SerializeField]
	private GroundWave groundWave;

	private List<GroundWave> currentGroundWaves = new List<GroundWave>();

	private int maxGroundWaves = 3;

	[SerializeField]
	private Projectile projectile;

	[SerializeField]
	private GameObject decorativeProjectile;

	[SerializeField]
	private Transform[] projectileSpawnPoints;

	private bool teleportedToEscape;

	private int recentGroundWaves = 3;

	[SerializeField]
	private AudioClip tripleWindup;

	[SerializeField]
	private AudioClip verticalWindup;

	[SerializeField]
	private AudioClip spreeWindup;

	[SerializeField]
	private AudioClip projectileWindup;

	[SerializeField]
	private AudioClip groundwaveWindup;

	[SerializeField]
	private AudioClip reverseTeleportSound;

	[SerializeField]
	private AudioClip[] movementSounds;

	private float cooldown;

	private bool attacking;

	private bool meleeAttacking;

	private bool targetOverride;

	private Vector3 targetOverridePosition;

	private bool lockRotation;

	private bool inAction;

	private bool canRetreat;

	private bool inMeleeMode = true;

	private float attacksSinceModeChange;

	private TimeSince sinceLastAttack;

	private TimeSince sinceLastTeleport;

	private Vector3 teleportTarget;

	private TimeSince sincePathable;

	private TimeSince sinceVisible;

	private TimeSince sinceOverGround;

	private bool isTargetPathable;

	private bool isTargetVisible;

	private bool isTargetOverGround;

	private TimeSince sinceMeleeRange;

	private float cowardPlayerTimer;

	[Header("Special Behaviour")]
	public bool spawnAnimation;

	public bool startHidden;

	public bool dontSpamProjectiles;

	private float hiddenVisibilityTimer;

	public bool useMirrorPhase;

	public float mirrorPhaseHealth;

	public GameObject objectToMirrorPhase;

	[HideInInspector]
	public bool inMirrorPhase;

	public bool useEscapePoints;

	public Transform[] escapePoints;

	private List<Transform> usedEscapePoints = new List<Transform>();

	private int difficulty = -1;

	private TargetData lastTargetData;

	private VisionQuery targetQuery;

	private EnemyTarget target => eid.target;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => base.transform.position + base.transform.up * 3f;

	private void Start()
	{
		mach = GetComponent<Enemy>();
		eid = GetComponent<EnemyIdentifier>();
		nma = GetComponent<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();
		nma.updateRotation = false;
		sinceLastTeleport = 5f;
		UpdateDifficulty();
		if (startHidden)
		{
			inMeleeMode = false;
			teleportedToEscape = true;
			hiddenVisibilityTimer = 0f;
		}
		LayerMask lm = LayerMaskDefaults.Get(LMD.Environment);
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, lm, toHead: true));
		if (spawnAnimation || eid.spawnIn)
		{
			spawnAnimation = false;
			inAction = true;
			anim.Play("Teleport", 0, 0.65f);
		}
		TrackTick();
	}

	private void UpdateDifficulty()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			maxGroundWaves = 3;
			anim.speed = 1f;
			break;
		case 3:
			maxGroundWaves = 2;
			anim.speed = 1f;
			break;
		case 2:
			maxGroundWaves = 2;
			anim.speed = 0.9f;
			break;
		case 1:
			maxGroundWaves = 1;
			anim.speed = 0.85f;
			break;
		case 0:
			maxGroundWaves = 1;
			anim.speed = 0.75f;
			break;
		}
	}

	public void TrackTick()
	{
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		float updateRate = GetUpdateRate(nma, 0.2f, 0.5f, 30f);
		Invoke("TrackTick", updateRate);
		if (inAction || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (IsTargetOverGround(getNewValue: true))
		{
			sinceOverGround = 0f;
		}
		if (IsTargetPathable(getNewValue: true))
		{
			sincePathable = 0f;
		}
		if (target == null || (Object)(object)nma == null)
		{
			return;
		}
		if (target.position.y > target.GetNavPoint().y + 3f && Vector3.Distance(base.transform.position, new Vector3(target.position.x, base.transform.position.y, target.position.z)) < 10f)
		{
			cowardPlayerTimer = Mathf.MoveTowards(cowardPlayerTimer, 5f, updateRate);
		}
		else
		{
			cowardPlayerTimer = Mathf.MoveTowards(cowardPlayerTimer, 0f, updateRate * 2f);
		}
		if (!eid.enabled || !mach.grounded || mach.falling || mach.knockedBack || mach.isOnOffNavmeshLink || mach.isTraversingPortalLink)
		{
			return;
		}
		if (teleportedToEscape || stationary)
		{
			if (IsTargetVisible() && !stationary)
			{
				hiddenVisibilityTimer = Mathf.MoveTowards(hiddenVisibilityTimer, 1f, updateRate);
				Debug.Log("Target is visible");
			}
			else
			{
				lockRotation = true;
				NavMeshPath val = new NavMeshPath();
				NavMesh.CalculatePath(base.transform.position, target.GetNavPoint(), nma.areaMask, val);
				if (val.corners.Length > 1)
				{
					((Behaviour)(object)nma).enabled = false;
					base.transform.LookAt(new Vector3(val.corners[1].x, base.transform.position.y, val.corners[1].z));
					return;
				}
				hiddenVisibilityTimer = Mathf.MoveTowards(hiddenVisibilityTimer, 1f, updateRate);
				Debug.Log("Target is visible due to lack of corners");
			}
			if (hiddenVisibilityTimer >= 1f)
			{
				hiddenVisibilityTimer = 0f;
				lockRotation = false;
				teleportedToEscape = false;
			}
		}
		if (!stationary)
		{
			if (!mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget))
			{
				lastDimensionalTarget = Vector3.zero;
			}
			((Behaviour)(object)nma).enabled = true;
			Vector3 destination = ((!inMeleeMode && !meleeAttacking && IsTargetVisible()) ? GetEscapePosition() : ((!inMeleeMode || (attacking && !canRetreat) || !(cooldown > 0f)) ? GetApproachPosition(hasDimensionalTarget ? EnemyTarget.GetNavPoint(lastDimensionalTarget) : target.GetNavPoint()) : GetEscapePosition(10f)));
			mach.SetDestination(destination);
			if (!attacking)
			{
				TeleportCheck();
			}
		}
	}

	private Vector3 GetApproachPosition(Vector3 targetPos)
	{
		if (difficulty >= 4)
		{
			return targetPos;
		}
		float num = 4f;
		switch (difficulty)
		{
		case 3:
			num = 2f;
			break;
		case 2:
			num = 4f;
			break;
		case 0:
		case 1:
			num = 5f;
			break;
		}
		return targetPos - (targetPos - base.transform.position).normalized * num;
	}

	private Vector3 GetEscapePosition(float desiredDistance = 30f)
	{
		Vector3 vector = (hasDimensionalTarget ? lastDimensionalTarget : lastTargetData.position);
		Vector3 normalized = (new Vector3(vector.x, base.transform.position.y, vector.z) - base.transform.position).normalized;
		float num = 0f;
		Vector3 result = base.transform.position;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector2 = normalized * -1f;
			switch (i)
			{
			case 1:
				vector2 = Quaternion.AngleAxis(90f, base.transform.up) * normalized;
				break;
			case 2:
				vector2 = Quaternion.AngleAxis(-90f, base.transform.up) * normalized;
				break;
			case 3:
				vector2 = normalized;
				break;
			}
			if (CheckEscapePosition(vector2, desiredDistance, out var point, out var distance))
			{
				return EnemyTarget.GetNavPoint(target.position + Vector3.up + vector2 * desiredDistance);
			}
			if (distance > num)
			{
				result = point;
				num = distance;
			}
		}
		return result;
	}

	private bool CheckEscapePosition(Vector3 direction, float desiredDistance, out Vector3 point, out float distance)
	{
		if (!PortalPhysicsV2.Raycast(target.position + Vector3.up, direction, out var hitInfo, desiredDistance, LayerMaskDefaults.Get(LMD.Environment)))
		{
			if (PortalPhysicsV2.Raycast(target.position + Vector3.up + direction * desiredDistance, Vector3.down, desiredDistance, LayerMaskDefaults.Get(LMD.Environment)))
			{
				point = Vector3.zero;
				distance = desiredDistance;
				return true;
			}
			point = target.position;
			distance = 0f;
			return false;
		}
		if (currentGroundWaves.Count < maxGroundWaves && PortalPhysicsV2.Raycast(target.position + Vector3.up + direction * desiredDistance, Vector3.down, out hitInfo, desiredDistance, LayerMaskDefaults.Get(LMD.Environment)) && IsPathable(hitInfo.point))
		{
			point = hitInfo.point;
			distance = desiredDistance;
			return true;
		}
		point = hitInfo.point - direction.normalized;
		distance = hitInfo.distance;
		return false;
	}

	private void TeleportCheck()
	{
		if ((((inMeleeMode || currentGroundWaves.Count < maxGroundWaves) && (float)sincePathable >= 1f) || (!inMeleeMode && (float)sinceVisible > 1f)) && (float)sinceLastTeleport > 3f && IsTargetOverGround())
		{
			Teleport();
		}
	}

	private void TeleportToEscapePoint()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Invalid comparison between Unknown and I4
		if (escapePoints.Length == 0 || !PortalPhysicsV2.Raycast(target.position + Vector3.up, Vector3.down, 100f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			return;
		}
		Debug.Log("Tried to TeleportToEscapePoint");
		Transform transform = base.transform;
		float num = 0f;
		int num2 = 0;
		NavMeshPath val = new NavMeshPath();
		Vector3 vector = target.GetNavPoint();
		int num3 = Random.Range(0, 3);
		NavMeshHit val2 = default(NavMeshHit);
		if (!NavMesh.SamplePosition(vector, ref val2, 1f, nma.areaMask))
		{
			Vector3 vector2 = vector;
			Debug.Log("Target position not close to a NavMesh: " + vector2.ToString());
		}
		else
		{
			vector = ((NavMeshHit)(ref val2)).position;
		}
		NavMeshHit val3 = default(NavMeshHit);
		for (int i = 0; i < escapePoints.Length; i++)
		{
			if (!NavMesh.SamplePosition(escapePoints[i].position, ref val3, 1f, nma.areaMask))
			{
				Debug.Log("Position " + i + " not close to a NavMesh");
			}
			else if (!NavMesh.CalculatePath(((NavMeshHit)(ref val3)).position, vector, nma.areaMask, val))
			{
				Debug.Log("Position " + i + " invalid");
			}
			else if ((int)val.status == 1 && Vector3.Distance(vector, val.corners[val.corners.Length - 1]) > 10f)
			{
				Debug.Log("Position " + i + " is only partial");
			}
			else
			{
				if (usedEscapePoints.Contains(escapePoints[i]))
				{
					continue;
				}
				switch (num3)
				{
				case 0:
				{
					float distance = val.GetDistance();
					if (distance > num && val.corners.Length < 2)
					{
						transform = escapePoints[i];
						num = distance;
					}
					break;
				}
				case 1:
				{
					int num5 = val.corners.Length;
					if (num5 > num2)
					{
						transform = escapePoints[i];
						num2 = num5;
					}
					break;
				}
				case 2:
				{
					float num4 = val.GetDistance() + (float)(val.corners.Length * 10);
					if (num4 > num)
					{
						transform = escapePoints[i];
						num = num4;
					}
					break;
				}
				}
			}
		}
		if (!(transform == base.transform))
		{
			if (usedEscapePoints.Count == 2)
			{
				usedEscapePoints.RemoveAt(0);
			}
			usedEscapePoints.Add(transform);
			Teleport(transform.position);
			teleportedToEscape = true;
			hiddenVisibilityTimer = 0f;
		}
	}

	private void Update()
	{
		if (IsTargetVisible(getNewValue: true))
		{
			sinceVisible = 0f;
		}
		if (target != null)
		{
			if (!inAction && !attacking && useMirrorPhase && !inMirrorPhase && mach.health < mirrorPhaseHealth)
			{
				Teleport();
			}
			if (!lockRotation)
			{
				Vector3 vector = (targetOverride ? targetOverridePosition : lastTargetData.position);
				Vector3 vector2 = new Vector3(vector.x, base.transform.position.y, vector.z);
				base.transform.rotation = Quaternion.LookRotation(vector2 - base.transform.position);
			}
			AttackCheck();
		}
	}

	private void AttackCheck()
	{
		float num = Vector3.Distance(base.transform.position, target.position);
		bool flag = num < 10f;
		if (flag)
		{
			sinceMeleeRange = 0f;
		}
		if (attacking || inAction)
		{
			return;
		}
		CheckMode();
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		if (!(cooldown > 0f))
		{
			if (cowardPlayerTimer > 4f && IsTargetVisible())
			{
				attacksSinceModeChange += 1f;
				ProjectileBarrage();
			}
			if (flag)
			{
				PickMeleeAttack();
			}
			else if (!inMeleeMode)
			{
				PickRangedAttack();
			}
			else if ((((float)sinceMeleeRange > 2f && num > 15f) || (float)sinceMeleeRange > 3f) && IsTargetVisible())
			{
				attacksSinceModeChange += 1f;
				ProjectileBarrage();
			}
		}
	}

	private void CheckMode()
	{
		if (inMeleeMode && ((float)sinceLastAttack > 5f || (float)sinceOverGround > 3f))
		{
			inMeleeMode = false;
			sinceLastAttack = 0f;
			attacksSinceModeChange = 0f;
			return;
		}
		UpdateCurrentGroundWaves();
		if (inMeleeMode)
		{
			if (currentGroundWaves.Count >= maxGroundWaves || attacksSinceModeChange < 3f)
			{
				return;
			}
		}
		else if (stationary || (!teleportedToEscape && currentGroundWaves.Count < maxGroundWaves && recentGroundWaves < 3) || (teleportedToEscape && recentGroundWaves < 5) || (float)sinceOverGround > 3f)
		{
			return;
		}
		inMeleeMode = !inMeleeMode;
		attacksSinceModeChange = 0f;
		if (inMeleeMode)
		{
			lockRotation = false;
			if ((float)sinceVisible > 1f && !teleportedToEscape)
			{
				Teleport();
				return;
			}
			teleportedToEscape = false;
			sinceMeleeRange = 0f;
			UpdateCurrentGroundWaves();
		}
		else
		{
			recentGroundWaves = 0;
			if (escapePoints.Length != 0)
			{
				TeleportToEscapePoint();
			}
		}
	}

	private void PickMeleeAttack()
	{
		if ((target.isOnGround || !IsTargetOverGround()) && target.position.y > base.transform.position.y + 3f)
		{
			SwingVertical();
			previousMeleeAttack = MirrorReaperMelee.SwingVertical;
			attacksSinceModeChange += 1f;
			return;
		}
		int num = Random.Range(1, 4);
		if (num == (int)previousMeleeAttack)
		{
			num = ((num == 3) ? 1 : (num + 1));
		}
		switch ((MirrorReaperMelee)num)
		{
		case MirrorReaperMelee.SwingTriple:
			SwingTriple();
			break;
		case MirrorReaperMelee.SwingVertical:
			SwingVertical();
			break;
		case MirrorReaperMelee.SwingSpree:
			SwingSpree();
			break;
		}
		previousMeleeAttack = (MirrorReaperMelee)num;
		attacksSinceModeChange += 1f;
	}

	private void PickRangedAttack()
	{
		previousMeleeAttack = MirrorReaperMelee.None;
		UpdateCurrentGroundWaves();
		if (IsTargetPathable() && (currentGroundWaves.Count < maxGroundWaves || stationary || teleportedToEscape))
		{
			if (currentGroundWaves.Count >= maxGroundWaves)
			{
				currentGroundWaves[0].lifetime = 0f;
				currentGroundWaves.RemoveAt(0);
			}
			GroundWave();
			if (teleportedToEscape || !inMeleeMode)
			{
				recentGroundWaves++;
			}
		}
		else
		{
			if (!stationary && !IsTargetVisible() && IsTargetOverGround())
			{
				Teleport();
				return;
			}
			if (!stationary || IsTargetVisible())
			{
				ProjectileBarrage();
			}
		}
		attacksSinceModeChange += 1f;
	}

	private void PrepAttack(bool ranged = false)
	{
		cooldown = 0.5f;
		attacking = true;
		meleeAttacking = !ranged;
		canRetreat = false;
	}

	private void SwingTriple()
	{
		PrepAttack();
		anim.Play("SwingTriple", 0, 0f);
		PlaySound(tripleWindup);
	}

	private void SwingVertical()
	{
		PrepAttack();
		anim.Play("SwingVertical", 0, 0f);
		PlaySound(verticalWindup);
	}

	private void SwingSpree()
	{
		PrepAttack();
		anim.Play("SwingSpree", 0, 0f);
		PlaySound(spreeWindup);
	}

	private void GroundWave()
	{
		PrepAttack(ranged: true);
		anim.Play("GroundWave", 0, 0f);
		PlaySound(groundwaveWindup);
	}

	private void ProjectileBarrage()
	{
		anim.SetFloat("ProjectileBarrageSpeed", (float)((dontSpamProjectiles || !((float)sinceOverGround > 5f)) ? 1 : 2));
		cooldown = ((!dontSpamProjectiles && (float)sinceOverGround > 5f) ? 0f : 0.5f);
		attacking = true;
		canRetreat = false;
		anim.Play("ProjectileBarrage", 0, 0f);
		PlaySound(projectileWindup);
	}

	private void Teleport()
	{
		NavMeshHit val = default(NavMeshHit);
		if (NavMesh.SamplePosition(EnemyTarget.GetNavPoint(target.position), ref val, 5f, nma.areaMask))
		{
			Teleport(((NavMeshHit)(ref val)).position);
		}
	}

	private void Teleport(Vector3 point)
	{
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		anim.Play("Teleport", 0, 0f);
		sinceLastTeleport = 0f;
		teleportTarget = point;
		GameObject gameObject = Object.Instantiate(teleportEffect, base.transform.position, base.transform.rotation);
		if (inMirrorPhase)
		{
			EnemyIdentifier.SendToPortalLayer(gameObject);
		}
		if (gameObject.TryGetComponent<AudioSource>(out var component))
		{
			component.clip = reverseTeleportSound;
			component.Play(tracked: true);
		}
	}

	public void TeleportNow()
	{
		nma.Warp(teleportTarget);
		sinceMeleeRange = 0f;
		if (useMirrorPhase && !inMirrorPhase && mach.health < mirrorPhaseHealth)
		{
			OutdoorsChecker[] componentsInChildren = GetComponentsInChildren<OutdoorsChecker>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
			inMirrorPhase = true;
			eid.mirrorOnly = true;
			objectToMirrorPhase.layer = 30;
			eid.SendAttachedProjectilesToMirrorLayer();
			inMeleeMode = true;
			attacksSinceModeChange = 0f;
		}
		GameObject gob = Object.Instantiate(teleportEffect, teleportTarget, base.transform.rotation);
		if (inMirrorPhase)
		{
			EnemyIdentifier.SendToPortalLayer(gob);
		}
	}

	private void UpdateCurrentGroundWaves()
	{
		if (currentGroundWaves.Count <= 0)
		{
			return;
		}
		for (int num = currentGroundWaves.Count - 1; num >= 0; num--)
		{
			if (currentGroundWaves[num] == null)
			{
				currentGroundWaves.RemoveAt(num);
			}
		}
	}

	public void SpawnGroundWave()
	{
		VerticalDamageStart();
		GroundWave groundWave = Object.Instantiate(this.groundWave, base.transform.position, base.transform.rotation);
		groundWave.target = eid.target;
		groundWave.transform.SetParent(base.transform.parent ? base.transform.parent : GoreZone.ResolveGoreZone(base.transform).transform);
		currentGroundWaves.Add(groundWave);
		if (inMirrorPhase && difficulty >= 4)
		{
			EnemyIdentifier.SendToPortalLayer(groundWave.gameObject);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			groundWave.lifetime = 15f;
			break;
		case 2:
		case 3:
			groundWave.lifetime = 10f;
			break;
		case 0:
		case 1:
			groundWave.lifetime = 5f;
			break;
		}
		Breakable componentInChildren = groundWave.GetComponentInChildren<Breakable>();
		if ((bool)componentInChildren)
		{
			switch (difficulty)
			{
			case 3:
			case 4:
			case 5:
				componentInChildren.durability = 5f;
				break;
			case 2:
				componentInChildren.durability = 3f;
				break;
			case 0:
			case 1:
				componentInChildren.durability = 1f;
				break;
			}
		}
		groundWave.eid = eid;
		groundWave.difficulty = difficulty;
	}

	public void SpawnDecorativeProjectiles()
	{
		for (int i = 0; i < projectileSpawnPoints.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(decorativeProjectile, projectileSpawnPoints[i].position, projectileSpawnPoints[i].rotation);
			gameObject.transform.SetParent(projectileSpawnPoints[i], worldPositionStays: true);
			if (inMirrorPhase)
			{
				EnemyIdentifier.SendToPortalLayer(gameObject);
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Body, eid);
			if (GraphicsSettings.bloodEnabled && (bool)gore)
			{
				gore.transform.position = projectileSpawnPoints[i].position;
				gore.transform.SetParent(GoreZone.ResolveGoreZone(base.transform).goreZone, worldPositionStays: true);
				gore.SetActive(value: true);
			}
		}
	}

	public void SpawnProjectiles()
	{
		PlayMovementSound();
		bool flag = Physics.Raycast(base.transform.position, Vector3.up, 31f, LayerMaskDefaults.Get(LMD.Environment));
		for (int i = 0; i < projectileSpawnPoints.Length; i++)
		{
			if (projectileSpawnPoints[i].childCount > 0)
			{
				for (int num = projectileSpawnPoints[i].childCount - 1; num >= 0; num--)
				{
					Object.Destroy(projectileSpawnPoints[i].GetChild(num).gameObject);
				}
			}
			Projectile projectile = Object.Instantiate(this.projectile, projectileSpawnPoints[i].position, Quaternion.LookRotation(flag ? (target.position - base.transform.position) : base.transform.up));
			projectile.transform.SetParent(base.transform.parent, worldPositionStays: true);
			projectile.safeEnemyType = EnemyType.MirrorReaper;
			projectile.speed = Random.Range(15, 25);
			projectile.target = target;
			if (inMirrorPhase && difficulty >= 4)
			{
				EnemyIdentifier.SendToPortalLayer(projectile.gameObject);
			}
		}
	}

	public void Death()
	{
		rig.weight = 0f;
		UpdateCurrentGroundWaves();
		if (currentGroundWaves.Count > 0)
		{
			for (int num = currentGroundWaves.Count - 1; num >= 0; num--)
			{
				currentGroundWaves[num].lifetime = 0f;
			}
		}
		Object.Destroy(this);
	}

	public void LowDamageStart()
	{
		lowSwingCheck.DamageStart();
		DamageStart();
	}

	public void VerticalDamageStart()
	{
		verticalSwingCheck.DamageStart();
		DamageStart();
	}

	public void HighDamageStart()
	{
		highSwingCheck.DamageStart();
		DamageStart();
	}

	private void DamageStart()
	{
		scytheTrail.emitting = true;
		PlayMovementSound();
	}

	public void DamageStop()
	{
		highSwingCheck.DamageStop();
		lowSwingCheck.DamageStop();
		verticalSwingCheck.DamageStop();
		scytheTrail.emitting = false;
		targetOverride = false;
		lockRotation = false;
	}

	public void StartMoving()
	{
		attacking = true;
		inAction = false;
		((Behaviour)(object)nma).enabled = true;
	}

	public void StopAction()
	{
		attacking = false;
		meleeAttacking = false;
		canRetreat = true;
		sinceLastAttack = 0f;
		inAction = false;
		((Behaviour)(object)nma).enabled = true;
	}

	public void PredictTarget()
	{
		lockRotation = true;
		if (difficulty > 1)
		{
			float time = 1f;
			switch (difficulty)
			{
			case 4:
			case 5:
				time = 1.5f;
				break;
			case 3:
				time = 1f;
				break;
			case 2:
				time = 0.5f;
				break;
			}
			Vector3 vector = target.PredictTargetPosition(time, includeGravity: true, assumeGroundMovement: true);
			base.transform.rotation = Quaternion.LookRotation(new Vector3(vector.x, base.transform.position.y, vector.z) - base.transform.position);
			PlayMovementSound();
		}
	}

	private bool IsTargetVisible(bool getNewValue = false)
	{
		if (target == null)
		{
			if (getNewValue)
			{
				isTargetVisible = false;
			}
			return false;
		}
		if (getNewValue)
		{
			isTargetVisible = vision.TrySee(targetQuery, out var data);
			if (isTargetVisible)
			{
				lastTargetData = data.ToData();
			}
		}
		return isTargetVisible;
	}

	private bool IsTargetOverGround(bool getNewValue = false)
	{
		if (target == null || (Object)(object)nma == null)
		{
			if (getNewValue)
			{
				isTargetOverGround = false;
			}
			return false;
		}
		if (getNewValue)
		{
			NavMeshHit val = default(NavMeshHit);
			isTargetOverGround = PortalPhysicsV2.Raycast(target.position + Vector3.up, base.transform.up * -1f, out var hitInfo, 30f, LayerMaskDefaults.Get(LMD.Environment)) && NavMesh.SamplePosition(hitInfo.point, ref val, 1f, nma.areaMask);
		}
		return isTargetOverGround;
	}

	private bool IsTargetPathable(bool getNewValue = false)
	{
		if (target == null)
		{
			if (getNewValue)
			{
				isTargetPathable = false;
			}
			return false;
		}
		if (!isTargetOverGround)
		{
			isTargetPathable = false;
		}
		else if (getNewValue)
		{
			isTargetPathable = IsPathable(target.GetNavPoint());
		}
		return isTargetPathable;
	}

	private bool IsPathable(Vector3 point)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		if (!(Object)(object)nma || !((Behaviour)(object)nma).enabled || !nma.isOnNavMesh)
		{
			return false;
		}
		NavMeshPath val = new NavMeshPath();
		nma.CalculatePath(point, val);
		return (int)val.status == 0;
	}

	private void PlayMovementSound()
	{
		PlaySound(movementSounds[Random.Range(0, movementSounds.Length)], 0.25f);
	}

	private void PlaySound(AudioClip clip, float pitchRange = 0.1f)
	{
		if (!((Object)(object)clip == null))
		{
			((Component)(object)clip.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.allGroup, base.transform.position, 25, 1f, 1f, Random.Range(1f - pitchRange, 1f + pitchRange), (AudioRolloffMode)1, 1f, 50f)).transform.SetParent(base.transform, worldPositionStays: true);
		}
	}

	private void CanRetreat()
	{
		canRetreat = true;
	}
}
