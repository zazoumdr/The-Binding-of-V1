using UnityEngine;

public class FinalDoorOpener : MonoBehaviour
{
	public bool startTimer;

	public bool startMusic;

	[HideInInspector]
	public bool opened;

	private bool opening;

	private bool closed;

	private FinalDoor fd;

	private void Awake()
	{
		fd = GetComponentInParent<FinalDoor>();
		if (!opened)
		{
			if (fd != null)
			{
				fd.Open();
			}
			if (fd != null)
			{
				opening = true;
				Invoke("GoTime", 1f);
			}
			else
			{
				GoTime();
			}
		}
	}

	private void OnEnable()
	{
		if (closed)
		{
			if (fd != null)
			{
				fd.Open();
			}
			if (fd != null)
			{
				Invoke("GoTime", 1f);
			}
			else
			{
				GoTime();
			}
		}
	}

	public void GoTime()
	{
		CancelInvoke("GoTime");
		if (!opened)
		{
			opening = false;
			opened = true;
			MonoSingleton<OnLevelStart>.Instance.StartLevel(startTimer, startMusic);
		}
	}

	public void Close()
	{
		if (opened || opening)
		{
			closed = true;
			opened = false;
			opening = false;
			CancelInvoke("GoTime");
			if ((bool)fd)
			{
				fd.Close();
			}
		}
	}
}
