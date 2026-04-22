using UnityEngine;

namespace Sandbox.Arm;

public class DestroyMode : ISandboxArmMode
{
	private SandboxArm hostArm;

	private static readonly int Tap = Animator.StringToHash("Tap");

	private static readonly int Point = Animator.StringToHash("Point");

	public string Name => "Destroy";

	public bool CanOpenMenu => true;

	public bool Raycast => true;

	public virtual string Icon => "destroy";

	public void OnEnable(SandboxArm arm)
	{
		hostArm = arm;
		arm.ResetAnimator();
		if (((Behaviour)(object)arm.animator).isActiveAndEnabled)
		{
			arm.animator.SetBool(Point, true);
		}
	}

	public void OnDisable()
	{
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}

	public void OnPrimaryDown()
	{
		if (hostArm.hit.collider == null)
		{
			return;
		}
		EnemyIdentifierIdentifier component = hostArm.hit.collider.GetComponent<EnemyIdentifierIdentifier>();
		GameObject obj;
		if ((bool)component && (bool)component.eid)
		{
			EnemySpawnableInstance componentInParent = component.eid.GetComponentInParent<EnemySpawnableInstance>();
			obj = (componentInParent ? componentInParent.gameObject : component.eid.gameObject);
		}
		else
		{
			SpawnableInstance prop = SandboxUtils.GetProp(hostArm.hit.collider.gameObject);
			if (prop == null)
			{
				DietProp dietProp = hostArm.hit.collider.GetComponent<DietProp>();
				if (!dietProp)
				{
					return;
				}
				if (dietProp.parent != null)
				{
					dietProp = dietProp.parent;
				}
				obj = dietProp.gameObject;
			}
			else
			{
				obj = prop.gameObject;
			}
		}
		if (hostArm.hit.collider.TryGetComponent<SpawnableInstance>(out var component2) && hostArm.hit.collider.TryGetComponent<Rigidbody>(out var component3) && component3.isKinematic && (bool)MonoSingleton<SandboxNavmesh>.Instance && (!component || !component.eid))
		{
			MonoSingleton<SandboxNavmesh>.Instance.MarkAsDirty(component2);
		}
		Object.Instantiate(hostArm.genericBreakParticles).transform.position = hostArm.hit.collider.bounds.center;
		Object.Destroy(obj);
		hostArm.destroySound.Play(tracked: true);
		hostArm.animator.SetTrigger(Tap);
	}

	public void OnPrimaryUp()
	{
	}

	public void OnSecondaryDown()
	{
	}

	public void OnSecondaryUp()
	{
	}
}
