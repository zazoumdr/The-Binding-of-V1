using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CameraController : MonoSingleton<CameraController>
{
	private const float RotationOffsetSpeed = 5f;

	private const float TransitionRotationZSmoothTime = 0.25f;

	private const float TiltRotationZSmoothTime = 0.5f;

	private const float GravitySlerpSnapAngle = 0.01f;

	public bool invert;

	public float minimumX = -89f;

	public float maximumX = 89f;

	public float minimumY = -360f;

	public float maximumY = 360f;

	public OptionsManager opm;

	public float scroll;

	public Vector3 defaultTarget;

	public Vector3 originalPos;

	public Vector3 defaultPos;

	private Vector3 targetPos;

	public GameObject player;

	public NewMovement nm;

	[HideInInspector]
	public Camera cam;

	public bool activated;

	public int gamepadFreezeCount;

	public float rotationY;

	public float rotationX;

	public float transitionRotationZ;

	public float tiltRotationZ;

	public bool reverseX;

	public bool reverseY;

	public float cameraShaking;

	public float movementHor;

	public float movementVer;

	public int dodgeDirection;

	public float defaultFov;

	private AudioMixer[] audmix;

	private bool mouseUnlocked;

	private AssistController asscon;

	[SerializeField]
	private GameObject parryLight;

	[SerializeField]
	private GameObject parryFlash;

	[SerializeField]
	private Camera hudCamera;

	private float aspectRatio;

	private bool pixeled;

	private bool tilt;

	private float currentStop;

	private bool zooming;

	private float zoomTarget;

	private LayerMask environmentMask;

	public bool platformerCamera;

	public Quaternion rotationOffset = Quaternion.identity;

	public Quaternion gravityRotation = Quaternion.identity;

	public Vector3 gravityVec;

	public float transitionRotationZSmooth;

	public float tiltRotationZSmooth;

	public Quaternion rotation;

	private void Awake()
	{
		audmix = (AudioMixer[])(object)new AudioMixer[5]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.goreSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound,
			MonoSingleton<AudioMixerController>.Instance.doorSound,
			MonoSingleton<AudioMixerController>.Instance.unfreezeableSound
		};
		nm = MonoSingleton<NewMovement>.Instance;
		if ((bool)nm)
		{
			player = nm.gameObject;
		}
	}

	private void Start()
	{
		cam = GetComponent<Camera>();
		if ((bool)MonoSingleton<StatsManager>.Instance)
		{
			asscon = MonoSingleton<AssistController>.Instance;
		}
		originalPos = base.transform.localPosition;
		defaultPos = base.transform.localPosition;
		defaultTarget = base.transform.localPosition;
		targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.2f, defaultPos.z);
		float fieldOfView = MonoSingleton<PrefsManager>.Instance.GetFloat("fieldOfView");
		if (platformerCamera)
		{
			fieldOfView = 105f;
		}
		cam.fieldOfView = fieldOfView;
		defaultFov = cam.fieldOfView;
		tilt = MonoSingleton<PrefsManager>.Instance.GetBool("cameraTilt");
		if (opm == null && (bool)MonoSingleton<StatsManager>.Instance && (bool)MonoSingleton<OptionsManager>.Instance)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
		}
		AudioMixer[] array = audmix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat("allPitch", 1f);
		}
		CheckAspectRatio();
		CheckMouseReverse();
		environmentMask = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void OnEnable()
	{
		if (MonoSingleton<OptionsManager>.Instance.frozen || MonoSingleton<OptionsManager>.Instance.paused)
		{
			MonoSingleton<CameraController>.Instance.activated = true;
			activated = false;
		}
		CheckAspectRatio();
		CheckMouseReverse();
		float fieldOfView = MonoSingleton<PrefsManager>.Instance.GetFloat("fieldOfView");
		if (platformerCamera)
		{
			fieldOfView = 105f;
		}
		cam.fieldOfView = fieldOfView;
		defaultFov = cam.fieldOfView;
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "cameraTilt":
			if (value is bool flag)
			{
				tilt = flag;
			}
			break;
		case "fieldOfView":
			if (value is float fieldOfView)
			{
				defaultFov = fieldOfView;
				cam.fieldOfView = fieldOfView;
			}
			break;
		case "mouseReverseX":
		case "mouseReverseY":
			CheckMouseReverse();
			break;
		case "resolutionHeight":
		case "resolutionWidth":
		case "fullscreen":
			CheckAspectRatio();
			break;
		}
	}

	private void LateUpdate()
	{
		if (!nm)
		{
			return;
		}
		Vector3 gravityDirection = player.GetGravityDirection();
		gravityVec = Vector3.Slerp(gravityVec, gravityDirection, 5f * Time.deltaTime);
		if (Vector3.Angle(gravityVec, gravityDirection) < 0.01f)
		{
			gravityVec = gravityDirection;
		}
		Vector3 fromDirection = gravityRotation * Vector3.up;
		Vector3 toDirection = -gravityVec;
		gravityRotation = Quaternion.FromToRotation(fromDirection, toDirection) * gravityRotation;
		CheckAspectRatio();
		if (Input.GetKeyDown(KeyCode.F1) && Debug.isDebugBuild)
		{
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
		if (cameraShaking > 0f)
		{
			if ((bool)MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused)
			{
				base.transform.localPosition = defaultPos;
			}
			else
			{
				Vector3 vector = base.transform.parent.localToWorldMatrix.MultiplyPoint3x4(defaultPos);
				Vector3 vector2 = vector;
				if (cameraShaking > 1f)
				{
					vector2 += base.transform.right * UnityEngine.Random.Range(-1, 2);
					vector2 += base.transform.up * UnityEngine.Random.Range(-1, 2);
				}
				else
				{
					vector2 += base.transform.right * (cameraShaking * UnityEngine.Random.Range(-1f, 1f));
					vector2 += base.transform.up * (cameraShaking * UnityEngine.Random.Range(-1f, 1f));
				}
				if (Physics.Raycast(vector, vector2 - vector, out var hitInfo, Vector3.Distance(vector2, vector) + 0.4f, environmentMask))
				{
					base.transform.position = hitInfo.point - (vector2 - vector).normalized * 0.5f;
				}
				else
				{
					base.transform.position = vector2;
				}
				cameraShaking -= Time.unscaledDeltaTime * 3f;
			}
		}
		if (platformerCamera)
		{
			return;
		}
		if (player == null)
		{
			player = nm.gameObject;
		}
		scroll = Input.GetAxis("Mouse ScrollWheel");
		bool flag = activated;
		if (MonoSingleton<InputManager>.TryGetInstance(out InputManager inputManager) && inputManager.LastButtonDevice is Gamepad && gamepadFreezeCount > 0)
		{
			flag = false;
		}
		if (GameStateManager.Instance.CameraLocked)
		{
			flag = false;
		}
		if (flag)
		{
			float num = 1f;
			Vector2 vector3 = MonoSingleton<InputManager>.Instance.InputSource.Look.ReadValue<Vector2>();
			if (zooming)
			{
				num = cam.fieldOfView / defaultFov;
			}
			if (!reverseY)
			{
				rotationX += vector3.y * (opm.mouseSensitivity / 10f) * num;
			}
			else
			{
				rotationX -= vector3.y * (opm.mouseSensitivity / 10f) * num;
			}
			if (!reverseX)
			{
				rotationY += vector3.x * (opm.mouseSensitivity / 10f) * num;
			}
			else
			{
				rotationY -= vector3.x * (opm.mouseSensitivity / 10f) * num;
			}
			float f = Mathf.DeltaAngle(0f, rotationX);
			if (Mathf.Abs(f) > 90f)
			{
				rotationX = 90f * Mathf.Sign(f);
			}
			ApplyRotations();
			if (rotationOffset != Quaternion.identity)
			{
				rotationOffset = Quaternion.Slerp(rotationOffset, Quaternion.identity, Time.deltaTime * 5f);
				if (Quaternion.Angle(rotationOffset, Quaternion.identity) < 0.1f)
				{
					rotationOffset = Quaternion.identity;
				}
			}
		}
		if (Time.deltaTime > 0f)
		{
			transitionRotationZ = Mathf.SmoothDampAngle(transitionRotationZ, 0f, ref transitionRotationZSmooth, 0.25f);
			tiltRotationZ = Mathf.SmoothDampAngle(tiltRotationZ, 0f, ref tiltRotationZSmooth, 0.5f);
		}
		if (zooming)
		{
			cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, zoomTarget, Time.deltaTime * 300f);
		}
		else if (nm.boost)
		{
			if (dodgeDirection == 0)
			{
				cam.fieldOfView = defaultFov - defaultFov / 20f;
			}
			else if (dodgeDirection == 1)
			{
				cam.fieldOfView = defaultFov + defaultFov / 10f;
			}
		}
		else
		{
			cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, defaultFov, Time.deltaTime * 300f);
		}
		if ((bool)hudCamera)
		{
			if (zooming)
			{
				hudCamera.fieldOfView = Mathf.MoveTowards(hudCamera.fieldOfView, zoomTarget, Time.deltaTime * 300f);
			}
			else if (hudCamera.fieldOfView != 90f)
			{
				hudCamera.fieldOfView = Mathf.MoveTowards(hudCamera.fieldOfView, 90f, Time.deltaTime * 300f);
			}
		}
		float num2 = 0f;
		float num3 = movementHor * -1f;
		float num4 = tiltRotationZ;
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		num2 = ((!tilt) ? Mathf.MoveTowards(num4, 0f, Time.deltaTime * 25f * (Mathf.Abs(num4) + 0.01f)) : (nm.boost ? Mathf.MoveTowards(num4, num3 * 5f, Time.deltaTime * 100f * (Mathf.Abs(num4 - num3 * 5f) + 0.01f)) : Mathf.MoveTowards(num4, num3, Time.deltaTime * 25f * (Mathf.Abs(num4 - num3) + 0.01f))));
		if (flag)
		{
			tiltRotationZ = num2;
		}
		if (defaultPos != defaultTarget)
		{
			defaultPos = Vector3.MoveTowards(defaultPos, defaultTarget, ((defaultTarget - defaultPos).magnitude + 0.5f) * Time.deltaTime * 10f);
		}
		if (!nm.activated || !(cameraShaking <= 0f))
		{
			return;
		}
		if (nm.walking && nm.standing && defaultPos == defaultTarget)
		{
			base.transform.localPosition = new Vector3(Mathf.MoveTowards(base.transform.localPosition.x, targetPos.x, Time.deltaTime * 0.5f), Mathf.MoveTowards(base.transform.localPosition.y, targetPos.y, Time.deltaTime * 0.5f * (Mathf.Min(nm.rb.velocity.magnitude, 15f) / 15f)), Mathf.MoveTowards(base.transform.localPosition.z, targetPos.z, Time.deltaTime * 0.5f));
			if (base.transform.localPosition == targetPos && targetPos != defaultPos)
			{
				targetPos = defaultPos;
			}
			else if (base.transform.localPosition == targetPos && targetPos == defaultPos)
			{
				targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
			}
		}
		else
		{
			base.transform.localPosition = defaultPos;
			targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
		}
	}

	public void ApplyRotations(bool debug = false)
	{
		player.transform.localRotation = gravityRotation * Quaternion.AngleAxis(rotationY, Vector3.up);
		MonoSingleton<NewMovement>.Instance.rb.rotation = player.transform.rotation;
		base.transform.localRotation = Quaternion.AngleAxis(0f - rotationX, Vector3.right) * Quaternion.AngleAxis(transitionRotationZ, Vector3.forward) * Quaternion.AngleAxis(tiltRotationZ, Vector3.forward) * rotationOffset;
	}

	public void Transform(Matrix4x4 matrix, Vector3? gravity, Quaternion? proposedRotation = null)
	{
		Quaternion quaternion = gravityRotation;
		if (gravity.HasValue)
		{
			Vector3 vector = gravityVec;
			gravityVec = gravity.Value.normalized;
			if (vector.normalized != gravityVec)
			{
				_ = matrix.rotation * quaternion;
				gravityRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(matrix.MultiplyVector(base.transform.forward), gravityVec), -gravityVec);
			}
		}
		Quaternion quaternion2 = quaternion * Quaternion.AngleAxis(rotationY, Vector3.up) * Quaternion.AngleAxis(0f - rotationX, Vector3.right) * Quaternion.AngleAxis(transitionRotationZ, Vector3.forward) * rotationOffset;
		rotationOffset = Quaternion.identity;
		Quaternion quaternion3 = matrix.rotation * quaternion2;
		Quaternion quaternion4 = Quaternion.Inverse(gravityRotation);
		Quaternion quaternion5 = quaternion4 * quaternion3;
		Quaternion quaternion6 = quaternion5;
		if (proposedRotation.HasValue)
		{
			Quaternion quaternion7 = quaternion4 * proposedRotation.Value;
			float num = Mathf.Abs(Mathf.DeltaAngle(0f, quaternion5.eulerAngles.z));
			float num2 = Mathf.Abs(Mathf.DeltaAngle(0f, quaternion7.eulerAngles.z));
			if (num > 50f && num2 < num - 30f)
			{
				rotationOffset = Quaternion.Inverse(quaternion7) * quaternion5;
				quaternion6 = quaternion7;
			}
		}
		rotationY = quaternion6.eulerAngles.y;
		rotationX = 0f - quaternion6.eulerAngles.x;
		transitionRotationZ = quaternion6.eulerAngles.z;
		ApplyRotations();
	}

	public void SetRotation(Quaternion rotation, Vector3 newGravity, Quaternion? offsetRotation = null)
	{
		Vector3 eulerAngles = rotation.eulerAngles;
		rotationX = 0f - eulerAngles.x;
		rotationY = eulerAngles.y;
		transitionRotationZ = eulerAngles.z;
		ApplyRotations(debug: true);
		if (offsetRotation.HasValue)
		{
			rotationOffset = offsetRotation.Value;
		}
	}

	private void FixedUpdate()
	{
		_ = activated;
	}

	public void CameraShake(float shakeAmount)
	{
		float num = MonoSingleton<PrefsManager>.Instance.GetFloat("screenShake");
		if (num != 0f && cameraShaking < shakeAmount * num)
		{
			cameraShaking = shakeAmount * num;
		}
	}

	public void StopShake()
	{
		cameraShaking = 0f;
		base.transform.localPosition = defaultPos;
	}

	public void ResetCamera(float degreesY, float degreesX = 0f)
	{
		rotationY = degreesY;
		rotationX = degreesX;
		transitionRotationZ = 0f;
		transitionRotationZSmooth = 0f;
		tiltRotationZ = 0f;
		tiltRotationZSmooth = 0f;
		rotationOffset = Quaternion.identity;
	}

	public void Zoom(float amount)
	{
		zooming = true;
		zoomTarget = amount;
	}

	public void StopZoom()
	{
		zooming = false;
	}

	public void ResetToDefaultPos()
	{
		base.transform.localPosition = defaultPos;
		targetPos = new Vector3(defaultPos.x, defaultPos.y - 0.1f, defaultPos.z);
	}

	public Vector3 GetDefaultPos()
	{
		return base.transform.parent.localToWorldMatrix.MultiplyPoint3x4(defaultPos);
	}

	public void CheckAspectRatio()
	{
		if (!cam)
		{
			cam = GetComponent<Camera>();
		}
		if (!Mathf.Approximately(aspectRatio, cam.aspect))
		{
			aspectRatio = cam.aspect;
			float x = Mathf.Min(aspectRatio / 1.778f, 1f);
			if ((bool)hudCamera)
			{
				hudCamera.transform.localScale = new Vector3(x, 1f, 1f);
			}
		}
	}

	public void CheckMouseReverse()
	{
		reverseX = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseX");
		reverseY = MonoSingleton<PrefsManager>.Instance.GetBool("mouseReverseY");
	}
}
