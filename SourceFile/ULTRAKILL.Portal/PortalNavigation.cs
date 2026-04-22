using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PrivateAPIBridge;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace ULTRAKILL.Portal;

public class PortalNavigation
{
	private NativeList<BoxcastCommand> boxcastCommands;

	private const int STACK_THRESHOLD = 64;

	private float linkRemovalTimer;

	public bool TryGetNavPoint(PortalHandle handle, out Vector3 point)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Portal portalObject = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(handle);
		NativePortalTransform transform = portalObject.GetTransform(handle.side);
		PlaneShape shape = portalObject.GetShape();
		Vector3 normalized = Vector3.ProjectOnPlane(float3.op_Implicit(-transform.forward), Vector3.up).normalized;
		float boundingRadius = shape.GetBoundingRadius();
		point = transform.centerManaged + normalized * 1f;
		NavMeshHit val = default(NavMeshHit);
		if (!NavMesh.SamplePosition(point, ref val, boundingRadius * 1.5f, -1))
		{
			return false;
		}
		Vector3 source = ((NavMeshHit)(ref val)).position;
		PlaneShapeExtensions.GetClosestPoint(shape.width, shape.height, transform.center, transform.right, transform.up, in Unsafe.As<Vector3, float3>(ref source), in Unsafe.As<Vector3, float3>(ref point), out var closest);
		if (Vector3.SqrMagnitude(source - Unsafe.As<float3, Vector3>(ref closest)) > 49f)
		{
			return false;
		}
		point = ((NavMeshHit)(ref val)).position;
		return true;
	}

	public void GenerateLinks(PortalScene scene, PortalIdentifier identifier)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		identifier.ResetDirtiness();
		PortalHandle handle = identifier.Handle;
		Portal portalObject = scene.GetPortalObject(handle);
		if (!portalObject.GetTravelFlags(handle.side).HasFlag(PortalTravellerFlags.Enemy))
		{
			return;
		}
		NativePortalTransform transform = portalObject.GetTransform(handle.side);
		NativePortalTransform transform2 = portalObject.GetTransform(handle.side.Reverse());
		if (Vector3.Dot(float3.op_Implicit(transform.up), float3.op_Implicit(transform2.up)) < -0.1f)
		{
			return;
		}
		PlaneShape shape = portalObject.GetShape();
		float num = shape.width / 2f;
		float num2 = shape.height / 2f;
		int num3 = Mathf.CeilToInt(num);
		int num4 = Mathf.CeilToInt(shape.height / 4f);
		int num5 = (num4 + 1) * (num3 + 1);
		int num6 = Mathf.Max(1, NavMesh.GetSettingsCount());
		int num7 = (num3 + 1) * (num4 + 1) * num6;
		NavMeshLinkData[] array = (NavMeshLinkData[])(object)new NavMeshLinkData[num7];
		NavMeshLinkInstance[] array2 = (NavMeshLinkInstance[])(object)new NavMeshLinkInstance[num7];
		Vector3[] array3 = new Vector3[num7];
		float num8 = portalObject.LinkOffset(handle.side);
		float num9 = portalObject.LinkOffset(handle.side.Reverse());
		bool flag = transform.IsFloor();
		bool flag2 = transform2.IsFloor();
		Vector3 vector = ((!flag) ? new Vector3(transform.back.x, 0f, transform.back.z).normalized : transform.backManaged);
		vector *= num8;
		Vector3 vector2 = ((!flag2) ? new Vector3(transform2.back.x, 0f, transform2.back.z).normalized : transform2.backManaged);
		vector2 *= num9;
		Vector3 vector3 = transform.centerManaged + transform.leftManaged * num + transform.upManaged * num2;
		Vector3 vector4 = transform2.centerManaged + transform2.rightManaged * num + transform2.upManaged * num2;
		float num10 = 0.5f;
		float num11 = 1f / (float)num3;
		float num12 = 1f / (float)num4;
		Vector3 zero = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		bool flag3 = transform.left.y != 0f || transform2.left.y != 0f;
		int settingsCount = NavMesh.GetSettingsCount();
		_ = stackalloc int[512];
		int[] array4 = null;
		Span<int> span = ((settingsCount < 64) ? ((Span<int>)(array4 = ArrayPool<int>.Shared.Rent(64))) : stackalloc int[64]);
		Span<int> span2 = span;
		span2 = span2.Slice(0, settingsCount);
		NavMeshBuildSettings val = default(NavMeshBuildSettings);
		for (int i = 0; i < span2.Length; i++)
		{
			NavMeshExtensions.GetSettingsByIndex_Injected(i, ref val);
			span2[i] = ((NavMeshBuildSettings)(ref val)).agentTypeID;
		}
		Span<NavMeshLinkData> span3 = array.AsSpan();
		Span<Vector3> span4 = array3.AsSpan();
		Span<NavMeshLinkInstance> span5 = array2.AsSpan();
		NavMeshHit val2 = default(NavMeshHit);
		NavMeshHit val3 = default(NavMeshHit);
		for (int j = 0; j <= num3; j++)
		{
			float num13 = shape.width * ((float)j * num11);
			if (!flag3)
			{
				if (j == 0)
				{
					num13 += num10;
				}
				else if (j == num3)
				{
					num13 -= num10;
				}
			}
			Vector3 vector5 = vector3 + transform.rightManaged * num13;
			Vector3 vector6 = vector4 + transform2.leftManaged * num13;
			for (int k = 0; k <= num4; k++)
			{
				float num14 = shape.height * ((float)k * num12);
				Vector3 vector7 = vector5 + transform.downManaged * num14;
				Vector3 vector8 = vector6 + transform2.downManaged * num14;
				Vector3 vector9 = vector;
				if (flag)
				{
					vector9 += (vector7 - transform.centerManaged).normalized * 0.5f;
				}
				Vector3 vector10 = vector7 + vector9;
				Vector3 vector11 = vector2;
				if (flag2)
				{
					vector11 += (vector8 - transform2.centerManaged).normalized * 0.5f;
				}
				Vector3 vector12 = vector8 + vector11;
				float num15 = Mathf.Min(shape.width * num11, shape.height * num12) / 2f;
				num15 += portalObject.additionalSampleThreshold;
				bool num16 = NavMesh.SamplePosition(vector10, ref val2, num15, -1);
				bool flag4 = NavMesh.SamplePosition(vector12, ref val3, num15, -1);
				bool flag5 = !num16 || !flag4;
				NavMeshLinkData val4 = default(NavMeshLinkData);
				((NavMeshLinkData)(ref val4)).startPosition = ((NavMeshHit)(ref val2)).position;
				((NavMeshLinkData)(ref val4)).endPosition = ((NavMeshHit)(ref val3)).position;
				((NavMeshLinkData)(ref val4)).costModifier = 0f;
				((NavMeshLinkData)(ref val4)).bidirectional = false;
				NavMeshLinkData val5 = val4;
				for (int l = 0; l < span2.Length; l++)
				{
					int index = l * num5 + (j * (num4 + 1) + k);
					int agentTypeID = span2[l];
					ref NavMeshLinkData reference = ref span3[index];
					((NavMeshLinkData)(ref val5)).agentTypeID = agentTypeID;
					reference = val5;
					ref NavMeshLinkInstance reference2 = ref span5[index];
					if (!flag5)
					{
						int num17 = NavMeshExtensions.AddLinkInternal_Injected(ref reference, ref zero, ref identity);
						NavMeshLinkInstanceExtensions.SetID(ref reference2, num17);
						((NavMeshLinkInstance)(ref reference2)).owner = identifier;
					}
					span4[index] = vector7;
				}
			}
		}
		if (array4 != null)
		{
			ArrayPool<int>.Shared.Return(array4);
		}
		identifier.SetLinks(array, array2, array3);
	}

	public void Sync(PortalScene scene)
	{
		List<PortalIdentifier> identifiers = scene.identifiers;
		int count = identifiers.Count;
		for (int i = 0; i < count; i++)
		{
			PortalIdentifier portalIdentifier = identifiers[i];
			_ = portalIdentifier.Handle;
			if (portalIdentifier.UpdateDirtiness() && portalIdentifier.CheckUpdateDiff())
			{
				portalIdentifier.QueueLinksForRemoval();
				GenerateLinks(scene, portalIdentifier);
			}
		}
	}

	public void RemoveQueuedLinks(PortalScene scene)
	{
		linkRemovalTimer += Time.deltaTime;
		if (!(linkRemovalTimer < 0.3f))
		{
			linkRemovalTimer = 0f;
			List<PortalIdentifier> identifiers = scene.identifiers;
			int count = identifiers.Count;
			for (int i = 0; i < count; i++)
			{
				identifiers[i].RemoveQueuedLinks();
			}
		}
	}
}
