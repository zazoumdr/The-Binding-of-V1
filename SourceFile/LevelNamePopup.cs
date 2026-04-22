using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class LevelNamePopup : MonoSingleton<LevelNamePopup>
{
	public TMP_Text layerText;

	private string layerString;

	public TMP_Text nameText;

	private string nameString;

	private bool activated;

	private bool fadingOut;

	private AudioSource aud;

	private float textTimer;

	private int currentLetter;

	private bool countTime;

	private Coroutine nameAppearRoutine;

	private void Start()
	{
		MapInfoBase mapInfoBase = MapInfoBase.Instance;
		if ((bool)mapInfoBase)
		{
			layerString = mapInfoBase.layerName;
			nameString = mapInfoBase.levelName;
		}
		aud = GetComponent<AudioSource>();
		layerText.text = "";
		nameText.text = "";
	}

	private void Update()
	{
		if (countTime)
		{
			textTimer += Time.deltaTime;
		}
		if (fadingOut)
		{
			Color color = ((Graphic)layerText).color;
			color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime);
			((Graphic)layerText).color = color;
			((Graphic)nameText).color = color;
			if (color.a <= 0f)
			{
				fadingOut = false;
			}
		}
	}

	public void NameAppearDelayed(float delay)
	{
		Invoke("NameAppear", delay);
	}

	public void NameAppear()
	{
		if (!activated)
		{
			activated = true;
			nameAppearRoutine = StartCoroutine(ShowLayerText());
		}
	}

	public void NameAppearForce()
	{
		if (activated)
		{
			NameReset();
		}
		NameAppear();
	}

	public void CustomNameAppear(string layerName, string levelName)
	{
		if (activated)
		{
			NameReset();
		}
		layerString = layerName;
		nameString = levelName;
		NameAppear();
	}

	public void NameReset()
	{
		if (activated)
		{
			activated = false;
			MapInfoBase mapInfoBase = MapInfoBase.Instance;
			if ((bool)mapInfoBase)
			{
				layerString = mapInfoBase.layerName;
				nameString = mapInfoBase.levelName;
			}
			if (nameAppearRoutine != null)
			{
				StopCoroutine(nameAppearRoutine);
				nameAppearRoutine = null;
			}
			fadingOut = false;
			Color color = ((Graphic)layerText).color;
			color.a = 1f;
			((Graphic)layerText).color = color;
			((Graphic)nameText).color = color;
			layerText.text = "";
			nameText.text = "";
			countTime = false;
			textTimer = 0f;
			currentLetter = 0;
			if (aud.isPlaying)
			{
				aud.Stop();
			}
		}
	}

	private IEnumerator ShowLayerText()
	{
		countTime = true;
		currentLetter = 0;
		aud.Play(tracked: true);
		while (currentLetter <= layerString.Length)
		{
			while (textTimer >= 0.01f && currentLetter <= layerString.Length)
			{
				textTimer -= 0.01f;
				layerText.text = layerString.Substring(0, currentLetter);
				currentLetter++;
			}
			yield return new WaitForSeconds(0.01f);
		}
		countTime = false;
		aud.Stop();
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(ShowNameText());
	}

	private IEnumerator ShowNameText()
	{
		countTime = true;
		currentLetter = 0;
		aud.Play(tracked: true);
		while (currentLetter <= nameString.Length)
		{
			while (textTimer >= 0.01f && currentLetter <= nameString.Length)
			{
				textTimer -= 0.01f;
				nameText.text = nameString.Substring(0, currentLetter);
				currentLetter++;
			}
			yield return new WaitForSeconds(0.01f);
		}
		countTime = false;
		aud.Stop();
		yield return new WaitForSeconds(3f);
		fadingOut = true;
		nameAppearRoutine = null;
	}
}
