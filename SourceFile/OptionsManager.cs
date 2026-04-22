using System;
using System.Linq;
using Logic;
using SettingsMenu.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OptionsManager : MonoSingleton<OptionsManager>
{
	public static bool selectedSomethingThisFrame;

	public bool mainMenu;

	[HideInInspector]
	public bool paused;

	public bool inIntro;

	public bool frozen;

	[HideInInspector]
	public GameObject pauseMenu;

	[HideInInspector]
	public SettingsMenu.Components.SettingsMenu optionsMenu;

	public GameObject progressChecker;

	private NewMovement nm;

	private GunControl gc;

	private FistControl fc;

	[HideInInspector]
	public float mouseSensitivity;

	[HideInInspector]
	public float simplifiedDistance;

	[HideInInspector]
	public bool simplifyEnemies;

	[HideInInspector]
	public bool outlinesOnly;

	private int screenWidth;

	private int screenHeight;

	[HideInInspector]
	public Toggle fullScreen;

	[HideInInspector]
	public float bloodstainChance;

	[HideInInspector]
	public float maxGore;

	[HideInInspector]
	public float maxStains;

	[HideInInspector]
	public GameObject playerPosInfo;

	[HideInInspector]
	public bool dontUnpause;

	public bool previousWeaponState;

	public static bool forceRadiance;

	public static bool forceSand;

	public static bool forcePuppet;

	public static bool forceBossBars;

	public static float radianceTier = 1f;

	private void Awake()
	{
		if (GameObject.FindWithTag("OptionsManager") == null)
		{
			UnityEngine.Object.Instantiate(progressChecker);
		}
		base.transform.SetParent(null);
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "mouseSensitivity":
			if (value is float num4)
			{
				mouseSensitivity = num4;
			}
			break;
		case "bloodStainChance":
			if (value is float num6)
			{
				bloodstainChance = num6 * 100f;
			}
			break;
		case "bloodStainMax":
			if (value is float num2)
			{
				maxStains = num2;
			}
			break;
		case "maxGore":
			if (value is float num5)
			{
				maxGore = num5;
			}
			break;
		case "simplifyEnemies":
			if (value is int num3)
			{
				SetSimplifyEnemies(num3);
			}
			break;
		case "simplifyEnemiesDistance":
			if (value is float num)
			{
				simplifiedDistance = num;
			}
			break;
		}
	}

	public void SetSimplifyEnemies(int value)
	{
		bool flag = (simplifyEnemies = value != 0);
		outlinesOnly = value == 1;
		Debug.Log($"val: {value} simplifyEnemies: {flag} outlinesOnly: {outlinesOnly}");
	}

	private void Start()
	{
		mouseSensitivity = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("mouseSensitivity");
		bloodstainChance = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("bloodStainChance") * 100f;
		maxStains = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("bloodStainMax");
		maxGore = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("maxGore");
		SetSimplifyEnemies(MonoSingleton<PrefsManager>.Instance.GetInt("simplifyEnemies"));
		simplifiedDistance = MonoSingleton<PrefsManager>.Instance.GetFloat("simplifyEnemiesDistance");
		if (!(MonoSingleton<CheatsController>.Instance == null) && !MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			if (forceRadiance)
			{
				forceRadiance = false;
			}
			if (forceSand)
			{
				forceSand = false;
			}
			if (forcePuppet)
			{
				forcePuppet = false;
			}
			if (forceBossBars)
			{
				forceBossBars = false;
			}
			if (radianceTier != 1f)
			{
				radianceTier = 1f;
			}
		}
	}

	private void Update()
	{
		if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))
		{
			if (!Screen.fullScreen)
			{
				Screen.SetResolution(Screen.resolutions.Last().width, Screen.resolutions.Last().height, fullscreen: true);
			}
			else
			{
				Screen.fullScreen = false;
			}
		}
		if (frozen)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame && !inIntro && !mainMenu)
		{
			if (!paused)
			{
				Pause();
			}
			else if (!dontUnpause)
			{
				if (SandboxHud.SavesMenuOpen)
				{
					Debug.Log("Closing sandbox saves menu first");
					MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
					return;
				}
				CloseOptions();
				UnPause();
			}
		}
		if (mainMenu && !paused)
		{
			Pause();
		}
	}

	private void LateUpdate()
	{
		if (paused)
		{
			if (mainMenu)
			{
				Time.timeScale = 1f;
			}
			if (!mainMenu)
			{
				Time.timeScale = 0f;
			}
		}
		selectedSomethingThisFrame = false;
	}

	public void Pause()
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		if (!mainMenu)
		{
			nm.enabled = false;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 0f);
			if ((bool)MonoSingleton<MusicManager>.Instance)
			{
				MonoSingleton<MusicManager>.Instance.FilterMusic();
			}
		}
		GameStateManager.Instance.RegisterState(new GameState("pause", new GameObject[2] { pauseMenu, optionsMenu.gameObject })
		{
			cursorLock = LockMode.Unlock,
			cameraInputLock = LockMode.Lock,
			playerInputLock = LockMode.Lock
		});
		MonoSingleton<CameraController>.Instance.activated = false;
		gc.activated = false;
		paused = true;
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: true);
		}
		VideoPlayer[] array = UnityEngine.Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer val in array)
		{
			if (val.isPlaying)
			{
				val.Pause();
			}
		}
	}

	public void UnPause()
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		CloseOptions();
		paused = false;
		Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale * MonoSingleton<TimeController>.Instance.timeScaleModifier;
		MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 1f);
		MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", 1f);
		if ((bool)MonoSingleton<MusicManager>.Instance)
		{
			MonoSingleton<MusicManager>.Instance.UnfilterMusic();
		}
		if (!nm.dead)
		{
			nm.enabled = true;
			MonoSingleton<CameraController>.Instance.activated = true;
			if (!fc || !fc.shopping)
			{
				if (!gc.stayUnarmed)
				{
					gc.activated = true;
				}
				if (fc != null)
				{
					fc.activated = true;
				}
			}
		}
		if ((bool)pauseMenu)
		{
			pauseMenu.SetActive(value: false);
		}
		VideoPlayer[] array = UnityEngine.Object.FindObjectsOfType<VideoPlayer>();
		foreach (VideoPlayer val in array)
		{
			if (val.isPaused)
			{
				val.Play();
			}
		}
	}

	public void Freeze()
	{
		frozen = true;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = false;
		previousWeaponState = !gc.noWeapons;
		gc.NoWeapon();
		gc.enabled = false;
	}

	public void UnFreeze()
	{
		frozen = false;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
			gc = nm.GetComponentInChildren<GunControl>();
			fc = nm.GetComponentInChildren<FistControl>();
		}
		MonoSingleton<CameraController>.Instance.activated = true;
		if (previousWeaponState)
		{
			gc.YesWeapon();
		}
		gc.enabled = true;
	}

	public void RestartCheckpoint()
	{
		UnPause();
		StatsManager statsManager = MonoSingleton<StatsManager>.Instance;
		if (!statsManager.infoSent)
		{
			statsManager.Restart();
		}
	}

	public void RestartMission()
	{
		SceneHelper.RestartSceneAsync().ContinueWith(this, delegate
		{
			if ((bool)MonoSingleton<MapVarManager>.Instance)
			{
				MonoSingleton<MapVarManager>.Instance.ResetStores();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	public void OpenOptions()
	{
		pauseMenu.SetActive(value: false);
		optionsMenu.gameObject.SetActive(value: true);
	}

	public void CloseOptions()
	{
		optionsMenu.gameObject.SetActive(value: false);
		if ((bool)MonoSingleton<CheatsManager>.Instance)
		{
			MonoSingleton<CheatsManager>.Instance.HideMenu();
		}
		pauseMenu.SetActive(value: true);
	}

	public void QuitMission()
	{
		Time.timeScale = 1f;
		SceneHelper.LoadScene("Main Menu");
	}

	public void QuitGame()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			Application.Quit();
		}
	}

	public void ChangeLevel(string levelname)
	{
		SetChangeLevelPosition(noPosition: true);
		SceneHelper.LoadScene(levelname);
	}

	public void ChangeLevelAbrupt(string scene)
	{
		SceneHelper.LoadScene(scene);
	}

	public void ChangeLevelWithPosition(string levelname)
	{
		if (Application.CanStreamedLevelBeLoaded(levelname))
		{
			SetChangeLevelPosition(noPosition: false);
			SceneManager.LoadScene(levelname);
		}
		else
		{
			SceneHelper.LoadScene("Main Menu");
		}
	}

	public void SetChangeLevelPosition(bool noPosition)
	{
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
		}
		PlayerPosInfo component = UnityEngine.Object.Instantiate(playerPosInfo).GetComponent<PlayerPosInfo>();
		component.velocity = nm.GetComponent<Rigidbody>().velocity;
		component.wooshTime = nm.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time;
		component.noPosition = noPosition;
	}
}
