using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
	protected ZombieMelee zm => script as ZombieMelee;

	public ZombieProjectiles zp => script as ZombieProjectiles;

	protected override void SetSpeed()
	{
		if (limp)
		{
			return;
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!(Object)(object)nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			speedMultiplier = 1.5f;
			break;
		case 3:
			speedMultiplier = 1.25f;
			break;
		case 2:
			speedMultiplier = 1f;
			break;
		case 1:
			speedMultiplier = 0.75f;
			break;
		case 0:
			speedMultiplier = 0.5f;
			break;
		}
		if ((bool)zm)
		{
			switch (difficulty)
			{
			case 4:
			case 5:
				nma.acceleration = 120f;
				nma.angularSpeed = 9000f;
				nma.speed = 20f;
				break;
			case 3:
				nma.acceleration = 60f;
				nma.angularSpeed = 2600f;
				nma.speed = 20f;
				break;
			case 2:
				nma.acceleration = 30f;
				nma.angularSpeed = 800f;
				nma.speed = 20f;
				break;
			case 1:
				nma.acceleration = 30f;
				nma.angularSpeed = 400f;
				nma.speed = 15f;
				break;
			case 0:
				nma.acceleration = 15f;
				nma.angularSpeed = 400f;
				nma.speed = 10f;
				break;
			}
		}
		else if (eid.enemyType == EnemyType.Soldier)
		{
			float num = 15f;
			if (difficulty == 4)
			{
				num = 17.5f;
			}
			else if (difficulty == 5)
			{
				num = 20f;
			}
			nma.speed = num * speedMultiplier;
			anim.SetFloat("RunSpeed", ((difficulty == 5) ? 1.75f : 1f) * speedMultiplier);
			nma.angularSpeed = 480f;
			nma.acceleration = 480f;
		}
		else
		{
			nma.speed = 10f * speedMultiplier;
			nma.angularSpeed = 800f;
			nma.acceleration = 30f;
		}
		NavMeshAgent obj = nma;
		obj.acceleration *= eid.totalSpeedModifier;
		NavMeshAgent obj2 = nma;
		obj2.angularSpeed *= eid.totalSpeedModifier;
		NavMeshAgent obj3 = nma;
		obj3.speed *= eid.totalSpeedModifier;
		if ((bool)(Object)(object)nma)
		{
			defaultSpeed = nma.speed;
		}
		if ((bool)(Object)(object)anim)
		{
			if (variableSpeed)
			{
				anim.speed = 1f * speedMultiplier;
			}
			else if (difficulty >= 2)
			{
				anim.speed = 1f * eid.totalSpeedModifier;
			}
			else if (difficulty == 1)
			{
				anim.speed = 0.875f * eid.totalSpeedModifier;
			}
			else if (difficulty == 0)
			{
				anim.speed = 0.75f * eid.totalSpeedModifier;
			}
		}
	}

	protected override void Update()
	{
		if (knockBackCharge > 0f)
		{
			knockBackCharge = Mathf.MoveTowards(knockBackCharge, 0f, Time.deltaTime);
		}
	}

	protected override void FixedUpdate()
	{
		if (parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
		if (limp)
		{
			return;
		}
		if (knockedBack && knockBackCharge <= 0f && rb.velocity.magnitude < 1f && gc.onGround)
		{
			StopKnockBack();
		}
		else if (knockedBack)
		{
			if (eid.useBrakes || gc.onGround)
			{
				if (knockBackCharge <= 0f && gc.onGround)
				{
					brakes = Mathf.MoveTowards(brakes, 0f, 0.0005f * brakes);
				}
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, rb.velocity.y - juggleWeight, rb.velocity.z * 0.95f * brakes);
			}
			else if (!eid.useBrakes)
			{
				brakes = 1f;
			}
			nma.updatePosition = false;
			nma.updateRotation = false;
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
		}
		if (grounded && (Object)(object)nma != null && ((Behaviour)(object)nma).enabled && variableSpeed && nma.isOnNavMesh)
		{
			if (nma.isStopped || nma.velocity == Vector3.zero || stopped)
			{
				anim.SetFloat("RunSpeed", 1f);
			}
			else
			{
				anim.SetFloat("RunSpeed", nma.velocity.magnitude / nma.speed);
			}
		}
		else if (!grounded && gc.onGround)
		{
			grounded = true;
			nma.speed = defaultSpeed;
		}
		isOnOffNavmeshLink = nma.isOnOffMeshLink;
		if (!gc.onGround && !falling && !nma.isOnOffMeshLink)
		{
			grounded = false;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
			((Behaviour)(object)nma).enabled = false;
			falling = true;
			anim.SetBool("Falling", true);
			anim.SetTrigger("StartFalling");
			if (zp != null)
			{
				zp.CancelAttack();
			}
			if (zm != null && !zm.diving)
			{
				zm.CancelAttack();
			}
		}
	}

	public override void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, Vector3 hurtPos = default(Vector3), GameObject sourceWeapon = null, bool fromExplosion = false)
	{
		//IL_0b25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2a: Unknown result type (might be due to invalid IL or missing references)
		string hitLimb = "";
		bool flag = false;
		bool flag2 = false;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if ((bool)gc && !gc.onGround && eid.hitter != "fire")
		{
			multiplier *= 1.5f;
		}
		if (force != Vector3.zero && !limp)
		{
			KnockBack(force / 100f);
			if (eid.hitter == "heavypunch" || (eid.hitter == "cannonball" && (bool)gc && !gc.onGround))
			{
				eid.useBrakes = false;
			}
			else
			{
				eid.useBrakes = true;
			}
		}
		if (chestExploding && health <= 0f && (target.CompareTag("Limb") || target.CompareTag("EndLimb")) && target.GetComponentInParent<EnemyIdentifier>() != null)
		{
			ChestExplodeEnd();
		}
		GameObject gameObject = null;
		if (bsm == null)
		{
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
		}
		if ((bool)zm && zm.diving)
		{
			zm.CancelAttack();
		}
		if (eid.hitter == "punch")
		{
			if (attacking)
			{
				if (!InvincibleEnemies.Enabled && !eid.blessed)
				{
					health -= ((parryFramesLeft > 0) ? 4 : 5);
				}
				attacking = false;
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
			}
			else
			{
				parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
			}
		}
		if (target.CompareTag("Head"))
		{
			float num = 1f * multiplier + multiplier * critMultiplier;
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				gameObject = ((!(num >= 1f) && !(health <= 0f)) ? bsm.GetGore(GoreType.Small, eid, fromExplosion) : bsm.GetGore(GoreType.Head, eid, fromExplosion));
			}
			if (!limp)
			{
				flag2 = true;
				hitLimb = "head";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter != "fire" && eid.hitter != "sawblade")
				{
					float num2 = 1f;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
					{
						num2 = 0.5f;
					}
					else if (eid.hitter == "Explosion")
					{
						num2 = 0.25f;
					}
					if (target.transform.parent != null && target.transform.parent.GetComponentInParent<Rigidbody>() != null)
					{
						target.transform.parent.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
					}
					if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && eid.hitter != "harpoon")
					{
						GameObject gameObject2 = null;
						GetGoreZone();
						for (int i = 0; (float)i < 6f * num2; i++)
						{
							gameObject2 = bsm.GetGib(BSType.skullChunk);
							ReadyGib(gameObject2, target);
						}
						for (int j = 0; (float)j < 4f * num2; j++)
						{
							gameObject2 = bsm.GetGib(BSType.brainChunk);
							ReadyGib(gameObject2, target);
						}
						for (int k = 0; (float)k < 2f * num2; k++)
						{
							gameObject2 = bsm.GetGib(BSType.eyeball);
							ReadyGib(gameObject2, target);
							gameObject2 = bsm.GetGib(BSType.jawChunk);
							ReadyGib(gameObject2, target);
						}
					}
				}
			}
		}
		else if (target.CompareTag("Limb") || target.CompareTag("EndLimb"))
		{
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			float num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				if (eid.hitter == "hammer")
				{
					gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
				}
				else if (((num >= 1f || health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
				{
					gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
				}
			}
			_ = (target.transform.position - base.transform.position).normalized;
			if (!limp)
			{
				flag2 = true;
				hitLimb = "limb";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter == "sawblade")
				{
					if (!chestExploded && target.transform.position.y > chest.transform.position.y - 1f)
					{
						ChestExplosion(cut: true);
					}
				}
				else if (eid.hitter != "fire" && eid.hitter != "harpoon")
				{
					if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && eid.hitter != "explosion" && target.gameObject.CompareTag("Limb"))
					{
						float num3 = 1f;
						GetGoreZone();
						if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone")
						{
							num3 = 0.5f;
						}
						for (int l = 0; (float)l < 4f * num3; l++)
						{
							GameObject gib = bsm.GetGib(BSType.gib);
							ReadyGib(gib, target);
						}
					}
					else
					{
						target.transform.localScale = Vector3.zero;
						target.SetActive(value: false);
					}
				}
			}
		}
		else
		{
			float num = multiplier;
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
			{
				if (!attacking && (target.gameObject != chest || health - num > 0f))
				{
					num = 0f;
				}
				else if (attacking && (target.gameObject == chest || eid.target.GetVelocity().magnitude > 18f))
				{
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						num *= 2f;
					}
					MonoSingleton<NewMovement>.Instance.Parry(eid);
				}
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			if (eid.hitter != "fire" && num > 0f)
			{
				gameObject = ((eid.hitter == "hammer") ? bsm.GetGore(GoreType.Head, eid, fromExplosion) : ((!(num >= 1f) && !(health <= 0f)) ? bsm.GetGore(GoreType.Small, eid, fromExplosion) : bsm.GetGore(GoreType.Body, eid, fromExplosion)));
			}
			if (health <= 0f && target.gameObject == chest && eid.hitter != "fire")
			{
				if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone" || eid.hitter == "sawblade")
				{
					chestHP = 0f;
				}
				else
				{
					chestHP -= num;
				}
				if (chestHP <= 0f && eid.hitter != "harpoon")
				{
					ChestExplosion(eid.hitter == "sawblade", fromExplosion);
				}
			}
			if (!limp)
			{
				flag2 = true;
				hitLimb = "body";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if (eid.hitter != "sawblade" && target.GetComponentInParent<Rigidbody>() != null)
				{
					target.GetComponentInParent<Rigidbody>().AddForce(force * 10f);
				}
			}
		}
		if (gameObject != null)
		{
			GetGoreZone();
			gameObject.transform.position = target.transform.position;
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component)
			{
				CollisionModule collision = component.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						((CollisionModule)(ref collision)).enabled = false;
					}
					component.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component.hpAmount = 1;
					AudioSource component2 = component.GetComponent<AudioSource>();
					component2.volume *= 0.8f;
				}
				if (!noheal)
				{
					component.GetReady();
				}
			}
		}
		if (health <= 0f)
		{
			if (eid.hitter == "sawblade")
			{
				Cut(target);
			}
			else if (eid.hitter != "harpoon" && eid.hitter != "fire")
			{
				if (target.CompareTag("Limb"))
				{
					if (target.transform.childCount > 0)
					{
						Transform child = target.transform.GetChild(0);
						CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
						GetGoreZone();
						if (componentsInChildren.Length != 0)
						{
							CharacterJoint[] array = componentsInChildren;
							foreach (CharacterJoint characterJoint in array)
							{
								if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component3))
								{
									component3.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
								}
								characterJoint.transform.SetParent(gz.transform);
								Object.Destroy(characterJoint);
							}
						}
						if (target.TryGetComponent<CharacterJoint>(out var component4))
						{
							component4.connectedBody = null;
							Object.Destroy(component4);
						}
						target.transform.position = child.position;
						target.transform.SetParent(child);
						child.SetParent(gz.transform, worldPositionStays: true);
						Object.Destroy(target.GetComponent<Rigidbody>());
					}
					Object.Destroy(target.GetComponent<Collider>());
					target.transform.localScale = Vector3.zero;
					target.SetActive(value: false);
				}
				else if (target.CompareTag("EndLimb") || target.CompareTag("Head"))
				{
					target.transform.localScale = Vector3.zero;
					target.SetActive(value: false);
				}
			}
		}
		if (health > 0f && !limp && hurtSounds.Length != 0 && !eid.blessed && eid.hitter != "blocked")
		{
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			aud.volume = hurtSoundVol;
			aud.SetPitch(Random.Range(0.85f, 1.35f));
			aud.priority = 12;
			aud.Play(tracked: true);
		}
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (multiplier == 0f || eid.puppet)
		{
			flag2 = false;
		}
		if (!flag2 || !(eid.hitter != "enemy"))
		{
			return;
		}
		if (scalc == null)
		{
			scalc = MonoSingleton<StyleCalculator>.Instance;
		}
		if (health <= 0f)
		{
			flag = true;
			if ((bool)gc && !gc.onGround)
			{
				if (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon")
				{
					scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, eid);
				}
				else if (eid.hitter == "ground slam")
				{
					scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, eid);
				}
				else if (eid.hitter != "deathzone")
				{
					scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, eid);
				}
			}
		}
		if (eid.hitter != "secret" && (bool)scalc)
		{
			scalc.HitCalculator(eid.hitter, "zombie", hitLimb, flag, eid, sourceWeapon);
		}
		if (flag && eid.hitter != "fire")
		{
			Flammable componentInChildren = GetComponentInChildren<Flammable>();
			if ((bool)componentInChildren && componentInChildren.burning && (bool)scalc)
			{
				scalc.shud.AddPoints(50, "ultrakill.finishedoff", sourceWeapon, eid);
			}
		}
	}

	public override void ChestExplodeEnd()
	{
		((Behaviour)(object)anim).enabled = false;
		anim.StopPlayback();
		Object.Destroy((Object)(object)anim);
		rbs = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = rbs;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody != null)
			{
				rigidbody.isKinematic = false;
				rigidbody.SetGravityMode(useGravity: true);
				if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
				{
					component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
				}
			}
		}
		chestExploding = false;
	}

	public override void ChestExplosion(bool cut = false, bool fromExplosion = false)
	{
		if (chestExploded)
		{
			return;
		}
		GetGoreZone();
		if (!cut)
		{
			CharacterJoint[] componentsInChildren = chest.GetComponentsInChildren<CharacterJoint>();
			if (componentsInChildren.Length != 0)
			{
				CharacterJoint[] array = componentsInChildren;
				foreach (CharacterJoint characterJoint in array)
				{
					if (characterJoint.transform.parent.parent == chest.transform)
					{
						Rigidbody[] componentsInChildren2 = characterJoint.transform.GetComponentsInChildren<Rigidbody>();
						foreach (Rigidbody obj in componentsInChildren2)
						{
							obj.isKinematic = false;
							obj.SetGravityMode(useGravity: true);
						}
						if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
						{
							component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
						}
						Object.Destroy(characterJoint);
					}
					else if (characterJoint.transform == chest.transform)
					{
						if (characterJoint.TryGetComponent<Collider>(out var component2))
						{
							Object.Destroy(component2);
						}
						Object.Destroy(characterJoint);
					}
				}
			}
			if (chest.TryGetComponent<Rigidbody>(out var component3))
			{
				Object.Destroy(component3);
			}
			if (!limp && !eid.exploded && !eid.dead)
			{
				if (gc.onGround)
				{
					rb.isKinematic = true;
					knockedBack = false;
				}
				anim.Rebind();
				anim.SetTrigger("ChestExplosion");
				chestExploding = true;
			}
		}
		GetGoreZone();
		if (MonoSingleton<BloodsplatterManager>.Instance.forceOn || MonoSingleton<BloodsplatterManager>.Instance.forceGibs || MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			GetGoreZone();
			for (int k = 0; k < 6; k++)
			{
				GameObject gib = bsm.GetGib((k < 2) ? BSType.jawChunk : BSType.gib);
				ReadyGib(gib, chest);
			}
			if (!eid.sandified)
			{
				GameObject fromQueue = bsm.GetFromQueue(BSType.chestExplosion);
				gz.SetGoreZone(fromQueue);
				fromQueue.transform.SetPositionAndRotation(chest.transform.parent.position, chest.transform.parent.rotation);
				fromQueue.transform.SetParent(chest.transform.parent, worldPositionStays: true);
			}
		}
		EnemyIdentifierIdentifier[] componentsInChildren3 = chest.GetComponentsInChildren<EnemyIdentifierIdentifier>();
		for (int l = 0; l < componentsInChildren3.Length; l++)
		{
			if (!componentsInChildren3[l])
			{
				continue;
			}
			GoreType got;
			switch (componentsInChildren3[l].gameObject.tag)
			{
			case "Head":
				got = GoreType.Head;
				break;
			case "EndLimb":
			case "Limb":
				got = GoreType.Limb;
				break;
			default:
				got = GoreType.Body;
				break;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(got, eid, fromExplosion);
			if ((bool)gore)
			{
				gore.transform.position = chest.transform.position;
				Bloodsplatter component4 = gore.GetComponent<Bloodsplatter>();
				if ((bool)component4)
				{
					component4.hpAmount = 10;
				}
				if (gz != null && gz.goreZone != null)
				{
					gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				if (!noheal && (bool)component4)
				{
					component4.GetReady();
				}
			}
		}
		if (!cut)
		{
			chest.transform.localScale = Vector3.zero;
		}
		else
		{
			if (!limp)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.halfoff", null, eid);
			}
			Cut(chest);
		}
		chestExploded = true;
	}

	public override void Cut(GameObject target)
	{
		if (target.TryGetComponent<CharacterJoint>(out var component))
		{
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && component.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
			{
				component2.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
			}
			Object.Destroy(component);
			target.transform.SetParent(gz.transform, worldPositionStays: true);
			Rigidbody[] componentsInChildren = target.transform.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody obj in componentsInChildren)
			{
				obj.isKinematic = false;
				obj.SetGravityMode(useGravity: true);
				obj.angularDrag = 0.001f;
				obj.maxAngularVelocity = float.PositiveInfinity;
				obj.velocity = Vector3.zero;
				obj.AddForce(Vector3.up * (target.CompareTag("Head") ? 250 : 25), ForceMode.VelocityChange);
				obj.AddTorque(target.transform.right * 1f, ForceMode.VelocityChange);
			}
		}
	}

	public override void ParryableCheck(bool partial = false)
	{
		attacking = true;
		if (parryFramesLeft > 0)
		{
			eid.hitter = "punch";
			eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
			parryFramesLeft = 0;
		}
	}
}
