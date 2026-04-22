using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class UnderwaterController : MonoSingleton<UnderwaterController>
{
	public Image overlay;

	private Color defaultColor;

	private Color offColor;

	private HashSet<Water> touchingWaters = new HashSet<Water>();

	private AudioLowPassFilter lowPass;

	public bool inWater;

	private AudioSource aud;

	public AudioClip underWater;

	public AudioClip surfacing;

	private Collider col;

	private List<Water> toRemove = new List<Water>();

	private void OnDisable()
	{
		toRemove.Clear();
		foreach (Water touchingWater in touchingWaters)
		{
			if (touchingWater == null || !touchingWater.IsCollidingWithWater(col))
			{
				toRemove.Add(touchingWater);
			}
		}
		foreach (Water item in toRemove)
		{
			touchingWaters.Remove(item);
		}
		Shader.DisableKeyword("UNDERWATER");
		if (inWater && touchingWaters.Count == 0)
		{
			RemoveFromWater();
		}
	}

	private void Start()
	{
		if ((Object)(object)overlay != null)
		{
			defaultColor = ((Graphic)overlay).color;
			defaultColor.a = 0.3f;
			((Behaviour)(object)overlay).enabled = false;
			aud = ((Component)(object)overlay).GetComponent<AudioSource>();
		}
		col = GetComponent<Collider>();
	}

	public void EnterWater(Water enteredWater)
	{
		Shader.EnableKeyword("UNDERWATER");
		aud.clip = underWater;
		aud.loop = true;
		aud.Play(tracked: true);
		touchingWaters.Add(enteredWater);
		UpdateColor(enteredWater.clr);
		MonoSingleton<AudioMixerController>.Instance.IsInWater(isInWater: true);
		inWater = true;
	}

	public void OutWater(Water enteredWater)
	{
		touchingWaters.Remove(enteredWater);
		if (touchingWaters.Count == 0)
		{
			RemoveFromWater();
		}
	}

	private void RemoveFromWater()
	{
		Shader.DisableKeyword("UNDERWATER");
		if (!((Object)(object)overlay == null))
		{
			Shader.SetGlobalColor("_UnderwaterOverlay", offColor);
			if (base.gameObject.scene.isLoaded && (bool)MonoSingleton<AudioMixerController>.Instance)
			{
				MonoSingleton<AudioMixerController>.Instance.IsInWater(isInWater: false);
			}
			aud.clip = surfacing;
			aud.loop = false;
			aud.Play(tracked: true);
			inWater = false;
		}
	}

	public void UpdateColor(Color newColor)
	{
		if (newColor != new Color(0f, 0f, 0f, 0f))
		{
			newColor.a = 0.3f;
			Shader.SetGlobalColor("_UnderwaterOverlay", newColor);
		}
		else
		{
			Shader.SetGlobalColor("_UnderwaterOverlay", defaultColor);
		}
	}
}
