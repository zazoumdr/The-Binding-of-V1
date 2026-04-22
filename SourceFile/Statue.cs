using ULTRAKILL.Cheats;
using UnityEngine;

public class Statue : Enemy
{
	protected override void FixedUpdate()
	{
		if (parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
		if (!affectedByGravity || limp)
		{
			return;
		}
		if (knockedBack && knockBackCharge <= 0f && rb.velocity.magnitude < 1f && gc.onGround)
		{
			StopKnockBack();
		}
		else if (knockedBack)
		{
			if (knockBackCharge <= 0f)
			{
				brakes = Mathf.MoveTowards(brakes, 0f, 0.0005f * brakes);
			}
			if (rb.velocity.y > 0f)
			{
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, (rb.velocity.y - juggleWeight) * brakes, rb.velocity.z * 0.95f * brakes);
			}
			else
			{
				rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, rb.velocity.y - juggleWeight, rb.velocity.z * 0.95f * brakes);
			}
			juggleWeight += 0.00025f;
			nma.updatePosition = false;
			nma.updateRotation = false;
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
		}
		else if ((bool)gc)
		{
			if (!grounded && gc.onGround)
			{
				grounded = true;
			}
			else if (grounded && !gc.onGround)
			{
				grounded = false;
			}
		}
		if (!gc)
		{
			return;
		}
		if (!gc.onGround && !falling && !nma.isOnOffMeshLink)
		{
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
			((Behaviour)(object)nma).enabled = false;
			falling = true;
			anim.SetBool("Falling", true);
		}
		else if (gc.onGround && falling)
		{
			if (fallSpeed <= -50f && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				eid.Splatter();
				return;
			}
			fallSpeed = 0f;
			nma.updatePosition = true;
			nma.updateRotation = true;
			rb.isKinematic = true;
			rb.SetGravityMode(useGravity: false);
			((Behaviour)(object)nma).enabled = true;
			nma.Warp(base.transform.position);
			falling = false;
			anim.SetBool("Falling", false);
		}
	}

	public override void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, Vector3 hurtPos, GameObject sourceWeapon = null, bool fromExplosion = false)
	{
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0adf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae4: Unknown result type (might be due to invalid IL or missing references)
		string hitLimb = "";
		bool dead = false;
		bool flag = false;
		bool flag2 = false;
		float num = 0f;
		GameObject gameObject = null;
		float num2 = health;
		if (((bool)base.mass && isMassDieing) || eid == null)
		{
			return;
		}
		if (target.gameObject.CompareTag("Head"))
		{
			num = 1f * multiplier + multiplier * critMultiplier;
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
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
				flag = true;
				hitLimb = "head";
			}
			if (health <= 0f && !limp)
			{
				GoLimp();
			}
		}
		else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			num = 1f * multiplier + 0.5f * multiplier * critMultiplier;
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
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
				else if ((num >= 1f && health > 0f) || (health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
				{
					gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
				}
			}
			if (!limp)
			{
				flag = true;
				hitLimb = "limb";
			}
			if (health <= 0f && !limp)
			{
				GoLimp();
			}
		}
		else
		{
			num = 1f * multiplier;
			if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
			{
				if (!parryable && (!partiallyParryable || parryables == null || !parryables.Contains(target.transform)) && (target.gameObject != chest || health - num > 0f))
				{
					num = 0f;
				}
				else if ((parryable && (target.gameObject == chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f)) || (partiallyParryable && parryables != null && parryables.Contains(target.transform)))
				{
					num *= 1.5f;
					parryable = false;
					partiallyParryable = false;
					parryables.Clear();
					MonoSingleton<NewMovement>.Instance.Parry(eid);
					SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
				}
			}
			if (extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
			{
				num *= extraDamageMultiplier;
				flag2 = true;
			}
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
				else if ((num >= 1f && health > 0f) || (health <= 0f && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
				{
					gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
				}
				else if (eid.hitter != "explosion")
				{
					gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
				}
			}
			if (!limp)
			{
				flag = true;
				hitLimb = "body";
			}
			if (health <= 0f)
			{
				if (!limp)
				{
					GoLimp();
				}
				if ((bool)target && target.GetComponentInParent<Rigidbody>() != null)
				{
					target.GetComponentInParent<Rigidbody>().AddForce(force);
				}
			}
		}
		if (base.mass != null)
		{
			if (base.mass.spearShot && (bool)base.mass.tempSpear && base.mass.tailHitboxes.Contains(target))
			{
				MassSpear component = base.mass.tempSpear.GetComponent<MassSpear>();
				if (component != null && component.hitPlayer)
				{
					if (num >= 1f || component.spearHealth - num <= 0f)
					{
						GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
						ReadyGib(gore, base.mass.tailEnd.GetChild(0).gameObject);
					}
					component.spearHealth -= num;
				}
			}
			else if (base.mass.spearShot && !base.mass.tempSpear)
			{
				base.mass.spearShot = false;
			}
		}
		if (gameObject != null)
		{
			if (gz == null)
			{
				gz = GoreZone.ResolveGoreZone(base.transform);
			}
			if (hurtPos != Vector3.zero)
			{
				gameObject.transform.position = hurtPos;
			}
			else
			{
				gameObject.transform.position = target.transform.position;
			}
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (bigBlood)
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component2 = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component2)
			{
				CollisionModule collision = component2.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						((CollisionModule)(ref collision)).enabled = false;
					}
					component2.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component2.hpAmount = 1;
					AudioSource component3 = component2.GetComponent<AudioSource>();
					component3.volume *= 0.8f;
				}
				if (!noheal)
				{
					component2.GetReady();
				}
			}
		}
		if ((bool)eid && eid.hitter == "punch")
		{
			bool flag3 = parryables != null && parryables.Count > 0 && parryables.Contains(target.transform);
			if (parryable || (partiallyParryable && (flag3 || (parryFramesLeft > 0 && parryFramesOnPartial))))
			{
				parryable = false;
				partiallyParryable = false;
				parryables.Clear();
				if (!InvincibleEnemies.Enabled && !eid.blessed)
				{
					num = 5f;
				}
				if (!eid.blessed && !InvincibleEnemies.Enabled)
				{
					health -= num;
				}
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: true, eid);
				SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				parryFramesOnPartial = flag3;
				parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
			}
		}
		if (flag2 && (num >= 1f || (eid.hitter == "shotgun" && Random.Range(0f, 1f) > 0.5f) || (eid.hitter == "nail" && Random.Range(0f, 1f) > 0.85f)))
		{
			gameObject = ((!(extraDamageMultiplier >= 2f)) ? bsm.GetGore(GoreType.Limb, eid, fromExplosion) : bsm.GetGore(GoreType.Head, eid, fromExplosion));
			if ((bool)gameObject)
			{
				gameObject.transform.position = target.transform.position;
				if (gz != null && gz.goreZone != null)
				{
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				Bloodsplatter component4 = gameObject.GetComponent<Bloodsplatter>();
				if ((bool)component4)
				{
					CollisionModule collision2 = component4.GetComponent<ParticleSystem>().collision;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
					{
						if (Random.Range(0f, 1f) > 0.5f)
						{
							((CollisionModule)(ref collision2)).enabled = false;
						}
						component4.hpAmount = 3;
					}
					else if (eid.hitter == "nail")
					{
						component4.hpAmount = 1;
						AudioSource component5 = component4.GetComponent<AudioSource>();
						component5.volume *= 0.8f;
					}
					if (!noheal)
					{
						component4.GetReady();
					}
				}
			}
		}
		if (health > 0f && hurtSounds.Length != 0 && !eid.blessed)
		{
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			aud.volume = 0.75f;
			aud.SetPitch(Random.Range(0.85f, 1.35f));
			aud.priority = 12;
			aud.Play(tracked: true);
		}
		if (multiplier == 0f || eid.puppet)
		{
			flag = false;
		}
		if (flag && eid.hitter != "enemy")
		{
			if (scalc == null)
			{
				scalc = MonoSingleton<StyleCalculator>.Instance;
			}
			MinosArm component6 = GetComponent<MinosArm>();
			if (health <= 0f && !component6)
			{
				dead = true;
				if ((bool)gc && !gc.onGround && !eid.flying)
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
				scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
			}
		}
		if ((!woundedMaterial && !woundedModel) || !(num2 >= originalHealth / 2f) || !(health < originalHealth / 2f))
		{
			return;
		}
		if ((bool)woundedParticle)
		{
			Object.Instantiate(woundedParticle, chest.transform.position, Quaternion.identity);
		}
		if (eid.puppet)
		{
			return;
		}
		if ((bool)woundedModel)
		{
			woundedModel.SetActive(value: true);
			smr.gameObject.SetActive(value: false);
			return;
		}
		smr.material = woundedMaterial;
		if (smr.TryGetComponent<EnemySimplifier>(out var component7))
		{
			component7.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
			component7.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
		}
	}

	public override void ParryableCheck(bool partial = false)
	{
		if (partial)
		{
			partiallyParryable = true;
		}
		else
		{
			parryable = true;
		}
		if (parryFramesLeft > 0 && (!partial || parryFramesOnPartial))
		{
			eid.hitter = "punch";
			eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
			parryFramesLeft = 0;
		}
	}
}
