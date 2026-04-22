using UnityEngine;

public class EventOnRandomTimer : MonoBehaviour
{
	public float timerMinimum;

	public float timerMaximum;

	private float currentTarget;

	private TimeSince timer;

	public bool forceOnStart;

	public bool noMinimumOnFirst;

	private bool activated;

	public UltrakillEvent onTimer;

	private void Start()
	{
		if (forceOnStart)
		{
			onTimer.Invoke();
		}
	}

	private void OnEnable()
	{
		Randomize();
	}

	private void Update()
	{
		if ((float)timer > currentTarget)
		{
			onTimer.Invoke();
			Randomize();
		}
	}

	private void Randomize()
	{
		timer = 0f;
		currentTarget = Random.Range((!activated && noMinimumOnFirst) ? 0f : timerMinimum, timerMaximum);
		activated = true;
	}
}
