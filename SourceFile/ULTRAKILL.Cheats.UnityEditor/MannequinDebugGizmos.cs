using UnityEngine;

namespace ULTRAKILL.Cheats.UnityEditor;

public class MannequinDebugGizmos : ICheat
{
	private static MannequinDebugGizmos _lastInstance;

	public static bool Enabled
	{
		get
		{
			if (Application.isEditor && Debug.isDebugBuild)
			{
				return _lastInstance?.IsActive ?? false;
			}
			return false;
		}
	}

	public string LongName => "Mannequin Debug Gizmos";

	public string Identifier => "ultrakill.editor.debug-gizmos";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => null;

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		IsActive = Application.isEditor;
		_lastInstance = this;
	}

	public void Disable()
	{
		IsActive = false;
	}
}
