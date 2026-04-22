using UnityEngine;

namespace ULTRAKILL.Cheats.UnityEditor;

public class NapalmDebugVoxels : ICheat
{
	private static NapalmDebugVoxels _lastInstance;

	private bool active;

	public static bool Enabled
	{
		get
		{
			if (Application.isEditor && Debug.isDebugBuild)
			{
				return _lastInstance?.active ?? false;
			}
			return false;
		}
	}

	public string LongName => "Napalm Debug Voxels";

	public string Identifier => "ultrakill.editor.debug-napalm-voxels";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		active = Application.isEditor;
		_lastInstance = this;
	}

	public void Disable()
	{
		active = false;
	}
}
