using UnityEngine;

public interface IBloodstainReceiver
{
	bool HandleBloodstainHit(in RaycastHit hit);
}
