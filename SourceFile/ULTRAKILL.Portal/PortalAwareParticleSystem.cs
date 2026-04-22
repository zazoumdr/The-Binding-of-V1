using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public sealed class PortalAwareParticleSystem : MonoBehaviour
{
	private const ParticleSystemCustomData CustomDataStream = (ParticleSystemCustomData)0;

	public bool raycastCollision;

	public Bloodsplatter blood;

	public ParticleSystem _system;

	private PortalManagerV2 portalManager;

	public JobHandle responseHandle;

	public float4x4 toWorld;

	public float4x4 toLocal;

	public NativeArray<float4> trailPositions;

	private GCHandle gcHandle;

	private void OnParticleUpdateJobScheduled()
	{
	}

	private void Awake()
	{
		_system = GetComponent<ParticleSystem>();
		portalManager = MonoSingleton<PortalManagerV2>.Instance;
		if (!blood)
		{
			blood = GetComponentInChildren<Bloodsplatter>();
		}
	}

	private void OnDestroy()
	{
		responseHandle.Complete();
	}

	private void OnEnable()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		toWorld = float4x4.op_Implicit(Matrix4x4.TRS(((Component)(object)_system).transform.position, ((Component)(object)_system).transform.rotation, Vector3.one));
		Matrix4x4.Inverse3DAffine(float4x4.op_Implicit(toWorld), ref Unsafe.As<float4x4, Matrix4x4>(ref toLocal));
		portalManager.Particles.Register(this);
	}

	private void OnDisable()
	{
		portalManager.Particles.Deregister(this);
	}
}
