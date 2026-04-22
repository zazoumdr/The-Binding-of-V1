using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCubemap : MonoBehaviour
{
	public int startRecursions = 1;

	public bool automaticPosition = true;

	public CubemapMode cubemapMode;

	public float cubemapStrength = 0.5f;

	public List<MeshRenderer> additionalRenderers = new List<MeshRenderer>();

	private Transform room;

	private MeshRenderer[] roomObjects;

	private Bounds roomBounds;

	private Cubemap cubemap;

	private Camera cam;

	private MaterialPropertyBlock propertyBlock;

	private void Awake()
	{
		room = base.transform.parent;
		roomObjects = room.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		cam = base.gameObject.AddComponent<Camera>();
		cam.enabled = false;
		if (automaticPosition)
		{
			roomBounds = roomObjects[0].bounds;
			for (int i = 1; i < roomObjects.Length; i++)
			{
				roomBounds.Encapsulate(roomObjects[i].bounds);
			}
			cam.transform.position = roomBounds.center;
		}
		int num = 1;
		num |= 0x40;
		num |= 0x80;
		num |= 0x100;
		num |= 0x1000000;
		num |= 0x2000000;
		cam.cullingMask = num;
		cubemap = new Cubemap(256, TextureFormat.ARGB32, mipChain: false);
		cubemap.filterMode = FilterMode.Point;
		propertyBlock = new MaterialPropertyBlock();
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateCubeMapNextFrame());
	}

	private IEnumerator UpdateCubeMapNextFrame()
	{
		yield return null;
		for (int i = 0; i < startRecursions; i++)
		{
			UpdateCubemap();
		}
		startRecursions = 1;
	}

	public void UpdateCubemap()
	{
		cam.RenderToCubemap(cubemap);
		roomObjects = room.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < roomObjects.Length; i++)
		{
			MeshRenderer obj = roomObjects[i];
			obj.GetPropertyBlock(propertyBlock);
			propertyBlock.SetTexture("_CubeTex", cubemap);
			propertyBlock.SetFloat("_CubeMode", (float)cubemapMode);
			propertyBlock.SetFloat("_ReflectionStrength", cubemapStrength);
			obj.SetPropertyBlock(propertyBlock);
		}
		foreach (MeshRenderer additionalRenderer in additionalRenderers)
		{
			additionalRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetTexture("_CubeTex", cubemap);
			propertyBlock.SetFloat("_ReflectionStrength", cubemapStrength);
			additionalRenderer.SetPropertyBlock(propertyBlock);
		}
	}

	public void DelayUpdate(float delayTime)
	{
		Invoke("UpdateCubemap", delayTime);
	}
}
