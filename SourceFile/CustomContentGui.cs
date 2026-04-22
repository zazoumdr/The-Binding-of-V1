using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CustomContentGui : MonoBehaviour
{
	[SerializeField]
	private GameObject typeSelectionMenu;

	[Space]
	[SerializeField]
	private GameObject grid;

	[SerializeField]
	private GameObject gridLoadingBlocker;

	[SerializeField]
	private CustomLevelPanel itemTemplate;

	[SerializeField]
	private CustomContentCategory categoryTemplate;

	[SerializeField]
	private CustomContentGrid gridTemplate;

	[SerializeField]
	private GameObject pagination;

	[Space]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ForceLayoutRebuilds forceLayoutRebuilds;

	[SerializeField]
	private GameObject workshopError;

	[SerializeField]
	private GameObject fetchingPanel;

	[SerializeField]
	private GameObject loadingFailed;

	[SerializeField]
	private InputField workshopSearch;

	[SerializeField]
	private Dropdown difficultyDropdown;

	[SerializeField]
	private GameObject workshopButtons;

	[SerializeField]
	private Button[] workshopTabButtons;

	[SerializeField]
	private GameObject localButtons;

	[SerializeField]
	private Dropdown localSortModeDropdown;

	[Space]
	[SerializeField]
	public CampaignViewScreen campaignView;

	private Action afterLegacyAgonyInterrupt;

	private UnscaledTimeSince timeSinceStart;

	private FileSystemWatcher watcher;

	private bool localListUpdatePending;

	private int currentPage = 1;

	private static LocalSortMode currentLocalSortMode = LocalSortMode.Name;

	private static WorkshopTab currentWorkshopTab = WorkshopTab.Trending;

	private static bool lastTabWorkshop;

	public static CustomCampaign lastCustomCampaign;

	public static bool wasAgonyOpen { get; private set; }

	public void ShowLocalMaps()
	{
		lastTabWorkshop = false;
		base.gameObject.SetActive(value: true);
		typeSelectionMenu.SetActive(value: false);
		RefreshCustomMaps();
	}

	public void ShowWorkshopMaps()
	{
		lastTabWorkshop = true;
		base.gameObject.SetActive(value: true);
		typeSelectionMenu.SetActive(value: false);
		currentPage = 1;
		RefreshWorkshopItems(currentPage);
	}

	public void ReturnToTypeSelection()
	{
		base.gameObject.SetActive(value: false);
		typeSelectionMenu.SetActive(value: true);
	}

	private void ResetItems()
	{
		for (int i = 2; i < grid.transform.childCount; i++)
		{
			if (!(pagination == grid.transform.GetChild(i).gameObject))
			{
				UnityEngine.Object.Destroy(grid.transform.GetChild(i).gameObject);
			}
		}
		itemTemplate.gameObject.SetActive(value: false);
		categoryTemplate.gameObject.SetActive(value: false);
		gridTemplate.gameObject.SetActive(value: false);
	}

	public void DismissBlockers()
	{
		loadingFailed.SetActive(value: false);
	}

	public void ShowInExplorer()
	{
		Application.OpenURL("file://" + GameProgressSaver.customMapsPath);
	}

	public void SetLocalSortMode(int option)
	{
		currentLocalSortMode = (LocalSortMode)option;
		MonoSingleton<PrefsManager>.Instance.SetInt("agonyLocalSortMode", option);
		RefreshCustomMaps();
	}

	public void SetDifficulty(int dif)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", dif);
	}

	public void SetWorkshopTab(int tab)
	{
		Button[] array = workshopTabButtons;
		for (int i = 0; i < array.Length; i++)
		{
			((Selectable)array[i]).interactable = true;
		}
		MonoSingleton<PrefsManager>.Instance.SetInt("agonyWorkshopTab", tab);
		currentWorkshopTab = (WorkshopTab)tab;
		((Selectable)workshopTabButtons[(int)currentWorkshopTab]).interactable = false;
		List<WorkshopTab> list = new List<WorkshopTab>
		{
			WorkshopTab.Favorite,
			WorkshopTab.Subscribed,
			WorkshopTab.YourUploads
		};
		((Selectable)workshopSearch).interactable = !list.Contains(currentWorkshopTab);
		if (!((Selectable)workshopSearch).interactable)
		{
			workshopSearch.text = string.Empty;
		}
		RefreshWorkshopItems();
	}

	public async void RefreshWorkshopItems(int page = 1, bool lockScroll = false)
	{
	}

	public void LoadMore()
	{
		currentPage++;
		RefreshWorkshopItems(currentPage, lockScroll: true);
	}

	public async void RefreshCustomMaps()
	{
	}

	private void Update()
	{
		if (localListUpdatePending)
		{
			localListUpdatePending = false;
			Debug.Log("Refreshing local maps");
			RefreshCustomMaps();
		}
	}
}
