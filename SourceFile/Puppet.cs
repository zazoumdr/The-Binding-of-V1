using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class Puppet : EnemyScript
{
	private NavMeshAgent nma;

	[SerializeField]
	private SwingCheck2 sc;

	private Animator anim;

	private EnemyIdentifier eid;

	private Enemy mach;

	private Rigidbody rb;

	private VisionQuery nearestQuery;

	private TargetData nearestData;

	private TargetHandle nearestHandle;

	private Vector3 targetPos;

	private bool inAction;

	private bool moving;

	private Vision vision => mach.vision;

	private void Start()
	{
		nma = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		mach = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		nearestQuery = new VisionQuery("Nearest", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid));
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.25f));
		if (eid.target != null && !inAction && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && !mach.isTraversingPortalLink && Physics.Raycast(eid.target.position, Vector3.down, out var hitInfo, 120f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			mach.SetDestination(hitInfo.point);
		}
	}

	private void Update()
	{
		if (eid.target != null)
		{
			UpdateTargetVision();
			if (!inAction && eid.target != null)
			{
				if (Vector3.Distance(base.transform.position, targetPos) < 5f)
				{
					Swing();
				}
			}
			else if (moving)
			{
				rb.MovePosition(base.transform.position + base.transform.forward * Time.deltaTime * 15f);
			}
		}
		anim.SetBool("Walking", !inAction && (nma.velocity.magnitude > 1.5f || mach.isTraversingPortalLink));
	}

	private void UpdateTargetVision()
	{
		if (vision.TrySee(nearestQuery, out var data))
		{
			nearestData = data.ToData();
			nearestHandle = nearestData.handle;
		}
		if (nearestHandle != null)
		{
			targetPos = nearestData.position;
		}
		else
		{
			targetPos = eid.target.position;
		}
	}

	private void Swing()
	{
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		anim.Play("Swing", -1, 0f);
		if (eid.target != null)
		{
			base.transform.LookAt(ToPlanePos(targetPos));
		}
	}

	private void DamageStart()
	{
		sc.DamageStart();
		moving = true;
	}

	private void DamageStop()
	{
		sc.DamageStop();
		moving = false;
	}

	private void StopAction()
	{
		inAction = false;
		if (mach.gc.onGround)
		{
			((Behaviour)(object)nma).enabled = true;
		}
	}
}
