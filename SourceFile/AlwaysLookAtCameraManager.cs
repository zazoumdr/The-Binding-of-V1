using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class AlwaysLookAtCameraManager : MonoSingleton<AlwaysLookAtCameraManager>
{
	internal List<AlwaysLookAtCamera> lookAtCameraList = new List<AlwaysLookAtCamera>();

	private PortalManagerV2 _subscribedPortalManager;

	public void Add(AlwaysLookAtCamera lookAtCamera)
	{
		if (!lookAtCameraList.Contains(lookAtCamera))
		{
			lookAtCameraList.Add(lookAtCamera);
		}
	}

	public void Remove(AlwaysLookAtCamera lookAtCamera)
	{
		lookAtCameraList.Remove(lookAtCamera);
	}

	private void OnEnable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 portalManagerV))
		{
			_subscribedPortalManager = portalManagerV;
			portalManagerV.RenderFrom += UpdateLookAts;
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 portalManagerV))
		{
			_subscribedPortalManager = portalManagerV;
			portalManagerV.RenderFrom += UpdateLookAts;
		}
	}

	private void UpdateLookAts(Camera cam)
	{
		Matrix4x4 cameraToWorldMatrix = cam.cameraToWorldMatrix;
		for (int i = 0; i < lookAtCameraList.Count; i++)
		{
			lookAtCameraList[i].FaceCamera(cameraToWorldMatrix);
		}
	}
}
