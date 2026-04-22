using UnityEngine;

public class Readable : MonoBehaviour
{
	[SerializeField]
	[TextArea(3, 12)]
	private string content;

	[SerializeField]
	private bool instantScan;

	private int gameObjectInstanceId;

	private void Awake()
	{
		gameObjectInstanceId = base.gameObject.GetInstanceID();
	}

	public void PickUp()
	{
		MonoSingleton<ScanningStuff>.Instance.oldWeaponState = !MonoSingleton<GunControl>.Instance.noWeapons;
		Invoke("StartScan", 0.5f);
	}

	public void PutDown()
	{
		CancelInvoke("StartScan");
		MonoSingleton<ScanningStuff>.Instance.ResetState();
	}

	private void StartScan()
	{
		MonoSingleton<ScanningStuff>.Instance.ScanBook(content, instantScan, base.gameObject.GetInstanceID());
	}

	private void OnDestroy()
	{
		if ((bool)MonoSingleton<ScanningStuff>.Instance)
		{
			MonoSingleton<ScanningStuff>.Instance.ReleaseScroll(gameObjectInstanceId);
		}
	}
}
