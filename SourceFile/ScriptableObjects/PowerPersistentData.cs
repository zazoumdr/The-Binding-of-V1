using UnityEngine;

namespace ScriptableObjects;

[CreateAssetMenu(fileName = "Power Persistent Data", menuName = "ULTRAKILL/Enemy Persistent Data/Power")]
public class PowerPersistentData : EnemyPersistentData
{
	[field: SerializeField]
	public float RepeatedIntroSpeedFactor { get; private set; } = 3f;

	[field: SerializeField]
	public bool RepeatedIntroOverrideClip { get; private set; }

	[field: SerializeField]
	public AudioClip[] RepeatedIntroClips { get; private set; }

	public bool PerformedIntro { get; set; }

	public override void ResetRuntimeData()
	{
		base.ResetRuntimeData();
		PerformedIntro = false;
	}
}
