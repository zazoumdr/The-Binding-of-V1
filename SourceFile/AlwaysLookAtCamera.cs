using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour
{
	public UpdateType updateType = UpdateType.LateUpdate;

	public TargetData? overrideTargetData;

	public Transform overrideTarget;

	public EnemyTarget target;

	[Space]
	[Tooltip("If the target is player (null), use the camera instead of the player head position. Helpful in third-person mode.")]
	public bool preferCameraOverHead;

	[Tooltip("Copies camera's rotation instead of looking at the camera, this will mean the object always appears flat like a sprite.")]
	public bool faceScreenInsteadOfCamera;

	[Tooltip("Always face camera's origin point instead of tracking the camera when it's shaking.")]
	public bool ignoreScreenShake;

	public bool dontRotateIfBlind;

	[Tooltip("Only track the target if there is a direct line of sight, otherwise stay still")]
	public bool onlyTrackWithLineOfSight;

	public float speed;

	public bool easeIn;

	public float maxAngle;

	[Space]
	public bool useXAxis = true;

	public bool useYAxis = true;

	public bool useZAxis = true;

	[Space]
	public Vector3 rotationOffset;

	[Space]
	public float maxXAxisFromParent;

	public float maxYAxisFromParent;

	public float maxZAxisFromParent;

	public bool useLocalMode;

	public bool dontUseFaceCamera;

	[Header("Enemy")]
	public EnemyIdentifier eid;

	private int difficulty;

	public bool difficultyVariance;

	private float difficultySpeedMultiplier = 1f;

	private ParticleSystemForceUpdater[] particleSystemForceUpdaters;

	private PortalManagerV2 _subscribedPortalManager;

	public unsafe void FaceCamera(Matrix4x4 matrix)
	{
		if (dontUseFaceCamera || target == null || !target.isPlayer || speed != 0f || (bool)overrideTarget || overrideTargetData.HasValue)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (onlyTrackWithLineOfSight)
		{
			Vector3 position2 = target.position;
			if (Physics.Raycast(position, position2 - position, Vector3.Distance(position, position2), LayerMaskDefaults.Get(LMD.Environment)))
			{
				return;
			}
		}
		if (speed == 0f && useXAxis && useYAxis && useZAxis && faceScreenInsteadOfCamera)
		{
			float* ptr = (float*)(&matrix);
			Vector3 upwards = *(Vector3*)(ptr + 4);
			Vector3 forward = *(Vector3*)(ptr + 8);
			base.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * Quaternion.LookRotation(forward, upwards);
		}
		else
		{
			float* ptr2 = (float*)(&matrix);
			Vector3 worldPosition = *(Vector3*)(ptr2 + 12);
			if (!useXAxis)
			{
				worldPosition.x = position.x;
			}
			if (!useYAxis)
			{
				worldPosition.y = position.y;
			}
			if (!useZAxis)
			{
				worldPosition.z = position.z;
			}
			base.transform.LookAt(worldPosition);
		}
		if (maxXAxisFromParent != 0f || maxYAxisFromParent != 0f || maxZAxisFromParent != 0f || rotationOffset != Vector3.zero)
		{
			Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
			if (maxXAxisFromParent != 0f)
			{
				base.transform.localRotation = Quaternion.Euler(Mathf.Clamp(eulerAngles.x, 0f - maxXAxisFromParent, maxXAxisFromParent), eulerAngles.y, eulerAngles.z);
			}
			if (maxYAxisFromParent != 0f)
			{
				base.transform.localRotation = Quaternion.Euler(eulerAngles.x, Mathf.Clamp(eulerAngles.y, 0f - maxYAxisFromParent, maxYAxisFromParent), eulerAngles.z);
			}
			if (maxZAxisFromParent != 0f)
			{
				base.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, Mathf.Clamp(eulerAngles.z, 0f - maxZAxisFromParent, maxZAxisFromParent));
			}
			if (rotationOffset != Vector3.zero)
			{
				base.transform.localRotation = Quaternion.Euler(eulerAngles + rotationOffset);
			}
		}
		ParticleSystemForceUpdater[] array = particleSystemForceUpdaters;
		foreach (ParticleSystemForceUpdater particleSystemForceUpdater in array)
		{
			if (!(particleSystemForceUpdater == null))
			{
				particleSystemForceUpdater.ForceUpdate();
			}
		}
	}

	private void Start()
	{
		if ((bool)eid && eid.difficultyOverride >= 0)
		{
			difficulty = eid.difficultyOverride;
		}
		else
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		particleSystemForceUpdaters = new ParticleSystemForceUpdater[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystemForceUpdater orAddComponent = ((Component)(object)componentsInChildren[i]).GetOrAddComponent<ParticleSystemForceUpdater>();
			particleSystemForceUpdaters[i] = orAddComponent;
		}
		UpdateDifficulty();
		EnsureTargetExists();
		SlowUpdate();
	}

	private void OnEnable()
	{
		if (MonoSingleton<AlwaysLookAtCameraManager>.TryGetInstance(out AlwaysLookAtCameraManager instance))
		{
			instance.Add(this);
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<AlwaysLookAtCameraManager>.TryGetInstance(out AlwaysLookAtCameraManager instance))
		{
			instance.Remove(this);
		}
	}

	private void OnDestroy()
	{
		if (MonoSingleton<AlwaysLookAtCameraManager>.TryGetInstance(out AlwaysLookAtCameraManager instance))
		{
			instance.Remove(this);
		}
	}

	private void EnsureTargetExists()
	{
		if (target == null || !target.isValid)
		{
			target = ((overrideTarget == null) ? EnemyTarget.TrackPlayer() : new EnemyTarget(overrideTarget));
		}
		else if (overrideTarget != null && target.trackedTransform != overrideTarget)
		{
			target = new EnemyTarget(overrideTarget);
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.5f);
		EnsureTargetExists();
	}

	private void Update()
	{
		if (updateType == UpdateType.Update)
		{
			Tick();
		}
	}

	private void FixedUpdate()
	{
		if (updateType == UpdateType.FixedUpdate)
		{
			Tick();
		}
	}

	private void LateUpdate()
	{
		if (updateType == UpdateType.LateUpdate)
		{
			Tick();
		}
	}

	private void Tick()
	{
		if ((target == null && !overrideTargetData.HasValue) || !target.isValid || (dontRotateIfBlind && BlindEnemies.Blind))
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (onlyTrackWithLineOfSight)
		{
			Vector3 position2 = target.position;
			if (Physics.Raycast(position, position2 - position, Vector3.Distance(position, position2), LayerMaskDefaults.Get(LMD.Environment)))
			{
				return;
			}
		}
		float num = speed;
		if ((bool)eid)
		{
			num *= eid.totalSpeedModifier;
		}
		if (difficultyVariance)
		{
			num *= difficultySpeedMultiplier;
		}
		bool flag = (overrideTargetData.HasValue ? overrideTargetData.Value.target.isPlayer : target.isPlayer);
		Transform transform = ((!((preferCameraOverHead || faceScreenInsteadOfCamera) && flag)) ? target.headTransform : MonoSingleton<CameraController>.Instance.cam.transform);
		if (speed == 0f && useXAxis && useYAxis && useZAxis)
		{
			if (flag && ignoreScreenShake)
			{
				base.transform.LookAt(MonoSingleton<CameraController>.Instance.GetDefaultPos());
			}
			else if (faceScreenInsteadOfCamera)
			{
				base.transform.rotation = transform.rotation;
				base.transform.Rotate(Vector3.up * 180f, Space.Self);
			}
			else if (overrideTargetData.HasValue)
			{
				base.transform.LookAt(overrideTargetData.Value.position);
			}
			else
			{
				base.transform.LookAt(transform);
			}
		}
		else
		{
			Vector3 vector = (overrideTargetData.HasValue ? overrideTargetData.Value.position : transform.position);
			if (flag && ignoreScreenShake)
			{
				vector = MonoSingleton<CameraController>.Instance.GetDefaultPos();
			}
			if (!useLocalMode)
			{
				if (!useXAxis)
				{
					vector.x = position.x;
				}
				if (!useYAxis)
				{
					vector.y = position.y;
				}
				if (!useZAxis)
				{
					vector.z = position.z;
				}
			}
			Quaternion quaternion = Quaternion.LookRotation(vector - position, Vector3.up);
			Quaternion b = quaternion;
			if (useLocalMode)
			{
				Transform parent = base.transform.parent;
				Vector3 eulerAngles = (Quaternion.Inverse((parent != null) ? parent.rotation : Quaternion.identity) * quaternion).eulerAngles;
				Vector3 eulerAngles2 = base.transform.localRotation.eulerAngles;
				if (useXAxis)
				{
					eulerAngles.x = eulerAngles2.x;
				}
				if (useYAxis)
				{
					eulerAngles.y = eulerAngles2.y;
				}
				if (useZAxis)
				{
					eulerAngles.z = eulerAngles2.z;
				}
				quaternion = Quaternion.Euler(eulerAngles);
				b = ((parent != null) ? parent.rotation : Quaternion.identity) * quaternion;
			}
			if (maxAngle != 0f && Quaternion.Angle(base.transform.rotation, b) > maxAngle)
			{
				return;
			}
			if (speed == 0f)
			{
				if (useLocalMode)
				{
					base.transform.localRotation = quaternion;
				}
				else
				{
					base.transform.rotation = quaternion;
				}
			}
			if (easeIn)
			{
				float num2 = 1f;
				if (difficultyVariance)
				{
					if (difficulty == 1)
					{
						num2 = 0.8f;
					}
					else if (difficulty == 0)
					{
						num2 = 0.5f;
					}
				}
				if (useLocalMode)
				{
					base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, quaternion, Time.deltaTime * num * (Quaternion.Angle(base.transform.localRotation, quaternion) * num2));
				}
				else
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * num * (Quaternion.Angle(base.transform.rotation, quaternion) * num2));
				}
			}
			else if (useLocalMode)
			{
				base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, quaternion, Time.deltaTime * num);
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * num);
			}
		}
		Vector3 eulerAngles3 = base.transform.localRotation.eulerAngles;
		if (maxXAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(Mathf.Clamp(eulerAngles3.x, 0f - maxXAxisFromParent, maxXAxisFromParent), eulerAngles3.y, eulerAngles3.z);
		}
		if (maxYAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(eulerAngles3.x, Mathf.Clamp(eulerAngles3.y, 0f - maxYAxisFromParent, maxYAxisFromParent), eulerAngles3.z);
		}
		if (maxZAxisFromParent != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(eulerAngles3.x, eulerAngles3.y, Mathf.Clamp(eulerAngles3.z, 0f - maxZAxisFromParent, maxZAxisFromParent));
		}
		if (rotationOffset != Vector3.zero)
		{
			base.transform.localRotation = Quaternion.Euler(eulerAngles3 + rotationOffset);
		}
	}

	public void ChangeOverrideTarget(EnemyTarget target)
	{
		this.target = target;
		overrideTarget = target.trackedTransform;
	}

	public void ChangeOverrideTarget(Transform target)
	{
		this.target = new EnemyTarget(target);
		overrideTarget = target;
	}

	public void SnapToTarget()
	{
		EnsureTargetExists();
		if (target != null)
		{
			Vector3 headPosition = target.headPosition;
			if (!useXAxis)
			{
				headPosition.x = base.transform.position.x;
			}
			if (!useYAxis)
			{
				headPosition.y = base.transform.position.y;
			}
			if (!useZAxis)
			{
				headPosition.z = base.transform.position.z;
			}
			Quaternion rotation = Quaternion.LookRotation(headPosition - base.transform.position);
			base.transform.rotation = rotation;
		}
	}

	public void ChangeSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	public void ChangeDifficulty(int newDiff)
	{
		difficulty = newDiff;
		UpdateDifficulty();
	}

	public void UpdateDifficulty()
	{
		if (difficulty == 1)
		{
			difficultySpeedMultiplier = 0.8f;
		}
		else if (difficulty == 0)
		{
			difficultySpeedMultiplier = 0.6f;
		}
	}
}
