using UnityEngine;

public class Bloodstain : MonoBehaviour
{
	public int trackedIndex;

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			MonoSingleton<BloodsplatterManager>.Instance.props[trackedIndex] = default(BloodsplatterManager.InstanceProperties);
			base.transform.hasChanged = false;
		}
	}
}
