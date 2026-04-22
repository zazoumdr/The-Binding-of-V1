using UnityEngine;

public class GasolineStain : MonoBehaviour
{
	private Vector3 initialSize;

	private int index;

	private MeshRenderer mRend;

	private MaterialPropertyBlock propBlock;

	public Transform Parent { get; private set; }

	public bool IsStatic { get; private set; } = true;

	public bool IsFloor { get; private set; }

	private void Awake()
	{
		initialSize = base.transform.localScale;
	}

	private void Start()
	{
		IsFloor = CalculateDot() > 0.25f;
		initialSize = base.transform.localScale;
	}

	private float CalculateDot()
	{
		return Vector3.Dot(-base.transform.forward, Vector3.up);
	}

	public void AttachTo(Collider other, bool clipToSurface)
	{
		Transform transform = other.transform;
		GameObject gameObject = other.gameObject;
		base.transform.SetParent(transform, worldPositionStays: true);
		Parent = transform;
		if (gameObject.CompareTag("Moving") || ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(transform)) || (gameObject.TryGetComponent<Rigidbody>(out var component) && !component.isKinematic))
		{
			IsStatic = false;
		}
		else
		{
			IsStatic = true;
		}
		StainVoxelManager instance = MonoSingleton<StainVoxelManager>.Instance;
		if (!instance.usedComputeShadersAtStart)
		{
			mRend = GetComponent<MeshRenderer>();
			mRend.enabled = true;
			propBlock = new MaterialPropertyBlock();
			propBlock.SetInteger("_Index", Random.Range(0, 5));
			propBlock.SetFloat("_ClipToSurface", clipToSurface ? 1 : 0);
			mRend.SetPropertyBlock(propBlock);
		}
		else
		{
			instance.AddGasolineStain(base.transform, clipToSurface);
		}
		Vector3 forward = base.transform.forward;
		Vector3 worldPosition = base.transform.position + forward * -0.5f;
		StainVoxel stainVoxel = instance.CreateOrGetVoxel(worldPosition);
		VoxelProxy voxelProxy = stainVoxel.CreateOrGetProxyFor(this);
		instance.AcknowledgeNewStain(stainVoxel);
		if (!IsStatic && (bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(transform) && transform.TryGetComponent<ScrollingTexture>(out var component2) && !component2.attachedObjects.Contains(voxelProxy.transform))
		{
			component2.attachedObjects.Add(voxelProxy.transform);
		}
	}

	public void OnTransformParentChanged()
	{
		initialSize = base.transform.localScale;
	}

	public void SetSize(float size)
	{
		base.transform.localScale = initialSize * size;
	}
}
