using UnityEngine;

public class BloodDestroyer : MonoBehaviour, IBloodstainReceiver
{
	private void OnEnable()
	{
		MonoSingleton<BloodsplatterManager>.Instance.bloodDestroyers++;
	}

	private void OnDisable()
	{
		MonoSingleton<BloodsplatterManager>.Instance.bloodDestroyers--;
	}

	public bool HandleBloodstainHit(in RaycastHit hit)
	{
		return false;
	}

	bool IBloodstainReceiver.HandleBloodstainHit(in RaycastHit hit)
	{
		return HandleBloodstainHit(in hit);
	}
}
