using ScriptableObjects;
using UnityEngine;

public abstract class MapInfoBase : MonoBehaviour
{
	public static MapInfoBase Instance;

	public string layerName = "LAYER /// NUMBER";

	public string levelName = "LEVEL NAME";

	public bool sandboxTools;

	public bool hideStockHUD;

	public bool replaceCheckpointButtonWithSkip;

	public bool forceUpdateEnemyRenderers;

	public bool continuousGibCollisions;

	public bool removeGibsWithoutAbsorbers;

	public float gibRemoveTime = 5f;

	public TipOfTheDay tipOfTheDay;

	internal virtual void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
	}
}
