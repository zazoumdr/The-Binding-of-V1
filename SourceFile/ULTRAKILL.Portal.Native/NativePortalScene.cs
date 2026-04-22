using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ULTRAKILL.Portal.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal.Native;

[BurstCompile]
public struct NativePortalScene : IDisposable
{
	[MarshalAs(UnmanagedType.U1)]
	public bool valid;

	private UnsafeHashMap<long, int> handleToIndex;

	public NativeList<NativePortal> portals;

	public NativeList<NativePortal> renderPortals;

	private NativePortal invalidPortal;

	public unsafe ref NativePortal LookupPortal(in PortalHandle handle)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = default(int);
		if (portals.IsCreated && handleToIndex.TryGetValue(handle.PackedKey, ref num))
		{
			return ref NativeListUnsafeUtility.GetUnsafeReadOnlyPtr<NativePortal>(portals)[num];
		}
		fixed (NativePortal* ptr = &invalidPortal)
		{
			return ref *ptr;
		}
	}

	public static NativePortalScene Create(List<Portal> portalComponents, List<PortalIdentifier> identifiers, Dictionary<PortalHandle, PortalIdentifier> portalIdentifiersLookup)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		int count = portalComponents.Count;
		NativePortalScene result = new NativePortalScene
		{
			portals = new NativeList<NativePortal>(count * 2, AllocatorHandle.op_Implicit(Allocator.TempJob)),
			renderPortals = new NativeList<NativePortal>(count * 2, AllocatorHandle.op_Implicit(Allocator.TempJob)),
			handleToIndex = new UnsafeHashMap<long, int>(count * 2, AllocatorHandle.op_Implicit(Allocator.TempJob))
		};
		result.invalidPortal = new NativePortal
		{
			valid = false
		};
		Vector3 position = MonoSingleton<NewMovement>.Instance.transform.position;
		int count2 = portalComponents.Count;
		for (int i = 0; i < count2; i++)
		{
			Portal portal = portalComponents[i];
			if (!portal || !portal.isActiveAndEnabled || !portal.entry || !portal.entry.gameObject.activeInHierarchy || !portal.exit || !portal.exit.gameObject.activeInHierarchy)
			{
				continue;
			}
			if (portal.disableRange > 0f)
			{
				float num = portal.disableRange * portal.disableRange;
				if (Vector3.SqrMagnitude(position - portal.entry.position) > num && Vector3.SqrMagnitude(position - portal.exit.position) > num)
				{
					continue;
				}
			}
			int instanceID = portal.GetInstanceID();
			PlaneShape plane = portal.GetShape();
			NativePortal enter = default(NativePortal);
			NativePortal exit = default(NativePortal);
			PortalHandle enterHandle = new PortalHandle(instanceID, PortalSide.Enter);
			PortalHandle exitHandle = new PortalHandle(instanceID, PortalSide.Exit);
			portal.entry.GetPositionAndRotation(out var position2, out var rotation);
			Vector3 position3;
			Quaternion rotation2;
			if (portal.mirror)
			{
				position3 = position2;
				rotation2 = rotation;
			}
			else
			{
				portal.exit.GetPositionAndRotation(out position3, out rotation2);
			}
			NativePortalExtensions.CalculateData(ref enter, ref exit, in enterHandle, in exitHandle, in plane, in Unsafe.As<Vector3, float3>(ref position2), in Unsafe.As<Quaternion, quaternion>(ref rotation), in Unsafe.As<Vector3, float3>(ref position3), in Unsafe.As<Quaternion, quaternion>(ref rotation2));
			enter.renderData = new NativePortalRenderData
			{
				overrideFog = portal.enableOverrideFog,
				fogData = new FogData
				{
					useFog = (portal.useFogEnter ? 1 : 0),
					fogColor = portal.overrideFogColorEnter,
					fogStart = portal.overrideFogStartEnter,
					fogEnd = portal.overrideFogEndEnter
				},
				canSeeItself = portal.canSeeItself,
				canSeePortalLayer = portal.canSeePortalLayer,
				maxRecursions = portal.maxRecursions,
				appearsInRecursions = portal.appearsInRecursions,
				mirror = portal.mirror,
				renderSettings = portal.renderSettings,
				clippingMethod = portal.clippingMethod,
				supportsInfiniteRecursion = portal.supportInfiniteRecursion
			};
			if (portal.mirror)
			{
				result.renderPortals.Add(ref enter);
				continue;
			}
			enter.travellerFlags = portal.GetTravelFlags(PortalSide.Enter);
			enter.audioData = new NativePortalAudioData
			{
				listenerMode = (portal.consumeAudio ? AudioListenerMode.Consume : AudioListenerMode.Listen),
				audioVelocityUpdateMode = (AudioVelocityUpdateMode)0,
				lastPosition = position2,
				velocity = Vector3.zero,
				travelMatrix = float4x4.identity,
				updateIndex = 0
			};
			exit.renderData = new NativePortalRenderData
			{
				overrideFog = portal.enableOverrideFog,
				fogData = new FogData
				{
					useFog = (portal.useFogExit ? 1 : 0),
					fogColor = portal.overrideFogColorExit,
					fogStart = portal.overrideFogStartExit,
					fogEnd = portal.overrideFogEndExit
				},
				canSeeItself = portal.canSeeItself,
				canSeePortalLayer = portal.canSeePortalLayer,
				maxRecursions = portal.maxRecursions,
				appearsInRecursions = portal.appearsInRecursions,
				mirror = portal.mirror,
				renderSettings = portal.renderSettings,
				clippingMethod = portal.clippingMethod,
				supportsInfiniteRecursion = portal.supportInfiniteRecursion
			};
			exit.travellerFlags = portal.GetTravelFlags(PortalSide.Exit);
			exit.audioData = new NativePortalAudioData
			{
				listenerMode = (portal.consumeAudio ? AudioListenerMode.Consume : AudioListenerMode.Listen),
				audioVelocityUpdateMode = (AudioVelocityUpdateMode)0,
				lastPosition = position3,
				velocity = Vector3.zero,
				travelMatrix = float4x4.identity,
				updateIndex = 0
			};
			PortalIdentifier orAddComponent = portal.entry.GetOrAddComponent<PortalIdentifier>();
			orAddComponent.Handle = enterHandle;
			orAddComponent.isTraversable = portal.GetTravelFlags(PortalSide.Enter).HasAllFlags(PortalTravellerFlags.Enemy);
			portalIdentifiersLookup[enterHandle] = orAddComponent;
			identifiers.Add(orAddComponent);
			PortalIdentifier orAddComponent2 = portal.exit.GetOrAddComponent<PortalIdentifier>();
			orAddComponent2.Handle = exitHandle;
			orAddComponent2.isTraversable = !portal.mirror && portal.GetTravelFlags(PortalSide.Exit).HasAllFlags(PortalTravellerFlags.Enemy);
			portalIdentifiersLookup[exitHandle] = orAddComponent2;
			identifiers.Add(orAddComponent2);
			int length = result.portals.Length;
			result.handleToIndex[enterHandle.PackedKey] = length;
			result.portals.Add(ref enter);
			result.renderPortals.Add(ref enter);
			result.handleToIndex[exitHandle.PackedKey] = length + 1;
			result.portals.Add(ref exit);
			result.renderPortals.Add(ref exit);
		}
		result.valid = true;
		return result;
	}

	public void Recalculate(List<Portal> components)
	{
		for (int i = 0; i < components.Count; i++)
		{
			Portal portal = components[i];
			int instanceID = portal.GetInstanceID();
			PortalHandle handle = new PortalHandle(instanceID, PortalSide.Enter);
			PortalHandle handle2 = new PortalHandle(instanceID, PortalSide.Exit);
			ref NativePortal reference = ref LookupPortal(in handle);
			if (reference.valid)
			{
				ref NativePortal reference2 = ref LookupPortal(in handle2);
				if (reference2.valid)
				{
					PlaneShape plane = portal.GetShape();
					portal.entry.GetPositionAndRotation(out var position, out var rotation);
					portal.exit.GetPositionAndRotation(out var position2, out var rotation2);
					NativePortalExtensions.CalculateData(ref reference, ref reference2, in handle, in handle2, in plane, in Unsafe.As<Vector3, float3>(ref position), in Unsafe.As<Quaternion, quaternion>(ref rotation), in Unsafe.As<Vector3, float3>(ref position2), in Unsafe.As<Quaternion, quaternion>(ref rotation2));
				}
			}
		}
	}

	public void Dispose()
	{
		valid = false;
		if (portals.IsCreated)
		{
			portals.Dispose();
		}
		if (handleToIndex.IsCreated)
		{
			handleToIndex.Dispose();
		}
		if (renderPortals.IsCreated)
		{
			renderPortals.Dispose();
		}
	}
}
