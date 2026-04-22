using UnityEngine;

public class PrefConditional : MonoBehaviour
{
	public string prefName;

	public bool isLocal;

	public bool isInt;

	public UltrakillEvent valueEvent;

	private void Start()
	{
		CheckValue();
	}

	public void CheckValue()
	{
		if (isInt)
		{
			if ((isLocal ? MonoSingleton<PrefsManager>.Instance.GetIntLocal(prefName) : MonoSingleton<PrefsManager>.Instance.GetInt(prefName)) > 0)
			{
				valueEvent.Invoke();
			}
			else
			{
				valueEvent.Revert();
			}
		}
		else if (isLocal ? MonoSingleton<PrefsManager>.Instance.GetBoolLocal(prefName) : MonoSingleton<PrefsManager>.Instance.GetBool(prefName))
		{
			valueEvent.Invoke();
		}
		else
		{
			valueEvent.Revert();
		}
	}
}
