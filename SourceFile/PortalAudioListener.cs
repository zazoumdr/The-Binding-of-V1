using System.Runtime.CompilerServices;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PortalIdentifier))]
public sealed class PortalAudioListener : VirtualAudioListener
{
	private PortalManagerV2 _portalManager;

	private PortalIdentifier _identifier;

	private float _width;

	private float _height;

	private float3 _center;

	private float3 _right;

	private float3 _up;

	private float3 _forward;

	private PortalTransform _transform;

	private void Awake()
	{
		_portalManager = MonoSingleton<PortalManagerV2>.Instance;
		_identifier = GetComponent<PortalIdentifier>();
	}

	protected override void UpdateCachedValuesCore()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (!_identifier)
		{
			_identifier = GetComponent<PortalIdentifier>();
		}
		NativePortalScene nativeScene = _portalManager.Scene.nativeScene;
		NativePortal nativePortal = nativeScene.LookupPortal(_identifier.Handle);
		if (nativePortal.valid)
		{
			float4x4 toWorld = nativePortal.transform.toWorld;
			_width = nativePortal.dimensions.x;
			_height = nativePortal.dimensions.y;
			_center = ((float4)(ref toWorld.c3)).xyz;
			_forward = ((float4)(ref toWorld.c2)).xyz;
			_up = ((float4)(ref toWorld.c1)).xyz;
			_right = ((float4)(ref toWorld.c0)).xyz;
		}
	}

	public override Vector3 GetInputPosition(Vector3 position)
	{
		PlaneShapeExtensions.GetClosestPoint(_width, _height, in _center, in _right, in _up, in _forward, in Unsafe.As<Vector3, float3>(ref position), out var closest);
		return Unsafe.As<float3, Vector3>(ref closest);
	}
}
