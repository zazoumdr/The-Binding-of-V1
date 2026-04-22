using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaderboardManagementScreen : MonoBehaviour
{
	public struct CachedLevelScore
	{
		public int? anyPercentScore;

		public bool anyPercentInvalid;

		public int? pRankScore;

		public bool pRankInvalid;
	}

	[SerializeField]
	private LeaderboardManagementEntry template;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject loadingPanel;

	[SerializeField]
	private Button refreshAllButton;

	[SerializeField]
	private GameObject autoFixOverlay;

	[SerializeField]
	private TMP_Text autoFixDetailsText;

	[SerializeField]
	private BasicConfirmationDialog autoFixDoneDialog;

	[SerializeField]
	private TMP_Text autoFixSummary;

	private readonly Dictionary<string, LeaderboardManagementEntry> entries = new Dictionary<string, LeaderboardManagementEntry>();

	private readonly Dictionary<string, CachedLevelScore> scoreCache = new Dictionary<string, CachedLevelScore>();

	private readonly List<int> fetchQueue = new List<int>();

	private Coroutine fetchCoroutine;

	private bool fetchComplete;

	private bool autoFix;

	private int pendingFetchCount;

	private int autoFixPendingCount;

	private int autoFixedCount;

	private void OnEnable()
	{
		autoFix = false;
		autoFixedCount = 0;
		autoFixPendingCount = 0;
		pendingFetchCount = 0;
		autoFixOverlay.gameObject.SetActive(value: false);
		autoFixDoneDialog.Cancel();
		template.gameObject.SetActive(value: false);
		((Selectable)refreshAllButton).interactable = false;
		if (scoreCache.Count == 0)
		{
			fetchQueue.Clear();
			fetchQueue.AddRange(GetMissionName.EnumerateMissionNumbers().Where(IsMissionUnlocked));
			pendingFetchCount = fetchQueue.Count;
			fetchComplete = false;
			loadingPanel.SetActive(value: true);
			loadingPanel.transform.SetAsLastSibling();
			fetchCoroutine = StartCoroutine(FetchCoroutine());
			return;
		}
		foreach (var (text2, cached) in scoreCache)
		{
			if (entries.ContainsKey(text2))
			{
				continue;
			}
			foreach (int item in GetMissionName.EnumerateMissionNumbers())
			{
				if (!(GetMissionName.GetSceneName(item) != text2))
				{
					UpdateEntry(item, text2, cached);
					break;
				}
			}
		}
		if (fetchQueue.Count > 0)
		{
			loadingPanel.SetActive(value: true);
			loadingPanel.transform.SetAsLastSibling();
			fetchCoroutine = StartCoroutine(FetchCoroutine());
		}
		else if (fetchComplete)
		{
			SetFetchPending(pending: false);
		}
	}

	private bool IsMissionUnlocked(int missionNum)
	{
		return GameProgressSaver.GetRank(missionNum, returnNull: true) != null;
	}

	private void OnDisable()
	{
		if (fetchCoroutine != null)
		{
			StopCoroutine(fetchCoroutine);
			fetchCoroutine = null;
		}
	}

	private IEnumerator FetchCoroutine()
	{
		while (fetchQueue.Count > 0)
		{
			int missionNum = fetchQueue[0];
			fetchQueue.RemoveAt(0);
			string sceneName = GetMissionName.GetSceneName(missionNum);
			Task<int?> anyTask = MonoSingleton<LeaderboardController>.Instance.GetOwnLeaderboardScore(sceneName + " Any%");
			Task<int?> pRankTask = MonoSingleton<LeaderboardController>.Instance.GetOwnLeaderboardScore(sceneName + " PRank");
			while (!anyTask.IsCompleted || !pRankTask.IsCompleted)
			{
				yield return null;
			}
			pendingFetchCount = fetchQueue.Count;
			if (autoFix)
			{
				UpdateAutoFixDetails();
			}
			CachedLevelScore cachedLevelScore = default(CachedLevelScore);
			if (!anyTask.IsFaulted)
			{
				cachedLevelScore.anyPercentScore = anyTask.Result;
				int? anyPercentScore = cachedLevelScore.anyPercentScore;
				cachedLevelScore.anyPercentInvalid = anyPercentScore.HasValue && anyPercentScore.GetValueOrDefault() <= 0;
				anyPercentScore = cachedLevelScore.anyPercentScore;
				if (anyPercentScore.HasValue && anyPercentScore == int.MaxValue)
				{
					cachedLevelScore.anyPercentScore = null;
				}
			}
			if (!pRankTask.IsFaulted)
			{
				cachedLevelScore.pRankScore = pRankTask.Result;
				int? anyPercentScore = cachedLevelScore.pRankScore;
				cachedLevelScore.pRankInvalid = anyPercentScore.HasValue && anyPercentScore.GetValueOrDefault() <= 0;
				anyPercentScore = cachedLevelScore.pRankScore;
				if (anyPercentScore.HasValue && anyPercentScore == int.MaxValue)
				{
					cachedLevelScore.pRankScore = null;
				}
			}
			scoreCache[sceneName] = cachedLevelScore;
			UpdateEntry(missionNum, sceneName, cachedLevelScore);
			loadingPanel.transform.SetAsLastSibling();
			if (autoFix)
			{
				if (cachedLevelScore.anyPercentInvalid)
				{
					RequestReset(sceneName + " Any%");
				}
				if (cachedLevelScore.pRankInvalid)
				{
					RequestReset(sceneName + " PRank");
				}
			}
		}
		SetFetchPending(pending: false);
		if (autoFix)
		{
			autoFixOverlay.SetActive(value: false);
			autoFixSummary.text = $"Cleaned up <color=orange>{autoFixedCount}</color> invalid scores.";
			autoFixDoneDialog.ShowDialog();
		}
	}

	private void UpdateEntry(int missionNum, string sceneName, CachedLevelScore cached)
	{
		if (!entries.TryGetValue(sceneName, out var value))
		{
			value = UnityEngine.Object.Instantiate(template, container);
			value.gameObject.SetActive(value: true);
			value.missionNumber = missionNum;
			entries[sceneName] = value;
			((UnityEvent)(object)value.anyPercentResetButton.onClick).AddListener((UnityAction)delegate
			{
				RequestReset(sceneName + " Any%");
			});
			((UnityEvent)(object)value.pRankResetButton.onClick).AddListener((UnityAction)delegate
			{
				RequestReset(sceneName + " PRank");
			});
		}
		value.UpdateEntry(missionNum, cached);
	}

	private void UpdateAutoFixDetails()
	{
		if (!((UnityEngine.Object)(object)autoFixDetailsText == null))
		{
			autoFixDetailsText.text = $"Remaining leaderboards: <color=orange>{pendingFetchCount}</color>\n" + $"Pending fixes: <color=orange>{autoFixPendingCount}</color>\n" + $"Fixed scores: <color=orange>{autoFixedCount}</color>";
		}
	}

	private void SetFetchPending(bool pending)
	{
		if ((bool)(UnityEngine.Object)(object)refreshAllButton)
		{
			((Selectable)refreshAllButton).interactable = !pending;
		}
		if ((bool)loadingPanel)
		{
			loadingPanel.SetActive(pending);
			if (pending)
			{
				loadingPanel.transform.SetAsLastSibling();
			}
		}
		fetchComplete = !pending;
		if (!pending)
		{
			fetchCoroutine = null;
		}
	}

	private void ClearCache()
	{
		if (fetchCoroutine != null)
		{
			StopCoroutine(fetchCoroutine);
			fetchCoroutine = null;
		}
		scoreCache.Clear();
		foreach (KeyValuePair<string, LeaderboardManagementEntry> entry in entries)
		{
			UnityEngine.Object.Destroy(entry.Value.gameObject);
		}
		entries.Clear();
	}

	public void RefreshAll()
	{
		ClearCache();
		fetchQueue.Clear();
		fetchQueue.AddRange(GetMissionName.EnumerateMissionNumbers());
		SetFetchPending(pending: true);
		fetchCoroutine = StartCoroutine(FetchCoroutine());
	}

	public void StartAutoFix()
	{
		Debug.Log("[LeaderboardManagement] Starting auto-fix...");
		LevelSelectAct[] array = UnityEngine.Object.FindObjectsByType<LevelSelectAct>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		ClearCache();
		base.gameObject.SetActive(value: true);
		autoFix = true;
		autoFixOverlay.SetActive(value: true);
		UpdateAutoFixDetails();
	}

	public async void RequestReset(string boardKey)
	{
		Debug.Log("[LeaderboardManagement] Reset requested for: " + boardKey);
		if (autoFix)
		{
			autoFixPendingCount++;
			UpdateAutoFixDetails();
		}
		try
		{
			await MonoSingleton<LeaderboardController>.Instance.ResetLeaderboardScore(boardKey);
			bool flag = boardKey.EndsWith(" PRank");
			string key = boardKey.Substring(0, boardKey.Length - (flag ? " PRank".Length : " Any%".Length));
			if (scoreCache.TryGetValue(key, out var value))
			{
				if (flag)
				{
					value.pRankScore = null;
					value.pRankInvalid = false;
				}
				else
				{
					value.anyPercentScore = null;
					value.anyPercentInvalid = false;
				}
				scoreCache[key] = value;
				if (entries.TryGetValue(key, out var value2))
				{
					value2.UpdateEntry(value2.missionNumber, value);
				}
			}
			if (autoFix)
			{
				autoFixPendingCount--;
				autoFixedCount++;
				UpdateAutoFixDetails();
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"[LeaderboardManagement] Failed to reset {boardKey}: {arg}");
			if (autoFix)
			{
				autoFixPendingCount--;
				UpdateAutoFixDetails();
			}
		}
	}
}
