using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DoubleRender : MonoBehaviour
{
	public List<int> subMeshesToIgnore = new List<int>();

	public Material radiantMat;

	public Renderer thisRend;

	private CommandBuffer cb;

	private CameraController cc;

	private Camera currentCam;

	public int shouldOutline;

	private bool isActive;

	private void Awake()
	{
		cc = MonoSingleton<CameraController>.Instance;
		currentCam = cc.cam;
		radiantMat = new Material(MonoSingleton<PostProcessV2_Handler>.Instance.radiantBuff);
		thisRend = GetComponent<Renderer>();
		cb = new CommandBuffer
		{
			name = "BuffRender"
		};
		Mesh mesh = null;
		if (thisRend is SkinnedMeshRenderer)
		{
			mesh = (thisRend as SkinnedMeshRenderer).sharedMesh;
		}
		else if (thisRend is MeshRenderer)
		{
			mesh = thisRend.GetComponent<MeshFilter>().sharedMesh;
		}
		if (mesh != null)
		{
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				if (!subMeshesToIgnore.Contains(i))
				{
					cb.DrawRenderer(thisRend, radiantMat, i);
				}
			}
		}
		radiantMat.SetFloat("_ForceOutline", 0.5f);
		PostProcessV2_Handler? instance = MonoSingleton<PostProcessV2_Handler>.Instance;
		instance.onReinitialize = (Action<bool>)Delegate.Combine(instance.onReinitialize, new Action<bool>(Reinitialize));
	}

	public void Reinitialize(bool forceReinitialize = false)
	{
		if (!thisRend.enabled && (forceReinitialize || isActive))
		{
			currentCam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
			isActive = false;
		}
		if (thisRend.enabled && (forceReinitialize || !isActive))
		{
			currentCam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
			isActive = true;
		}
		radiantMat.SetFloat("_Outline", shouldOutline);
		if (currentCam != cc.cam)
		{
			currentCam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
			currentCam = cc.cam;
			currentCam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
		}
	}

	private void LateUpdate()
	{
		Reinitialize();
	}

	public void OnDisable()
	{
		if (currentCam != null)
		{
			currentCam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
		}
		isActive = false;
		if (MonoSingleton<PostProcessV2_Handler>.Instance != null)
		{
			PostProcessV2_Handler? instance = MonoSingleton<PostProcessV2_Handler>.Instance;
			instance.onReinitialize = (Action<bool>)Delegate.Remove(instance.onReinitialize, new Action<bool>(Reinitialize));
		}
	}

	public void RemoveEffect()
	{
		currentCam.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cb);
		if (MonoSingleton<PostProcessV2_Handler>.Instance != null)
		{
			PostProcessV2_Handler? instance = MonoSingleton<PostProcessV2_Handler>.Instance;
			instance.onReinitialize = (Action<bool>)Delegate.Remove(instance.onReinitialize, new Action<bool>(Reinitialize));
		}
		UnityEngine.Object.Destroy(this);
		isActive = false;
	}

	public void SetOutline(int showOultine)
	{
		shouldOutline = showOultine;
	}
}
