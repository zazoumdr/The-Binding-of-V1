using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class TimeTracker : MonoSingleton<TimeTracker>
{
	[HideInInspector]
	public DateTime timeNow;

	[HideInInspector]
	public float hours;

	[HideInInspector]
	public float minutes;

	[HideInInspector]
	public float seconds;

	private void Update()
	{
		timeNow = DateTime.Now;
		hours = timeNow.Hour;
		minutes = timeNow.Minute;
		seconds = timeNow.Second;
	}
}
