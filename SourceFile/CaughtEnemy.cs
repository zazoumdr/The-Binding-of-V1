using UnityEngine;

public class CaughtEnemy
{
	public EnemyIdentifier original;

	public SavedEnemy savedEnemy;

	public Vector3 position;

	public Quaternion rotation;

	public GameObject puppet;

	public CaughtEnemy(EnemyIdentifier newOriginal, SavedEnemy newSaved)
	{
		original = newOriginal;
		savedEnemy = newSaved;
		position = original.transform.position;
		rotation = original.transform.rotation;
	}

	public void UpdatePosition(Vector3 newPosition, Quaternion newRotation)
	{
		position = newPosition;
		rotation = newRotation;
	}

	public void UpdatePuppet(GameObject newPuppet)
	{
		puppet = newPuppet;
		if (puppet.TryGetComponent<EnemyIdentifier>(out var component))
		{
			component.stationary = original.stationary;
		}
	}
}
