using UnityEngine;

public class Clock : MonoBehaviour
{
	public Transform hour;

	public Transform minute;

	private TimeTracker tracker;

	private void Start()
	{
		tracker = MonoSingleton<TimeTracker>.Instance;
	}

	private void Update()
	{
		hour.localRotation = Quaternion.Euler(0f, (tracker.hours % 12f / 12f + tracker.minutes / 1440f) * 360f, 0f);
		minute.localRotation = Quaternion.Euler(0f, (tracker.minutes / 60f + tracker.seconds / 3600f) * 360f, 0f);
	}
}
