using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class MarkedForDeath : MonoSingleton<MarkedForDeath>
{
	private void OnEnable()
	{
		EnemyIdentifier[] array = Object.FindObjectsOfType<EnemyIdentifier>();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (!array[num].dead)
			{
				array[num].PlayerMarkedForDeath();
			}
		}
	}
}
