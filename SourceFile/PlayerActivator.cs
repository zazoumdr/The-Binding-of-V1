using UnityEngine;

public class PlayerActivator : MonoBehaviour
{
	private NewMovement nm;

	private bool activated;

	[SerializeField]
	private bool startTimer;

	[SerializeField]
	private bool onlyActivatePlayer;

	private GunControl gc;

	public static Vector3 lastActivatedPosition;

	private void OnTriggerEnter(Collider other)
	{
		if (!activated && other.gameObject.CompareTag("Player"))
		{
			Activate();
		}
	}

	public void Activate()
	{
		if (activated)
		{
			return;
		}
		nm = MonoSingleton<NewMovement>.Instance;
		gc = MonoSingleton<GunControl>.Instance;
		GameStateManager.Instance.PopState("pit-falling");
		if (!nm.activated)
		{
			nm.activated = true;
			nm.cc.activated = true;
			nm.cc.CameraShake(1f);
			nm.cc.enabled = true;
			AudioSource component = GetComponent<AudioSource>();
			if ((bool)(Object)(object)component)
			{
				component.Play(tracked: true);
			}
		}
		activated = true;
		if (!onlyActivatePlayer)
		{
			gc.YesWeapon();
			ActivateObjects();
		}
		if (startTimer)
		{
			MonoSingleton<StatsManager>.Instance.StartTimer();
		}
		MonoSingleton<FistControl>.Instance.YesFist();
	}

	private void ActivateObjects()
	{
		MonoSingleton<PlayerActivatorRelay>.Instance.ResetIndex();
		MonoSingleton<PlayerActivatorRelay>.Instance.Activate();
		if (nm.levelOver)
		{
			nm.levelOver = false;
			MonoSingleton<StatsManager>.Instance.UnhideShit();
		}
		lastActivatedPosition = MonoSingleton<NewMovement>.Instance.transform.position;
	}
}
