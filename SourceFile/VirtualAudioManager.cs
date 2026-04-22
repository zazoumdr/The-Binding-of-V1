using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[DisallowMultipleComponent]
[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public sealed class VirtualAudioManager : MonoSingleton<VirtualAudioManager>
{
	internal unsafe delegate int LoopOverListeners_00002261_0024PostfixBurstDelegate(NativePortal* portals, int portalCount, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, ref VirtualAudioFilter.FilterData filter);

	internal static class LoopOverListeners_00002261_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(LoopOverListeners_00002261_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static LoopOverListeners_00002261_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static int Invoke(NativePortal* portals, int portalCount, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, ref VirtualAudioFilter.FilterData filter)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<NativePortal*, int, ref float3, ref float3, ref float3, ref VirtualAudioFilter.FilterData, int>)functionPointer)(portals, portalCount, ref listenerPosition, ref listenerRight, ref trackedPos, ref filter);
				}
			}
			return LoopOverListeners_0024BurstManaged(portals, portalCount, in listenerPosition, in listenerRight, in trackedPos, ref filter);
		}
	}

	[SerializeField]
	private AudioListener m_Listener;

	[SerializeField]
	private Vector3 m_LastListenerPosition;

	[SerializeField]
	private Vector3 m_ListenerVelocity;

	[Min(0f)]
	[SerializeField]
	private float m_DopplerFactor = 1f;

	private int m_UpdateCount;

	private NewMovement m_Player;

	private readonly List<VirtualAudioFilter> m_Sources = new List<VirtualAudioFilter>();

	private readonly List<VirtualAudioListener> m_Listeners = new List<VirtualAudioListener>();

	private readonly HashSet<AudioSource> m_PlayOnAwake = new HashSet<AudioSource>(ReferenceEqualityComparer.Instance);

	public float dopplerFactor
	{
		get
		{
			return m_DopplerFactor;
		}
		set
		{
			m_DopplerFactor = Mathf.Max(0f, value);
		}
	}

	public Vector3 lastListenerPosition => m_LastListenerPosition;

	public Vector3 listenerVelocity => m_ListenerVelocity;

	private void OnDisable()
	{
		VirtualAudioFilter[] array = UnityEngine.Object.FindObjectsByType<VirtualAudioFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (VirtualAudioFilter obj in array)
		{
			obj.trackedIndex = -1;
			UnityEngine.Object.Destroy(obj);
		}
		m_Sources.Clear();
		m_PlayOnAwake.Clear();
	}

	private void FixedUpdate()
	{
		DoUpdate((AudioVelocityUpdateMode)1, m_UpdateCount++);
	}

	private void Update()
	{
		UpdatePlayOnAwakeSources();
		DoUpdate((AudioVelocityUpdateMode)2, m_UpdateCount++);
	}

	private void DoUpdate(AudioVelocityUpdateMode velocityUpdateMode, int updateIndex)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		AudioListener audioListener = GetAudioListener();
		UpdateAudioListener(audioListener, velocityUpdateMode);
		UpdateSources(velocityUpdateMode, updateIndex, audioListener);
	}

	private void UpdateAudioListener(AudioListener listener, AudioVelocityUpdateMode velocityUpdateMode)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)(UnityEngine.Object)(object)listener)
		{
			if ((int)listener.velocityUpdateMode == 0)
			{
				listener.velocityUpdateMode = GetAutomaticUpdateMode(((UnityEngine.Component)(object)listener).gameObject);
			}
			if (listener.velocityUpdateMode == velocityUpdateMode)
			{
				Vector3 position = ((UnityEngine.Component)(object)listener).transform.position;
				m_ListenerVelocity = GetPositionDelta(position, m_LastListenerPosition);
				m_LastListenerPosition = position;
			}
		}
	}

	private unsafe void UpdateSources(AudioVelocityUpdateMode velocityUpdateMode, int updateIndex, AudioListener audioListener)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Invalid comparison between Unknown and I4
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		int num = m_Sources.Count;
		Span<VirtualAudioFilter> span = CollectionsMarshal.AsSpan(m_Sources);
		Transform obj = ((UnityEngine.Component)(object)audioListener).transform;
		Vector3 source = obj.position;
		Vector3 source2 = obj.right;
		bool flag = true;
		if (m_Player == null)
		{
			m_Player = MonoSingleton<NewMovement>.Instance;
		}
		if (m_Player != null)
		{
			flag = Time.frameCount > m_Player.lastTraversalFrame + 1;
		}
		float deltaTime = Time.deltaTime;
		PortalManagerV2 portalManagerV = MonoSingleton<PortalManagerV2>.Instance;
		if (portalManagerV == null || portalManagerV.Scene == null)
		{
			return;
		}
		NativeList<NativePortal> portals = portalManagerV.Scene.nativeScene.portals;
		if (!portals.IsCreated)
		{
			return;
		}
		Span<NativePortal> span2 = portals.AsArray().AsSpan();
		int length = span2.Length;
		float3 listenerPosition = Unsafe.As<Vector3, float3>(ref source);
		float3 listenerRight = Unsafe.As<Vector3, float3>(ref source2);
		fixed (NativePortal* portals2 = span2)
		{
			for (int i = 0; i < num; i++)
			{
				VirtualAudioFilter virtualAudioFilter = span[i];
				AudioSource source3 = virtualAudioFilter.source;
				if (!(UnityEngine.Object)(object)source3 || !virtualAudioFilter)
				{
					span[i] = span[--num];
					span[i].trackedIndex = i--;
					virtualAudioFilter.trackedIndex = -1;
					UnityEngine.Object.Destroy(virtualAudioFilter);
					continue;
				}
				if (!((Behaviour)(object)source3).isActiveAndEnabled || (!source3.isPlaying && !source3.IsPaused() && !source3.isVirtual))
				{
					if (source3.playOnAwake)
					{
						m_PlayOnAwake.Add(source3);
					}
					span[i] = span[--num];
					span[i].trackedIndex = i--;
					virtualAudioFilter.trackedIndex = -1;
					virtualAudioFilter.enabled = false;
					continue;
				}
				virtualAudioFilter.UpdateCachedValues(updateIndex);
				if ((int)source3.velocityUpdateMode == 0)
				{
					source3.velocityUpdateMode = GetAutomaticUpdateMode(((UnityEngine.Component)(object)source3).gameObject);
				}
				if ((int)velocityUpdateMode == 2)
				{
					virtualAudioFilter.UpdateFilterData();
					Vector3 source4 = ((UnityEngine.Component)(object)source3).transform.position;
					if (virtualAudioFilter.spatialBlend > 0f)
					{
						if (LoopOverListeners(portals2, length, in listenerPosition, in listenerRight, in Unsafe.As<Vector3, float3>(ref source4), ref virtualAudioFilter.filterData) == 0)
						{
							virtualAudioFilter.AddOutput(source, source2, source4, 0f);
						}
					}
					else
					{
						virtualAudioFilter.AddOutput(source, source2, source4, 0f, 0f);
					}
					virtualAudioFilter.EndUpdate();
				}
				if (flag && source3.velocityUpdateMode == velocityUpdateMode)
				{
					virtualAudioFilter.UpdateVelocity(ref m_LastListenerPosition, ref m_ListenerVelocity, m_DopplerFactor, deltaTime);
				}
			}
		}
		CollectionsMarshal.SetCount<VirtualAudioFilter>(m_Sources, num);
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private unsafe static int LoopOverListeners(NativePortal* portals, int portalCount, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, ref VirtualAudioFilter.FilterData filter)
	{
		return LoopOverListeners_00002261_0024BurstDirectCall.Invoke(portals, portalCount, in listenerPosition, in listenerRight, in trackedPos, ref filter);
	}

	private void UpdatePlayOnAwakeSources()
	{
		m_PlayOnAwake.RemoveWhere(delegate(AudioSource source)
		{
			if ((UnityEngine.Object)(object)source == null)
			{
				return true;
			}
			if (!((UnityEngine.Component)(object)source).gameObject.activeInHierarchy)
			{
				((UnityEngine.Component)(object)source).GetOrAddComponent<PlayOnAwakeTracker>();
				return true;
			}
			if (!source.playOnAwake)
			{
				return true;
			}
			if (((Behaviour)(object)source).isActiveAndEnabled && source.isPlaying)
			{
				MonoSingleton<VirtualAudioManager>.Instance.AddAudioSource(source);
				if (source.playOnAwake)
				{
					return true;
				}
			}
			return false;
		});
	}

	public AudioListener GetAudioListener()
	{
		if ((bool)(UnityEngine.Object)(object)m_Listener && ((Behaviour)(object)m_Listener).isActiveAndEnabled)
		{
			return m_Listener;
		}
		m_Listener = UnityEngine.Object.FindAnyObjectByType<AudioListener>(FindObjectsInactive.Exclude);
		if ((bool)(UnityEngine.Object)(object)m_Listener)
		{
			m_LastListenerPosition = ((UnityEngine.Component)(object)m_Listener).transform.position;
			m_ListenerVelocity = Vector3.zero;
		}
		return m_Listener;
	}

	public static AudioVelocityUpdateMode GetAutomaticUpdateMode(GameObject gameObject)
	{
		if (!gameObject.TryGetComponent<Rigidbody>(out var component))
		{
			Transform parent = gameObject.transform.parent;
			while (parent != null)
			{
				if (parent.TryGetComponent<Rigidbody>(out component))
				{
					return (AudioVelocityUpdateMode)1;
				}
				parent = parent.parent;
			}
			return (AudioVelocityUpdateMode)2;
		}
		return (AudioVelocityUpdateMode)1;
	}

	public static Vector3 GetPositionDelta(Vector3 pos, Vector3 lastPosition)
	{
		float num = ((Time.deltaTime > 0f) ? Time.deltaTime : 1f);
		return (lastPosition - pos) / num;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal void AddAudioListener(VirtualAudioListener listener)
	{
		if (listener.trackedIndex < 0)
		{
			listener.trackedIndex = m_Listeners.Count;
			m_Listeners.Add(listener);
		}
	}

	internal void AddAudioSource(AudioSource source)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!((Behaviour)(object)source).isActiveAndEnabled)
		{
			if (source.playOnAwake)
			{
				((UnityEngine.Component)(object)source).GetOrAddComponent<PlayOnAwakeTracker>();
			}
			return;
		}
		if (((UnityEngine.Component)(object)source).TryGetComponent(out VirtualAudioFilter component))
		{
			AddAudioSource(component);
			return;
		}
		int firstAudioFilterIndex = ((UnityEngine.Component)(object)source).gameObject.GetFirstAudioFilterIndex();
		component = ((UnityEngine.Component)(object)source).gameObject.AddComponent<VirtualAudioFilter>();
		if (firstAudioFilterIndex != -1)
		{
			while (component.GetComponentIndex() > firstAudioFilterIndex && component.MoveComponentUp())
			{
			}
		}
		AddAudioSource(component);
	}

	internal void AddAudioSource(VirtualAudioFilter filter)
	{
		if (filter.trackedIndex < 0 && !((UnityEngine.Object)(object)filter.source == null))
		{
			filter.trackedIndex = m_Sources.Count;
			m_Sources.Add(filter);
			filter.enabled = true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal unsafe static int LoopOverListeners_0024BurstManaged(NativePortal* portals, int portalCount, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, ref VirtualAudioFilter.FilterData filter)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		float num2 = filter.maxDistance * filter.maxDistance;
		for (int i = 0; i < portalCount; i++)
		{
			NativePortal nativePortal = portals[i];
			if (nativePortal.renderData.mirror)
			{
				continue;
			}
			PlaneShapeExtensions.GetClosestPoint(nativePortal.dimensions.x, nativePortal.dimensions.y, nativePortal.transform.center, nativePortal.transform.right, nativePortal.transform.up, nativePortal.transform.forward, in trackedPos, out var closest);
			float num3 = math.lengthsq(trackedPos - closest);
			if (num3 > num2)
			{
				continue;
			}
			float num4 = math.sqrt(num3);
			if (nativePortal.audioData.listenerMode == AudioListenerMode.Consume)
			{
				num++;
			}
			float3 val = math.transform(nativePortal.travelMatrix, closest);
			if (math.isfinite(val.x + val.y + val.z))
			{
				float3 val2 = val - listenerPosition;
				float num5 = math.length(val2);
				float num6 = num4 + num5;
				if (num6 < filter.closestDistance)
				{
					filter.closestDistance = num6;
					filter.closestPosition = val;
				}
				float3 val3 = val2 / (num5 + 1E-10f);
				float num7 = (((int)filter.rolloffMode == 1) ? math.saturate(math.unlerp(filter.maxDistance, filter.minDistance, num6)) : (((int)filter.rolloffMode != 0) ? filter.customRolloffCurve.Evaluate(num6 / filter.maxDistance) : (filter.minDistance / math.max(num6, 1E-06f))));
				num7 = math.lerp(1f, num7, filter.spatialBlend);
				float num8 = math.dot(val3, listenerRight);
				float num9 = num7 * math.sqrt(0.5f * (1f - num8));
				float num10 = num7 * math.sqrt(0.5f * (1f + num8));
				if (math.isfinite(num9 + num10))
				{
					filter.gainL += num9;
					filter.gainR += num10;
					filter.weight += num7;
				}
			}
		}
		return num;
	}
}
