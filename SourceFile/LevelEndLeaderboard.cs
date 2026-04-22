using System;
using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class LevelEndLeaderboard : MonoBehaviour
{
	[SerializeField]
	private GameObject template;

	[SerializeField]
	private TMP_Text templateUsername;

	[SerializeField]
	private TMP_Text templateTime;

	[SerializeField]
	private TMP_Text templateDifficulty;

	[SerializeField]
	private GameObject[] templateHighlight;

	[Space]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private TMP_Text leaderboardType;

	[SerializeField]
	private TMP_Text switchTypeInput;

	[Space]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private float controllerScrollSpeed = 250f;

	[Space]
	[SerializeField]
	private GameObject loadingPanel;

	private bool displayPRank;

	private int ownEntryIndex = -1;

	private InputControlScheme keyboardControlScheme;

	private const string LeftBracket = "<color=white>[";

	private const string RightBracket = "]</color>";

	private void Start()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		keyboardControlScheme = MonoSingleton<InputManager>.Instance.InputSource.Actions.KeyboardMouseScheme;
	}

	private void OnEnable()
	{
		if (!SceneHelper.CurrentScene.StartsWith("Level ") || !LeaderboardController.ShowLevelEndLeaderboards || MonoSingleton<StatsManager>.Instance.firstPlayThrough)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Debug.Log("Fetching level leaderboards for " + SceneHelper.CurrentScene);
		displayPRank = MonoSingleton<StatsManager>.Instance.rankScore == 12;
		StartCoroutine(Fetch(SceneHelper.CurrentScene));
	}

	private IEnumerator Fetch(string levelName)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			yield break;
		}
		ResetEntries();
		loadingPanel.SetActive(value: true);
		((Component)(object)scrollRect).gameObject.SetActive(value: false);
		leaderboardType.text = (displayPRank ? "P RANK" : "ANY RANK");
		Task<LeaderboardEntry[]> entryTask = MonoSingleton<LeaderboardController>.Instance.GetLevelScores(levelName, displayPRank);
		while (!entryTask.IsCompleted)
		{
			yield return null;
		}
		if (entryTask.Result == null)
		{
			yield break;
		}
		LeaderboardEntry[] result = entryTask.Result;
		int entryCount = 0;
		LeaderboardEntry[] array = result;
		foreach (LeaderboardEntry val in array)
		{
			TMP_Text obj = templateUsername;
			Friend user = val.User;
			obj.text = ((Friend)(ref user)).Name;
			int score = val.Score;
			int num = score / 60000;
			float num2 = (float)(score - num * 60000) / 1000f;
			templateTime.text = $"{num}:{num2:00.000}";
			int? num3 = null;
			if (val.Details.Length != 0)
			{
				num3 = val.Details[0];
			}
			if (LeaderboardProperties.Difficulties.Length <= num3)
			{
				Debug.LogWarning($"Difficulty {num3} is out of range for {levelName}");
				continue;
			}
			templateDifficulty.text = ((!num3.HasValue) ? "UNKNOWN" : LeaderboardProperties.Difficulties[num3.Value].ToUpper());
			GameObject[] array2 = templateHighlight;
			if (array2 != null && array2.Length > 0)
			{
				array2 = templateHighlight;
				foreach (GameObject gameObject in array2)
				{
					if (!(gameObject == null))
					{
						user = val.User;
						gameObject.SetActive(((Friend)(ref user)).IsMe);
					}
				}
			}
			user = val.User;
			if (((Friend)(ref user)).IsMe)
			{
				ownEntryIndex = entryCount;
			}
			GameObject obj2 = UnityEngine.Object.Instantiate(template, container);
			obj2.SetActive(value: true);
			SteamController.FetchAvatar(obj2.GetComponentInChildren<RawImage>(), val.User);
			entryCount++;
		}
		loadingPanel.SetActive(value: false);
		((Component)(object)scrollRect).gameObject.SetActive(value: true);
		if ((bool)(UnityEngine.Object)(object)scrollRect)
		{
			yield return null;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container);
			LeaderboardProperties.ScrollToEntry(scrollRect, ownEntryIndex, entryCount);
		}
	}

	private void ResetEntries()
	{
		ownEntryIndex = -1;
		foreach (Transform item in container)
		{
			if (!(item == template.transform))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void Update()
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		InputBinding? val = null;
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.Action.bindings.Count > 0)
			{
				Enumerator<InputBinding> enumerator = MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.Action.bindings.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						InputBinding current = enumerator.Current;
						if (!((InputBinding)(ref current)).groups.Contains(((InputControlScheme)(ref keyboardControlScheme)).bindingGroup))
						{
							val = current;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
			}
		}
		else
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.PreviousVariation.Action.bindings.Count > 0)
			{
				Enumerator<InputBinding> enumerator = MonoSingleton<InputManager>.Instance.InputSource.PreviousVariation.Action.bindings.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						InputBinding current2 = enumerator.Current;
						if (((InputBinding)(ref current2)).groups.Contains(((InputControlScheme)(ref keyboardControlScheme)).bindingGroup))
						{
							val = current2;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
			}
			if (!val.HasValue && MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.Action.bindings.Count > 0)
			{
				Enumerator<InputBinding> enumerator = MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.Action.bindings.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						InputBinding current3 = enumerator.Current;
						if (((InputBinding)(ref current3)).groups.Contains(((InputControlScheme)(ref keyboardControlScheme)).bindingGroup))
						{
							val = current3;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
			}
		}
		if (!val.HasValue)
		{
			switchTypeInput.text = "<color=white>[<color=orange>NO BINDING</color>]</color>";
			return;
		}
		TMP_Text obj = switchTypeInput;
		object obj2;
		if (!val.HasValue)
		{
			obj2 = null;
		}
		else
		{
			InputBinding valueOrDefault = val.GetValueOrDefault();
			obj2 = ((InputBinding)(ref valueOrDefault)).ToDisplayString((DisplayStringOptions)0, (InputControl)null).ToUpper();
		}
		obj.text = "<color=white>[<color=orange>" + (string?)obj2 + "</color>]</color>";
		if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.PreviousVariation.WasPerformedThisFrame)
		{
			displayPRank = !displayPRank;
			StopAllCoroutines();
			StartCoroutine(Fetch(SceneHelper.CurrentScene));
		}
		if (!(UnityEngine.Object)(object)scrollRect || !scrollRect.content)
		{
			return;
		}
		float num = ((InputControl<float>)(object)((Vector2Control)Mouse.current.scroll).y).ReadValue();
		if (num != 0f)
		{
			ScrollRect obj3 = scrollRect;
			obj3.verticalNormalizedPosition += num / scrollRect.content.sizeDelta.y;
		}
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad && Gamepad.current != null)
		{
			Vector2 vector = ((InputControl<Vector2>)(object)Gamepad.current.rightStick).ReadValue();
			if (vector.y != 0f)
			{
				ScrollRect obj4 = scrollRect;
				obj4.verticalNormalizedPosition += 0.01f * Time.unscaledDeltaTime * controllerScrollSpeed * Mathf.Sign(vector.y);
			}
		}
	}
}
