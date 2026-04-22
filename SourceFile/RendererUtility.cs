using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class RendererUtility
{
	public static Bounds GetBounds<T>(List<T> renderers, bool allowSMRs = true, HashSet<Renderer> renderersToIgnore = null) where T : Renderer
	{
		return GetBounds(CollectionsMarshal.AsSpan(renderers), allowSMRs, renderersToIgnore);
	}

	public static Bounds GetBounds<T>(ReadOnlySpan<T> renderers, bool allowSMRs = true, HashSet<Renderer> renderersToIgnore = null) where T : Renderer
	{
		Bounds result = default(Bounds);
		bool flag = renderersToIgnore != null;
		int length = renderers.Length;
		int i;
		for (i = 0; i < length; i++)
		{
			T val = renderers[i];
			if ((allowSMRs || !(val is SkinnedMeshRenderer skinnedMeshRenderer) || !skinnedMeshRenderer.sharedMesh.isReadable) && val.enabled && val.gameObject.activeInHierarchy && (!flag || !renderersToIgnore.Contains(val)))
			{
				Bounds bounds = val.bounds;
				if (!(bounds.extents == default(Vector3)))
				{
					result = bounds;
					break;
				}
			}
		}
		for (i++; i < length; i++)
		{
			T val2 = renderers[i];
			if ((allowSMRs || !(val2 is SkinnedMeshRenderer skinnedMeshRenderer2) || !skinnedMeshRenderer2.sharedMesh.isReadable) && val2.enabled && val2.gameObject.activeInHierarchy && (!flag || !renderersToIgnore.Contains(val2)))
			{
				Bounds bounds2 = val2.bounds;
				if (!(bounds2.extents == default(Vector3)))
				{
					result.Encapsulate(bounds2);
				}
			}
		}
		return result;
	}
}
