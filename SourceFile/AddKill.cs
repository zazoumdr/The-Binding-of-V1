using UnityEngine;

public class AddKill : MonoBehaviour
{
	private void Start()
	{
		GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
		if (goreZone != null && goreZone.checkpoint != null)
		{
			goreZone.AddDeath();
			goreZone.checkpoint.sm.kills++;
		}
		else
		{
			MonoSingleton<StatsManager>.Instance.kills++;
		}
	}
}
