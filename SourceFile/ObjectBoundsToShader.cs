using UnityEngine;

[ExecuteInEditMode]
public class ObjectBoundsToShader : MonoBehaviour
{
	public bool useCustomCenter;

	public Vector3 customCenter;

	private MeshRenderer rend;

	private MaterialPropertyBlock propertyBlock;

	[HideInInspector]
	public CausticVolumeManager manager;

	private Vector3 lastCustomBounds;

	private void OnValidate()
	{
		manager = MonoSingleton<CausticVolumeManager>.Instance;
		if (manager == null)
		{
			manager = Object.FindAnyObjectByType<CausticVolumeManager>(FindObjectsInactive.Include);
		}
		if (manager == null)
		{
			GameObject gameObject = new GameObject("CausticVolumeManager");
			manager = gameObject.AddComponent<CausticVolumeManager>();
		}
		manager.AddObject(this);
	}

	private void OnEnable()
	{
		manager.AddObject(this);
	}

	private void OnDisable()
	{
		manager.RemoveObject(this);
	}

	private void OnDestroy()
	{
		if (!(manager == null))
		{
			manager.RemoveObject(this);
		}
	}

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			base.transform.hasChanged = false;
			if (manager != null)
			{
				manager.isDirty = true;
			}
		}
		else if (customCenter != lastCustomBounds)
		{
			lastCustomBounds = customCenter;
			if (manager != null)
			{
				manager.isDirty = true;
			}
		}
	}

	public void UpdateRendererBounds()
	{
		rend = GetComponent<MeshRenderer>();
		if (!(rend == null))
		{
			if (!useCustomCenter)
			{
				customCenter = rend.bounds.center;
			}
			Vector4 value = (useCustomCenter ? customCenter : rend.bounds.center);
			value.w = manager.causticVolumes.Count;
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			rend.GetPropertyBlock(propertyBlock);
			propertyBlock.SetVector("_BoundsCenter_VolumeCount", value);
			rend.SetPropertyBlock(propertyBlock);
		}
	}
}
