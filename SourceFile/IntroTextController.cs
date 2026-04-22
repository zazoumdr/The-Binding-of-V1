using SettingsMenu.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class IntroTextController : MonoBehaviour
{
	public bool firstTime;

	public GameObject page1Screen;

	public GameObject page1SecondTimeScreen;

	public GameObject page2Screen;

	public GameObject page2NoFade;

	public GameObject[] deactivateOnIntroEnd;

	public Slider soundSlider;

	public Slider sfxSlider;

	public Slider musicSlider;

	private AudioMixer[] audmix;

	private Image img;

	private TMP_Text page1Text;

	private TMP_Text page2Text;

	private float fadeValue;

	private bool inMenu;

	public bool introOver;

	private bool skipped;

	private float introOverWait = 1f;

	private Rigidbody rb;

	private void Awake()
	{
		Time.timeScale = 1f;
		audmix = (AudioMixer[])(object)new AudioMixer[2]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound
		};
		firstTime = !GameProgressSaver.GetIntro();
		if (firstTime && !GameStateManager.Instance.introCheckComplete)
		{
			soundSlider.value = 100f;
			sfxSlider.value = 0f;
			musicSlider.value = 0f;
			MonoSingleton<PrefsManager>.Instance.SetFloat("allVolume", 1f);
			MonoSingleton<PrefsManager>.Instance.SetFloat("musicVolume", 0f);
			MonoSingleton<PrefsManager>.Instance.SetFloat("sfxVolume", 0f);
			page1Screen.SetActive(value: true);
		}
		else
		{
			soundSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume") * 100f;
			sfxSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("sfxVolume") * 100f;
			musicSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("musicVolume") * 100f;
			page1SecondTimeScreen.SetActive(value: true);
		}
	}

	public void DoneWithSetting()
	{
		if (page1Screen.activeSelf)
		{
			page1Screen.GetComponent<IntroText>().DoneWithSetting();
		}
		if (page1SecondTimeScreen.activeSelf)
		{
			page1SecondTimeScreen.GetComponent<IntroText>().DoneWithSetting();
		}
	}

	public void ApplyPreset(SettingsPreset preset)
	{
		preset.Apply();
	}

	private void Start()
	{
		float num = 0f;
		audmix[0].GetFloat("allVolume", ref num);
		Debug.Log("Mixer Volume " + num);
		AudioMixer[] array = audmix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat("allVolume", -80f);
		}
		audmix[0].GetFloat("allVolume", ref num);
		Debug.Log("Mixer Volume " + num);
		Invoke("SlowDown", 0.1f);
		MonoSingleton<OptionsManager>.Instance.inIntro = true;
		rb = MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>();
		rb.velocity = Vector3.zero;
		rb.SetGravityMode(useGravity: false);
	}

	private void SlowDown()
	{
		inMenu = true;
	}

	private void Update()
	{
		if (inMenu)
		{
			rb.velocity = Vector3.zero;
			rb.SetGravityMode(useGravity: false);
			if (page2Screen.activeSelf)
			{
				MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>().SetGravityMode(useGravity: true);
				inMenu = false;
			}
			if (!firstTime && MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
			{
				inMenu = false;
				introOver = true;
				skipped = true;
				MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.down * 100f;
			}
		}
		else
		{
			if (!introOver)
			{
				return;
			}
			if (!(Object)(object)img)
			{
				img = GetComponent<Image>();
				if (page1Screen.activeSelf)
				{
					page1Text = page1Screen.GetComponent<TMP_Text>();
				}
				else if (page1SecondTimeScreen.activeSelf)
				{
					page1Text = page1SecondTimeScreen.GetComponent<TMP_Text>();
				}
				page2Text = page2Screen.GetComponent<TMP_Text>();
				fadeValue = 1f;
				page2NoFade.SetActive(!skipped);
				MonoSingleton<AudioMixerController>.Instance.forceOff = false;
			}
			if (fadeValue > 0f)
			{
				fadeValue = Mathf.MoveTowards(fadeValue, 0f, Time.deltaTime * (skipped ? 1f : 0.375f));
				Color color = ((Graphic)img).color;
				color.a = fadeValue;
				((Graphic)img).color = color;
				AudioMixer[] array = audmix;
				foreach (AudioMixer val in array)
				{
					float num = 0f;
					val.GetFloat("allVolume", ref num);
					if ((Object)(object)val == (Object)(object)MonoSingleton<AudioMixerController>.Instance.musicSound && MonoSingleton<AudioMixerController>.Instance.musicVolume > 0f)
					{
						val.SetFloat("allVolume", Mathf.MoveTowards(num, Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.musicVolume) * 20f, Time.deltaTime * Mathf.Abs(num)));
					}
					else if ((Object)(object)val == (Object)(object)MonoSingleton<AudioMixerController>.Instance.allSound)
					{
						val.SetFloat("allVolume", Mathf.MoveTowards(num, Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.sfxVolume) * 20f, Time.deltaTime * Mathf.Abs(num)));
					}
				}
				if ((bool)(Object)(object)page1Text)
				{
					color = ((Graphic)page1Text).color;
					color.a = fadeValue;
					((Graphic)page1Text).color = color;
				}
				color = ((Graphic)page2Text).color;
				color.a = fadeValue;
				((Graphic)page2Text).color = color;
			}
			else if (introOverWait > 0f)
			{
				if (introOverWait == 1f)
				{
					AudioMixer[] array = audmix;
					foreach (AudioMixer val2 in array)
					{
						if ((Object)(object)val2 == (Object)(object)MonoSingleton<AudioMixerController>.Instance.musicSound && MonoSingleton<AudioMixerController>.Instance.musicVolume > 0f)
						{
							val2.SetFloat("allVolume", Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.musicVolume) * 20f);
						}
						else if ((Object)(object)val2 == (Object)(object)MonoSingleton<AudioMixerController>.Instance.allSound && MonoSingleton<AudioMixerController>.Instance.sfxVolume > 0f)
						{
							val2.SetFloat("allVolume", Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.sfxVolume) * 20f);
						}
					}
				}
				introOverWait = Mathf.MoveTowards(introOverWait, 0f, Time.deltaTime);
			}
			else
			{
				MonoSingleton<OptionsManager>.Instance.inIntro = false;
				GameObject[] array2 = deactivateOnIntroEnd;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].SetActive(value: false);
				}
			}
		}
	}
}
