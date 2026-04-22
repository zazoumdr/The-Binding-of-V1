using UnityEngine;

public class Bleeder : MonoBehaviour
{
	private GoreZone gz;

	public EnemyType[] ignoreTypes;

	public void GetHit(Vector3 point, GoreType type, bool fromExplosion = false)
	{
		if (gz == null)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(type, isUnderwater: false, isSandified: false, isBlessed: false, null, fromExplosion);
		if ((bool)gore)
		{
			gore.transform.position = point;
			gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
			Bloodsplatter component = gore.GetComponent<Bloodsplatter>();
			gore.SetActive(value: true);
			if ((bool)component)
			{
				component.GetReady();
			}
		}
	}
}
