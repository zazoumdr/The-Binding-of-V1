using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererPortalHelper : MonoBehaviour
{
	private class SegmentManager
	{
		private readonly Transform container;

		private readonly List<LineRenderer> renderers = new List<LineRenderer>();

		private readonly List<GameObject> objects = new List<GameObject>();

		private LineRendererProperties properties;

		public int Count => renderers.Count;

		public SegmentManager(Transform parent, LineRendererProperties template)
		{
			container = new GameObject(parent.name + "_PortalSegments").transform;
			container.SetParent(parent, worldPositionStays: false);
			properties = template;
		}

		public void DisableAll()
		{
			for (int i = 0; i < objects.Count; i++)
			{
				objects[i].SetActive(value: false);
			}
		}

		public LineRenderer GetSegment(int index)
		{
			EnsureSegmentCount(index + 1);
			return renderers[index];
		}

		public void EnsureSegmentCount(int count)
		{
			while (renderers.Count < count)
			{
				CreateSegment();
			}
			for (int i = 0; i < count; i++)
			{
				if (!objects[i].activeSelf)
				{
					objects[i].SetActive(value: true);
				}
			}
		}

		private void CreateSegment()
		{
			GameObject gameObject = new GameObject($"PortalSegment_{renderers.Count}");
			gameObject.transform.SetParent(container, worldPositionStays: false);
			LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
			lineRenderer.useWorldSpace = true;
			properties.ApplyBasicPropertiesTo(lineRenderer);
			lineRenderer.enabled = false;
			objects.Add(gameObject);
			renderers.Add(lineRenderer);
		}

		public void CleanupSegments(int activeCount)
		{
			for (int i = activeCount; i < renderers.Count; i++)
			{
				if (renderers[i].enabled)
				{
					renderers[i].enabled = false;
				}
				if (objects[i].activeSelf)
				{
					objects[i].SetActive(value: false);
				}
			}
		}

		public void Dispose()
		{
			if (container != null)
			{
				if (!container.gameObject.scene.isLoaded)
				{
					return;
				}
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(container.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(container.gameObject);
				}
			}
			renderers.Clear();
			objects.Clear();
		}
	}

	[Serializable]
	internal struct LineRendererProperties
	{
		public Material material;

		public ShadowCastingMode shadowCastingMode;

		public bool receiveShadows;

		public LightProbeUsage lightProbeUsage;

		public ReflectionProbeUsage reflectionProbeUsage;

		public int sortingLayerID;

		public int sortingOrder;

		public bool allowOcclusionWhenDynamic;

		public MaterialPropertyBlock propertyBlock;

		public bool hasPropertyBlock;

		public Gradient gradient;

		public AnimationCurve widthCurve;

		public Color startColor;

		public Color endColor;

		public float startWidth;

		public float endWidth;

		public float widthMultiplier;

		public LineAlignment alignment;

		public LineTextureMode textureMode;

		public int numCapVertices;

		public int numCornerVertices;

		public Vector2 textureScale;

		public void CopyFrom(LineRenderer source)
		{
			material = source.material;
			shadowCastingMode = source.shadowCastingMode;
			receiveShadows = source.receiveShadows;
			lightProbeUsage = source.lightProbeUsage;
			reflectionProbeUsage = source.reflectionProbeUsage;
			sortingLayerID = source.sortingLayerID;
			sortingOrder = source.sortingOrder;
			allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;
			gradient = source.colorGradient;
			widthCurve = source.widthCurve;
			startColor = source.startColor;
			endColor = source.endColor;
			startWidth = source.startWidth;
			endWidth = source.endWidth;
			widthMultiplier = source.widthMultiplier;
			alignment = source.alignment;
			textureMode = source.textureMode;
			numCapVertices = source.numCapVertices;
			numCornerVertices = source.numCornerVertices;
			textureScale = source.textureScale;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			source.GetPropertyBlock(materialPropertyBlock);
			if (!materialPropertyBlock.isEmpty)
			{
				propertyBlock = materialPropertyBlock;
				hasPropertyBlock = true;
			}
			else
			{
				hasPropertyBlock = false;
			}
		}

		public void CopyDynamicFrom(LineRenderer source)
		{
			gradient = source.colorGradient;
			widthCurve = source.widthCurve;
			startColor = source.startColor;
			endColor = source.endColor;
			startWidth = source.startWidth;
			endWidth = source.endWidth;
			widthMultiplier = source.widthMultiplier;
			textureScale = source.textureScale;
			textureMode = source.textureMode;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			source.GetPropertyBlock(materialPropertyBlock);
			if (materialPropertyBlock != null && !materialPropertyBlock.isEmpty)
			{
				propertyBlock = materialPropertyBlock;
				hasPropertyBlock = true;
			}
		}

		public void ApplyBasicPropertiesTo(LineRenderer target)
		{
			target.material = material;
			target.shadowCastingMode = shadowCastingMode;
			target.receiveShadows = receiveShadows;
			target.lightProbeUsage = lightProbeUsage;
			target.reflectionProbeUsage = reflectionProbeUsage;
			target.sortingLayerID = sortingLayerID;
			target.sortingOrder = sortingOrder;
			target.allowOcclusionWhenDynamic = allowOcclusionWhenDynamic;
			target.alignment = alignment;
			target.textureMode = textureMode;
			target.numCapVertices = numCapVertices;
			target.numCornerVertices = numCornerVertices;
			target.textureScale = textureScale;
			target.textureMode = textureMode;
			if (hasPropertyBlock && propertyBlock != null)
			{
				target.SetPropertyBlock(propertyBlock);
			}
		}

		public void ApplyTo(LineRenderer target)
		{
			ApplyBasicPropertiesTo(target);
			target.colorGradient = gradient;
			target.widthCurve = widthCurve;
			target.startColor = startColor;
			target.endColor = endColor;
			target.startWidth = startWidth;
			target.endWidth = endWidth;
			target.widthMultiplier = widthMultiplier;
		}
	}

	[Tooltip("If true, uses intermediate positions (as in, excluding the first and last position) from the original LineRenderer as world-space offsets relative to the straight line path between segments.")]
	public bool useIntermediatePositionsAsOffsets;

	private LineRenderer originalLineRenderer;

	private Transform segmentsContainer;

	private readonly List<PortalTraversalV2> traversals = new List<PortalTraversalV2>();

	private LineRendererProperties properties;

	private SegmentManager segmentManager;

	private Vector3 originalStartPoint;

	private Vector3 originalEndPoint;

	private readonly List<Vector3> originalIntermediatePoints = new List<Vector3>();

	private readonly List<Vector3> tempSegmentPoints = new List<Vector3>();

	public static LineRendererPortalHelper GetOrCreateHelper(LineRenderer lr)
	{
		if (lr == null)
		{
			return null;
		}
		if (lr.TryGetComponent<LineRendererPortalHelper>(out var component))
		{
			return component;
		}
		return lr.gameObject.AddComponent<LineRendererPortalHelper>();
	}

	private void Awake()
	{
		originalLineRenderer = GetComponent<LineRenderer>();
		properties.CopyFrom(originalLineRenderer);
		segmentManager = new SegmentManager(base.transform, properties);
	}

	private void LateUpdate()
	{
		properties.CopyDynamicFrom(originalLineRenderer);
		FetchOriginalPoints();
		bool num = traversals.Count > 0;
		bool flag = useIntermediatePositionsAsOffsets && originalIntermediatePoints.Count > 0;
		if (num || flag)
		{
			if (originalLineRenderer.enabled)
			{
				originalLineRenderer.enabled = false;
			}
			RebuildSegments();
		}
		else
		{
			if (!originalLineRenderer.enabled)
			{
				originalLineRenderer.enabled = true;
			}
			segmentManager.CleanupSegments(0);
		}
	}

	private void OnDestroy()
	{
		segmentManager?.Dispose();
		traversals.Clear();
		originalIntermediatePoints.Clear();
	}

	public void UpdateTraversals(IList<PortalTraversalV2> newTraversals)
	{
		traversals.Clear();
		if (newTraversals != null)
		{
			traversals.AddRange(newTraversals);
		}
		segmentManager.EnsureSegmentCount(traversals.Count + 1);
	}

	public void DisableSegments()
	{
		segmentManager.DisableAll();
	}

	public void UpdatePropertyBlock(MaterialPropertyBlock propertyBlock)
	{
		if (propertyBlock != null)
		{
			properties.propertyBlock = propertyBlock;
			properties.hasPropertyBlock = true;
		}
		else
		{
			properties.propertyBlock = null;
			properties.hasPropertyBlock = false;
		}
		if (segmentManager == null)
		{
			return;
		}
		for (int i = 0; i < segmentManager.Count; i++)
		{
			LineRenderer segment = segmentManager.GetSegment(i);
			if (segment != null && segment.gameObject.activeSelf)
			{
				segment.SetPropertyBlock(propertyBlock);
			}
		}
	}

	private void FetchOriginalPoints()
	{
		int positionCount = originalLineRenderer.positionCount;
		originalIntermediatePoints.Clear();
		if (positionCount > 0)
		{
			originalStartPoint = originalLineRenderer.GetPosition(0);
			if (positionCount > 1)
			{
				originalEndPoint = originalLineRenderer.GetPosition(positionCount - 1);
				if (useIntermediatePositionsAsOffsets && positionCount > 2)
				{
					for (int i = 1; i < positionCount - 1; i++)
					{
						originalIntermediatePoints.Add(originalLineRenderer.GetPosition(i));
					}
				}
			}
			else
			{
				originalEndPoint = originalStartPoint;
			}
		}
		else
		{
			originalStartPoint = base.transform.position;
			originalEndPoint = base.transform.position;
		}
	}

	private void RebuildSegments()
	{
		int num = traversals.Count + 1;
		segmentManager.EnsureSegmentCount(num);
		float totalPathLength = CalculateTotalPathLength();
		BuildSegmentGeometry(totalPathLength);
		segmentManager.CleanupSegments(num);
	}

	private float CalculateTotalPathLength()
	{
		float num = 0f;
		Vector3 exitPoint = originalStartPoint;
		for (int i = 0; i < traversals.Count; i++)
		{
			num += Vector3.Distance(exitPoint, traversals[i].entrancePoint);
			exitPoint = traversals[i].exitPoint;
		}
		num += Vector3.Distance(exitPoint, originalEndPoint);
		return Mathf.Max(num, Mathf.Epsilon);
	}

	private void BuildSegmentGeometry(float totalPathLength)
	{
		int numShapePoints = (useIntermediatePositionsAsOffsets ? (2 + originalIntermediatePoints.Count) : 2);
		float num = 0f;
		Vector3 exitPoint = originalStartPoint;
		for (int i = 0; i < traversals.Count + 1; i++)
		{
			Vector3 vector = ((i < traversals.Count) ? traversals[i].entrancePoint : originalEndPoint);
			float num2 = Vector3.Distance(exitPoint, vector);
			float num3 = num + num2;
			float tSegStart = num / totalPathLength;
			float tSegEnd = num3 / totalPathLength;
			BuildSegmentPoints(exitPoint, vector, tSegStart, tSegEnd, numShapePoints);
			ConfigureSegmentRenderer(i, tSegStart, tSegEnd);
			num = num3;
			if (i < traversals.Count)
			{
				exitPoint = traversals[i].exitPoint;
			}
		}
	}

	private void BuildSegmentPoints(Vector3 segStart, Vector3 segEnd, float tSegStart, float tSegEnd, int numShapePoints)
	{
		tempSegmentPoints.Clear();
		tempSegmentPoints.Add(segStart);
		if (useIntermediatePositionsAsOffsets && originalIntermediatePoints.Count > 0)
		{
			AddIntermediatePointsWithOffsets(segStart, segEnd, tSegStart, tSegEnd, numShapePoints);
		}
		tempSegmentPoints.Add(segEnd);
	}

	private void AddIntermediatePointsWithOffsets(Vector3 segStart, Vector3 segEnd, float tSegStart, float tSegEnd, int numShapePoints)
	{
		for (int i = 1; i < numShapePoints - 1; i++)
		{
			float num = (float)i / (float)(numShapePoints - 1);
			if (num > tSegStart + Mathf.Epsilon && num < tSegEnd - Mathf.Epsilon)
			{
				float num2 = tSegEnd - tSegStart;
				float t = ((num2 < Mathf.Epsilon) ? 0f : ((num - tSegStart) / num2));
				Vector3 vector = Vector3.Lerp(segStart, segEnd, t);
				tempSegmentPoints.Add(vector + originalIntermediatePoints[i - 1]);
			}
		}
	}

	private void ConfigureSegmentRenderer(int segmentIndex, float tSegStart, float tSegEnd)
	{
		LineRenderer segment = segmentManager.GetSegment(segmentIndex);
		segment.enabled = true;
		segment.positionCount = tempSegmentPoints.Count;
		segment.SetPositions(tempSegmentPoints.ToArray());
		ApplySegmentVisualProperties(segment, tSegStart, tSegEnd);
	}

	private void ApplySegmentVisualProperties(LineRenderer lr, float tSegStart, float tSegEnd)
	{
		ApplyWidthProperties(lr, tSegStart, tSegEnd);
		ApplyColorProperties(lr, tSegStart, tSegEnd);
		lr.widthMultiplier = properties.widthMultiplier;
	}

	private void ApplyColorProperties(LineRenderer lr, float tSegStart, float tSegEnd)
	{
		if (properties.gradient != null && properties.gradient.colorKeys.Length != 0)
		{
			lr.colorGradient = properties.gradient;
			lr.startColor = properties.gradient.Evaluate(tSegStart);
			lr.endColor = properties.gradient.Evaluate(tSegEnd);
		}
		else
		{
			lr.colorGradient = null;
			lr.startColor = Color.Lerp(properties.startColor, properties.endColor, tSegStart);
			lr.endColor = Color.Lerp(properties.startColor, properties.endColor, tSegEnd);
		}
	}

	private void ApplyWidthProperties(LineRenderer lr, float tSegStart, float tSegEnd)
	{
		AnimationCurve widthCurve = properties.widthCurve;
		if (widthCurve != null && widthCurve.length > 0)
		{
			lr.widthCurve = properties.widthCurve.CreateSubCurve(tSegStart, tSegEnd);
			return;
		}
		lr.widthCurve = null;
		lr.startWidth = Mathf.Lerp(properties.startWidth, properties.endWidth, tSegStart);
		lr.endWidth = Mathf.Lerp(properties.startWidth, properties.endWidth, tSegEnd);
	}
}
