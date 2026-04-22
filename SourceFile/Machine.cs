using ULTRAKILL.Cheats;
using UnityEngine;

public class Machine : Enemy
{
	public Gutterman gm;

	protected override void Update()
	{
		if (knockBackCharge > 0f)
		{
			knockBackCharge = Mathf.MoveTowards(knockBackCharge, 0f, Time.deltaTime);
		}
		if (healing && !limp && (bool)symbiote)
		{
			health = Mathf.MoveTowards(health, symbiote.health, Time.deltaTime * 10f);
			eid.health = health;
			if (health >= symbiote.health)
			{
				healing = false;
				if ((bool)base.sm)
				{
					base.sm.downed = false;
				}
				if ((bool)base.sisy)
				{
					base.sisy.downed = false;
				}
				if ((bool)base.fm)
				{
					base.fm.downed = false;
				}
			}
		}
		if (falling && rb != null && !overrideFalling && (!(Object)(object)nma || !nma.isOnOffMeshLink))
		{
			fallTime += Time.deltaTime;
			if ((bool)base.man)
			{
				noFallDamage = base.man.inControl;
				if (fallTime > 0.2f && !base.man.inControl)
				{
					parryable = true;
				}
			}
			if (gc.onGround && falling && (Object)(object)nma != null)
			{
				if (fallSpeed <= -60f && !noFallDamage && !InvincibleEnemies.Enabled && !eid.blessed && (!gc.fallSuppressed || eid.unbounceable))
				{
					if (eid == null)
					{
						eid = GetComponent<EnemyIdentifier>();
					}
					eid.Splatter();
					return;
				}
				fallSpeed = 0f;
				nma.updatePosition = true;
				nma.updateRotation = true;
				if (!base.sm || !base.sm.moveAtTarget)
				{
					rb.isKinematic = true;
				}
				if ((Object)(object)aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if ((bool)(Object)(object)aud && (Object)(object)aud.clip == (Object)(object)scream && aud.isPlaying)
				{
					aud.Stop();
				}
				rb.SetGravityMode(useGravity: false);
				((Behaviour)(object)nma).enabled = true;
				nma.Warp(base.transform.position);
				falling = false;
				anim.SetBool("Falling", false);
				if ((bool)base.man)
				{
					if (fallTime > 0.2f)
					{
						base.man.Landing();
					}
					else
					{
						base.man.inControl = true;
					}
					base.man.ResetMovementTarget();
				}
			}
			else if (eid.underwater && (bool)(Object)(object)aud && (Object)(object)aud.clip == (Object)(object)scream && aud.isPlaying)
			{
				aud.Stop();
			}
			else if (fallTime > 0.05f && rb.velocity.y < fallSpeed)
			{
				fallSpeed = rb.velocity.y;
				reduceFallTime = 0.5f;
				if ((Object)(object)aud == null)
				{
					aud = GetComponent<AudioSource>();
				}
				if ((bool)(Object)(object)aud && !aud.isPlaying && !limp && !noFallDamage && !eid.underwater && (!Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, float.PositiveInfinity, lmaskWater, QueryTriggerInteraction.Collide) || ((hitInfo.distance > 42f || rb.velocity.y < -60f) && hitInfo.transform.gameObject.layer != 4)))
				{
					aud.clip = scream;
					aud.volume = 1f;
					aud.priority = 78;
					aud.SetPitch(Random.Range(0.8f, 1.2f));
					aud.Play(tracked: true);
				}
			}
			else if (fallTime > 0.05f && rb.velocity.y > fallSpeed)
			{
				reduceFallTime = Mathf.MoveTowards(reduceFallTime, 0f, Time.deltaTime);
				if (reduceFallTime <= 0f)
				{
					fallSpeed = rb.velocity.y;
				}
			}
			else if (rb.velocity.y > 0f)
			{
				fallSpeed = 0f;
			}
		}
		else if (fallTime > 0f)
		{
			fallTime = 0f;
		}
	}

	protected override void FixedUpdate()
	{
		if (parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
		if (!limp && gc != null && !overrideFalling)
		{
			if (knockedBack && knockBackCharge <= 0f && (rb.velocity.magnitude < 1f || base.v2 != null) && gc.onGround)
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
				if ((Object)(object)nma != null)
				{
					nma.updatePosition = false;
					nma.updateRotation = false;
					((Behaviour)(object)nma).enabled = false;
					rb.isKinematic = false;
					rb.SetGravityMode(useGravity: true);
				}
			}
			if (!grounded && gc.onGround)
			{
				grounded = true;
			}
			else if (grounded && !gc.onGround)
			{
				grounded = false;
			}
			if (!gc.onGround && !falling && (Object)(object)nma != null && (!((Behaviour)(object)nma).enabled || !nma.isOnOffMeshLink))
			{
				rb.isKinematic = false;
				rb.SetGravityMode(useGravity: true);
				((Behaviour)(object)nma).enabled = false;
				falling = true;
				anim.SetBool("Falling", true);
				if (base.sc != null)
				{
					base.sc.StopFire();
				}
				if (base.tur != null)
				{
					base.tur.CancelAim(instant: true);
				}
				if ((bool)base.man && base.man.inAction && !base.man.jumping && !base.man.inControl)
				{
					base.man.CancelActions();
				}
			}
		}
		if (hitJiggleRoot != null && hitJiggleRoot.localPosition != jiggleRootPosition)
		{
			hitJiggleRoot.localPosition = Vector3.MoveTowards(hitJiggleRoot.localPosition, jiggleRootPosition, (Vector3.Distance(hitJiggleRoot.localPosition, jiggleRootPosition) + 1f) * 100f * Time.fixedDeltaTime);
		}
	}

	public override void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, Vector3 hurtPos = default(Vector3), GameObject sourceWeapon = null, bool fromExplosion = false)
	{
		//IL_11c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11c8: Unknown result type (might be due to invalid IL or missing references)
		string hitLimb = "";
		bool dead = false;
		bool flag = false;
		float num = multiplier;
		GameObject gameObject = null;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (force != Vector3.zero && !limp && base.sm == null && (base.v2 == null || !base.v2.inIntro) && (base.tur == null || !base.tur.lodged || eid.hitter == "heavypunch" || eid.hitter == "railcannon" || eid.hitter == "cannonball" || eid.hitter == "hammer"))
		{
			if ((bool)base.tur && base.tur.lodged)
			{
				base.tur.CancelAim(instant: true);
				base.tur.Unlodge();
			}
			KnockBack(force / 100f);
			if (eid.hitter == "heavypunch" || ((bool)gc && !gc.onGround && eid.hitter == "cannonball"))
			{
				eid.useBrakes = false;
			}
			else
			{
				eid.useBrakes = true;
			}
		}
		if (base.v2 != null && base.v2.secondEncounter && eid.hitter == "heavypunch")
		{
			base.v2.InstaEnrage();
		}
		if (base.sc != null && target.gameObject == base.sc.canister && !base.sc.canisterHit && eid.hitter == "revolver")
		{
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				base.sc.canisterHit = true;
			}
			if (!eid.dead && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(200, "ultrakill.instakill", sourceWeapon, eid);
			}
			MonoSingleton<TimeController>.Instance.ParryFlash();
			Invoke("CanisterExplosion", 0.1f);
			return;
		}
		if (base.tur != null && base.tur.aiming && (eid.hitter == "revolver" || eid.hitter == "coin") && base.tur.interruptables.Contains(target.transform))
		{
			base.tur.Interrupt();
		}
		if ((bool)gm)
		{
			if (gm.hasShield && !eid.dead && (eid.hitter == "heavypunch" || eid.hitter == "hammer"))
			{
				gm.ShieldBreak();
			}
			if (gm.hasShield)
			{
				multiplier /= 1.5f;
			}
			if (gm.fallen && !gm.exploded && eid.hitter == "ground slam")
			{
				gm.Explode();
				MonoSingleton<NewMovement>.Instance.Launch(Vector3.up * 750f);
			}
		}
		if ((bool)base.mf && base.mf.dying && eid.hitter == "heavypunch")
		{
			base.mf.DeadLaunch(force);
		}
		if (eid.hitter == "punch")
		{
			bool flag2 = parryables != null && parryables.Count > 0 && parryables.Contains(target.transform);
			if (parryable || (partiallyParryable && (flag2 || (parryFramesLeft > 0 && parryFramesOnPartial))))
			{
				parryable = false;
				partiallyParryable = false;
				parryables.Clear();
				if (!InvincibleEnemies.Enabled && !eid.blessed)
				{
					health -= ((parryFramesLeft > 0) ? 4 : 5);
				}
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
				if (base.sm != null && health > 0f)
				{
					if (!base.sm.enraged)
					{
						base.sm.Knockdown(fromExplosion);
					}
					else
					{
						base.sm.Enrage();
					}
				}
				else
				{
					SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				parryFramesOnPartial = flag2;
				parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
			}
		}
		else if ((bool)base.min && base.min.ramTimer > 0f && eid.hitter == "ground slam")
		{
			base.min.GotSlammed();
		}
		if ((bool)base.sisy && num > 0f)
		{
			if (eid.burners.Count > 0)
			{
				if (eid.hitter != "fire")
				{
					if (num <= 0.5f)
					{
						gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
						base.sisy.PlayHurtSound(1);
					}
					else
					{
						gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
						base.sisy.PlayHurtSound(2);
					}
				}
				else
				{
					base.sisy.PlayHurtSound();
				}
			}
			else if (eid.hitter != "fire")
			{
				gameObject = bsm.GetGore(GoreType.Smallest, eid, fromExplosion);
			}
		}
		float num2 = 0f;
		if (target.gameObject.CompareTag("Head"))
		{
			num2 = 1f;
		}
		else if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			num2 = 0.5f;
		}
		num = multiplier + num2 * multiplier * critMultiplier;
		if (num2 == 0f && (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone"))
		{
			if (!parryable && (target.gameObject != chest || health - num > 0f))
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
				if (base.sm != null && health - num > 0f)
				{
					if (!base.sm.enraged)
					{
						base.sm.Knockdown(fromExplosion);
					}
					else
					{
						base.sm.Enrage();
					}
				}
				else
				{
					SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if ((bool)base.sisy && !limp && eid.hitter == "fire" && health > 0f && health - num < 0.01f && !eid.isGasolined)
		{
			num = health - 0.01f;
		}
		if (!eid.blessed && !InvincibleEnemies.Enabled)
		{
			health -= num;
		}
		if (!gameObject && eid.hitter != "fire" && num > 0f)
		{
			if ((num2 == 1f && (num >= 1f || health <= 0f)) || eid.hitter == "hammer")
			{
				gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
			}
			else if (((num >= 1f || health <= 0f) && eid.hitter != "explosion") || (eid.hitter == "explosion" && target.gameObject.CompareTag("EndLimb")))
			{
				gameObject = ((!target.gameObject.CompareTag("Body")) ? bsm.GetGore(GoreType.Limb, eid, fromExplosion) : bsm.GetGore(GoreType.Body, eid, fromExplosion));
			}
			else if (eid.hitter != "explosion")
			{
				gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
			}
		}
		if (!limp)
		{
			flag = true;
			string text = target.gameObject.tag.ToLower();
			if (text == "endlimb")
			{
				text = "limb";
			}
			hitLimb = text;
		}
		if (health <= 0f)
		{
			if (symbiotic)
			{
				if (base.sm != null && !base.sm.downed && symbiote.health > 0f)
				{
					base.sm.downed = true;
					base.sm.Down(fromExplosion);
					Invoke("StartHealing", 3f);
				}
				else if (base.sisy != null && !base.sisy.downed && symbiote.health > 0f)
				{
					base.sisy.downed = true;
					base.sisy.Knockdown(base.transform.position + base.transform.forward);
					Invoke("StartHealing", 3f);
				}
				else if (symbiote.health <= 0f)
				{
					symbiotic = false;
					if (!limp)
					{
						GoLimp(fromExplosion);
					}
				}
			}
			else
			{
				if (!limp)
				{
					GoLimp(fromExplosion);
				}
				if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && !target.gameObject.CompareTag("EndLimb"))
				{
					float num3 = 1f;
					if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
					{
						num3 = 0.5f;
					}
					string text2 = target.gameObject.tag;
					if (!(text2 == "Head"))
					{
						if (text2 == "Limb")
						{
							for (int i = 0; (float)i < 4f * num3; i++)
							{
								GameObject gib = bsm.GetGib(BSType.gib);
								if ((bool)gib && (bool)gz && (bool)gz.gibZone)
								{
									ReadyGib(gib, target);
								}
							}
							if (target.transform.childCount > 0 && dismemberment)
							{
								Transform child = target.transform.GetChild(0);
								CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
								if (componentsInChildren.Length != 0)
								{
									CharacterJoint[] array = componentsInChildren;
									foreach (CharacterJoint characterJoint in array)
									{
										if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
										{
											component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
										}
										Object.Destroy(characterJoint);
									}
								}
								CharacterJoint component2 = target.GetComponent<CharacterJoint>();
								if (component2 != null)
								{
									component2.connectedBody = null;
									Object.Destroy(component2);
								}
								target.transform.position = child.position;
								target.transform.SetParent(child);
								child.SetParent(gz.gibZone);
								Object.Destroy(target.GetComponent<Rigidbody>());
							}
						}
					}
					else
					{
						for (int k = 0; (float)k < 6f * num3; k++)
						{
							GameObject gib = bsm.GetGib(BSType.skullChunk);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
						for (int l = 0; (float)l < 4f * num3; l++)
						{
							GameObject gib = bsm.GetGib(BSType.brainChunk);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
						for (int m = 0; (float)m < 2f * num3; m++)
						{
							GameObject gib = bsm.GetGib(BSType.eyeball);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
							gib = bsm.GetGib(BSType.jawChunk);
							if ((bool)gib && (bool)gz && (bool)gz.gibZone)
							{
								ReadyGib(gib, target);
							}
						}
					}
				}
				if (dismemberment)
				{
					if (!target.gameObject.CompareTag("Body"))
					{
						if (target.TryGetComponent<Collider>(out var component3))
						{
							Object.Destroy(component3);
						}
						target.transform.localScale = Vector3.zero;
					}
					else if (target.gameObject == chest && base.v2 == null && base.sc == null)
					{
						chestHP -= num;
						if (chestHP <= 0f || eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
						{
							CharacterJoint[] componentsInChildren2 = target.GetComponentsInChildren<CharacterJoint>();
							if (componentsInChildren2.Length != 0)
							{
								CharacterJoint[] array = componentsInChildren2;
								foreach (CharacterJoint characterJoint2 in array)
								{
									if (characterJoint2.transform.parent.parent == chest.transform)
									{
										if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint2.TryGetComponent<EnemyIdentifierIdentifier>(out var component4))
										{
											component4.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
										}
										Object.Destroy(characterJoint2);
										characterJoint2.transform.parent = null;
									}
								}
							}
							if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
							{
								for (int n = 0; n < 2; n++)
								{
									GameObject gib2 = bsm.GetGib(BSType.gib);
									if ((bool)gib2 && (bool)gz && (bool)gz.gibZone)
									{
										ReadyGib(gib2, target);
									}
								}
							}
							GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
							gore.transform.position = target.transform.position;
							gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
							target.transform.localScale = Vector3.zero;
						}
					}
				}
			}
			if (limp)
			{
				Rigidbody componentInParent = target.GetComponentInParent<Rigidbody>();
				if (componentInParent != null)
				{
					componentInParent.AddForce(force);
				}
			}
		}
		if (gameObject != null)
		{
			if (!gz)
			{
				gz = GoreZone.ResolveGoreZone(base.transform);
			}
			if (thickLimbs && target.TryGetComponent<Collider>(out var component5))
			{
				gameObject.transform.position = component5.ClosestPoint(MonoSingleton<NewMovement>.Instance.transform.position);
			}
			else
			{
				gameObject.transform.position = target.transform.position;
			}
			if (eid.hitter == "drill")
			{
				gameObject.transform.localScale *= 2f;
			}
			if (gz != null && gz.goreZone != null)
			{
				gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
			}
			Bloodsplatter component6 = gameObject.GetComponent<Bloodsplatter>();
			if ((bool)component6)
			{
				CollisionModule collision = component6.GetComponent<ParticleSystem>().collision;
				if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
				{
					if (Random.Range(0f, 1f) > 0.5f)
					{
						((CollisionModule)(ref collision)).enabled = false;
					}
					component6.hpAmount = 3;
				}
				else if (eid.hitter == "nail")
				{
					component6.hpAmount = 1;
					AudioSource component7 = component6.GetComponent<AudioSource>();
					component7.volume *= 0.8f;
				}
				if (!noheal)
				{
					component6.GetReady();
				}
			}
		}
		if ((health > 0f || symbiotic) && hurtSounds.Length != 0 && !eid.blessed)
		{
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			if ((bool)base.tur)
			{
				aud.volume = 0.85f;
			}
			else if ((bool)base.min)
			{
				aud.volume = 1f;
			}
			else
			{
				aud.volume = 0.5f;
			}
			if (base.sm != null)
			{
				aud.SetPitch(Random.Range(0.85f, 1.35f));
			}
			else
			{
				aud.SetPitch(Random.Range(0.9f, 1.1f));
			}
			aud.priority = 12;
			aud.Play(tracked: true);
		}
		if (num == 0f || eid.puppet)
		{
			flag = false;
		}
		if (!flag || !(eid.hitter != "enemy"))
		{
			return;
		}
		if (scalc == null)
		{
			scalc = MonoSingleton<StyleCalculator>.Instance;
		}
		if (health <= 0f && !symbiotic && (base.v2 == null || !base.v2.dontDie) && (!eid.flying || (bool)base.mf))
		{
			dead = true;
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
		else if (health > 0f && (bool)gc && !gc.onGround && (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon"))
		{
			scalc.shud.AddPoints(20, "ultrakill.fireworksweak", sourceWeapon, eid);
		}
		if (eid.hitter != "secret")
		{
			if (bigKill)
			{
				scalc.HitCalculator(eid.hitter, "spider", hitLimb, dead, eid, sourceWeapon);
			}
			else
			{
				scalc.HitCalculator(eid.hitter, "machine", hitLimb, dead, eid, sourceWeapon);
			}
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
