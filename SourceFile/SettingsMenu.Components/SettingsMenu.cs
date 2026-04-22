using SettingsMenu.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SettingsMenu.Components;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SettingsMenu : MonoSingleton<SettingsMenu>
{
	[SerializeField]
	private Transform navigationRail;

	[SerializeField]
	private Transform pageContainer;

	private GameObject[] pageGameObjects;

	private SettingsPageBuilder[] pageBuilders;

	private SettingsLogicBase[] settingsLogic;

	private bool initialized;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!initialized)
		{
			pageGameObjects = new GameObject[pageContainer.childCount];
			for (int i = 0; i < pageContainer.childCount; i++)
			{
				pageGameObjects[i] = pageContainer.GetChild(i).gameObject;
			}
			pageBuilders = pageContainer.GetComponentsInChildren<SettingsPageBuilder>(includeInactive: true);
			settingsLogic = pageContainer.GetComponentsInChildren<SettingsLogicBase>(includeInactive: true);
			SettingsLogicBase[] array = settingsLogic;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Initialize(this);
			}
			initialized = true;
		}
	}

	public void OnPrefChanged(string key, object value)
	{
		SettingsLogicBase[] array = settingsLogic;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnPrefChanged(key, value);
		}
	}

	public void SetActivePage(GameObject targetPage)
	{
		GameObject[] array = pageGameObjects;
		foreach (GameObject gameObject in array)
		{
			gameObject.gameObject.SetActive(gameObject == targetPage);
		}
		if (targetPage.TryGetComponent<SettingsPageBuilder>(out var component))
		{
			component.SetSelected();
		}
	}

	public static void SetSelected(Selectable selectable)
	{
		OptionsManager.selectedSomethingThisFrame = true;
		EventSystem.current.SetSelectedGameObject(((Component)(object)selectable).gameObject);
	}

	public bool TryGetItemBuilderInstance<T>(SettingsItem item, out T builder) where T : SettingsBuilderBase
	{
		builder = null;
		SettingsPageBuilder[] array = pageBuilders;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetItemBuilderInstance<T>(item, out builder))
			{
				return true;
			}
		}
		return false;
	}
}
