using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(RawImage))]
public class SmileRenderer : MonoBehaviour
{
	public float width;

	public float height;

	private Camera cam;

	private RenderTexture rt;

	private RawImage displayImage;

	private CanvasScaler scaler;

	private float pixelScale;

	private int pixelWidth;

	private int pixelHeight;

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			Setup();
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			StartCoroutine(WaitSetup());
		}
	}

	private IEnumerator WaitSetup()
	{
		HudOpenEffect opener = GetComponentInParent<HudOpenEffect>();
		displayImage = GetComponent<RawImage>();
		((Graphic)displayImage).color = Color.clear;
		if (opener != null)
		{
			if (!opener.animating)
			{
				yield return null;
			}
			while (opener.animating)
			{
				yield return null;
			}
		}
		Setup();
		CreateTex();
		((Graphic)displayImage).color = Color.white;
	}

	private void Setup()
	{
		cam = GetComponent<Camera>();
		cam.cullingMask = 1 << LayerMask.NameToLayer("Invisible");
		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = Color.clear;
		cam.orthographic = true;
		cam.nearClipPlane = -1f;
		cam.farClipPlane = 1f;
		pixelScale = 1f;
		scaler = GetComponentInParent<CanvasScaler>();
		if ((Object)(object)scaler != null)
		{
			pixelScale = scaler.referencePixelsPerUnit;
		}
		displayImage = GetComponent<RawImage>();
		((Graphic)displayImage).raycastTarget = false;
		Vector3[] array = new Vector3[4];
		((Graphic)displayImage).rectTransform.GetWorldCorners(array);
		width = Vector3.Distance(array[0], array[3]);
		height = Vector3.Distance(array[0], array[1]);
		pixelWidth = (int)(width * pixelScale);
		pixelHeight = (int)(height * pixelScale);
		cam.orthographicSize = Mathf.Max(width, height) / 2f;
	}

	private void CreateTex()
	{
		if (pixelWidth > 0 && pixelHeight > 0)
		{
			rt = new RenderTexture(pixelWidth, pixelHeight, 32, RenderTextureFormat.ARGB32);
			rt.filterMode = FilterMode.Point;
			cam.targetTexture = rt;
			displayImage.texture = rt;
		}
	}
}
