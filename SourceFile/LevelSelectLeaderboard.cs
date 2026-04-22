using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelSelectLeaderboard : MonoBehaviour
{
	public string layerLevelNumber;

	[SerializeField]
	private GameObject scrollRectContainer;

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
	private ScrollRect scrollRect;

	[SerializeField]
	private Transform container;

	[Space]
	[SerializeField]
	private Sprite unselectedSprite;

	[SerializeField]
	private Sprite selectedSprite;

	[SerializeField]
	private Image anyPercentButton;

	[SerializeField]
	private Image pRankButton;

	[SerializeField]
	private TMP_Text anyPercentLabel;

	[SerializeField]
	private TMP_Text pRankLabel;

	[Space]
	[SerializeField]
	private GameObject loadingPanel;

	[SerializeField]
	private GameObject noItemsPanel;

	[Space]
	[SerializeField]
	private InputActionAsset inputActionAsset;

	[SerializeField]
	private float controllerScrollSpeed = 250f;

	private bool pRankSelected;

	private int ownEntryIndex = -1;

	private LevelSelectPanel levelSelect;

	private InputAction scrollSublistAction;

	public void RefreshAnyPercent()
	{
		anyPercentButton.sprite = selectedSprite;
		pRankButton.sprite = unselectedSprite;
		((Graphic)anyPercentLabel).color = Color.black;
		((Graphic)pRankLabel).color = Color.white;
		container.gameObject.SetActive(value: false);
		scrollRectContainer.SetActive(value: false);
		loadingPanel.SetActive(value: true);
		noItemsPanel.SetActive(value: false);
		ResetEntries();
		pRankSelected = false;
		StopAllCoroutines();
		StartCoroutine(Fetch("Level " + layerLevelNumber));
	}

	public void RefreshPRank()
	{
		anyPercentButton.sprite = unselectedSprite;
		pRankButton.sprite = selectedSprite;
		((Graphic)anyPercentLabel).color = Color.white;
		((Graphic)pRankLabel).color = Color.black;
		container.gameObject.SetActive(value: false);
		scrollRectContainer.SetActive(value: false);
		loadingPanel.SetActive(value: true);
		noItemsPanel.SetActive(value: false);
		ResetEntries();
		pRankSelected = true;
		StopAllCoroutines();
		StartCoroutine(Fetch("Level " + layerLevelNumber));
	}

	private void OnEnable()
	{
		RefreshAnyPercent();
	}

	private void ResetEntries()
	{
		ownEntryIndex = -1;
		foreach (Transform item in container)
		{
			if (!(item.gameObject == template))
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private bool IsLayerSelected()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		LevelSelectPanel componentInParent = currentSelectedGameObject.GetComponentInParent<LevelSelectPanel>();
		if (componentInParent == null)
		{
			return false;
		}
		return levelSelect == componentInParent;
	}

	private IEnumerator Fetch(string levelName)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			yield break;
		}
		Task<LeaderboardEntry[]> entryTask = MonoSingleton<LeaderboardController>.Instance.GetLevelScores(levelName, pRankSelected);
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
			Friend user;
			if (val.Score <= 0)
			{
				user = val.User;
				if (!((Friend)(ref user)).IsMe)
				{
					continue;
				}
				Debug.LogWarning("Invalid time for self player on " + levelName);
				MonoSingleton<LeaderboardFixDialog>.Instance?.ShowDialog();
			}
			TMP_Text obj = templateUsername;
			user = val.User;
			obj.text = ((Friend)(ref user)).Name;
			int score = val.Score;
			int num = score / 60000;
			float num2 = (float)(score - num * 60000) / 1000f;
			templateTime.text = $"{num}:{num2:00.000}";
			int? num3 = null;
			int[] details = val.Details;
			if (details != null && details.Length > 0)
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
			GameObject obj2 = Object.Instantiate(template, container);
			obj2.SetActive(value: true);
			SteamController.FetchAvatar(obj2.GetComponentInChildren<RawImage>(), val.User);
			entryCount++;
		}
		if (result.Length == 0)
		{
			noItemsPanel.SetActive(value: true);
		}
		loadingPanel.SetActive(value: false);
		container.gameObject.SetActive(value: true);
		scrollRectContainer.SetActive(value: true);
		if ((bool)(Object)(object)scrollRect)
		{
			yield return null;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container);
			LeaderboardProperties.ScrollToEntry(scrollRect, ownEntryIndex, entryCount);
		}
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			UpdateLeaderboardScroll();
		}
	}

	public void SwitchLeaderboardType(bool pRank)
	{
		if (pRank)
		{
			RefreshPRank();
		}
		else
		{
			RefreshAnyPercent();
		}
	}

	private void UpdateLeaderboardScroll()
	{
		Vector2 vector = scrollSublistAction.ReadValue<Vector2>();
		if (!(vector == Vector2.zero) && IsLayerSelected())
		{
			if (vector.y > 0f)
			{
				ScrollRect obj = scrollRect;
				obj.verticalNormalizedPosition += 0.01f * Time.deltaTime * controllerScrollSpeed;
			}
			else if (vector.y < 0f)
			{
				ScrollRect obj2 = scrollRect;
				obj2.verticalNormalizedPosition -= 0.01f * Time.deltaTime * controllerScrollSpeed;
			}
		}
	}

	private void Start()
	{
		scrollSublistAction = inputActionAsset.FindAction("ScrollSublist", false);
		if (!scrollSublistAction.enabled)
		{
			Debug.Log("Enabling scroll sublist action");
			scrollSublistAction.Enable();
		}
	}

	private void Awake()
	{
		levelSelect = GetComponentInParent<LevelSelectPanel>();
	}
}
