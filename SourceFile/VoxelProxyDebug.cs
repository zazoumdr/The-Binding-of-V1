using ULTRAKILL.Cheats.UnityEditor;
using UnityEngine;

public class VoxelProxyDebug : MonoBehaviour
{
	private VoxelProxy voxelProxy;

	private void Awake()
	{
		voxelProxy = GetComponent<VoxelProxy>();
	}

	private void OnDrawGizmos()
	{
		if (voxelProxy == null)
		{
			return;
		}
		if (!NapalmDebugVoxels.Enabled)
		{
			Object.Destroy(this);
			return;
		}
		Gizmos.color = (voxelProxy.isStatic ? Color.blue : Color.green);
		Gizmos.DrawWireCube(base.transform.position, Vector3.one * 2.75f);
		if (!voxelProxy.isStatic)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(voxelProxy.voxel.RoundedWorldPosition, Vector3.one * 2.75f);
		}
	}
}
