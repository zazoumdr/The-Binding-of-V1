using System;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
	private int crossHairType;

	private Image mainch;

	public Image[] altchs;

	public Image[] chuds;

	public Sprite[] circles;

	public Material invertMaterial;

	private void Start()
	{
		mainch = GetComponent<Image>();
		MonoSingleton<StatsManager>.Instance.crosshair = base.gameObject;
		CheckCrossHair();
	}

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
		switch (key)
		{
		case "crossHair":
		case "crossHairColor":
		case "crossHairHud":
			CheckCrossHair();
			break;
		}
	}

	public void CheckCrossHair()
	{
		if ((UnityEngine.Object)(object)mainch == null)
		{
			mainch = GetComponent<Image>();
		}
		Image[] array;
		if (HideUI.Active)
		{
			((Behaviour)(object)mainch).enabled = false;
			array = altchs;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)(object)array[i]).enabled = false;
			}
			array = chuds;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)(object)array[i]).enabled = false;
			}
			return;
		}
		crossHairType = MonoSingleton<PrefsManager>.Instance.GetInt("crossHair");
		switch (crossHairType)
		{
		case 0:
		{
			((Behaviour)(object)mainch).enabled = false;
			array = altchs;
			for (int j = 0; j < array.Length; j++)
			{
				((Behaviour)(object)array[j]).enabled = false;
			}
			break;
		}
		case 1:
		{
			((Behaviour)(object)mainch).enabled = true;
			array = altchs;
			for (int j = 0; j < array.Length; j++)
			{
				((Behaviour)(object)array[j]).enabled = false;
			}
			break;
		}
		case 2:
		{
			((Behaviour)(object)mainch).enabled = true;
			array = altchs;
			for (int j = 0; j < array.Length; j++)
			{
				((Behaviour)(object)array[j]).enabled = true;
			}
			break;
		}
		}
		Color color = Color.white;
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairColor");
		switch (num)
		{
		case 0:
		case 1:
			color = Color.white;
			break;
		case 2:
			color = Color.gray;
			break;
		case 3:
			color = Color.black;
			break;
		case 4:
			color = Color.red;
			break;
		case 5:
			color = Color.green;
			break;
		case 6:
			color = Color.blue;
			break;
		case 7:
			color = Color.cyan;
			break;
		case 8:
			color = Color.yellow;
			break;
		case 9:
			color = Color.magenta;
			break;
		}
		if (num == 0)
		{
			((Graphic)mainch).material = invertMaterial;
		}
		else
		{
			((Graphic)mainch).material = null;
		}
		((Graphic)mainch).color = color;
		array = altchs;
		foreach (Image val in array)
		{
			((Graphic)val).color = color;
			if (num == 0)
			{
				((Graphic)val).material = invertMaterial;
			}
			else
			{
				((Graphic)val).material = null;
			}
		}
		int num2 = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairHud");
		if (num2 == 0)
		{
			array = chuds;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)(object)array[i]).enabled = false;
			}
			return;
		}
		array = chuds;
		foreach (Image obj in array)
		{
			((Behaviour)(object)obj).enabled = true;
			obj.sprite = circles[num2 - 1];
		}
	}
}
