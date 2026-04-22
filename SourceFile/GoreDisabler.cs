using UnityEngine;

public class GoreDisabler : MonoBehaviour
{
	private void Awake()
	{
		if (!MonoSingleton<BloodsplatterManager>.Instance.forceGibs && !MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
