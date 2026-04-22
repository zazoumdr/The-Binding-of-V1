using System.Collections;
using UnityEngine;

public class LevelSelectAct : MonoBehaviour
{
	private LayerSelect[] childLayers;

	private PlayerInput inputSource;

	private bool currentLeaderboardMode;

	private void Awake()
	{
		childLayers = GetComponentsInChildren<LayerSelect>(includeInactive: true);
		inputSource = MonoSingleton<InputManager>.Instance.InputSource;
	}

	private void Update()
	{
		if (LeaderboardController.ShowLevelLeaderboards && (inputSource.NextWeapon.WasPerformedThisFrame || inputSource.LastWeapon.WasPerformedThisFrame || inputSource.PreviousVariation.WasPerformedThisFrame))
		{
			string text = "";
			if (inputSource.NextWeapon.WasPerformedThisFrame)
			{
				text = inputSource.NextWeapon.LastUsedBinding;
			}
			else if (inputSource.LastWeapon.WasPerformedThisFrame)
			{
				text = inputSource.LastWeapon.LastUsedBinding;
			}
			else if (inputSource.PreviousVariation.WasPerformedThisFrame)
			{
				text = inputSource.PreviousVariation.LastUsedBinding;
			}
			if (text != null && !string.IsNullOrEmpty(text) && !text.Contains("/dpad/"))
			{
				ChangeLeaderboardType(!currentLeaderboardMode);
			}
		}
	}

	public void ChangeLeaderboardType(bool pRank)
	{
		currentLeaderboardMode = pRank;
		StopAllCoroutines();
		LayerSelect[] array = childLayers;
		foreach (LayerSelect layer in array)
		{
			StartCoroutine(SwitchLeaderboardsSequence(layer, pRank));
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator SwitchLeaderboardsSequence(LayerSelect layer, bool pRank)
	{
		LevelSelectLeaderboard[] childLeaderboards = layer.childLeaderboards;
		for (int i = 0; i < childLeaderboards.Length; i++)
		{
			childLeaderboards[i].SwitchLeaderboardType(pRank);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
