using UnityEngine;

public class DisableChildScrollingTexturesOnPref : MonoBehaviour
{
	public bool localPref;

	public string prefName;

	public bool disableIfTrue = true;

	private void OnEnable()
	{
		bool flag = (localPref ? MonoSingleton<PrefsManager>.Instance.GetBoolLocal(prefName) : MonoSingleton<PrefsManager>.Instance.GetBool(prefName));
		if (disableIfTrue ? flag : (!flag))
		{
			ScrollingTexture[] componentsInChildren = GetComponentsInChildren<ScrollingTexture>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}
}
