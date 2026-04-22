using UnityEngine;

public class EnemySortieInjector : MonoBehaviour
{
	public EnemyScript[] enemies;

	public Transform[] sortiePositions;

	public void Inject()
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			enemies[i].SetSortiePos(sortiePositions[i].position);
		}
	}
}
