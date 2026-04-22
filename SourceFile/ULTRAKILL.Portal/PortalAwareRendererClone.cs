using System.Collections.Generic;
using UnityEngine;

namespace ULTRAKILL.Portal;

public class PortalAwareRendererClone : MonoBehaviour
{
	private static readonly List<Vector4> tempClipPlanes = new List<Vector4>();

	private static MaterialPropertyBlock _propertyBlock;

	public PortalAwareRenderer.Clone Owner;

	public PortalAwareRendererTarget Target;

	public Transform TargetTransform;

	public GameObject TargetObject;

	public PortalAwareRenderer PortalAwareRenderer;

	private Collider collider;

	private Renderer renderer;

	private bool hasCollider;

	private bool hasRenderer;

	private Material cachedTargetMaterial;

	private GameObject gameObj;

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

	private void LateUpdate()
	{
		bool activeSelf = TargetObject.activeSelf;
		if (gameObj.activeSelf != activeSelf)
		{
			gameObj.SetActive(activeSelf);
			if (!activeSelf)
			{
				return;
			}
		}
		base.transform.localScale = TargetTransform.lossyScale;
		if (hasCollider)
		{
			if (Target.TryGetCollider(out var collider) && this.collider != null)
			{
				bool flag = collider.enabled;
				if (this.collider.enabled != collider.enabled)
				{
					this.collider.enabled = flag;
				}
				if (flag)
				{
					this.collider.isTrigger = collider.isTrigger;
				}
			}
			else
			{
				hasCollider = false;
			}
		}
		if (!hasRenderer)
		{
			return;
		}
		if (Target.TryGetRenderer(out var renderer) && this.renderer != null)
		{
			bool flag2 = renderer.enabled;
			if (this.renderer.enabled != renderer.enabled)
			{
				this.renderer.enabled = flag2;
			}
			if (flag2)
			{
				Material sharedMaterial = renderer.sharedMaterial;
				if (sharedMaterial != cachedTargetMaterial)
				{
					cachedTargetMaterial = sharedMaterial;
					this.renderer.sharedMaterials = renderer.sharedMaterials;
					PortalAwareRenderer.SetClipPlaneKeyword(this.renderer, enabled: true);
				}
				this.renderer.GetPropertyBlock(_propertyBlock);
				int num = (int)_propertyBlock.GetFloat(ShaderProperties.ClipPlaneCount);
				if (num > 0)
				{
					_propertyBlock.GetVectorArray(ShaderProperties.ClipPlanes, tempClipPlanes);
					renderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetFloat(ShaderProperties.ClipPlaneCount, num);
					_propertyBlock.SetVectorArray(ShaderProperties.ClipPlanes, tempClipPlanes);
					this.renderer.SetPropertyBlock(_propertyBlock);
				}
				if (renderer is SpriteRenderer spriteRenderer && this.renderer is SpriteRenderer spriteRenderer2)
				{
					spriteRenderer2.color = spriteRenderer.color;
					spriteRenderer2.sprite = spriteRenderer.sprite;
				}
			}
		}
		else
		{
			hasRenderer = false;
		}
	}

	private void OnDestroy()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			instance.RemoveTransformAccessPair(base.transform);
		}
		if (hasRenderer && renderer != null)
		{
			PortalAwareRenderer.SetClipPlaneKeyword(renderer, enabled: false);
		}
		Object.Destroy(base.gameObject);
	}

	private void Awake()
	{
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		gameObj = base.gameObject;
	}
}
