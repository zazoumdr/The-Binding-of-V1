using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[DisallowMultipleComponent]
public sealed class VirtualAudioFilter : MonoBehaviour
{
	public struct FilterData
	{
		public float gainL;

		public float gainR;

		public float weight;

		public AudioRolloffMode rolloffMode;

		public NativeCurve customRolloffCurve;

		public int consumedCounter;

		internal float maxDistance;

		internal float minDistance;

		internal float spatialBlend;

		internal float closestDistance;

		internal float3 closestPosition;
	}

	internal delegate bool UpdateVelocityBurst_0000223A_0024PostfixBurstDelegate(ref float3 lastPosition, ref float _dopplerPitch, float sourceDopplerLevel, in float3 currentPosition, in float3 rbVelocity, in float3 closestPosition, in float3 lastListenerPosition, in float3 listenerVelocity, float pitch, float dopplerFactor, float deltaTime, float spatialBlend, bool hasRigidBody, out float sourcePitch);

	internal static class UpdateVelocityBurst_0000223A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(UpdateVelocityBurst_0000223A_0024PostfixBurstDelegate).TypeHandle);
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

		static UpdateVelocityBurst_0000223A_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static bool Invoke(ref float3 lastPosition, ref float _dopplerPitch, float sourceDopplerLevel, in float3 currentPosition, in float3 rbVelocity, in float3 closestPosition, in float3 lastListenerPosition, in float3 listenerVelocity, float pitch, float dopplerFactor, float deltaTime, float spatialBlend, bool hasRigidBody, out float sourcePitch)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float, float, ref float3, ref float3, ref float3, ref float3, ref float3, float, float, float, float, bool, ref float, bool>)functionPointer)(ref lastPosition, ref _dopplerPitch, sourceDopplerLevel, ref currentPosition, ref rbVelocity, ref closestPosition, ref lastListenerPosition, ref listenerVelocity, pitch, dopplerFactor, deltaTime, spatialBlend, hasRigidBody, ref sourcePitch);
				}
			}
			return UpdateVelocityBurst_0024BurstManaged(ref lastPosition, ref _dopplerPitch, sourceDopplerLevel, in currentPosition, in rbVelocity, in closestPosition, in lastListenerPosition, in listenerVelocity, pitch, dopplerFactor, deltaTime, spatialBlend, hasRigidBody, out sourcePitch);
		}
	}

	internal delegate void AddOutputBurst_0000223F_0024PostfixBurstDelegate(ref FilterData filter, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, float initialDistance, float spatialBlend);

	internal static class AddOutputBurst_0000223F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(AddOutputBurst_0000223F_0024PostfixBurstDelegate).TypeHandle);
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

		static AddOutputBurst_0000223F_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref FilterData filter, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, float initialDistance, float spatialBlend)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref FilterData, ref float3, ref float3, ref float3, float, float, void>)functionPointer)(ref filter, ref listenerPosition, ref listenerRight, ref trackedPos, initialDistance, spatialBlend);
					return;
				}
			}
			AddOutputBurst_0024BurstManaged(ref filter, in listenerPosition, in listenerRight, in trackedPos, initialDistance, spatialBlend);
		}
	}

	internal unsafe delegate void ProcessStereo_00002242_0024PostfixBurstDelegate(float* data, int sampleCount, float gainL, float gainR);

	internal static class ProcessStereo_00002242_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(ProcessStereo_00002242_0024PostfixBurstDelegate).TypeHandle);
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

		static ProcessStereo_00002242_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(float* data, int sampleCount, float gainL, float gainR)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float*, int, float, float, void>)functionPointer)(data, sampleCount, gainL, gainR);
					return;
				}
			}
			ProcessStereo_0024BurstManaged(data, sampleCount, gainL, gainR);
		}
	}

	private const float SpeedOfSound = 340f;

	private const float InvSpeedOfSound = 0.0029411765f;

	private AudioSource _source;

	private ulong _gain;

	[SerializeField]
	[HideInInspector]
	private bool _ranAwake;

	[Range(-3f, 3f)]
	[SerializeField]
	private float _pitch = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _spatialBlend;

	private float _dopplerPitch = 1f;

	private Vector3 _lastPosition;

	private int _updateIndex;

	private Rigidbody _rigidBody;

	private bool _hasRigidBody;

	internal NativeCurve customRolloffCurve;

	public FilterData filterData;

	internal int trackedIndex { get; set; } = -1;

	public AudioSource source
	{
		get
		{
			if (this != null && _source == null)
			{
				_source = GetComponent<AudioSource>();
			}
			return _source;
		}
	}

	public float spatialBlend
	{
		get
		{
			if (!base.enabled)
			{
				return source.spatialBlend;
			}
			return _spatialBlend;
		}
		set
		{
			_spatialBlend = Mathf.Clamp01(value);
			if (base.enabled)
			{
				source.spatialBlend = value;
			}
		}
	}

	public float pitch
	{
		get
		{
			if (!base.enabled)
			{
				return source.pitch;
			}
			return _pitch;
		}
		set
		{
			if (float.IsInfinity(value))
			{
				Debug.LogError("Attempt to set pitch to infinite value ignored!", this);
				return;
			}
			if (float.IsNaN(value))
			{
				Debug.LogError("Attempt to set pitch to NaN value ignored!", this);
				return;
			}
			_pitch = value;
			source.pitch = (base.enabled ? (value * _dopplerPitch) : value);
		}
	}

	public void UpdateCachedValues(int updateIndex)
	{
		if (_updateIndex != updateIndex)
		{
			_updateIndex = updateIndex;
			if (_rigidBody == null)
			{
				_hasRigidBody = TryGetComponent<Rigidbody>(out _rigidBody);
			}
		}
	}

	private void Awake()
	{
		_source = GetComponent<AudioSource>();
		if (_ranAwake)
		{
			ResetAudioSource();
			MonoSingleton<VirtualAudioManager>.Instance.AddAudioSource(this);
		}
		_ranAwake = true;
	}

	private void OnEnable()
	{
		if ((UnityEngine.Object)(object)_source == null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		_pitch = _source.pitch;
		_spatialBlend = _source.spatialBlend;
		_lastPosition = ((Component)(object)_source).transform.position;
		_source.spatialBlend = 0f;
		_source.bypassEffects = false;
		UpdateFilterData();
	}

	private void OnDisable()
	{
		Volatile.Write(ref _gain, 0uL);
		ResetAudioSource();
	}

	private void ResetAudioSource()
	{
		if ((UnityEngine.Object)(object)_source != null)
		{
			_source.spatialBlend = _spatialBlend;
			_source.pitch = _pitch;
			UpdateFilterData();
		}
	}

	internal void UpdateVelocity(ref Vector3 lastListenerPosition, ref Vector3 listenerVelocity, float dopplerFactor, float deltaTime)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = (_hasRigidBody ? _rigidBody.velocity : Vector3.zero);
		if (UpdateVelocityBurst(ref Unsafe.As<Vector3, float3>(ref _lastPosition), ref _dopplerPitch, _source.dopplerLevel, in Unsafe.As<Vector3, float3>(ref position), in Unsafe.As<Vector3, float3>(ref vector), in filterData.closestPosition, in Unsafe.As<Vector3, float3>(ref lastListenerPosition), in Unsafe.As<Vector3, float3>(ref listenerVelocity), _pitch, dopplerFactor, deltaTime, _spatialBlend, _hasRigidBody, out var sourcePitch) && base.enabled)
		{
			_source.pitch = sourcePitch;
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static bool UpdateVelocityBurst(ref float3 lastPosition, ref float _dopplerPitch, float sourceDopplerLevel, in float3 currentPosition, in float3 rbVelocity, in float3 closestPosition, in float3 lastListenerPosition, in float3 listenerVelocity, float pitch, float dopplerFactor, float deltaTime, float spatialBlend, bool hasRigidBody, out float sourcePitch)
	{
		return UpdateVelocityBurst_0000223A_0024BurstDirectCall.Invoke(ref lastPosition, ref _dopplerPitch, sourceDopplerLevel, in currentPosition, in rbVelocity, in closestPosition, in lastListenerPosition, in listenerVelocity, pitch, dopplerFactor, deltaTime, spatialBlend, hasRigidBody, out sourcePitch);
	}

	private static float3 BurstGetPositionDelta(in float3 currentPosition, in float3 lastPosition, in float deltaTime)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = ((deltaTime > 0f) ? deltaTime : 1f);
		return (lastPosition - currentPosition) / num;
	}

	internal void UpdateFilterData()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		filterData.gainL = 0f;
		filterData.gainR = 0f;
		filterData.weight = 0f;
		filterData.rolloffMode = _source.rolloffMode;
		if ((int)filterData.rolloffMode == 2)
		{
			filterData.customRolloffCurve.Update(_source.GetCustomCurve((AudioSourceCurveType)0), 32);
		}
		filterData.consumedCounter = 0;
		filterData.maxDistance = _source.maxDistance;
		filterData.minDistance = _source.minDistance;
		filterData.spatialBlend = _spatialBlend;
		filterData.closestDistance = float.PositiveInfinity;
	}

	internal void AddOutput(Vector3 listenerPosition, Vector3 listenerRight, Vector3 position, float initialDistance)
	{
		AddOutput(listenerPosition, listenerRight, position, initialDistance, filterData.spatialBlend);
	}

	internal void AddOutput(Vector3 listenerPosition, Vector3 listenerRight, Vector3 position, float initialDistance, float spatialBlend)
	{
		if (filterData.spatialBlend == 0f)
		{
			filterData.weight = 1f;
			filterData.gainL = 0.5f;
			filterData.gainR = 0.5f;
		}
		else
		{
			AddOutputBurst(ref filterData, in Unsafe.As<Vector3, float3>(ref listenerPosition), in Unsafe.As<Vector3, float3>(ref listenerRight), in Unsafe.As<Vector3, float3>(ref position), initialDistance, spatialBlend);
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private static void AddOutputBurst(ref FilterData filter, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, float initialDistance, float spatialBlend)
	{
		AddOutputBurst_0000223F_0024BurstDirectCall.Invoke(ref filter, in listenerPosition, in listenerRight, in trackedPos, initialDistance, spatialBlend);
	}

	internal void EndUpdate()
	{
		if (filterData.weight > 1f)
		{
			filterData.gainL /= filterData.weight;
			filterData.gainR /= filterData.weight;
		}
		float num = Mathf.InverseLerp(0f, 0.01f, Mathf.Abs(_pitch * _dopplerPitch));
		filterData.gainL *= num;
		filterData.gainR *= num;
		Vector2 vector = Vector2.ClampMagnitude(new Vector2(filterData.gainL, filterData.gainR), 1f);
		if (float.IsFinite(vector.x) && float.IsFinite(vector.y))
		{
			Volatile.Write(ref _gain, (uint)BitConverter.SingleToInt32Bits(vector.x) | ((ulong)(uint)BitConverter.SingleToInt32Bits(vector.y) << 32));
		}
	}

	private unsafe void OnAudioFilterRead(float[] data, int channels)
	{
		if (channels == 2 && _spatialBlend != 0f)
		{
			fixed (float* data2 = data)
			{
				ulong num = Volatile.Read(ref _gain);
				float gainL = BitConverter.Int32BitsToSingle((int)num);
				float gainR = BitConverter.Int32BitsToSingle((int)(num >> 32));
				ProcessStereo(data2, data.Length / 2, gainL, gainR);
			}
		}
	}

	[BurstCompile]
	private unsafe static void ProcessStereo(float* data, int sampleCount, float gainL, float gainR)
	{
		ProcessStereo_00002242_0024BurstDirectCall.Invoke(data, sampleCount, gainL, gainR);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static bool UpdateVelocityBurst_0024BurstManaged(ref float3 lastPosition, ref float _dopplerPitch, float sourceDopplerLevel, in float3 currentPosition, in float3 rbVelocity, in float3 closestPosition, in float3 lastListenerPosition, in float3 listenerVelocity, float pitch, float dopplerFactor, float deltaTime, float spatialBlend, bool hasRigidBody, out float sourcePitch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float3 currentPosition2 = (lastPosition = currentPosition);
		if (math.lengthsq(lastPosition - currentPosition) > 10000f)
		{
			sourcePitch = 0f;
			return false;
		}
		float num = 1f;
		float3 val = (hasRigidBody ? rbVelocity : BurstGetPositionDelta(in currentPosition2, in lastPosition, in deltaTime));
		float3 val2 = closestPosition - lastListenerPosition;
		float3 val3 = val - listenerVelocity;
		if (sourceDopplerLevel > 0f && spatialBlend > 0f)
		{
			float num2 = dopplerFactor * sourceDopplerLevel;
			float num3 = math.length(val2);
			float num4 = ((num3 > 0f) ? (math.dot(val3, val2) / num3) : 0f);
			num = math.max(1E-06f, (340f - num4 * num2) * 0.0029411765f) * spatialBlend + (1f - spatialBlend);
		}
		if (math.isinf(num) || math.isnan(num) || math.abs(num) <= 0.1f)
		{
			sourcePitch = 0f;
			return false;
		}
		if (math.abs(_dopplerPitch - num) > 2f)
		{
			float num5 = 2f * deltaTime;
			if (math.abs(num - _dopplerPitch) <= num5)
			{
				_dopplerPitch += math.sign(num - _dopplerPitch) * num5;
			}
		}
		else
		{
			_dopplerPitch = num;
		}
		sourcePitch = pitch * _dopplerPitch;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static void AddOutputBurst_0024BurstManaged(ref FilterData filter, in float3 listenerPosition, in float3 listenerRight, in float3 trackedPos, float initialDistance, float spatialBlend)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		float3 val = trackedPos - listenerPosition;
		float num = math.length(val);
		float num2 = initialDistance + num;
		if (num2 < filter.closestDistance)
		{
			filter.closestDistance = num2;
			filter.closestPosition = trackedPos;
		}
		float3 val2 = math.normalize(val);
		float num3 = (((int)filter.rolloffMode == 1) ? math.saturate(math.unlerp(filter.maxDistance, filter.minDistance, num2)) : (((int)filter.rolloffMode != 0) ? ((filter.maxDistance > 0f) ? filter.customRolloffCurve.Evaluate(num2 / filter.maxDistance) : 1f) : (filter.minDistance / math.max(num2, 1E-06f))));
		num3 = math.lerp(1f, num3, spatialBlend);
		float num4 = math.dot(val2, listenerRight);
		float num5 = num3 * math.sqrt(0.5f * (1f - num4));
		float num6 = num3 * math.sqrt(0.5f * (1f + num4));
		if (math.isfinite(num5 + num6))
		{
			filter.gainL += num5;
			filter.gainR += num6;
			filter.weight += num3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void ProcessStereo_0024BurstManaged(float* data, int sampleCount, float gainL, float gainR)
	{
		for (int i = 0; i < sampleCount; i++)
		{
			float num = data[2 * i] + data[2 * i + 1];
			data[2 * i] = num * gainL;
			data[2 * i + 1] = num * gainR;
		}
	}
}
