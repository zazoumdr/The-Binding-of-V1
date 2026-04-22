using System;
using UnityEngine;

public class ViewModelFlip : MonoBehaviour
{
	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "weaponHoldPosition" && value is int num)
		{
			if (num == 2)
			{
				Left();
			}
			else
			{
				Right();
			}
		}
	}

	private void Start()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 2)
		{
			Left();
		}
		else
		{
			Right();
		}
	}

	public void Left()
	{
		base.transform.localScale = new Vector3(-1f, 1f, 1f);
	}

	public void Right()
	{
		base.transform.localScale = new Vector3(1f, 1f, 1f);
	}
}
