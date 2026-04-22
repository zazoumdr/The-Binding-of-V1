using System;
using System.Collections.Generic;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;

public class SubtitleController : MonoSingleton<SubtitleController>
{
	public class SubtitleData
	{
		public string caption;

		public float time;

		public GameObject origin;
	}

	[SerializeField]
	private Transform container;

	[SerializeField]
	private Subtitle subtitleLine;

	private Subtitle previousSubtitle;

	private List<SubtitleData> delayedSubs = new List<SubtitleData>();

	private bool subtitlesEnabled;

	public bool SubtitlesEnabled
	{
		get
		{
			if (subtitlesEnabled)
			{
				return !HideUI.Active;
			}
			return false;
		}
		set
		{
			subtitlesEnabled = value;
		}
	}

	private void Start()
	{
		SubtitlesEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("subtitlesEnabled");
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
		if (key == "subtitlesEnabled" && value is bool flag)
		{
			SubtitlesEnabled = flag;
		}
	}

	private void Update()
	{
		if (delayedSubs.Count <= 0)
		{
			return;
		}
		for (int num = delayedSubs.Count - 1; num >= 0; num--)
		{
			if (delayedSubs[num] == null || delayedSubs[num].origin == null || !delayedSubs[num].origin.activeInHierarchy)
			{
				delayedSubs.RemoveAt(num);
			}
			else
			{
				delayedSubs[num].time = Mathf.MoveTowards(delayedSubs[num].time, 0f, Time.deltaTime);
				if (delayedSubs[num].time <= 0f)
				{
					DisplaySubtitle(delayedSubs[num].caption);
					delayedSubs.RemoveAt(num);
				}
			}
		}
	}

	public void NotifyHoldEnd(Subtitle self)
	{
		if (previousSubtitle == self)
		{
			previousSubtitle = null;
		}
	}

	public void DisplaySubtitleTranslated(string translationKey)
	{
		_ = SubtitlesEnabled;
	}

	public void DisplaySubtitle(string caption, AudioSource audioSource = null, bool ignoreSetting = false)
	{
		if (ignoreSetting ? (!HideUI.Active) : SubtitlesEnabled)
		{
			Subtitle subtitle = UnityEngine.Object.Instantiate(subtitleLine, container, worldPositionStays: true);
			subtitle.GetComponentInChildren<TMP_Text>().text = caption;
			if ((UnityEngine.Object)(object)audioSource != null)
			{
				subtitle.distanceCheckObject = audioSource;
			}
			subtitle.gameObject.SetActive(value: true);
			if (!previousSubtitle)
			{
				subtitle.ContinueChain();
			}
			else
			{
				previousSubtitle.nextInChain = subtitle;
			}
			previousSubtitle = subtitle;
		}
	}

	public void DisplaySubtitle(string caption, float time, GameObject origin)
	{
		SubtitleData subtitleData = new SubtitleData();
		subtitleData.caption = caption;
		subtitleData.time = time;
		subtitleData.origin = origin;
		delayedSubs.Add(subtitleData);
	}

	public void CancelSubtitle(GameObject origin)
	{
		if (delayedSubs.Count <= 0)
		{
			return;
		}
		for (int num = delayedSubs.Count - 1; num >= 0; num--)
		{
			if (delayedSubs[num].origin == origin)
			{
				delayedSubs.RemoveAt(num);
			}
		}
	}
}
