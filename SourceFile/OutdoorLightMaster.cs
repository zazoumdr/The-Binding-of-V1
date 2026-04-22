using System.Collections.Generic;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OutdoorLightMaster : MonoSingleton<OutdoorLightMaster>
{
	public SkyboxAnimation skyboxAnimation = SkyboxAnimation.Rotate;

	private float skyboxRotation;

	private float skyboxWobbleSpeed = 1f;

	private float skyboxDefaultRotation;

	public bool inverse;

	private List<Light> outdoorLights = new List<Light>();

	public Light[] extraLights;

	public GameObject[] activateWhenOutside;

	[HideInInspector]
	public LayerMask normalMask;

	[HideInInspector]
	public LayerMask playerMask;

	private int requests;

	private bool firstDoorOpened;

	public bool waitForFirstDoorOpen = true;

	private Material skyboxMaterial;

	private Material tempSkybox;

	public List<AudioLowPassFilter> muffleWhenIndoors = new List<AudioLowPassFilter>();

	private List<float> muffleGoals = new List<float>();

	private bool muffleSounds;

	private float currentMuffle;

	public bool muffleWhenOutdoors;

	[HideInInspector]
	public List<Collider> outdoorsZonesCheckerable = new List<Collider>();

	private void Start()
	{
		Light[] componentsInChildren = GetComponentsInChildren<Light>(includeInactive: true);
		outdoorLights.AddRange(componentsInChildren);
		if (extraLights != null)
		{
			outdoorLights.AddRange(extraLights);
		}
		if (outdoorLights.Count != 0)
		{
			normalMask = 16777216;
			normalMask = (int)normalMask | 0x2000000;
			LayerMask layerMask = 8192;
			playerMask = (int)normalMask | (int)layerMask;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			if (inverse && (!waitForFirstDoorOpen || firstDoorOpened))
			{
				outdoorLight.cullingMask = playerMask;
			}
			else
			{
				outdoorLight.cullingMask = normalMask;
			}
			if (waitForFirstDoorOpen)
			{
				outdoorLight.enabled = false;
			}
		}
		if (skyboxAnimation == SkyboxAnimation.Wobble)
		{
			skyboxDefaultRotation = RenderSettings.skybox.GetFloat("_Rotation");
			skyboxRotation = skyboxDefaultRotation;
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(inverse && (!waitForFirstDoorOpen || firstDoorOpened));
			}
		}
		for (int j = 0; j < muffleWhenIndoors.Count; j++)
		{
			muffleGoals.Add(muffleWhenIndoors[j].cutoffFrequency);
		}
		muffleSounds = !inverse && waitForFirstDoorOpen && !firstDoorOpened;
		currentMuffle = (muffleSounds ? 1 : 0);
		UpdateMuffle();
	}

	private void Update()
	{
		if (skyboxAnimation != SkyboxAnimation.None && (bool)RenderSettings.skybox)
		{
			if (!tempSkybox)
			{
				UpdateSkyboxMaterial();
			}
			else
			{
				if (skyboxAnimation == SkyboxAnimation.Rotate)
				{
					skyboxRotation += Time.deltaTime;
					if (skyboxRotation >= 360f)
					{
						skyboxRotation -= 360f;
					}
				}
				else if (skyboxAnimation == SkyboxAnimation.Wobble)
				{
					if (skyboxRotation > skyboxDefaultRotation + 2.5f && skyboxWobbleSpeed > -1f)
					{
						skyboxWobbleSpeed = Mathf.MoveTowards(skyboxWobbleSpeed, -1f, Time.deltaTime / 2f);
					}
					else if (skyboxRotation < skyboxDefaultRotation - 2.5f && skyboxWobbleSpeed < 1f)
					{
						skyboxWobbleSpeed = Mathf.MoveTowards(skyboxWobbleSpeed, 1f, Time.deltaTime / 2f);
					}
					skyboxRotation += Time.deltaTime * 0.5f * skyboxWobbleSpeed;
				}
				RenderSettings.skybox.SetFloat("_Rotation", skyboxRotation);
			}
		}
		if ((muffleSounds && currentMuffle != 1f) || (!muffleSounds && currentMuffle != 0f))
		{
			currentMuffle = Mathf.MoveTowards(currentMuffle, muffleSounds ? 1 : 0, Time.deltaTime * 3f);
			UpdateMuffle();
		}
	}

	public void AddRequest()
	{
		requests++;
		if (requests != 1)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = (inverse ? normalMask : playerMask);
		}
		GameObject[] array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!inverse);
		}
		muffleSounds = (muffleWhenOutdoors ? (!inverse) : inverse);
	}

	public void RemoveRequest()
	{
		requests--;
		if (requests != 0)
		{
			return;
		}
		foreach (Light outdoorLight in outdoorLights)
		{
			outdoorLight.cullingMask = (inverse ? playerMask : normalMask);
		}
		GameObject[] array = activateWhenOutside;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(inverse);
		}
		muffleSounds = (muffleWhenOutdoors ? inverse : (!inverse));
	}

	public void FirstDoorOpen()
	{
		if (firstDoorOpened)
		{
			return;
		}
		firstDoorOpened = true;
		if (waitForFirstDoorOpen)
		{
			foreach (Light outdoorLight in outdoorLights)
			{
				if (inverse && requests <= 0)
				{
					outdoorLight.cullingMask = playerMask;
				}
				outdoorLight.enabled = true;
			}
		}
		if (!inverse || !waitForFirstDoorOpen || requests > 0)
		{
			return;
		}
		if (activateWhenOutside != null)
		{
			GameObject[] array = activateWhenOutside;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
		muffleSounds = (muffleWhenOutdoors ? inverse : (!inverse));
	}

	public void UpdateSkyboxMaterial()
	{
		if (!skyboxMaterial)
		{
			skyboxMaterial = RenderSettings.skybox;
		}
		tempSkybox = new Material(skyboxMaterial);
		RenderSettings.skybox = tempSkybox;
	}

	public void ForceMuffle(float target)
	{
		currentMuffle = Mathf.Clamp(target, 0f, 1f);
		UpdateMuffle();
	}

	private void UpdateMuffle()
	{
		for (int i = 0; i < muffleWhenIndoors.Count; i++)
		{
			if (!((Object)(object)muffleWhenIndoors[i] == null))
			{
				((Behaviour)(object)muffleWhenIndoors[i]).enabled = currentMuffle != 0f;
				muffleWhenIndoors[i].cutoffFrequency = Mathf.Lerp(5000f, muffleGoals[i], currentMuffle);
			}
		}
	}
}
