using Sandbox;
using UnityEngine;

public class SandboxPropPart : MonoBehaviour
{
	public SpawnableInstance parent;

	private void Awake()
	{
		if (parent == null)
		{
			parent = GetComponentInParent<SpawnableInstance>();
		}
	}
}
