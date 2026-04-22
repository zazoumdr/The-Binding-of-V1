using System.Collections.Generic;
using ULTRAKILL.Cheats.UnityEditor;
using UnityEngine;

public class VoxelProxy : MonoBehaviour
{
	[HideInInspector]
	public bool isStatic;

	public StainVoxel voxel;

	[HideInInspector]
	public Transform parent;

	private BurningVoxel burningVoxel;

	private VoxelProxyDebug debug;

	private bool exploded;

	public bool isBurning
	{
		get
		{
			if (!(burningVoxel != null))
			{
				return exploded;
			}
			return true;
		}
	}

	public List<GasolineStain> stains { get; } = new List<GasolineStain>();

	private void Awake()
	{
		if (burningVoxel == null)
		{
			burningVoxel = GetComponent<BurningVoxel>();
		}
		if (debug == null)
		{
			debug = GetComponent<VoxelProxyDebug>();
		}
	}

	public void SetParent(Transform parent, bool isStatic)
	{
		this.isStatic = isStatic;
		this.parent = parent;
		Vector3 vector = ComputeCombinedHierarchyScale(parent);
		base.transform.SetParent(parent, worldPositionStays: true);
		base.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
		base.transform.localRotation = Quaternion.identity;
	}

	private Vector3 ComputeCombinedHierarchyScale(Transform parent)
	{
		Vector3 vector = Vector3.one;
		Transform transform = parent;
		while (transform != null)
		{
			vector = Vector3.Scale(vector, transform.localScale);
			transform = transform.parent;
		}
		return vector;
	}

	public void Add(GasolineStain stain)
	{
		stains.Add(stain);
		stain.transform.SetParent(base.transform, worldPositionStays: true);
	}

	public bool IsMatch(ProxySearchMode searchMode)
	{
		if (stains.Count == 0)
		{
			return false;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (!searchMode.HasAllFlags(ProxySearchMode.IncludeStatic) && isStatic)
		{
			return false;
		}
		if (!searchMode.HasAllFlags(ProxySearchMode.IncludeDynamic) && !isStatic)
		{
			return false;
		}
		if (!searchMode.HasAllFlags(ProxySearchMode.IncludeBurning) && isBurning)
		{
			return false;
		}
		if (!searchMode.HasAllFlags(ProxySearchMode.IncludeNotBurning) && !isBurning)
		{
			return false;
		}
		if (searchMode.HasAllFlags(ProxySearchMode.FloorOnly))
		{
			foreach (GasolineStain stain in stains)
			{
				if (stain.IsFloor)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void DestroySelf()
	{
		Object.Destroy(base.gameObject);
	}

	public void StartBurningOrRefuel()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (burningVoxel != null)
			{
				burningVoxel.Refuel();
				return;
			}
			burningVoxel = base.gameObject.AddComponent<BurningVoxel>();
			burningVoxel.Initialize(this);
		}
	}

	public void ExplodeAndDestroy()
	{
		exploded = true;
		DestroySelf();
		GameObject gameObject = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.explosion, base.transform.position, Quaternion.identity);
		if (gameObject.TryGetComponent<ExplosionController>(out var component))
		{
			component.tryIgniteGasoline = false;
			component.forceSimple = true;
			Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
			foreach (Explosion obj in componentsInChildren)
			{
				obj.lowQuality = true;
				obj.HurtCooldownCollection = MonoSingleton<StainVoxelManager>.Instance.SharedHurtCooldownCollection;
			}
		}
	}

	private void Update()
	{
		if (debug == null && NapalmDebugVoxels.Enabled)
		{
			debug = base.gameObject.AddComponent<VoxelProxyDebug>();
		}
		if (!isStatic)
		{
			Vector3Int vector3Int = StainVoxelManager.WorldToVoxelPosition(base.transform.position);
			if (vector3Int != voxel.VoxelPosition)
			{
				MonoSingleton<StainVoxelManager>.Instance.UpdateProxyPosition(this, vector3Int);
			}
		}
	}

	private void OnDestroy()
	{
		if (debug != null)
		{
			Object.Destroy(debug);
		}
		if (base.gameObject.scene.isLoaded)
		{
			voxel.RemoveProxy(this);
		}
	}

	public void SetStainSize(float size)
	{
		foreach (GasolineStain stain in stains)
		{
			stain.SetSize(size);
		}
	}
}
