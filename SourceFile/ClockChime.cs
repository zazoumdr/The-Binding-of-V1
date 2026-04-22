using System;
using UnityEngine;

public class ClockChime : MonoBehaviour
{
	private int lastHour;

	private void Start()
	{
		lastHour = DateTime.Now.Hour;
	}

	private void Update()
	{
		int hour = DateTime.Now.Hour;
		if (lastHour != hour && hour % 12 == 4)
		{
			GetComponent<AudioSource>().Play();
		}
		lastHour = hour;
	}
}
