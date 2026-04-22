using System.Collections.Generic;
using plog;
using plog.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	private static readonly Logger Log = new Logger("PauseMenu");

	[SerializeField]
	private Button checkpointButton;

	[SerializeField]
	private TMP_Text checkpointText;

	private bool nonStandardCheckpointButton;

	private void OnEnable()
	{
		MapInfoBase instance = MapInfoBase.Instance;
		if (instance == null)
		{
			((Selectable)checkpointButton).interactable = false;
			Log.Warning("MapInfoBase.Instance is null", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		else if (instance.replaceCheckpointButtonWithSkip)
		{
			if (!nonStandardCheckpointButton)
			{
				if (StockMapInfo.Instance != null && !SceneHelper.IsPlayingCustom)
				{
					checkpointText.text = "SKIP";
					((Selectable)checkpointButton).interactable = true;
					((UnityEventBase)(object)checkpointButton.onClick).RemoveAllListeners();
					((UnityEvent)(object)checkpointButton.onClick).AddListener((UnityAction)OnCheckpointButton);
				}
				else
				{
					checkpointText.text = "NOT IMPLEMENTED";
					((Selectable)checkpointButton).interactable = false;
					Log.Warning("StockMapInfo is null or SceneHelper.IsPlayingCustom is true", (IEnumerable<Tag>)null, (string)null, (object)null);
				}
				nonStandardCheckpointButton = true;
			}
		}
		else
		{
			bool interactable = MonoSingleton<StatsManager>.Instance.currentCheckPoint != null;
			((Selectable)checkpointButton).interactable = interactable;
		}
	}

	public void OnCheckpointButton()
	{
		StockMapInfo instance = StockMapInfo.Instance;
		if (!(instance == null))
		{
			string nextSceneName = instance.nextSceneName;
			if (!string.IsNullOrEmpty(nextSceneName))
			{
				MonoSingleton<OptionsMenuToManager>.Instance.ChangeLevel(nextSceneName);
			}
		}
	}
}
