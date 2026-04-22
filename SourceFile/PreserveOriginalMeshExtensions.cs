using UnityEngine;

public static class PreserveOriginalMeshExtensions
{
	public static PreservedOriginalMesh PreserveMesh(this MeshFilter mf)
	{
		if (mf == null)
		{
			return null;
		}
		if (mf.TryGetComponent<PreservedOriginalMesh>(out var component))
		{
			if (component.mesh == null || component.mesh != mf.sharedMesh)
			{
				component.mesh = mf.sharedMesh;
			}
			return component;
		}
		component = mf.gameObject.AddComponent<PreservedOriginalMesh>();
		component.mesh = mf.sharedMesh;
		return component;
	}
}
