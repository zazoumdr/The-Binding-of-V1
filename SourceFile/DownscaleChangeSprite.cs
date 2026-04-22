using System;
using UnityEngine;
using UnityEngine.UI;

public class DownscaleChangeSprite : MonoBehaviour
{
	public Sprite normal;

	public Sprite downscaled;

	private Image img;

	private void Start()
	{
		CheckScale();
	}

	private void OnEnable()
	{
		CheckScale();
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "pixelization")
		{
			CheckScale();
		}
	}

	public void CheckScale()
	{
		if ((UnityEngine.Object)(object)img == null)
		{
			img = GetComponent<Image>();
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("pixelization") == 1)
		{
			img.sprite = downscaled;
		}
		else
		{
			img.sprite = normal;
		}
	}
}
