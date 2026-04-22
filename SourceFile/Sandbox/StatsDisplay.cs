using Steamworks;
using TMPro;
using UnityEngine;

namespace Sandbox;

public class StatsDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text textContent;

	private TimeSince timeSinceUpdate;

	private void UpdateDisplay()
	{
		if (!(SteamController.Instance == null) && SteamClient.IsValid)
		{
			SandboxStats sandboxStats = SteamController.Instance.GetSandboxStats();
			textContent.text = $"<color=#FF4343>{sandboxStats.brushesBuilt}</color> - Total boxes built\n" + $"<color=#FF4343>{sandboxStats.propsSpawned}</color> - Total props placed\n" + $"<color=#FF4343>{sandboxStats.enemiesSpawned}</color> - Total enemies spawned\n" + $"<color=#FF4343>{sandboxStats.hoursSpend:F1}h</color> - Total time in Sandbox\n";
		}
	}

	private void OnEnable()
	{
		UpdateDisplay();
		timeSinceUpdate = 0f;
	}

	private void Update()
	{
		if ((float)timeSinceUpdate > 2f)
		{
			timeSinceUpdate = 0f;
			UpdateDisplay();
		}
	}
}
