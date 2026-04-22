using System;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;

public class ThrownSword : MonoBehaviour
{
	private Rigidbody rb;

	private Collider col;

	public EnemyIdentifier thrownBy;

	public Vector3 targetPos;

	public Transform returnTransform;

	public bool active;

	public float speed;

	private bool returning;

	private bool calledReturn;

	public int type;

	public bool friendly;

	public bool deflected;

	private bool hittingPlayer;

	[HideInInspector]
	public bool thrownAtVoid;

	private TimeSince timeSince;

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	private Stack<PortalHandle> traversedPortals = new Stack<PortalHandle>();

	private int difficulty;

	private SimplePortalTraveler portalTraveler;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
	}

	private void Start()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (type == 1 && difficulty < 2)
		{
			Spin component = base.transform.parent.GetComponent<Spin>();
			if ((bool)component)
			{
				component.speed /= 2f;
			}
		}
		timeSince = 0f;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			if (!TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType(PortalTravellerType.ENEMY_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
		}
	}

	private void OnDestroy()
	{
		if (portalTraveler != null)
		{
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
		}
	}

	private void Update()
	{
		if (hittingPlayer)
		{
			base.transform.position = MonoSingleton<NewMovement>.Instance.transform.position;
		}
		else
		{
			if (!active)
			{
				return;
			}
			if (type == 0)
			{
				if (!returning && base.transform.position != targetPos)
				{
					if (difficulty == 1)
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, targetPos, Time.deltaTime * speed / 1.5f);
					}
					else if (difficulty == 0)
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, targetPos, Time.deltaTime * speed / 2f);
					}
					else
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, targetPos, Time.deltaTime * speed);
					}
					if (base.transform.position == targetPos && !calledReturn)
					{
						calledReturn = true;
						CancelInvoke("Return");
						Invoke("Return", 1f);
					}
					if (thrownAtVoid && (float)timeSince > 1f && Vector3.Distance(base.transform.position, thrownBy.transform.position) - 15f > Vector3.Distance(thrownBy.transform.position, thrownBy.target.headTransform.position))
					{
						calledReturn = true;
						Return();
					}
				}
				else if (returnTransform != null)
				{
					Vector3 vector = returnTransform.position;
					if (traversedPortals.TryPeek(out var result))
					{
						vector = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(result).GetTravelMatrix(result.side).MultiplyPoint3x4(vector);
					}
					if (difficulty == 1)
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Time.deltaTime * speed / 1.5f);
					}
					else if (difficulty == 0)
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Time.deltaTime * speed / 2f);
					}
					else
					{
						base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Time.deltaTime * speed);
					}
					if (!(base.transform.position == vector))
					{
						return;
					}
					SwordsMachine componentInParent = returnTransform.GetComponentInParent<SwordsMachine>();
					if (componentInParent != null)
					{
						if (!friendly)
						{
							componentInParent.SwordCatch();
						}
						else if (friendly)
						{
							componentInParent.Knockdown(fromExplosion: false, fromThrownSword: true, heavyKnockdown: true);
						}
					}
					UnityEngine.Object.Destroy(base.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			else
			{
				if (type != 1)
				{
					return;
				}
				if (!returning)
				{
					if (difficulty < 2)
					{
						base.transform.position += base.transform.parent.forward * (15 + difficulty * 5) * Time.deltaTime;
					}
					else if (difficulty < 4)
					{
						base.transform.position += base.transform.parent.forward * 25f * Time.deltaTime;
					}
					else
					{
						base.transform.position += base.transform.parent.forward * 35f * Time.deltaTime;
					}
					return;
				}
				if (base.transform.parent != null)
				{
					base.transform.parent = null;
				}
				if (returnTransform == null)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				base.transform.position = Vector3.MoveTowards(base.transform.position, returnTransform.position, Time.deltaTime * speed * 3f);
				if (!(base.transform.position == returnTransform.position))
				{
					return;
				}
				SwordsMachine componentInParent2 = returnTransform.GetComponentInParent<SwordsMachine>();
				if (componentInParent2 != null)
				{
					if (!friendly)
					{
						componentInParent2.SwordCatch();
					}
					else if (friendly)
					{
						componentInParent2.Knockdown(fromExplosion: false, fromThrownSword: true, heavyKnockdown: true);
					}
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void OnPortalTraversal(in PortalTravelDetails details)
	{
		Vector3 point = targetPos;
		point = details.enterToExit.inverse.MultiplyPoint3x4(point);
		targetPos = point;
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle portalHandle = details.portalSequence[i];
			if (traversedPortals.TryPeek(out var result))
			{
				if (result.Reverse() == portalHandle)
				{
					traversedPortals.Pop();
				}
			}
			else
			{
				traversedPortals.Push(portalHandle);
			}
		}
	}

	public void SetPoints(Vector3 target, Transform origin)
	{
		targetPos = target;
		returnTransform = origin;
		active = true;
		if (type == 1)
		{
			Invoke("Return", 1f);
		}
		else if (!thrownAtVoid)
		{
			Invoke("Return", 2f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Invoke("RecheckPlayerHit", 0.05f);
			hittingPlayer = true;
		}
		else
		{
			EnemyIdentifier enemyIdentifier = other.gameObject.GetComponent<EnemyIdentifier>();
			if (enemyIdentifier == null && other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
			{
				enemyIdentifier = component.eid;
			}
			if (enemyIdentifier != null && !hitEnemies.Contains(enemyIdentifier) && (thrownBy == null || (!enemyIdentifier.immuneToFriendlyFire && !EnemyIdentifier.CheckHurtException(thrownBy.enemyType, enemyIdentifier.enemyType))))
			{
				if (!enemyIdentifier.dead)
				{
					hitEnemies.Add(enemyIdentifier);
				}
				enemyIdentifier.hitter = "enemy";
				enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity, base.transform.position, 5f, tryForExplode: false, 0f, null, ignoreTotalDamageTakenMultiplier: true);
			}
		}
		if (deflected && LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) && !calledReturn)
		{
			targetPos = other.ClosestPoint(base.transform.position);
			if (targetPos.magnitude < 9999f)
			{
				base.transform.position = targetPos;
			}
			else
			{
				targetPos = base.transform.position;
			}
			rb.velocity = Vector3.zero;
			calledReturn = true;
			CancelInvoke("Return");
			Invoke("Return", 1f);
		}
	}

	private void RecheckPlayerHit()
	{
		if (hittingPlayer)
		{
			hittingPlayer = false;
			col.enabled = false;
			MonoSingleton<NewMovement>.Instance.GetHurt(30, invincible: true);
		}
	}

	private void Return()
	{
		if (!returning)
		{
			returning = true;
			if (type == 1)
			{
				col.enabled = false;
			}
			else
			{
				base.transform.LookAt(returnTransform);
			}
		}
	}

	public void GetParried()
	{
		CancelInvoke("RecheckPlayerHit");
		hittingPlayer = false;
		friendly = true;
		col.enabled = false;
		Return();
	}
}
