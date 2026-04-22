using System;
using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
	public TextMeshProUGUI textMesh;

	public Vector3 lastPos;

	public bool classicVersion;

	private TimeSince lastUpdate;

	public RectTransform rect;

	private int type;

	private void Awake()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnEnable()
	{
		type = MonoSingleton<PrefsManager>.Instance.GetInt("speedometer");
		base.gameObject.SetActive(type > 0);
	}

	private void OnDestroy()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string id, object value)
	{
		if (id == "speedometer" && value is int num)
		{
			base.gameObject.SetActive(num > 0);
			type = num;
		}
	}

	private void FixedUpdate()
	{
		float num = 0f;
		string arg = "";
		Vector3 playerVelocity = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(trueVelocity: true);
		Vector3 gravityDirection = MonoSingleton<NewMovement>.Instance.rb.GetGravityDirection();
		switch (type)
		{
		case 0:
			return;
		case 1:
			num = playerVelocity.magnitude;
			arg = "u";
			break;
		case 2:
			num = Vector3.ProjectOnPlane(playerVelocity, gravityDirection).magnitude;
			arg = "hu";
			break;
		case 3:
			num = Mathf.Abs(Vector3.Dot(playerVelocity, gravityDirection));
			arg = "vu";
			break;
		}
		if ((float)lastUpdate > 0.064f)
		{
			if (classicVersion)
			{
				((TMP_Text)textMesh).text = $"{num:0}";
			}
			else
			{
				((TMP_Text)textMesh).text = $"SPEED: {num:0.00} {arg}/s";
			}
			lastUpdate = 0f;
		}
	}
}
