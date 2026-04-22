using UnityEngine;

public class BossIdentifier : MonoBehaviour
{
	private EnemyIdentifier eid;

	private AlwaysLookAtCamera alac;

	private void Awake()
	{
		CheckDifficultyOverride();
	}

	private void OnEnable()
	{
		CheckDifficultyOverride();
	}

	public void CheckDifficultyOverride()
	{
		if (!eid && !TryGetComponent<EnemyIdentifier>(out eid))
		{
			if (MonoSingleton<AssistController>.Instance.majorEnabled && ((bool)alac || TryGetComponent<AlwaysLookAtCamera>(out alac)))
			{
				alac.ChangeDifficulty(MonoSingleton<AssistController>.Instance.difficultyOverride);
			}
			else
			{
				Object.Destroy(this);
			}
		}
		else if (MonoSingleton<AssistController>.Instance.majorEnabled)
		{
			eid.difficultyOverride = MonoSingleton<AssistController>.Instance.difficultyOverride;
		}
		else
		{
			eid.difficultyOverride = -1;
		}
	}
}
