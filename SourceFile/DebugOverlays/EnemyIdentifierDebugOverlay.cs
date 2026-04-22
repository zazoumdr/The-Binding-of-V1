using UnityEngine;

namespace DebugOverlays;

public class EnemyIdentifierDebugOverlay : MonoBehaviour
{
	private EnemyType enemyType;

	private EnemyClass enemyClass;

	private bool dead;

	private bool ignorePlayer;

	private bool attackEnemies;

	private EnemyTarget target;

	public void ConsumeData(EnemyType enemyType, EnemyClass enemyClass, bool dead, bool ignorePlayer, bool attackEnemies, EnemyTarget target)
	{
		this.enemyType = enemyType;
		this.enemyClass = enemyClass;
		this.dead = dead;
		this.ignorePlayer = ignorePlayer;
		this.attackEnemies = attackEnemies;
		this.target = target;
	}

	private void OnGUI()
	{
		Rect? onScreenRect = OnGUIHelper.GetOnScreenRect(base.transform.position);
		if (!onScreenRect.HasValue)
		{
			return;
		}
		Rect value = onScreenRect.Value;
		GUI.Label(value, $"{enemyType} ({enemyClass})");
		if (dead)
		{
			GUI.color = Color.red;
			value.y += 20f;
			GUI.Label(value, "Dead!");
			return;
		}
		GUI.color = Color.white;
		value.y += 20f;
		GUI.Label(value, $"Ignore player: {ignorePlayer}");
		value.y += 20f;
		GUI.Label(value, $"Attack enemies: {attackEnemies}");
		value.y += 20f;
		if (target == null)
		{
			GUI.color = Color.red;
			GUI.Label(value, "Target: Null");
		}
		else if (target.isPlayer)
		{
			GUI.Label(value, "Target: (Player)");
		}
		else if (target.targetTransform != null)
		{
			GUI.Label(value, "Target: (" + target.targetTransform.name + ")");
			Vector3 position = target.position;
			Vector3 vector = MonoSingleton<CameraController>.Instance.cam.WorldToScreenPoint(position);
			if (position.z > 0f)
			{
				Rect rect = new Rect(vector.x - 5f, (float)Screen.height - vector.y - 5f, 10f, 10f);
				GUI.color = Color.yellow;
				GUI.Box(rect, "");
				GUI.color = Color.white;
			}
		}
		GUI.color = Color.white;
	}
}
