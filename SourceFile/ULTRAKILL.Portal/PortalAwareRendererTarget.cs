using System.Collections.Generic;
using UnityEngine;

namespace ULTRAKILL.Portal;

public class PortalAwareRendererTarget : MonoBehaviour
{
	public readonly List<Transform> Clones = new List<Transform>();

	public PortalAwareRenderer Owner;

	private Collider collider;

	private Renderer renderer;

	private bool hasCollider;

	private bool hasRenderer;

	public void SetCollider(Collider collider)
	{
		this.collider = collider;
		hasCollider = collider != null;
	}

	public void SetRenderer(Renderer renderer)
	{
		this.renderer = renderer;
		hasRenderer = renderer != null;
	}

	public bool TryGetCollider(out Collider collider)
	{
		collider = this.collider;
		return hasCollider;
	}

	public bool TryGetRenderer(out Renderer renderer)
	{
		renderer = this.renderer;
		return hasRenderer;
	}

	private void LateUpdate()
	{
		if (hasCollider)
		{
			hasCollider = collider != null;
		}
		if (hasRenderer)
		{
			hasRenderer = renderer != null;
		}
	}

	private void OnDestroy()
	{
		PortalManagerV2 instance;
		bool flag = MonoSingleton<PortalManagerV2>.TryGetInstance(out instance);
		if (TryGetRenderer(out var renderer))
		{
			PortalAwareRenderer.SetClipPlaneKeyword(renderer, enabled: false);
		}
		int count = Clones.Count;
		for (int i = 0; i < count; i++)
		{
			Transform transform = Clones[i];
			if (!(transform == null))
			{
				if (flag)
				{
					instance.RemoveTransformAccessPair(transform);
				}
				Object.Destroy(transform.gameObject);
			}
		}
	}
}
