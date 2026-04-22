using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace ULTRAKILL.Portal;

public class PortalAwareRenderer : MonoBehaviour
{
	private enum ObjectType
	{
		Undefined,
		Enemy,
		Player
	}

	public sealed class Clone
	{
		public readonly Dictionary<Transform, Transform> ObjectsLookup = new Dictionary<Transform, Transform>();

		public PortalAwareRenderer PortalAwareRenderer;

		public Transform Transform;

		public GameObject GameObject;

		private PortalHandle? portalHandle;

		public bool TryGetPortalHandle(out PortalHandle result)
		{
			if (!portalHandle.HasValue)
			{
				result = default(PortalHandle);
				return false;
			}
			result = portalHandle.Value;
			return true;
		}

		public void SetPortalHandle(PortalHandle? handle)
		{
			portalHandle = handle;
		}
	}

	public sealed class SkinnedMeshBone
	{
		public readonly BoxCollider Collider;

		public readonly Vector3 BaseOffset;

		public readonly bool HasCopyPosRot;

		public readonly int ChildCount;

		private readonly bool hasLCPR;

		private readonly LateCopyPositionAndRotation lcpr;

		public SkinnedMeshBone(BoxCollider collider, int childCount)
		{
			Collider = collider;
			BaseOffset = collider.transform.localPosition;
			hasLCPR = collider.TryGetComponent<LateCopyPositionAndRotation>(out lcpr);
			ChildCount = childCount;
		}

		public bool TryGetLCPR(out LateCopyPositionAndRotation lcpr)
		{
			lcpr = this.lcpr;
			if (hasLCPR)
			{
				return lcpr != null;
			}
			return false;
		}
	}

	public const int MaxClipPlanes = 4;

	private static readonly List<Material> reusableMaterials = new List<Material>();

	private static readonly Dictionary<Transform, Transform> targetLookup = new Dictionary<Transform, Transform>();

	private static readonly HashSet<PortalHandle> clipPlanePortalHandles = new HashSet<PortalHandle>();

	private static readonly HashSet<long> parentPortalHashCodes = new HashSet<long>();

	private static readonly Vector4[] targetClipPlanes = new Vector4[4];

	private static readonly Vector3[] obbVertices = new Vector3[8];

	private static readonly Vector3[] portalVertices = new Vector3[4];

	private static readonly Vector3[] satAxes = new Vector3[6];

	private static MaterialPropertyBlock _propertyBlock;

	private NativePortalScene nativeScene;

	private static Vector3 portalSortCenter;

	private readonly List<PortalHandle> activeHandles = new List<PortalHandle>();

	private readonly List<Transform> targetObjects = new List<Transform>();

	private readonly List<Renderer> targetRenderers = new List<Renderer>();

	private readonly List<Clone> clones = new List<Clone>();

	private List<SkinnedMeshBone> smrBoundingBones;

	private HashSet<Renderer> targetRenderersToIgnore;

	[SerializeField]
	private ObjectType objectType = ObjectType.Enemy;

	[SerializeField]
	private bool useAdvancedSkinnedBoundsCheck;

	private Transform cachedTransform;

	private int activeCloneCount;

	private bool initializedTargets;

	private PortalAwareRenderer parent;

	private bool hasParent;

	private PortalHandle parentPortalHandle;

	private Bounds? boundsOverride;

	private int clipPlaneCount;

	private Func<PortalAwareRenderer, PortalHandle, bool> portalHandleFilter;

	public static bool IntersectCachedShapes()
	{
		int num = obbVertices.Length;
		int num2 = portalVertices.Length;
		int num3 = satAxes.Length;
		for (int i = 0; i < num3; i++)
		{
			ref Vector3 reference = ref satAxes[i];
			float num4 = float.MaxValue;
			float num5 = float.MinValue;
			for (int j = 0; j < num; j++)
			{
				float num6 = Vector3.Dot(obbVertices[j], reference);
				if (num6 < num4)
				{
					num4 = num6;
				}
				if (num6 > num5)
				{
					num5 = num6;
				}
			}
			float num7 = float.MaxValue;
			float num8 = float.MinValue;
			for (int k = 0; k < num2; k++)
			{
				float num9 = Vector3.Dot(portalVertices[k], reference);
				if (num9 < num7)
				{
					num7 = num9;
				}
				if (num9 > num8)
				{
					num8 = num9;
				}
			}
			if (num8 < num4 || num5 < num7 || num4 > num8 || num7 > num5)
			{
				return false;
			}
		}
		return true;
	}

	public static void CacheBoneVertices(BoxCollider coll, SkinnedMeshBone bone)
	{
		Transform transform = coll.transform;
		Vector3 lossyScale = transform.lossyScale;
		Vector3 vector = new Vector3(coll.size.x * lossyScale.x, coll.size.y * lossyScale.y, coll.size.z * lossyScale.z);
		Vector3 center = coll.center;
		if (bone.ChildCount == 1)
		{
			Vector3 vector2 = coll.transform.localPosition - bone.BaseOffset;
			vector2.x *= lossyScale.x;
			vector2.y *= lossyScale.y;
			vector2.z *= lossyScale.z;
			center -= vector2 * 0.5f;
			vector.x += Mathf.Abs(vector2.x);
			vector.y += Mathf.Abs(vector2.y);
			vector.z += Mathf.Abs(vector2.z);
		}
		center = transform.TransformPoint(center);
		Vector3 up = 0.5f * vector.y * transform.up;
		Vector3 right = 0.5f * vector.x * transform.right;
		Vector3 forward = 0.5f * vector.z * transform.forward;
		CacheBoxVertices(ref center, ref up, ref right, ref forward);
	}

	public static void CacheBoundsVertices(ref Vector3 center, ref Vector3 extents)
	{
		Vector3 up = new Vector3(0f, extents.y, 0f);
		Vector3 right = new Vector3(extents.x, 0f, 0f);
		Vector3 forward = new Vector3(0f, 0f, extents.z);
		CacheBoxVertices(ref center, ref up, ref right, ref forward);
	}

	public static void CacheBoxVertices(ref Vector3 center, ref Vector3 up, ref Vector3 right, ref Vector3 forward)
	{
		ref Vector3 reference = ref obbVertices[0];
		ref Vector3 reference2 = ref obbVertices[1];
		ref Vector3 reference3 = ref obbVertices[2];
		ref Vector3 reference4 = ref obbVertices[3];
		ref Vector3 reference5 = ref obbVertices[4];
		ref Vector3 reference6 = ref obbVertices[5];
		ref Vector3 reference7 = ref obbVertices[6];
		ref Vector3 reference8 = ref obbVertices[7];
		reference.x = center.x + right.x + up.x + forward.x;
		reference.y = center.y + right.y + up.y + forward.y;
		reference.z = center.z + right.z + up.z + forward.z;
		reference2.x = center.x + right.x + up.x - forward.x;
		reference2.y = center.y + right.y + up.y - forward.y;
		reference2.z = center.z + right.z + up.z - forward.z;
		reference3.x = center.x + right.x - up.x + forward.x;
		reference3.y = center.y + right.y - up.y + forward.y;
		reference3.z = center.z + right.z - up.z + forward.z;
		reference4.x = center.x + right.x - up.x - forward.x;
		reference4.y = center.y + right.y - up.y - forward.y;
		reference4.z = center.z + right.z - up.z - forward.z;
		reference5.x = center.x - right.x + up.x + forward.x;
		reference5.y = center.y - right.y + up.y + forward.y;
		reference5.z = center.z - right.z + up.z + forward.z;
		reference6.x = center.x - right.x + up.x - forward.x;
		reference6.y = center.y - right.y + up.y - forward.y;
		reference6.z = center.z - right.z + up.z - forward.z;
		reference7.x = center.x - right.x - up.x + forward.x;
		reference7.y = center.y - right.y - up.y + forward.y;
		reference7.z = center.z - right.z - up.z + forward.z;
		reference8.x = center.x - right.x - up.x - forward.x;
		reference8.y = center.y - right.y - up.y - forward.y;
		reference8.z = center.z - right.z - up.z - forward.z;
		satAxes[3] = up;
		satAxes[4] = right;
		satAxes[5] = forward;
	}

	public static void CachePortalVertices(in NativePortal portal)
	{
		portalVertices[0] = portal.v0Managed;
		portalVertices[1] = portal.v1Managed;
		portalVertices[2] = portal.v2Managed;
		portalVertices[3] = portal.v3Managed;
		NativePortalTransform nativePortalTransform = portal.transform;
		satAxes[0] = nativePortalTransform.upManaged;
		satAxes[1] = nativePortalTransform.rightManaged;
		satAxes[2] = nativePortalTransform.forwardManaged;
	}

	public static void SetClipPlaneKeyword(Renderer renderer, bool enabled)
	{
		renderer.GetSharedMaterials(reusableMaterials);
		int count = reusableMaterials.Count;
		for (int i = 0; i < count; i++)
		{
			SetClipPlaneKeyword(reusableMaterials[i], enabled);
		}
		reusableMaterials.Clear();
	}

	public static void SetClipPlaneKeyword(Material material, bool enabled)
	{
		if ((bool)material)
		{
			if (enabled)
			{
				material.EnableKeyword("PORTAL_CLIP_PLANE");
			}
			else
			{
				material.DisableKeyword("PORTAL_CLIP_PLANE");
			}
		}
	}

	public void SetPortalHandleFilter(Func<PortalAwareRenderer, PortalHandle, bool> filter)
	{
		portalHandleFilter = filter;
		foreach (Clone clone in clones)
		{
			if (!(clone.PortalAwareRenderer == null))
			{
				clone.PortalAwareRenderer.SetPortalHandleFilter(filter);
			}
		}
	}

	public void ClearPortalHandleFilter()
	{
		portalHandleFilter = null;
		foreach (Clone clone in clones)
		{
			if (!(clone.PortalAwareRenderer == null))
			{
				clone.PortalAwareRenderer.ClearPortalHandleFilter();
			}
		}
	}

	public void SetBoundsOverride(Bounds bounds)
	{
		boundsOverride = bounds;
	}

	public void ClearBoundsOverride()
	{
		boundsOverride = null;
	}

	private void RecursiveFindTargets(Transform targetObj)
	{
		targetObjects.Add(targetObj);
		targetObj.GetOrAddComponent<PortalAwareRendererTarget>().Owner = this;
		foreach (Transform item in targetObj)
		{
			RecursiveFindTargets(item);
		}
	}

	private void RecursiveClone(Clone clone, Transform targetTransform)
	{
		GameObject gameObject = targetTransform.gameObject;
		if (gameObject.TryGetComponent<GroundCheckEnemy>(out var _))
		{
			return;
		}
		targetTransform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
		Transform transform = new GameObject(gameObject.name).transform;
		transform.SetLocalPositionAndRotation(localPosition, localRotation);
		transform.localScale = targetTransform.localScale;
		transform.gameObject.SetActive(gameObject.activeInHierarchy);
		transform.gameObject.layer = gameObject.layer;
		transform.gameObject.tag = gameObject.tag;
		transform.parent = clone.Transform;
		clone.ObjectsLookup[targetTransform] = transform;
		PortalAwareRendererTarget orAddComponent = gameObject.GetOrAddComponent<PortalAwareRendererTarget>();
		orAddComponent.Clones.Add(transform);
		PortalAwareRendererClone portalAwareRendererClone = transform.gameObject.AddComponent<PortalAwareRendererClone>();
		portalAwareRendererClone.Owner = clone;
		portalAwareRendererClone.Target = orAddComponent;
		portalAwareRendererClone.TargetTransform = targetTransform;
		portalAwareRendererClone.TargetObject = gameObject;
		portalAwareRendererClone.PortalAwareRenderer = this;
		targetLookup[targetTransform] = transform;
		foreach (Transform item in targetTransform)
		{
			RecursiveClone(clone, item);
		}
	}

	private int GetIntersectingPortals(List<PortalHandle> portalHandles)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		if (portalHandles == null)
		{
			throw new ArgumentNullException("portalHandles");
		}
		portalHandles.Clear();
		clipPlanePortalHandles.Clear();
		clipPlaneCount = 0;
		PortalScene scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		Bounds bounds = boundsOverride ?? RendererUtility.GetBounds(targetRenderers, allowSMRs: true, targetRenderersToIgnore);
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Bounds bounds2 = boundsOverride ?? RendererUtility.GetBounds(targetRenderers, !useAdvancedSkinnedBoundsCheck, targetRenderersToIgnore);
		Vector3 center2 = bounds2.center;
		Vector3 extents2 = bounds2.extents;
		bool flag = portalHandleFilter != null;
		float num = 1.4142135f * Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
		float num2 = num * num;
		portalSortCenter = cachedTransform.position;
		if (!hasParent)
		{
			parentPortalHashCodes.Clear();
		}
		Vector3 vector = default(Vector3);
		nativeScene = scene.nativeScene;
		foreach (NativePortal portal2 in nativeScene.portals)
		{
			NativePortal portal = portal2;
			Plane plane = portal.plane;
			PortalHandle handle = portal.handle;
			if (((Plane)(ref plane)).SignedDistanceToPoint(float3.op_Implicit(portalSortCenter)) > 0f && num >= 20f)
			{
				continue;
			}
			float3 center3 = portal.transform.center;
			float num3 = math.max(portal.dimensions.x, portal.dimensions.y);
			float num4 = num3 * num3;
			vector.x = center3.x - center.x;
			vector.y = center3.y - center.y;
			vector.z = center3.z - center.z;
			if (vector.sqrMagnitude - num2 > num4 || (objectType == ObjectType.Enemy && (portal.travellerFlags & PortalTravellerFlags.Enemy) != PortalTravellerFlags.Enemy) || (objectType == ObjectType.Player && (portal.travellerFlags & PortalTravellerFlags.Player) != PortalTravellerFlags.Player) || parentPortalHashCodes.Contains(handle.PackedKey) || (flag && !portalHandleFilter(this, handle)))
			{
				continue;
			}
			CachePortalVertices(in portal);
			CacheBoundsVertices(ref center2, ref extents2);
			bool flag2 = IntersectCachedShapes();
			if (!flag2 && useAdvancedSkinnedBoundsCheck && smrBoundingBones != null)
			{
				int count = smrBoundingBones.Count;
				for (int i = 0; i < count; i++)
				{
					SkinnedMeshBone skinnedMeshBone = smrBoundingBones[i];
					BoxCollider collider = skinnedMeshBone.Collider;
					if (collider.gameObject.activeInHierarchy)
					{
						if (skinnedMeshBone.TryGetLCPR(out var lcpr))
						{
							lcpr.ManualUpdate();
						}
						CacheBoneVertices(collider, skinnedMeshBone);
						flag2 |= IntersectCachedShapes();
						if (flag2)
						{
							break;
						}
					}
				}
			}
			if (flag2)
			{
				portalHandles.Add(portal.handle);
				parentPortalHashCodes.Add(handle.PackedKey);
				parentPortalHashCodes.Add(handle.Reverse().PackedKey);
			}
		}
		nativeScene = MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene;
		portalHandles.Sort(delegate(PortalHandle a, PortalHandle b)
		{
			Vector3 centerManaged = nativeScene.LookupPortal(in a).transform.centerManaged;
			Vector3 centerManaged2 = nativeScene.LookupPortal(in b).transform.centerManaged;
			float num6 = Vector3.SqrMagnitude(centerManaged - portalSortCenter);
			float value = Vector3.SqrMagnitude(centerManaged2 - portalSortCenter);
			return num6.CompareTo(value);
		});
		if (hasParent && parentPortalHandle.IsValid())
		{
			NativePortal nativePortal = nativeScene.LookupPortal(in parentPortalHandle);
			if (nativePortal.renderData.clippingMethod != PortalClippingMethod.None)
			{
				targetClipPlanes[clipPlaneCount] = float4.op_Implicit(nativePortal.plane.NormalAndDistance);
				clipPlaneCount++;
			}
		}
		int count2 = portalHandles.Count;
		for (int num5 = 0; num5 < count2; num5++)
		{
			PortalHandle handle2 = portalHandles[num5];
			if (clipPlanePortalHandles.Contains(handle2) || handle2 == parentPortalHandle)
			{
				continue;
			}
			NativePortal nativePortal2 = nativeScene.LookupPortal(in handle2);
			if (nativePortal2.renderData.clippingMethod != PortalClippingMethod.None)
			{
				if (clipPlaneCount >= 3)
				{
					break;
				}
				targetClipPlanes[clipPlaneCount] = float4.op_Implicit(nativePortal2.plane.NormalAndDistance);
				clipPlaneCount++;
				clipPlanePortalHandles.Add(handle2.Reverse());
			}
		}
		return portalHandles.Count;
	}

	private void LateUpdate()
	{
		Think(!hasParent);
	}

	private void Think(bool allowChildUpdates = false)
	{
		PortalScene scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		NativePortalScene nativePortalScene = scene.nativeScene;
		if (scene.nativeScene.portals.Length <= 0)
		{
			return;
		}
		if (hasParent)
		{
			if (!allowChildUpdates)
			{
				return;
			}
			if (!parent || !parent.gameObject.activeInHierarchy)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}
		TryInitializeTargets();
		for (int num = targetRenderers.Count - 1; num >= 0; num--)
		{
			if (!targetRenderers[num])
			{
				targetRenderers.RemoveAt(num);
			}
		}
		int intersectingPortals = GetIntersectingPortals(activeHandles);
		for (int num2 = targetRenderers.Count - 1; num2 >= 0; num2--)
		{
			Renderer renderer = targetRenderers[num2];
			if (renderer.enabled && renderer.gameObject.activeInHierarchy)
			{
				SetClipPlaneKeyword(renderer, enabled: true);
				renderer.GetPropertyBlock(_propertyBlock);
				_propertyBlock.SetFloat(ShaderProperties.ClipPlaneCount, clipPlaneCount);
				_propertyBlock.SetVectorArray(ShaderProperties.ClipPlanes, targetClipPlanes);
				renderer.SetPropertyBlock(_propertyBlock);
			}
		}
		if (intersectingPortals == 0)
		{
			DeactivateClones();
			return;
		}
		EnsureClones(intersectingPortals);
		Vector3 forward = cachedTransform.forward;
		Vector3 up = cachedTransform.up;
		cachedTransform.GetPositionAndRotation(out var position, out var rotation);
		int count = clones.Count;
		for (int i = 0; i < count; i++)
		{
			Clone clone = clones[i];
			if (clone.GameObject.activeSelf)
			{
				PortalHandle handle = activeHandles[i];
				NativePortal nativePortal = nativePortalScene.LookupPortal(in handle);
				clone.SetPortalHandle(handle);
				ref Matrix4x4 reference = ref Unsafe.As<float4x4, Matrix4x4>(ref nativePortal.travelMatrix);
				Vector3 forward2 = reference.MultiplyVector(forward);
				Vector3 upwards = reference.MultiplyVector(up);
				Quaternion quaternion = Quaternion.LookRotation(forward2, upwards) * Quaternion.Inverse(rotation);
				Vector3 vector = reference.MultiplyPoint3x4(position);
				clone.Transform.SetPositionAndRotation(vector - quaternion * position, quaternion);
				clone.PortalAwareRenderer.parentPortalHandle = handle.Reverse();
				clone.PortalAwareRenderer.Think(allowChildUpdates: true);
			}
		}
	}

	private void DeactivateClones()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (activeCloneCount == 0)
		{
			return;
		}
		int count = clones.Count;
		if (count == 0)
		{
			return;
		}
		int count2 = targetRenderers.Count;
		for (int i = 0; i < count2; i++)
		{
			Renderer renderer = targetRenderers[i];
			if (!(renderer == null))
			{
				ParticleSystemRenderer val = (ParticleSystemRenderer)(object)((renderer is ParticleSystemRenderer) ? renderer : null);
				if (val != null)
				{
					MainModule main = ((Component)(object)val).GetComponent<ParticleSystem>().main;
					((MainModule)(ref main)).cullingMode = (ParticleSystemCullingMode)1;
					((Renderer)(object)val).forceRenderingOff = false;
				}
				SetClipPlaneKeyword(renderer, enabled: false);
				renderer.GetPropertyBlock(_propertyBlock);
				_propertyBlock.SetFloat(ShaderProperties.ClipPlaneCount, 0f);
				renderer.SetPropertyBlock(_propertyBlock);
			}
		}
		for (int j = 0; j < count; j++)
		{
			DeactivateClone(clones[j]);
		}
	}

	private void ActivateClone(Clone clone)
	{
		if (clone.GameObject.activeSelf)
		{
			return;
		}
		clone.GameObject.SetActive(value: true);
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			int count = targetObjects.Count;
			for (int i = 1; i < count; i++)
			{
				Transform transform = targetObjects[i];
				if (!(transform == null) && clone.ObjectsLookup.TryGetValue(transform, out var value) && !(value == null))
				{
					instance.AddTransformAccessPair(value, transform);
				}
			}
		}
		activeCloneCount++;
	}

	private void DeactivateClone(Clone clone)
	{
		clone.SetPortalHandle(null);
		if (!clone.GameObject.activeSelf)
		{
			return;
		}
		clone.GameObject.SetActive(value: false);
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		if (instance != null)
		{
			int count = targetObjects.Count;
			for (int i = 1; i < count; i++)
			{
				Transform transform = targetObjects[i];
				if (!(transform == null) && clone.ObjectsLookup.TryGetValue(transform, out var value) && !(value == null))
				{
					instance.RemoveTransformAccessPair(value);
				}
			}
		}
		activeCloneCount--;
	}

	private void Cleanup()
	{
		parentPortalHandle = PortalHandle.None;
		int count = clones.Count;
		for (int i = 0; i < count; i++)
		{
			Clone clone = clones[i];
			if (clone != null && !(clone.GameObject == null))
			{
				UnityEngine.Object.Destroy(clone.GameObject);
			}
		}
		clones.Clear();
		activeCloneCount = 0;
	}

	private void OnDisable()
	{
		Cleanup();
		if (hasParent && (parent == null || !parent.gameObject.activeInHierarchy))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		Cleanup();
		if (!hasParent || !(parent != null))
		{
			return;
		}
		for (int num = parent.clones.Count - 1; num >= 0; num--)
		{
			if (parent.clones[num].GameObject == base.gameObject)
			{
				parent.clones.RemoveAt(num);
				break;
			}
		}
	}

	private void EnsureClones(int desiredCloneCount)
	{
		int num = desiredCloneCount - clones.Count;
		for (int i = 0; i < num; i++)
		{
			CreateClone();
		}
		int count = clones.Count;
		for (int j = 0; j < count; j++)
		{
			Clone clone = clones[j];
			if (j < desiredCloneCount)
			{
				ActivateClone(clone);
			}
			else
			{
				DeactivateClone(clone);
			}
		}
	}

	private Clone CreateClone()
	{
		GameObject gameObject = new GameObject(base.name);
		Clone clone = new Clone
		{
			GameObject = gameObject,
			Transform = gameObject.transform
		};
		RecursiveClone(clone, base.transform);
		int count = targetObjects.Count;
		for (int i = 0; i < count; i++)
		{
			Transform transform = targetObjects[i];
			if (transform == null || !clone.ObjectsLookup.TryGetValue(transform, out var value) || value == null || clone.GameObject == null || !value.TryGetComponent<PortalAwareRendererClone>(out var component))
			{
				continue;
			}
			if (transform.TryGetComponent<PortalAwareRendererIgnore>(out var component2))
			{
				value.gameObject.AddComponent<PortalAwareRendererIgnore>();
			}
			if (transform.TryGetComponent<Renderer>(out var component3))
			{
				ParticleSystemRenderer val = (ParticleSystemRenderer)(object)((component3 is ParticleSystemRenderer) ? component3 : null);
				if (val == null)
				{
					if (!(component3 is MeshRenderer meshRenderer))
					{
						Transform value2;
						if (!(component3 is SkinnedMeshRenderer skinnedMeshRenderer))
						{
							if (component3 is SpriteRenderer spriteRenderer)
							{
								SpriteRenderer orAddComponent = value.GetOrAddComponent<SpriteRenderer>();
								component.SetRenderer(orAddComponent);
								orAddComponent.sprite = spriteRenderer.sprite;
								spriteRenderer.GetSharedMaterials(reusableMaterials);
								orAddComponent.SetSharedMaterials(reusableMaterials);
								reusableMaterials.Clear();
								orAddComponent.color = spriteRenderer.color;
								orAddComponent.flipX = spriteRenderer.flipX;
								orAddComponent.flipY = spriteRenderer.flipY;
								orAddComponent.drawMode = spriteRenderer.drawMode;
								orAddComponent.size = spriteRenderer.size;
								orAddComponent.tileMode = spriteRenderer.tileMode;
								orAddComponent.adaptiveModeThreshold = spriteRenderer.adaptiveModeThreshold;
								orAddComponent.spriteSortPoint = spriteRenderer.spriteSortPoint;
								orAddComponent.sortingLayerID = spriteRenderer.sortingLayerID;
								orAddComponent.sortingOrder = spriteRenderer.sortingOrder;
								orAddComponent.maskInteraction = spriteRenderer.maskInteraction;
							}
						}
						else if (!(skinnedMeshRenderer.rootBone == null) && targetLookup.TryGetValue(skinnedMeshRenderer.rootBone, out value2))
						{
							SkinnedMeshRenderer orAddComponent2 = value.GetOrAddComponent<SkinnedMeshRenderer>();
							component.SetRenderer(orAddComponent2);
							orAddComponent2.localBounds = skinnedMeshRenderer.localBounds;
							orAddComponent2.quality = skinnedMeshRenderer.quality;
							orAddComponent2.updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
							orAddComponent2.sharedMesh = skinnedMeshRenderer.sharedMesh;
							skinnedMeshRenderer.GetSharedMaterials(reusableMaterials);
							orAddComponent2.SetSharedMaterials(reusableMaterials);
							reusableMaterials.Clear();
							Transform[] bones = skinnedMeshRenderer.bones;
							int num = bones.Length;
							Transform[] array = new Transform[num];
							for (int j = 0; j < num; j++)
							{
								Transform transform2 = bones[j];
								if (transform2 != null && targetLookup.TryGetValue(transform2, out var value3))
								{
									array[j] = value3;
								}
							}
							orAddComponent2.rootBone = value2;
							orAddComponent2.bones = array;
						}
					}
					else
					{
						MeshFilter orAddComponent3 = value.GetOrAddComponent<MeshFilter>();
						if (meshRenderer.TryGetComponent<MeshFilter>(out var component4))
						{
							orAddComponent3.sharedMesh = component4.sharedMesh;
						}
						MeshRenderer orAddComponent4 = value.GetOrAddComponent<MeshRenderer>();
						component.SetRenderer(orAddComponent4);
						meshRenderer.GetSharedMaterials(reusableMaterials);
						orAddComponent4.SetSharedMaterials(reusableMaterials);
						reusableMaterials.Clear();
					}
				}
				else
				{
					if (((Component)(object)val).TryGetComponent(out component2))
					{
						continue;
					}
					if (!((Component)(object)val).gameObject.TryGetComponent<PortalAwareParticleSystem>(out var _))
					{
						((Component)(object)val).gameObject.AddComponent<PortalAwareParticleSystem>();
					}
				}
			}
			if (!transform.TryGetComponent<Collider>(out var component6))
			{
				continue;
			}
			EnemyIdentifier component8;
			if (transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component7))
			{
				value.gameObject.AddComponent<EnemyIdentifierIdentifier>().eid = component7.eid;
			}
			else if (transform.TryGetComponent<EnemyIdentifier>(out component8))
			{
				value.gameObject.AddComponent<EnemyIdentifierIdentifier>().eid = component8;
			}
			if (!(component6 is SphereCollider sphereCollider))
			{
				if (!(component6 is BoxCollider boxCollider))
				{
					if (!(component6 is CapsuleCollider capsuleCollider))
					{
						if (component6 is MeshCollider meshCollider)
						{
							MeshCollider meshCollider2 = value.gameObject.AddComponent<MeshCollider>();
							meshCollider2.convex = meshCollider.convex;
							meshCollider2.sharedMaterial = meshCollider.sharedMaterial;
							meshCollider2.sharedMesh = meshCollider.sharedMesh;
							component.SetCollider(meshCollider2);
							Physics.IgnoreCollision(component6, meshCollider2);
						}
					}
					else
					{
						CapsuleCollider capsuleCollider2 = value.gameObject.AddComponent<CapsuleCollider>();
						capsuleCollider2.isTrigger = capsuleCollider.isTrigger;
						capsuleCollider2.center = capsuleCollider.center;
						capsuleCollider2.radius = capsuleCollider.radius;
						capsuleCollider2.height = capsuleCollider.height;
						capsuleCollider2.direction = capsuleCollider.direction;
						component.SetCollider(capsuleCollider2);
						Physics.IgnoreCollision(component6, capsuleCollider2);
					}
				}
				else
				{
					BoxCollider boxCollider2 = value.gameObject.AddComponent<BoxCollider>();
					boxCollider2.isTrigger = boxCollider.isTrigger;
					boxCollider2.center = boxCollider.center;
					boxCollider2.size = boxCollider.size;
					component.SetCollider(boxCollider2);
					Physics.IgnoreCollision(component6, boxCollider2);
				}
			}
			else
			{
				SphereCollider sphereCollider2 = value.gameObject.AddComponent<SphereCollider>();
				sphereCollider2.isTrigger = sphereCollider.isTrigger;
				sphereCollider2.center = sphereCollider.center;
				sphereCollider2.radius = sphereCollider.radius;
				component.SetCollider(sphereCollider2);
				Physics.IgnoreCollision(component6, sphereCollider2);
			}
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		Collider[] componentsInChildren2 = clone.GameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		Collider[] array2 = componentsInChildren;
		foreach (Collider collider in array2)
		{
			Collider[] array3 = componentsInChildren2;
			foreach (Collider collider2 in array3)
			{
				if ((bool)collider && (bool)collider2 && collider != collider2)
				{
					Physics.IgnoreCollision(collider, collider2, ignore: true);
				}
			}
		}
		targetLookup.Clear();
		clone.GameObject.SetActive(value: false);
		PortalAwareRenderer portalAwareRenderer = clone.GameObject.AddComponent<PortalAwareRenderer>();
		portalAwareRenderer.hasParent = true;
		portalAwareRenderer.parent = this;
		portalAwareRenderer.portalHandleFilter = portalHandleFilter;
		clone.PortalAwareRenderer = portalAwareRenderer;
		clones.Add(clone);
		return clone;
	}

	private void Awake()
	{
		if (!MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		parentPortalHandle = PortalHandle.None;
		if (objectType == ObjectType.Undefined && TryGetComponent<EnemyScript>(out var _))
		{
			objectType = ObjectType.Enemy;
		}
		cachedTransform = base.transform;
	}

	private void TryInitializeTargets()
	{
		if (initializedTargets)
		{
			return;
		}
		initializedTargets = true;
		RecursiveFindTargets(base.transform);
		int count = targetObjects.Count;
		for (int i = 0; i < count; i++)
		{
			Transform transform = targetObjects[i];
			if (transform == null || !transform.TryGetComponent<PortalAwareRendererTarget>(out var component))
			{
				continue;
			}
			if (transform.TryGetComponent<Renderer>(out var component2))
			{
				if (!(component2 is SkinnedMeshRenderer skinnedMeshRenderer))
				{
					if (component2 is MeshRenderer || component2 is ParticleSystemRenderer || component2 is SpriteRenderer)
					{
						targetRenderers.Add(component2);
						component.SetRenderer(component2);
					}
				}
				else
				{
					if (useAdvancedSkinnedBoundsCheck)
					{
						Transform[] bones = skinnedMeshRenderer.bones;
						Bounds[] array = new Bounds[bones.Length];
						ArmatureUtility.ComputeLocalBounds(skinnedMeshRenderer.sharedMesh, array);
						for (int j = 0; j < bones.Length; j++)
						{
							BoxCollider boxCollider = bones[j].gameObject.AddComponent<BoxCollider>();
							boxCollider.center = array[j].center;
							boxCollider.size = array[j].size;
							boxCollider.enabled = false;
							int num = 0;
							foreach (Transform item in bones[j])
							{
								if (Enumerable.Contains(bones, item))
								{
									num++;
								}
							}
							if (smrBoundingBones == null)
							{
								smrBoundingBones = new List<SkinnedMeshBone>();
							}
							smrBoundingBones.Add(new SkinnedMeshBone(boxCollider, num));
						}
					}
					targetRenderers.Add(component2);
					component.SetRenderer(component2);
				}
				if (transform.TryGetComponent<PortalAwareRendererIgnore>(out var _) || component2 is ParticleSystemRenderer)
				{
					if (targetRenderersToIgnore == null)
					{
						targetRenderersToIgnore = new HashSet<Renderer>();
					}
					targetRenderersToIgnore.Add(component2);
				}
			}
			if (transform.TryGetComponent<Collider>(out var component4))
			{
				component.SetCollider(component4);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Bounds bounds = boundsOverride ?? RendererUtility.GetBounds(targetRenderers, allowSMRs: true, targetRenderersToIgnore);
		Bounds bounds2 = boundsOverride ?? RendererUtility.GetBounds(targetRenderers, !useAdvancedSkinnedBoundsCheck, targetRenderersToIgnore);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(bounds2.center, bounds2.size);
		Gizmos.color = Color.yellow * 0.5f;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		if (!useAdvancedSkinnedBoundsCheck || smrBoundingBones == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		int count = smrBoundingBones.Count;
		for (int i = 0; i < count; i++)
		{
			BoxCollider collider = smrBoundingBones[i].Collider;
			if (collider.gameObject.activeInHierarchy)
			{
				Gizmos.matrix = collider.transform.localToWorldMatrix;
				Gizmos.DrawWireCube(collider.center, collider.size);
			}
		}
	}
}
