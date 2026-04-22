using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class PlayerTracker : MonoSingleton<PlayerTracker>, ITarget
{
	public PlayerType playerType;

	private bool initialized;

	private NewMovement nmov;

	private CameraController cc;

	public GameObject platformerPlayerPrefab;

	[HideInInspector]
	public GameObject currentPlatformerPlayerPrefab;

	[HideInInspector]
	public PlatformerMovement pmov;

	private Transform player;

	private Transform target;

	private Rigidbody playerRb;

	[HideInInspector]
	public bool levelStarted;

	private bool startAsPlatformer;

	public PlatformerCameraType cameraType;

	public GameObject[] platformerFailSafes;

	public int Id
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.Id;
			}
			return nmov.Id;
		}
	}

	public TargetType Type => TargetType.PLAYER;

	public EnemyIdentifier EID => null;

	public GameObject GameObject
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.GameObject;
			}
			return nmov.GameObject;
		}
	}

	public Rigidbody Rigidbody
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.rb;
			}
			return nmov.rb;
		}
	}

	public Transform Transform
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.transform;
			}
			return nmov.transform;
		}
	}

	public Vector3 Position
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.Position;
			}
			return nmov.Position;
		}
	}

	public Vector3 HeadPosition
	{
		get
		{
			if (playerType != PlayerType.FPS)
			{
				return pmov.HeadPosition;
			}
			return nmov.HeadPosition;
		}
	}

	private void Start()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 portalManagerV))
		{
			portalManagerV.TargetTracker.RegisterTarget(this, base.destroyCancellationToken);
		}
	}

	public Transform GetPlayer()
	{
		if (!initialized)
		{
			Initialize();
		}
		return player;
	}

	public Transform GetTarget()
	{
		if (!initialized)
		{
			Initialize();
		}
		return target;
	}

	public Rigidbody GetRigidbody()
	{
		if (!initialized)
		{
			Initialize();
		}
		return playerRb;
	}

	public Vector3 PredictPlayerPosition(float time, bool aimAtHead = false, bool ignoreCollision = false)
	{
		Vector3 vector = GetPlayerVelocity() * time;
		if (!ignoreCollision && Physics.Raycast(playerRb.position, vector, out var hitInfo, vector.magnitude, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		if (aimAtHead)
		{
			return target.position + vector;
		}
		return playerRb.position + vector;
	}

	public Vector3 GetPlayerVelocity(bool trueVelocity = false)
	{
		if (!initialized)
		{
			Initialize();
		}
		Vector3 velocity = playerRb.velocity;
		if (!trueVelocity && MonoSingleton<NewMovement>.Instance.boost && !MonoSingleton<NewMovement>.Instance.sliding)
		{
			velocity /= 3f;
		}
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			velocity += MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity;
		}
		if (MonoSingleton<PlayerMovementParenting>.Instance != null)
		{
			Vector3 currentDelta = MonoSingleton<PlayerMovementParenting>.Instance.currentDelta;
			currentDelta *= 60f;
			velocity += currentDelta;
		}
		return velocity;
	}

	public bool GetOnGround()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (playerType != PlayerType.FPS || !MonoSingleton<NewMovement>.Instance.gc.onGround)
		{
			if (playerType == PlayerType.Platformer)
			{
				return MonoSingleton<PlatformerMovement>.Instance.groundCheck.onGround;
			}
			return false;
		}
		return true;
	}

	public void ChangeToPlatformer()
	{
		ChangeToPlatformer(false);
	}

	public void ChangeToPlatformer(bool ignorePreviousRotation = false)
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!pmov || !nmov || !currentPlatformerPlayerPrefab)
		{
			return;
		}
		if (cameraType == PlatformerCameraType.PlayerControlled)
		{
			pmov.freeCamera = true;
		}
		else
		{
			pmov.freeCamera = false;
		}
		if (!levelStarted)
		{
			startAsPlatformer = true;
		}
		else
		{
			if (playerType == PlayerType.Platformer)
			{
				return;
			}
			if (cameraType == PlatformerCameraType.PlayerControlled && !ignorePreviousRotation)
			{
				pmov.ResetCamera(MonoSingleton<CameraController>.Instance.rotationY, MonoSingleton<CameraController>.Instance.rotationX + 20f);
			}
			playerType = PlayerType.Platformer;
			ChangeTargetParent(player, pmov.transform, Vector3.up * 2.5f);
			ChangeTargetParent(target, pmov.transform, Vector3.up * 2.5f);
			nmov.gameObject.SetActive(value: false);
			currentPlatformerPlayerPrefab.transform.position = nmov.transform.position - Vector3.up * 1.5f;
			pmov.transform.position = currentPlatformerPlayerPrefab.transform.position;
			pmov.platformerCamera.transform.localPosition = Vector3.up * 2.5f;
			pmov.playerModel.transform.rotation = nmov.transform.rotation;
			currentPlatformerPlayerPrefab.SetActive(value: true);
			pmov.gameObject.SetActive(value: true);
			if ((bool)pmov.rb)
			{
				playerRb = pmov.rb;
			}
			else
			{
				playerRb = pmov.GetComponent<Rigidbody>();
			}
			pmov.CheckItem();
			playerRb.velocity = nmov.rb.velocity;
			MonoSingleton<PostProcessV2_Handler>.Instance?.ChangeCamera(hudless: true);
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
	}

	public void ChangeToFPS()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!pmov || !nmov || !currentPlatformerPlayerPrefab)
		{
			return;
		}
		if (!levelStarted)
		{
			startAsPlatformer = false;
		}
		else
		{
			if (playerType == PlayerType.FPS)
			{
				return;
			}
			playerType = PlayerType.FPS;
			nmov.transform.position = pmov.transform.position + Vector3.up * 1.5f;
			currentPlatformerPlayerPrefab.SetActive(value: false);
			playerRb = nmov.rb;
			nmov.gameObject.SetActive(value: true);
			ChangeTargetParent(player, nmov.transform);
			ChangeTargetParent(target, cc.transform);
			pmov.gameObject.SetActive(value: false);
			MonoSingleton<PostProcessV2_Handler>.Instance?.ChangeCamera(hudless: false);
			if (pmov.freeCamera)
			{
				cc.ResetCamera(pmov.rotationY, pmov.rotationX - 20f);
			}
			else
			{
				cc.ResetCamera(pmov.playerModel.transform.rotation.eulerAngles.y);
			}
			if ((bool)pmov.rb)
			{
				nmov.rb.velocity = pmov.rb.velocity;
			}
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void Initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		nmov = MonoSingleton<NewMovement>.Instance;
		cc = MonoSingleton<CameraController>.Instance;
		if (!nmov || !cc)
		{
			return;
		}
		Camera camera = null;
		if ((bool)cc && (bool)cc.cam)
		{
			camera = cc.cam;
		}
		else if ((bool)cc)
		{
			camera = cc.GetComponent<Camera>();
		}
		if (playerType == PlayerType.Platformer && !levelStarted)
		{
			startAsPlatformer = true;
			playerType = PlayerType.FPS;
		}
		GameObject gameObject = new GameObject();
		player = gameObject.transform;
		ChangeTargetParent(player, nmov.transform);
		if ((bool)nmov.rb)
		{
			playerRb = nmov.rb;
		}
		else
		{
			playerRb = nmov.GetComponent<Rigidbody>();
		}
		GameObject gameObject2 = new GameObject();
		target = gameObject2.transform;
		ChangeTargetParent(target, cc.transform);
		if (!pmov && !(player == null) && !(platformerPlayerPrefab == null))
		{
			currentPlatformerPlayerPrefab = Object.Instantiate(platformerPlayerPrefab, player.position, Quaternion.identity);
			pmov = currentPlatformerPlayerPrefab.GetComponentInChildren<PlatformerMovement>(includeInactive: true);
			if ((bool)camera)
			{
				currentPlatformerPlayerPrefab.GetComponentInChildren<Camera>(includeInactive: true).clearFlags = camera.clearFlags;
			}
		}
	}

	private void ChangeTargetParent(Transform toMove, Transform newParent, Vector3 offset = default(Vector3))
	{
		toMove.position = newParent.position + offset;
		toMove.SetParent(newParent);
	}

	public void CheckPlayerType()
	{
		if (playerType == PlayerType.FPS && (!MonoSingleton<NewMovement>.Instance || !MonoSingleton<NewMovement>.Instance.gameObject.activeInHierarchy))
		{
			ChangeToFPS();
		}
		else if (playerType == PlayerType.Platformer && (!currentPlatformerPlayerPrefab || !currentPlatformerPlayerPrefab.gameObject.activeInHierarchy))
		{
			ChangeToPlatformer();
		}
	}

	public void LevelStart()
	{
		if (levelStarted)
		{
			return;
		}
		levelStarted = true;
		if (MonoSingleton<OnLevelStart>.Instance != null)
		{
			MonoSingleton<OnLevelStart>.Instance.StartLevel();
		}
		if (startAsPlatformer)
		{
			ChangeToPlatformer(pmov.freeCamera);
		}
		else
		{
			if (playerType != PlayerType.FPS)
			{
				return;
			}
			GameObject[] array = platformerFailSafes;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
	}

	public void SetData(ref TargetData data)
	{
		if (playerType == PlayerType.FPS)
		{
			nmov.SetData(ref data);
		}
		else
		{
			pmov.SetData(ref data);
		}
	}

	public void UpdateCachedTransformData()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (playerType == PlayerType.FPS)
		{
			nmov.UpdateCachedTransformData();
		}
		else
		{
			pmov.UpdateCachedTransformData();
		}
	}
}
