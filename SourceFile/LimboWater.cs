using System;
using System.Collections.Generic;
using UnityEngine;

public class LimboWater : MonoBehaviour
{
	private NewMovement nm;

	private EnemyTracker et;

	private ComputeBuffer cb;

	private static int stride = 12;

	private Vector3[] positions;

	private void OnEnable()
	{
		nm = MonoSingleton<NewMovement>.Instance;
		et = MonoSingleton<EnemyTracker>.Instance;
		cb = new ComputeBuffer(256, stride, ComputeBufferType.Structured);
		positions = new Vector3[256];
	}

	private void Update()
	{
		List<EnemyIdentifier> currentEnemies = et.GetCurrentEnemies();
		int num = Math.Min(255, currentEnemies.Count);
		Shader.SetGlobalInteger("_FadePositionsCount", num + 1);
		Array.Clear(positions, 0, positions.Length);
		positions[0] = nm.transform.position;
		for (int i = 0; i < num; i++)
		{
			EnemyIdentifier enemyIdentifier = currentEnemies[i];
			if (!enemyIdentifier.dead)
			{
				positions[i + 1] = enemyIdentifier.transform.position;
			}
		}
		cb.SetData(positions);
		Shader.SetGlobalBuffer("_FadePositions", cb);
	}
}
