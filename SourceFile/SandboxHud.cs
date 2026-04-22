using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxHud : MonoSingleton<SandboxHud>
{
	[Header("Nav")]
	[SerializeField]
	private CanvasGroup navmeshNoticeGroup;

	[Space]
	[SerializeField]
	private GameObject sandboxSavesWindow;

	[SerializeField]
	private SandboxSaveItem sandboxSaveTemplate;

	[SerializeField]
	private Transform savesContainer;

	[Space]
	[SerializeField]
	private InputField newSaveName;

	[SerializeField]
	private Button newSaveButton;

	public static bool SavesMenuOpen
	{
		get
		{
			if ((bool)MonoSingleton<SandboxHud>.Instance)
			{
				return MonoSingleton<SandboxHud>.Instance.sandboxSavesWindow.activeSelf;
			}
			return false;
		}
	}

	private void Start()
	{
		navmeshNoticeGroup.alpha = 0f;
		UpdateNewSaveInput();
	}

	private void ResetSavesMenu()
	{
		sandboxSaveTemplate.gameObject.SetActive(value: false);
		for (int i = 1; i < savesContainer.childCount; i++)
		{
			Object.Destroy(savesContainer.GetChild(i).gameObject);
		}
	}

	private void BuildSavesMenu()
	{
		ResetSavesMenu();
		string[] array = MonoSingleton<SandboxSaver>.Instance.ListSaves();
		foreach (string save in array)
		{
			SandboxSaveItem sandboxSaveItem = Object.Instantiate(sandboxSaveTemplate);
			sandboxSaveItem.transform.SetParent(savesContainer, worldPositionStays: false);
			sandboxSaveItem.gameObject.SetActive(value: true);
			sandboxSaveItem.saveName.text = save + "<color=#7A7A7A>.pitr</color>";
			((UnityEvent)(object)sandboxSaveItem.deleteButton.onClick).AddListener((UnityAction)delegate
			{
				MonoSingleton<SandboxSaver>.Instance.Delete(save);
				BuildSavesMenu();
			});
			((UnityEvent)(object)sandboxSaveItem.loadButton.onClick).AddListener((UnityAction)delegate
			{
				MonoSingleton<SandboxSaver>.Instance.Load(save);
				HideSavesMenu();
				MonoSingleton<CheatsManager>.Instance.HideMenu();
				if (MonoSingleton<OptionsManager>.Instance.paused)
				{
					MonoSingleton<OptionsManager>.Instance.UnPause();
				}
			});
			((UnityEvent)(object)sandboxSaveItem.saveButton.onClick).AddListener((UnityAction)delegate
			{
				MonoSingleton<SandboxSaver>.Instance.Save(save);
				BuildSavesMenu();
			});
		}
	}

	public void OpenDirectory()
	{
		Application.OpenURL("file://" + SandboxSaver.SavePath);
	}

	public void UpdateNewSaveInput()
	{
		if (string.IsNullOrEmpty(newSaveName.text))
		{
			((Selectable)newSaveButton).interactable = false;
		}
		else
		{
			((Selectable)newSaveButton).interactable = !Path.GetInvalidFileNameChars().Any(newSaveName.text.Contains);
		}
	}

	public void NewSave()
	{
		MonoSingleton<SandboxSaver>.Instance.Save(newSaveName.text);
		newSaveName.text = string.Empty;
		UpdateNewSaveInput();
		BuildSavesMenu();
	}

	public void HideSavesMenu()
	{
		sandboxSavesWindow.SetActive(value: false);
	}

	public void ShowSavesMenu()
	{
		BuildSavesMenu();
		UpdateNewSaveInput();
		sandboxSavesWindow.SetActive(value: true);
	}

	public void NavmeshDirty()
	{
		navmeshNoticeGroup.alpha = 1f;
		StopAllCoroutines();
		CancelInvoke("NavmeshStartFadeOut");
		Invoke("NavmeshStartFadeOut", 3f);
	}

	private void NavmeshStartFadeOut()
	{
		StopAllCoroutines();
		StartCoroutine(FadeOutNotice());
	}

	private IEnumerator FadeOutNotice()
	{
		navmeshNoticeGroup.alpha = 1f;
		yield return null;
		while (navmeshNoticeGroup.alpha > 0f)
		{
			CanvasGroup obj = navmeshNoticeGroup;
			obj.alpha -= Time.deltaTime;
			yield return null;
		}
		HideNavmeshNotice();
	}

	public void HideNavmeshNotice()
	{
		navmeshNoticeGroup.alpha = 0f;
	}
}
