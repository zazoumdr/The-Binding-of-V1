using System.Collections.Generic;
using plog;
using plog.Models;
using Sandbox;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxNavmesh : MonoSingleton<SandboxNavmesh>
{
	private static readonly Logger Log = new Logger("SandboxNavmesh");

	[SerializeField]
	private NavMeshSurface surface;

	public bool isDirty;

	public UnityAction navmeshBuilt;

	private Vector3 defaultCenter;

	private Vector3 defaultSize;

	private void Awake()
	{
		defaultCenter = surface.center;
		defaultSize = surface.size;
	}

	public void MarkAsDirty(SpawnableInstance instance)
	{
		if (!isDirty && (!instance || (instance.frozen && (!(instance.sourceObject != null) || (!instance.sourceObject.isWater && !instance.sourceObject.triggerOnly && instance.sourceObject.spawnableObjectType != SpawnableObject.SpawnableObjectDataType.Enemy)))))
		{
			MonoSingleton<SandboxHud>.Instance.NavmeshDirty();
			isDirty = true;
			MonoSingleton<CheatsManager>.Instance.RenderCheatsInfo();
		}
	}

	public void Rebake()
	{
		surface.BuildNavMesh();
		Log.Info("Navmesh built", (IEnumerable<Tag>)null, (string)null, (object)null);
		isDirty = false;
		MonoSingleton<SandboxHud>.Instance.HideNavmeshNotice();
		if (navmeshBuilt != null)
		{
			navmeshBuilt();
		}
		MonoSingleton<CheatsManager>.Instance.RenderCheatsInfo();
	}

	private void OnDrawGizmos()
	{
		if (!((Object)(object)surface == null))
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(surface.center + base.transform.position, surface.size);
		}
	}

	public void ResetSizeToDefault()
	{
		surface.center = defaultCenter;
		surface.size = defaultSize;
	}

	public void EnsurePositionWithinBounds(Vector3 worldPosition)
	{
		Vector3 vector = surface.center + base.transform.position;
		float num = 1f;
		if (worldPosition.x < vector.x - surface.size.x / 2f)
		{
			float num2 = vector.x - surface.size.x / 2f - worldPosition.x + num;
			surface.center = new Vector3(surface.center.x - num2 / 2f, surface.center.y, surface.center.z);
			surface.size = new Vector3(surface.size.x + num2, surface.size.y, surface.size.z);
		}
		else if (worldPosition.x > vector.x + surface.size.x / 2f)
		{
			float num3 = worldPosition.x - (vector.x + surface.size.x / 2f) + num;
			surface.center = new Vector3(surface.center.x + num3 / 2f, surface.center.y, surface.center.z);
			surface.size = new Vector3(surface.size.x + num3, surface.size.y, surface.size.z);
		}
		if (worldPosition.y < vector.y - surface.size.y / 2f)
		{
			float num4 = vector.y - surface.size.y / 2f - worldPosition.y + num;
			surface.center = new Vector3(surface.center.x, surface.center.y - num4 / 2f, surface.center.z);
			surface.size = new Vector3(surface.size.x, surface.size.y + num4, surface.size.z);
		}
		else if (worldPosition.y > vector.y + surface.size.y / 2f)
		{
			float num5 = worldPosition.y - (vector.y + surface.size.y / 2f) + num;
			surface.center = new Vector3(surface.center.x, surface.center.y + num5 / 2f, surface.center.z);
			surface.size = new Vector3(surface.size.x, surface.size.y + num5, surface.size.z);
		}
		if (worldPosition.z < vector.z - surface.size.z / 2f)
		{
			float num6 = vector.z - surface.size.z / 2f - worldPosition.z + num;
			surface.center = new Vector3(surface.center.x, surface.center.y, surface.center.z - num6 / 2f);
			surface.size = new Vector3(surface.size.x, surface.size.y, surface.size.z + num6);
		}
		else if (worldPosition.z > vector.z + surface.size.z / 2f)
		{
			float num7 = worldPosition.z - (vector.z + surface.size.z / 2f) + num;
			surface.center = new Vector3(surface.center.x, surface.center.y, surface.center.z + num7 / 2f);
			surface.size = new Vector3(surface.size.x, surface.size.y, surface.size.z + num7);
		}
	}
}
