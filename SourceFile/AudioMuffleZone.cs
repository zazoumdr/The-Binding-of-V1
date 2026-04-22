using System.Collections.Generic;
using UnityEngine;

public class AudioMuffleZone : MonoBehaviour
{
	public List<AudioLowPassFilter> muffleTargets = new List<AudioLowPassFilter>();

	private List<float> muffleGoals = new List<float>();

	private bool muffleSounds;

	private float currentMuffle;

	public float speedMultiplier = 1f;

	public bool inverse;

	private int requests;

	private void Awake()
	{
		for (int i = 0; i < muffleTargets.Count; i++)
		{
			muffleGoals.Add(muffleTargets[i].cutoffFrequency);
		}
	}

	private void Start()
	{
		currentMuffle = (muffleSounds ? 1 : 0);
		UpdateMuffle();
	}

	private void OnDisable()
	{
		ClearRequests();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			AddRequest();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			RemoveRequest();
		}
	}

	public void AddRequest()
	{
		requests++;
		if (requests == 1)
		{
			muffleSounds = !inverse;
		}
	}

	public void RemoveRequest()
	{
		if (requests >= 1)
		{
			requests--;
		}
		if (requests == 0)
		{
			muffleSounds = inverse;
		}
	}

	public void ClearRequests()
	{
		requests = 0;
		muffleSounds = inverse;
		currentMuffle = 0f;
		UpdateMuffle();
	}

	private void Update()
	{
		if ((muffleSounds && currentMuffle != 1f) || (!muffleSounds && currentMuffle != 0f))
		{
			currentMuffle = Mathf.MoveTowards(currentMuffle, muffleSounds ? 1 : 0, Time.deltaTime * 3f * speedMultiplier);
			UpdateMuffle();
		}
	}

	private void UpdateMuffle()
	{
		for (int i = 0; i < muffleTargets.Count; i++)
		{
			if (!((Object)(object)muffleTargets[i] == null))
			{
				((Behaviour)(object)muffleTargets[i]).enabled = currentMuffle != 0f;
				muffleTargets[i].cutoffFrequency = Mathf.Lerp(5000f, muffleGoals[i], currentMuffle);
			}
		}
	}
}
