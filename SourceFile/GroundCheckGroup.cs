using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundCheckGroup : MonoBehaviour
{
	[SerializeField]
	private List<GroundCheck> instances;

	public bool onGround => instances.Any((GroundCheck gc) => gc.onGround);

	public bool touchingGround => instances.Any((GroundCheck gc) => gc.touchingGround);

	public bool hasImpacted
	{
		set
		{
			foreach (GroundCheck instance in instances)
			{
				instance.hasImpacted = value;
			}
		}
	}

	public bool heavyFall
	{
		get
		{
			return instances.Any((GroundCheck gc) => gc.isActiveAndEnabled && gc.heavyFall);
		}
		set
		{
			foreach (GroundCheck instance in instances)
			{
				instance.heavyFall = value;
			}
		}
	}

	public bool canJump => instances.Any((GroundCheck gc) => gc.isActiveAndEnabled && gc.canJump);

	public float sinceLastGrounded
	{
		get
		{
			float num = float.MinValue;
			foreach (GroundCheck instance in instances)
			{
				if (instance.isActiveAndEnabled)
				{
					num = Mathf.Max(num, instance.sinceLastGrounded.time);
				}
			}
			return num;
		}
	}

	public int forcedOff
	{
		get
		{
			int num = int.MinValue;
			foreach (GroundCheck instance in instances)
			{
				if (instance.isActiveAndEnabled)
				{
					num = Mathf.Max(num, instance.forcedOff);
				}
			}
			return num;
		}
	}

	public float superJumpChance
	{
		get
		{
			float num = float.MinValue;
			foreach (GroundCheck instance in instances)
			{
				if (instance.isActiveAndEnabled)
				{
					num = Mathf.Max(num, instance.superJumpChance);
				}
			}
			return num;
		}
	}

	public float bounceChance
	{
		get
		{
			float num = float.MinValue;
			foreach (GroundCheck instance in instances)
			{
				if (instance.isActiveAndEnabled)
				{
					num = Mathf.Max(num, instance.bounceChance);
				}
			}
			return num;
		}
		set
		{
			foreach (GroundCheck instance in instances)
			{
				instance.bounceChance = value;
			}
		}
	}

	public float extraJumpChance
	{
		get
		{
			float num = float.MinValue;
			foreach (GroundCheck instance in instances)
			{
				if (instance.isActiveAndEnabled)
				{
					num = Mathf.Max(num, instance.extraJumpChance);
				}
			}
			return num;
		}
	}

	public void AddInstance(GroundCheck instance)
	{
		if (!(instance == null))
		{
			instances.Add(instance);
		}
	}

	public void RemoveInstance(GroundCheck instance)
	{
		if (!(instance == null))
		{
			instances.Remove(instance);
		}
	}

	public void SetLocalPosition(Vector3 pos)
	{
		base.transform.localPosition = pos;
	}

	public void ForceOff()
	{
		foreach (GroundCheck instance in instances)
		{
			instance.ForceOff();
		}
	}

	public void StopForceOff()
	{
		foreach (GroundCheck instance in instances)
		{
			instance.StopForceOff();
		}
	}

	public void Update()
	{
		foreach (GroundCheck instance in instances)
		{
			if (instance.isActiveAndEnabled)
			{
				instance.UpdateState();
			}
		}
		MonoSingleton<PlayerFootsteps>.Instance.onGround = onGround;
	}
}
