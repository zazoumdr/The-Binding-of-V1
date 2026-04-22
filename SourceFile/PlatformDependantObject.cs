using SettingsMenu.Models;
using UnityEngine;

public class PlatformDependantObject : MonoBehaviour
{
	[SerializeField]
	private bool requiresSteam;

	[SerializeField]
	private bool requiresDiscord;

	[SerializeField]
	private bool requiresFileSystemAccess;

	[SerializeField]
	private bool hideInSolsticeRelease;

	[SerializeField]
	private UltrakillEvent onDestroy;

	private void Awake()
	{
		_ = requiresSteam;
		_ = requiresDiscord;
		_ = requiresFileSystemAccess;
		if (hideInSolsticeRelease && PlatformRequirements.IsCloudManagedRelease())
		{
			onDestroy.Invoke();
			Object.Destroy(base.gameObject);
		}
	}
}
