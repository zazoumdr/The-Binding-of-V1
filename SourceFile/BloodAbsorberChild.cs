using System.Collections.Generic;
using UnityEngine;

public class BloodAbsorberChild : MonoBehaviour, IBloodstainReceiver
{
	[HideInInspector]
	public BloodAbsorber bloodGroup;

	private MeshRenderer mRend;

	private void Start()
	{
		bloodGroup = GetComponentInParent<BloodAbsorber>();
		mRend = GetComponent<MeshRenderer>();
	}

	private void OnEnable()
	{
		BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
		if (instance != null)
		{
			instance.bloodAbsorberChildren++;
		}
	}

	private void OnDisable()
	{
		BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
		if (instance != null)
		{
			instance.bloodAbsorberChildren--;
		}
	}

	public bool HandleBloodstainHit(in RaycastHit hit)
	{
		bloodGroup.HandleBloodstainHit(in hit);
		return true;
	}

	public void ProcessWasherSpray(ref List<ParticleCollisionEvent> pEvents, Vector3 position)
	{
		bloodGroup.ProcessWasherSpray(ref pEvents, position, mRend);
	}

	bool IBloodstainReceiver.HandleBloodstainHit(in RaycastHit hit)
	{
		return HandleBloodstainHit(in hit);
	}
}
