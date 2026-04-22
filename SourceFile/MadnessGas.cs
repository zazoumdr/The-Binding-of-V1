using UnityEngine;

public class MadnessGas : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer == 12 && col.TryGetComponent<EnemyIdentifier>(out var component) && (!component.IgnorePlayer || !component.AttackEnemies))
		{
			component.madness = true;
			Transform transform = (component.weakPoint ? component.weakPoint.transform : ((component.overrideCenter != null) ? component.overrideCenter : component.transform));
			GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.madnessEffect, transform.position, Quaternion.identity);
			gameObject.transform.SetParent(transform, worldPositionStays: true);
			component.destroyOnDeath.Add(gameObject);
		}
	}
}
