using System;
using System.Runtime.CompilerServices;
using Gravity;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

[BurstCompile]
public class Portal : MonoBehaviour
{
	public struct PortalBurstData
	{
		public unsafe Vector3* entryPos;

		public unsafe Vector3* entryFwd;

		public unsafe Vector3* exitPos;

		public unsafe Vector3* exitFwd;

		public unsafe Quaternion* entryRot;

		public unsafe Quaternion* exitRot;

		public float hW;

		public float hH;

		public bool infiniteRecursion;

		public unsafe float4x4* portalScale;

		public unsafe float4x4* portalScaleInv;

		public unsafe float4x4* outEntryToWorld;

		public unsafe float4x4* outEntryToLocal;

		public unsafe float4x4* outExitToWorld;

		public unsafe float4x4* outExitToLocal;

		public unsafe float4x4* outTravel;

		public unsafe float4x4* outTravelRev;

		public unsafe float4* outEntryPlane;

		public unsafe float4* outExitPlane;

		public unsafe float3* enterVerts;

		public unsafe float3* exitVerts;

		public unsafe float4* enterVertsVec4;

		public unsafe float4* exitVertsVec4;

		public unsafe float4x4* entryBase;

		public unsafe float4x4* exitBase;
	}

	internal unsafe delegate void UpdatePortalBurst_00002973_0024PostfixBurstDelegate([NoAlias] PortalBurstData* d);

	internal static class UpdatePortalBurst_00002973_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(UpdatePortalBurst_00002973_0024PostfixBurstDelegate).TypeHandle);
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

		static UpdatePortalBurst_00002973_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke([NoAlias] PortalBurstData* d)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<PortalBurstData*, void>)functionPointer)(d);
					return;
				}
			}
			UpdatePortalBurst_0024BurstManaged(d);
		}
	}

	public static readonly float4x4 PORTAL_SCALE = float4x4.op_Implicit(Matrix4x4.Scale(new Vector3(-1f, 1f, -1f)));

	public static readonly float4x4 PORTAL_SCALE_INV = float4x4.op_Implicit(Matrix4x4.Scale(new Vector3(-1f, 1f, -1f)).inverse);

	[Header("Shape")]
	[SerializeReference]
	public IPortalShape shape;

	[Header("Linked Portals")]
	public Transform entry;

	public bool usePerceivedGravityOnEnter;

	public bool forceOrthogonalGravityOnEnter;

	public GravityVolume enterGravityVolume;

	[Space]
	public Transform exit;

	public bool usePerceivedGravityOnExit;

	public bool forceOrthogonalGravityOnExit;

	public GravityVolume exitGravityVolume;

	public float disableRange;

	[Header("Events")]
	public UnityEventPortalTravel onEntryTravel;

	public UnityEventPortalTravel onExitTravel;

	[Header("Settings")]
	public PortalTravellerFlags entryTravelFlags = PortalTravellerFlags.Player | PortalTravellerFlags.PlayerProjectile | PortalTravellerFlags.Enemy | PortalTravellerFlags.EnemyProjectile | PortalTravellerFlags.Other;

	public PortalTravellerFlags exitTravelFlags = PortalTravellerFlags.Player | PortalTravellerFlags.PlayerProjectile | PortalTravellerFlags.Enemy | PortalTravellerFlags.EnemyProjectile | PortalTravellerFlags.Other;

	public bool passThroughNonTraversals = true;

	public const float DEFAULT_OFFSET = 1.5f;

	public bool overrideLinkOffset;

	public float enterOffset = 1.5f;

	public float exitOffset = 1.5f;

	public float additionalSampleThreshold;

	public bool isMultiPanel;

	[Space]
	public PortalClippingMethod clippingMethod;

	public PortalSideFlags renderSettings = PortalSideFlags.Enter | PortalSideFlags.Exit;

	public bool mirror;

	public bool appearsInRecursions = true;

	public bool canSeeItself = true;

	public bool canSeePortalLayer = true;

	public bool allowCameraTraversals;

	public bool canHearAudio = true;

	public bool consumeAudio;

	public bool supportInfiniteRecursion;

	public bool updateLimboSkybox;

	[Space]
	public float minimumEntrySideSpeed;

	public float minimumExitSideSpeed;

	public int maxRecursions = 3;

	[Space(10f)]
	public Material overrideSkyboxEnter;

	public Material overrideSkyboxExit;

	[Space(10f)]
	public bool enableOverrideFog;

	public bool useFogEnter = true;

	public Color overrideFogColorEnter = Color.black;

	public float overrideFogStartEnter;

	public float overrideFogEndEnter = 300f;

	public bool useFogExit = true;

	public Color overrideFogColorExit = Color.black;

	public float overrideFogStartExit;

	public float overrideFogEndExit = 300f;

	[HideInInspector]
	public RenderTexture fakeEnterTex;

	[HideInInspector]
	public RenderTexture fakeExitTex;

	public Matrix4x4 fakeVPMatrix;

	private bool storeEnterThisFrame;

	private bool storeExitThisFrame;

	private int enterOnscreenIndex = -1;

	private int exitOnscreenIndex = -1;

	public NativePortalTransform entryTransform => GetTransform(PortalSide.Enter);

	public NativePortalTransform exitTransform => GetTransform(PortalSide.Exit);

	public UnityEventPortalTravel onTravel(PortalSide side)
	{
		if (side != PortalSide.Enter)
		{
			return onExitTravel;
		}
		return onEntryTravel;
	}

	public PortalTravellerFlags GetTravelFlags(PortalSide side)
	{
		if (side != PortalSide.Enter)
		{
			return exitTravelFlags;
		}
		return entryTravelFlags;
	}

	public float LinkOffset(PortalSide side)
	{
		if (!overrideLinkOffset)
		{
			return 1.5f;
		}
		if (side != PortalSide.Enter)
		{
			return exitOffset;
		}
		return enterOffset;
	}

	private void Start()
	{
		MonoSingleton<PortalManagerV2>.Instance.AddPortal(this);
		if (supportInfiniteRecursion)
		{
			PlaneShape planeShape = (PlaneShape)(object)shape;
			float num = planeShape.width / planeShape.height;
			int width = ((num > 0f) ? 256 : Mathf.RoundToInt(256f * num));
			int height = ((num > 0f) ? Mathf.RoundToInt(256f / num) : 256);
			fakeEnterTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
			fakeExitTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
		}
	}

	public void SetNeedToStoreTexture(int currentOnscreenIndex, PortalSide side)
	{
		if (supportInfiniteRecursion)
		{
			if (side == PortalSide.Enter)
			{
				storeEnterThisFrame = true;
				enterOnscreenIndex = currentOnscreenIndex;
			}
			else
			{
				storeExitThisFrame = true;
				exitOnscreenIndex = currentOnscreenIndex;
			}
		}
	}

	internal void TryStoreTexture(int onscreenHandleIndex, ref Matrix4x4 enterProjectionMatrix, ref Matrix4x4 enterViewMatrix, RenderTexture portalCompositeColor, PortalSide side)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (!supportInfiniteRecursion)
		{
			return;
		}
		bool flag = side == PortalSide.Enter;
		int num = (flag ? enterOnscreenIndex : exitOnscreenIndex);
		bool flag2 = (flag ? storeEnterThisFrame : storeExitThisFrame);
		if (onscreenHandleIndex == num && flag2)
		{
			if (flag)
			{
				storeEnterThisFrame = false;
			}
			else
			{
				storeExitThisFrame = false;
			}
			RenderTexture dest = (flag ? fakeEnterTex : fakeExitTex);
			Matrix4x4 value = enterProjectionMatrix * enterViewMatrix;
			Material fakeRecursionCopy = MonoSingleton<PortalManagerV2>.Instance.render.fakeRecursionCopy;
			NativePortalScene nativeScene = MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene;
			NativePortal nativePortal = nativeScene.LookupPortal(new PortalHandle(GetInstanceID(), side));
			if (nativePortal.valid)
			{
				PortalVertices vertices = nativePortal.vertices;
				Vector4[] values = new Vector4[4]
				{
					new Vector4(vertices.v0.x, vertices.v0.y, vertices.v0.z, 1f),
					new Vector4(vertices.v1.x, vertices.v1.y, vertices.v1.z, 1f),
					new Vector4(vertices.v2.x, vertices.v2.y, vertices.v2.z, 1f),
					new Vector4(vertices.v3.x, vertices.v3.y, vertices.v3.z, 1f)
				};
				fakeRecursionCopy.SetVectorArray("_PortalCorners", values);
				fakeRecursionCopy.SetMatrix("_PortalMatrix", value);
				Graphics.Blit(portalCompositeColor, dest, fakeRecursionCopy);
			}
		}
	}

	private void OnEnable()
	{
		if (TryGetComponent<VirtualAudioListener>(out var component))
		{
			component.enabled = true;
		}
		if (TryGetComponent<VirtualAudioOutput>(out var component2))
		{
			component2.enabled = true;
		}
	}

	private void OnDisable()
	{
		if (TryGetComponent<VirtualAudioListener>(out var component))
		{
			component.enabled = false;
		}
		if (TryGetComponent<VirtualAudioOutput>(out var component2))
		{
			component2.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!(entry == null))
		{
			_ = exit == null;
		}
	}

	private void OnDrawGizmos()
	{
		if (!(entry == null))
		{
			_ = exit == null;
		}
	}

	[BurstCompile(CompileSynchronously = true, DisableSafetyChecks = true)]
	private unsafe static void UpdatePortalBurst([NoAlias] PortalBurstData* d)
	{
		UpdatePortalBurst_00002973_0024BurstDirectCall.Invoke(d);
	}

	public void SetUpdatedSkyFog(PortalSide side)
	{
		SetUpdatedSkyFog(side == PortalSide.Enter);
	}

	public void SetEnterSkyFog()
	{
		SetUpdatedSkyFog(isEnter: true);
	}

	public void SetExitSkyFog()
	{
		SetUpdatedSkyFog(isEnter: false);
	}

	private void SetUpdatedSkyFog(bool isEnter)
	{
		if (enableOverrideFog)
		{
			RenderSettings.fog = (isEnter ? useFogEnter : useFogExit);
			RenderSettings.fogColor = (isEnter ? overrideFogColorEnter : overrideFogColorExit);
			RenderSettings.fogStartDistance = (isEnter ? overrideFogStartEnter : overrideFogStartExit);
			RenderSettings.fogEndDistance = (isEnter ? overrideFogEndEnter : overrideFogEndExit);
		}
		Material material = (isEnter ? overrideSkyboxEnter : overrideSkyboxExit);
		if (material != null)
		{
			RenderSettings.skybox = material;
		}
	}

	public PlaneShape GetShape()
	{
		return (PlaneShape)(object)shape;
	}

	[Obsolete("Use NativePortal directly instead where possible")]
	public Matrix4x4 GetTravelMatrix(PortalSide side)
	{
		NativePortal nativePortal = MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene.LookupPortal(new PortalHandle(GetInstanceID(), side));
		return nativePortal.travelMatrixManaged;
	}

	[Obsolete("Use NativePortal directly instead where possible")]
	public NativePortalTransform GetTransform(PortalSide side)
	{
		return MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene.LookupPortal(new PortalHandle(GetInstanceID(), side)).transform;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true, DisableSafetyChecks = true)]
	internal unsafe static void UpdatePortalBurst_0024BurstManaged([NoAlias] PortalBurstData* d)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		float3 entryPos = (float3)(*d->entryPos);
		quaternion entryRot = (quaternion)(*d->entryRot);
		float3 exitPos = (float3)(*d->exitPos);
		quaternion exitRot = (quaternion)(*d->exitRot);
		float3 entryFwd = (float3)(*d->entryFwd);
		float3 exitFwd = (float3)(*d->exitFwd);
		float4x4 portalScale = *d->portalScale;
		float4x4 portalScaleInv = *d->portalScaleInv;
		float4x4 val = float4x4.TRS(entryPos, entryRot, new float3(1));
		float4x4 val2 = math.fastinverse(val);
		float4x4 val3 = float4x4.TRS(exitPos, exitRot, new float3(1));
		float4x4 val4 = math.fastinverse(val3);
		Unsafe.Write(d->outTravel, math.mul(math.mul(val3, portalScale), val2));
		Unsafe.Write(d->outTravelRev, math.mul(math.mul(val, portalScaleInv), val4));
		Unsafe.Write(d->entryBase, val);
		Unsafe.Write((byte*)d->entryBase + Unsafe.SizeOf<float4x4>(), val2);
		float3* ptr = (float3*)((byte*)d->entryBase + (nint)2 * (nint)Unsafe.SizeOf<float4x4>());
		Unsafe.Write(ptr, ((float4)(ref val.c3)).xyz);
		Unsafe.Write((byte*)ptr + Unsafe.SizeOf<float3>(), ((float4)(ref val.c2)).xyz);
		Unsafe.Write((byte*)ptr + (nint)2 * (nint)Unsafe.SizeOf<float3>(), ((float4)(ref val.c1)).xyz);
		Unsafe.Write((byte*)ptr + (nint)3 * (nint)Unsafe.SizeOf<float3>(), ((float4)(ref val.c0)).xyz);
		Unsafe.Write((byte*)ptr + (nint)4 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr)[3]);
		Unsafe.Write((byte*)ptr + (nint)5 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr)[2]);
		Unsafe.Write((byte*)ptr + (nint)6 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr)[1]);
		Unsafe.Write(d->exitBase, val3);
		Unsafe.Write((byte*)d->exitBase + Unsafe.SizeOf<float4x4>(), val4);
		float3* ptr2 = (float3*)((byte*)d->exitBase + (nint)2 * (nint)Unsafe.SizeOf<float4x4>());
		Unsafe.Write(ptr2, ((float4)(ref val3.c3)).xyz);
		Unsafe.Write((byte*)ptr2 + Unsafe.SizeOf<float3>(), ((float4)(ref val3.c2)).xyz);
		Unsafe.Write((byte*)ptr2 + (nint)2 * (nint)Unsafe.SizeOf<float3>(), ((float4)(ref val3.c1)).xyz);
		Unsafe.Write((byte*)ptr2 + (nint)3 * (nint)Unsafe.SizeOf<float3>(), ((float4)(ref val3.c0)).xyz);
		Unsafe.Write((byte*)ptr2 + (nint)4 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr2)[3]);
		Unsafe.Write((byte*)ptr2 + (nint)5 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr2)[2]);
		Unsafe.Write((byte*)ptr2 + (nint)6 * (nint)Unsafe.SizeOf<float3>(), -((float3*)ptr2)[1]);
		Unsafe.Write(d->outEntryPlane, new float4(entryFwd, 0f - math.dot(entryFwd, entryPos)));
		Unsafe.Write(d->outExitPlane, new float4(exitFwd, 0f - math.dot(exitFwd, exitPos)));
		float4 val5 = default(float4);
		((float4)(ref val5))._002Ector(0f - d->hW, d->hW, d->hW, 0f - d->hW);
		float4 val6 = default(float4);
		((float4)(ref val6))._002Ector(d->hH, d->hH, 0f - d->hH, 0f - d->hH);
		float4 val7 = val.c0.x * val5 + val.c1.x * val6 + val.c3.x;
		float4 val8 = val.c0.y * val5 + val.c1.y * val6 + val.c3.y;
		float4 val9 = val.c0.z * val5 + val.c1.z * val6 + val.c3.z;
		float4 val10 = val3.c0.x * val5 + val3.c1.x * val6 + val3.c3.x;
		float4 val11 = val3.c0.y * val5 + val3.c1.y * val6 + val3.c3.y;
		float4 val12 = val3.c0.z * val5 + val3.c1.z * val6 + val3.c3.z;
		bool infiniteRecursion = d->infiniteRecursion;
		for (int i = 0; i < 4; i++)
		{
			Unsafe.Write((byte*)d->enterVerts + (nint)i * (nint)Unsafe.SizeOf<float3>(), new float3(((float4)(ref val7))[i], ((float4)(ref val8))[i], ((float4)(ref val9))[i]));
			Unsafe.Write((byte*)d->exitVerts + (nint)i * (nint)Unsafe.SizeOf<float3>(), new float3(((float4)(ref val10))[i], ((float4)(ref val11))[i], ((float4)(ref val12))[i]));
			if (infiniteRecursion)
			{
				Unsafe.Write((byte*)d->enterVertsVec4 + (nint)i * (nint)Unsafe.SizeOf<float4>(), new float4(((float4)(ref val7))[i], ((float4)(ref val8))[i], ((float4)(ref val9))[i], 1f));
				Unsafe.Write((byte*)d->exitVertsVec4 + (nint)i * (nint)Unsafe.SizeOf<float4>(), new float4(((float4)(ref val10))[i], ((float4)(ref val11))[i], ((float4)(ref val12))[i], 1f));
			}
		}
	}
}
