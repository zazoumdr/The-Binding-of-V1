using System;
using System.Collections.Generic;
using SettingsMenu.Components;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OptionsMenuToManager : MonoSingleton<OptionsMenuToManager>
{
	public GameObject pauseMenu;

	public SettingsMenu.Components.SettingsMenu optionsMenu;

	private OptionsManager opm;

	private Camera mainCam;

	private List<string> options;

	[Space]
	public BasicConfirmationDialog quitDialog;

	public BasicConfirmationDialog resetDialog;

	private void Start()
	{
		SetPauseMenu();
	}

	private void OnEnable()
	{
		SetPauseMenu();
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (!(optionsMenu == null))
		{
			optionsMenu.OnPrefChanged(key, value);
		}
	}

	private void SetPauseMenu()
	{
		opm = MonoSingleton<OptionsManager>.Instance;
		if ((bool)opm.pauseMenu)
		{
			if (opm.pauseMenu == pauseMenu)
			{
				return;
			}
			opm.pauseMenu.SetActive(value: false);
			opm.optionsMenu.gameObject.SetActive(value: false);
		}
		opm.pauseMenu = pauseMenu;
		opm.optionsMenu = optionsMenu;
		optionsMenu.Initialize();
	}

	public void EnableGamepadLookAndMove()
	{
		EnableGamepadLook();
		EnableGamepadMove();
	}

	public void DisableGamepadLookAndMove()
	{
		DisableGamepadLook();
		DisableGamepadMove();
	}

	public void EnableGamepadMove()
	{
		if (MonoSingleton<NewMovement>.Instance.gamepadFreezeCount > 0)
		{
			MonoSingleton<NewMovement>.Instance.gamepadFreezeCount--;
		}
	}

	public void EnableGamepadLook()
	{
		if (MonoSingleton<CameraController>.Instance.gamepadFreezeCount > 0)
		{
			MonoSingleton<CameraController>.Instance.gamepadFreezeCount--;
		}
	}

	public void DisableGamepadMove()
	{
		MonoSingleton<NewMovement>.Instance.gamepadFreezeCount++;
	}

	public void DisableGamepadLook()
	{
		MonoSingleton<CameraController>.Instance.gamepadFreezeCount++;
	}

	public void SetSelected(Selectable selectable)
	{
		SettingsMenu.Components.SettingsMenu.SetSelected(selectable);
	}

	public void Pause()
	{
		opm.Pause();
	}

	public void UnPause()
	{
		opm.UnPause();
	}

	public void RestartCheckpoint()
	{
		opm.RestartCheckpoint();
	}

	public void RestartMission()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("pauseMenuConfirmationDialogs");
		string currentScene = SceneHelper.CurrentScene;
		if (num == 0 || (num == 1 && currentScene == "Endless"))
		{
			resetDialog.ShowDialog();
		}
		else
		{
			RestartMissionNoConfirm();
		}
	}

	public void RestartMissionNoConfirm()
	{
		opm.RestartMission();
	}

	public void OpenOptions()
	{
		opm.OpenOptions();
	}

	public void CloseOptions()
	{
		opm.CloseOptions();
	}

	public void QuitMission()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("pauseMenuConfirmationDialogs");
		string currentScene = SceneHelper.CurrentScene;
		if (num == 0 || (num == 1 && currentScene == "Endless"))
		{
			quitDialog.ShowDialog();
		}
		else
		{
			QuitMissionNoConfirm();
		}
	}

	public void QuitMissionNoConfirm()
	{
		opm.QuitMission();
	}

	public void QuitGame()
	{
		opm.QuitGame();
	}

	public void CheckIfTutorialBeaten()
	{
		if (!GameProgressSaver.GetTutorial())
		{
			SceneHelper.LoadScene("Tutorial");
		}
	}

	public void ChangeLevel(string levelname)
	{
		opm.ChangeLevel(levelname);
	}

	public void MasterVolume(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("allVolume", stuff / 100f);
		AudioListener.volume = stuff / 100f;
	}

	public void SFXVolume(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("sfxVolume", stuff / 100f);
		if ((bool)MonoSingleton<AudioMixerController>.Instance)
		{
			MonoSingleton<AudioMixerController>.Instance.SetSFXVolume(stuff / 100f);
		}
	}

	public void MusicVolume(float stuff)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("musicVolume", stuff / 100f);
		if ((bool)MonoSingleton<AudioMixerController>.Instance)
		{
			MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume = stuff / 100f;
			MonoSingleton<AudioMixerController>.Instance.SetMusicVolume(stuff / 100f);
		}
	}
}
