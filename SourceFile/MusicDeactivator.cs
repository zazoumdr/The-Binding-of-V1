using UnityEngine;

public class MusicDeactivator : MonoBehaviour
{
	public bool oneTime;

	public bool forceOff;

	private void OnEnable()
	{
		MonoSingleton<MusicManager>.Instance.StopMusic();
		if (forceOff)
		{
			MonoSingleton<MusicManager>.Instance.forcedOff = true;
		}
		if (oneTime)
		{
			Object.Destroy(this);
		}
	}
}
