using UnityEngine;

public class BasicEnemyDataRelay : MonoBehaviour, IPlaceholdableComponent
{
	[HideInInspector]
	public EnemyType enemyType;

	[HideInInspector]
	public float health = 1f;

	public void WillReplace(GameObject oldObject, GameObject newObject, bool isSelfBeingReplaced)
	{
		if (isSelfBeingReplaced)
		{
			BasicEnemyDataRelay component = newObject.GetComponent<BasicEnemyDataRelay>();
			if ((bool)component)
			{
				component.Apply(this);
			}
		}
	}

	private void Apply(BasicEnemyDataRelay source)
	{
		if (TryGetComponent<Enemy>(out var component))
		{
			component.health = source.health;
		}
		if (TryGetComponent<Enemy>(out var component2))
		{
			component2.health = source.health;
		}
		if (TryGetComponent<Enemy>(out var component3))
		{
			component3.health = source.health;
		}
		Enemy componentInChildren = GetComponentInChildren<Enemy>();
		if ((bool)componentInChildren)
		{
			componentInChildren.health = source.health;
		}
		Enemy componentInChildren2 = GetComponentInChildren<Enemy>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.health = source.health;
		}
	}
}
