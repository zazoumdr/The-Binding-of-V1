using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class StainVoxel
{
	public readonly Vector3Int VoxelPosition;

	public readonly Vector3 RoundedWorldPosition;

	public readonly int HashCode;

	public VoxelProxy staticProxy;

	public Dictionary<Transform, List<VoxelProxy>> dynamicProxies;

	private readonly Collider[] waterOverlapResult = new Collider[3];

	private const string StaticVoxelName = "VoxelProxy";

	public bool isEmpty
	{
		get
		{
			if (staticProxy == null)
			{
				if (dynamicProxies != null)
				{
					return dynamicProxies.Count == 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool isBurning => HasBurningProxies();

	public StainVoxel(Vector3Int voxelPosition)
	{
		VoxelPosition = voxelPosition;
		HashCode = VoxelPosition.GetHashCode();
		RoundedWorldPosition = StainVoxelManager.VoxelToWorldPosition(voxelPosition);
		staticProxy = null;
		dynamicProxies = null;
	}

	public VoxelProxy CreateOrGetProxyFor(GasolineStain stain)
	{
		if (stain.IsStatic)
		{
			if (staticProxy == null)
			{
				staticProxy = CreateNewProxy(stain, isStatic: true);
			}
			staticProxy.Add(stain);
			return staticProxy;
		}
		if (dynamicProxies == null)
		{
			dynamicProxies = new Dictionary<Transform, List<VoxelProxy>>();
		}
		if (!dynamicProxies.ContainsKey(stain.Parent))
		{
			dynamicProxies[stain.Parent] = new List<VoxelProxy>();
		}
		if (dynamicProxies[stain.Parent].Count == 0)
		{
			VoxelProxy item = CreateNewProxy(stain, isStatic: false);
			dynamicProxies[stain.Parent].Add(item);
		}
		dynamicProxies[stain.Parent][0].Add(stain);
		return dynamicProxies[stain.Parent][0];
	}

	public void AcknowledgeNewStain()
	{
		if (!isBurning)
		{
			return;
		}
		foreach (VoxelProxy item in GetProxies(ProxySearchMode.Any).ToList())
		{
			item.StartBurningOrRefuel();
		}
	}

	public void AddProxy(VoxelProxy existingProxy)
	{
		if (existingProxy.isBurning)
		{
			TryIgnite();
		}
		else if (isBurning)
		{
			existingProxy.StartBurningOrRefuel();
		}
		if (existingProxy.isStatic)
		{
			staticProxy = existingProxy;
		}
		else
		{
			if (dynamicProxies == null)
			{
				dynamicProxies = new Dictionary<Transform, List<VoxelProxy>>();
			}
			if (!dynamicProxies.ContainsKey(existingProxy.parent))
			{
				dynamicProxies[existingProxy.parent] = new List<VoxelProxy>();
			}
			dynamicProxies[existingProxy.parent].Add(existingProxy);
		}
		existingProxy.voxel = this;
	}

	public void RemoveProxy(VoxelProxy proxy, bool destroy = true)
	{
		if (proxy.isStatic)
		{
			if (staticProxy == proxy)
			{
				staticProxy = null;
			}
		}
		else
		{
			if (dynamicProxies == null)
			{
				return;
			}
			if (dynamicProxies.ContainsKey(proxy.parent))
			{
				dynamicProxies[proxy.parent].Remove(proxy);
				if (dynamicProxies[proxy.parent].Count == 0)
				{
					dynamicProxies.Remove(proxy.parent);
				}
			}
			if (dynamicProxies.Count == 0)
			{
				dynamicProxies = null;
			}
		}
		if (destroy)
		{
			proxy.DestroySelf();
		}
		MonoSingleton<StainVoxelManager>.Instance.RefreshVoxel(this);
	}

	public void DestroySelf()
	{
		if (staticProxy != null)
		{
			staticProxy.DestroySelf();
		}
		if (dynamicProxies != null)
		{
			foreach (List<VoxelProxy> value in dynamicProxies.Values)
			{
				foreach (VoxelProxy item in value)
				{
					item.DestroySelf();
				}
			}
		}
		staticProxy = null;
		dynamicProxies = null;
	}

	public IEnumerable<VoxelProxy> GetProxies(ProxySearchMode mode)
	{
		if (mode.HasAllFlags(ProxySearchMode.IncludeStatic) && staticProxy != null && staticProxy.IsMatch(mode))
		{
			yield return staticProxy;
		}
		if (!mode.HasAllFlags(ProxySearchMode.IncludeDynamic) || dynamicProxies == null)
		{
			yield break;
		}
		foreach (List<VoxelProxy> value in dynamicProxies.Values)
		{
			foreach (VoxelProxy item in value)
			{
				if (item.IsMatch(mode))
				{
					yield return item;
				}
			}
		}
	}

	public bool HasFloorStains()
	{
		return GetProxies(ProxySearchMode.FloorOnly).Any();
	}

	public bool HasBurningProxies()
	{
		return GetProxies(ProxySearchMode.AnyBurning).Any();
	}

	public bool HasStains(ProxySearchMode mode)
	{
		return GetProxies(mode).Any();
	}

	public bool TryIgnite()
	{
		if (isBurning)
		{
			return false;
		}
		List<VoxelProxy> list = GetProxies(ProxySearchMode.AnyNotBurning).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		StainVoxelManager instance = MonoSingleton<StainVoxelManager>.Instance;
		bool flag = true;
		if (Physics.OverlapSphereNonAlloc(RoundedWorldPosition, 1.375f, waterOverlapResult, 16, QueryTriggerInteraction.Collide) > 0)
		{
			flag = false;
			int num = 65536;
			num |= 0x40000;
			int count = Physics.OverlapSphereNonAlloc(RoundedWorldPosition, 1.375f, waterOverlapResult, num, QueryTriggerInteraction.Collide);
			foreach (Collider item in waterOverlapResult.Take(count))
			{
				if (item.TryGetComponent<DryZone>(out var _))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			foreach (VoxelProxy item2 in list)
			{
				item2.StartBurningOrRefuel();
			}
		}
		else if (instance.ShouldExplodeAt(VoxelPosition))
		{
			foreach (VoxelProxy item3 in list)
			{
				item3.ExplodeAndDestroy();
			}
		}
		else
		{
			foreach (VoxelProxy item4 in list)
			{
				item4.DestroySelf();
			}
		}
		MonoSingleton<StainVoxelManager>.Instance.ScheduleFirePropagation(this);
		return true;
	}

	private VoxelProxy CreateNewProxy(GasolineStain stain, bool isStatic)
	{
		GameObject gameObject = new GameObject(GetProxyName());
		gameObject.transform.position = RoundedWorldPosition;
		VoxelProxy voxelProxy = gameObject.AddComponent<VoxelProxy>();
		voxelProxy.gameObject.AddComponent<DestroyOnCheckpointRestart>();
		Transform parent = stain.transform.parent;
		voxelProxy.SetParent(parent, isStatic);
		voxelProxy.voxel = this;
		return voxelProxy;
	}

	public string GetProxyName()
	{
		return "VoxelProxy";
	}

	public override int GetHashCode()
	{
		return HashCode;
	}

	public override bool Equals(object obj)
	{
		if (obj is StainVoxel stainVoxel)
		{
			return HashCode == stainVoxel.HashCode;
		}
		return false;
	}
}
