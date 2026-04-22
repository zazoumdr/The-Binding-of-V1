using UnityEngine;

public class CorrectCameraView : MonoBehaviour
{
	private Camera mainCam;

	private Camera hudCam;

	private Vector3 lastpos;

	private Quaternion lastrot;

	public Transform targetObject;

	public bool canModifyTarget = true;

	private void OnEnable()
	{
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		hudCam = MonoSingleton<PostProcessV2_Handler>.Instance.hudCam;
	}

	private void OnDisable()
	{
	}

	private void LateUpdate()
	{
		if (canModifyTarget)
		{
			mainCam = MonoSingleton<CameraController>.Instance.cam;
			hudCam = MonoSingleton<PostProcessV2_Handler>.Instance.hudCam;
			Vector3 position = hudCam.WorldToScreenPoint(base.transform.position);
			position = mainCam.ScreenToWorldPoint(position);
			Quaternion rotation = Quaternion.LookRotation(base.transform.position + base.transform.forward * 4f - position);
			targetObject.SetPositionAndRotation(position, rotation);
		}
	}

	private void OnPostRenderCallback(Camera cam)
	{
		if (canModifyTarget && !(cam != mainCam))
		{
			targetObject.SetPositionAndRotation(lastpos, lastrot);
		}
	}
}
