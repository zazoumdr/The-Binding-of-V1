using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroViolenceScreen : MonoBehaviour
{
	private Image img;

	private float fadeAmount;

	private bool fade;

	private float targetAlpha = 1f;

	private VideoPlayer vp;

	private bool videoOver;

	[SerializeField]
	private GameObject loadingScreen;

	[SerializeField]
	private Image red;

	private bool shouldLoadTutorial;

	private bool bundlesLoaded;

	private void Start()
	{
		img = GetComponent<Image>();
		vp = GetComponent<VideoPlayer>();
		vp.SetDirectAudioVolume((ushort)0, MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume") / 2f);
		if ((bool)loadingScreen && loadingScreen.TryGetComponent<AudioSource>(out var component))
		{
			component.volume = MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume") / 2f;
		}
		Application.targetFrameRate = Screen.currentResolution.refreshRate;
		QualitySettings.vSyncCount = (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("vSync") ? 1 : 0);
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = false;
	}

	private string GetTargetScene()
	{
		shouldLoadTutorial = !GameProgressSaver.GetIntro() || !GameProgressSaver.GetTutorial();
		if (!shouldLoadTutorial)
		{
			return "Main Menu";
		}
		return "Tutorial";
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
		{
			Skip();
		}
		if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
		{
			Skip();
		}
		if (!videoOver && vp.isPaused)
		{
			videoOver = true;
			vp.Stop();
			Invoke("FadeOut", 1f);
		}
		if (!fade)
		{
			return;
		}
		fadeAmount = Mathf.MoveTowards(fadeAmount, targetAlpha, Time.deltaTime);
		Color color = ((Graphic)img).color;
		color.a = fadeAmount;
		if (targetAlpha == 1f)
		{
			((Graphic)img).color = color;
		}
		else
		{
			((Graphic)red).color = color;
		}
		if (fadeAmount == targetAlpha)
		{
			if (fadeAmount == 1f)
			{
				fade = false;
				targetAlpha = 0f;
				Invoke("Red", 1.5f);
				Invoke("FadeOut", 3f);
			}
			else
			{
				SceneHelper.LoadScene(GetTargetScene());
				base.enabled = false;
			}
		}
	}

	private void Skip()
	{
		if (vp.isPlaying)
		{
			vp.Stop();
			Invoke("FadeOut", 1f);
			return;
		}
		if (fade)
		{
			((Behaviour)(object)img).enabled = false;
			targetAlpha = 0f;
			return;
		}
		CancelInvoke("FadeOut");
		((Behaviour)(object)img).enabled = false;
		targetAlpha = 0f;
		fade = true;
	}

	private void Red()
	{
		((Graphic)red).color = new Color(1f, 1f, 1f, 1f);
		((Component)(object)red).GetComponent<AudioSource>().Play(tracked: true);
		((Behaviour)(object)img).enabled = false;
	}

	private void FadeOut()
	{
		fade = true;
	}
}
