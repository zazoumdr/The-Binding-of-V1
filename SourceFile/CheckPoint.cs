using System.Collections.Generic;
using Logic;
using Sandbox.Arm;
using TriInspector;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class CheckPoint : MonoBehaviour
{
	[HideInInspector]
	public StatsManager sm;

	[HideInInspector]
	public bool activated;

	private bool firstTime = true;

	public GameObject graphic;

	public AssetReference activateEffect;

	[Required]
	public GameObject toActivate;

	[Header("Targets")]
	public bool inheritAllRooms;

	public bool unlockAllDoors;

	public GameObject[] rooms;

	public List<GameObject> roomsToInherit = new List<GameObject>();

	private List<string> inheritNames = new List<string>();

	private List<Transform> inheritParents = new List<Transform>();

	[HideInInspector]
	public List<GameObject> defaultRooms = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> newRooms = new List<GameObject>();

	public Door[] doorsToUnlock;

	public Door[] doorsToIgnore;

	private int i;

	private GameObject player;

	private NewMovement nm;

	private float tempRot;

	[HideInInspector]
	public int restartKills;

	[HideInInspector]
	public int stylePoints;

	[HideInInspector]
	public bool challengeAlreadyFailed;

	[HideInInspector]
	public bool challengeAlreadyDone;

	[HideInInspector]
	public Vector3 gravity;

	private StyleHUD shud;

	[Header("Automatic Resets")]
	public bool resetOnGetOtherCheckpoint;

	public bool resetOnDistance;

	public float autoResetDistance = 15f;

	private float resetSafetyTimer;

	private bool inDuringResetSafety;

	[Space]
	public bool startOff;

	public bool forceOff;

	public bool disableDuringCombat;

	public bool unteleportable;

	public bool invisible;

	[HideInInspector]
	public List<int> succesfulHitters = new List<int>();

	[Space]
	public UnityEvent onRestart;

	[HideInInspector]
	public float additionalSpawnRotation;

	private void Start()
	{
		GameObject[] array = rooms;
		foreach (GameObject item in array)
		{
			defaultRooms.Add(item);
		}
		for (int j = 0; j < defaultRooms.Count; j++)
		{
			if (!defaultRooms[j].TryGetComponent<GoreZone>(out var component))
			{
				component = defaultRooms[j].AddComponent<GoreZone>();
			}
			component.checkpoint = this;
			newRooms.Add(Object.Instantiate(defaultRooms[j], defaultRooms[j].transform.position, defaultRooms[j].transform.rotation, defaultRooms[j].transform.parent));
			defaultRooms[j].gameObject.SetActive(value: false);
			newRooms[j].gameObject.SetActive(value: true);
			Bonus[] componentsInChildren = newRooms[j].GetComponentsInChildren<Bonus>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				Bonus[] array2 = componentsInChildren;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].UpdateStatsManagerReference();
				}
			}
			defaultRooms[j].transform.position = new Vector3(defaultRooms[j].transform.position.x + 10000f, defaultRooms[j].transform.position.y, defaultRooms[j].transform.position.z);
		}
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		sm = MonoSingleton<StatsManager>.Instance;
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		if (inheritAllRooms)
		{
			roomsToInherit.Clear();
			inheritNames.Clear();
			inheritParents.Clear();
			GoreZone[] array3 = Object.FindObjectsOfType<GoreZone>(includeInactive: true);
			for (int k = 0; k < array3.Length; k++)
			{
				if (array3[k].isNewest)
				{
					roomsToInherit.Add(array3[k].gameObject);
				}
			}
		}
		for (int l = 0; l < roomsToInherit.Count; l++)
		{
			inheritNames.Add(roomsToInherit[l].name);
			inheritParents.Add(roomsToInherit[l].transform.parent);
		}
		if (unlockAllDoors)
		{
			Door[] array4 = Object.FindObjectsOfType<Door>(includeInactive: true);
			if (doorsToIgnore == null || doorsToIgnore.Length == 0)
			{
				doorsToUnlock = new Door[array4.Length];
				for (int m = 0; m < array4.Length; m++)
				{
					doorsToUnlock[m] = array4[m];
				}
			}
			else
			{
				List<Door> list = new List<Door>();
				bool flag = false;
				for (int n = 0; n < array4.Length; n++)
				{
					for (int num = 0; num < doorsToIgnore.Length; num++)
					{
						if (array4[n] == doorsToIgnore[num])
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list.Add(array4[n]);
					}
					else
					{
						flag = false;
					}
				}
				doorsToUnlock = new Door[list.Count];
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					doorsToUnlock[num2] = list[num2];
				}
			}
		}
		MonoSingleton<CheckPointsController>.Instance.AddCheckpoint(this);
		if (startOff)
		{
			activated = true;
			graphic?.SetActive(value: false);
			if (TryGetComponent<ModifyMaterial>(out var component2))
			{
				component2.ChangeEmissionIntensity(0f);
			}
		}
	}

	private void OnDisable()
	{
		inDuringResetSafety = false;
	}

	private bool ShouldBeOff()
	{
		if (!forceOff)
		{
			if (disableDuringCombat)
			{
				return MonoSingleton<MusicManager>.Instance.requestedThemes > 0f;
			}
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (resetSafetyTimer > 0f)
		{
			resetSafetyTimer = Mathf.MoveTowards(resetSafetyTimer, 0f, Time.deltaTime);
			if (resetSafetyTimer == 0f && inDuringResetSafety && !activated)
			{
				ActivateCheckPoint();
			}
		}
		if (activated && resetOnDistance && Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, base.transform.position) > autoResetDistance)
		{
			ReactivateCheckpoint();
		}
		if (!activated && (bool)graphic)
		{
			if ((ShouldBeOff() || invisible) && graphic.activeSelf)
			{
				graphic.SetActive(value: false);
			}
			else if (!ShouldBeOff() && !invisible && !graphic.activeSelf)
			{
				ReactivationEffect();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!activated && !ShouldBeOff() && ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)))
		{
			if (resetSafetyTimer > 0.25f)
			{
				inDuringResetSafety = true;
			}
			else
			{
				ActivateCheckPoint();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (inDuringResetSafety && ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)))
		{
			inDuringResetSafety = false;
		}
	}

	public void ActivateCheckPoint()
	{
		sm = MonoSingleton<StatsManager>.Instance;
		inDuringResetSafety = false;
		if ((bool)sm.currentCheckPoint && sm.currentCheckPoint != this)
		{
			MonoSingleton<NewMovement>.Instance.sameCheckpointRestarts = 0;
			if (sm.currentCheckPoint.resetOnGetOtherCheckpoint)
			{
				sm.currentCheckPoint.ReactivateCheckpoint();
			}
		}
		gravity = MonoSingleton<NewMovement>.Instance.rb.GetGravityVector();
		sm.currentCheckPoint = this;
		activated = true;
		if (!invisible && activateEffect.RuntimeKeyIsValid())
		{
			Object.Instantiate(activateEffect.ToAsset(), MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
		}
		if ((bool)graphic)
		{
			graphic.SetActive(value: false);
		}
		if ((bool)MonoSingleton<PlatformerMovement>.Instance)
		{
			MonoSingleton<CrateCounter>.Instance.SaveStuff();
		}
		if ((bool)MonoSingleton<MapVarManager>.Instance)
		{
			MonoSingleton<MapVarManager>.Instance.StashStore();
		}
		stylePoints = sm.stylePoints;
		restartKills = 0;
		if ((bool)MonoSingleton<ChallengeManager>.Instance)
		{
			challengeAlreadyFailed = MonoSingleton<ChallengeManager>.Instance.challengeFailed;
		}
		if ((bool)MonoSingleton<ChallengeManager>.Instance)
		{
			challengeAlreadyDone = MonoSingleton<ChallengeManager>.Instance.challengeDone;
		}
		if (!firstTime)
		{
			defaultRooms.Clear();
			newRooms.Clear();
			if (rooms.Length != 0)
			{
				GameObject[] array = rooms;
				foreach (GameObject gameObject in array)
				{
					roomsToInherit.Add(gameObject);
					inheritNames.Add(gameObject.name);
					inheritParents.Add(gameObject.transform.parent);
				}
				rooms = new GameObject[0];
			}
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		if (roomsToInherit.Count != 0)
		{
			for (int j = 0; j < roomsToInherit.Count; j++)
			{
				string text = inheritNames[j];
				text = text.Replace("(Clone)", "");
				GameObject gameObject2 = null;
				for (int num = inheritParents[j].childCount - 1; num >= 0; num--)
				{
					GameObject gameObject3 = inheritParents[j].GetChild(num).gameObject;
					if (gameObject3.name.Replace("(Clone)", "") == text)
					{
						if (gameObject2 == null)
						{
							gameObject2 = gameObject3;
						}
						else
						{
							Object.Destroy(gameObject3);
						}
					}
				}
				InheritRoom(gameObject2);
			}
		}
		MonoSingleton<BloodsplatterManager>.Instance.SaveBloodstains();
		firstTime = false;
	}

	public void OnRespawn()
	{
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		if ((bool)instance)
		{
			instance.Reset();
		}
		MonoSingleton<StainVoxelManager>.Instance.RemoveAllStains();
		MonoSingleton<BloodsplatterManager>.Instance.LoadBloodstains();
		if (player == null)
		{
			player = MonoSingleton<NewMovement>.Instance.gameObject;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (!nm)
		{
			nm = MonoSingleton<NewMovement>.Instance;
		}
		if (player.GetComponentInParent<GoreZone>() != null)
		{
			player.transform.parent = null;
		}
		player.transform.position = Vector3.one * -1000f;
		if ((bool)MonoSingleton<PlatformerMovement>.Instance)
		{
			if (MonoSingleton<PlatformerMovement>.Instance.GetComponentInParent<GoreZone>() != null)
			{
				MonoSingleton<PlatformerMovement>.Instance.transform.parent = null;
			}
			MonoSingleton<PlatformerMovement>.Instance.transform.position = Vector3.one * -1000f;
		}
		if ((bool)MonoSingleton<MapVarManager>.Instance)
		{
			MonoSingleton<MapVarManager>.Instance.RestoreStashedStore();
		}
		this.i = 0;
		if ((bool)SandboxArm.debugZone && !MapInfoBase.Instance.sandboxTools)
		{
			Object.Destroy(SandboxArm.debugZone.gameObject);
		}
		if (!activated)
		{
			activated = true;
			if (graphic != null)
			{
				graphic.SetActive(value: false);
			}
		}
		sm.kills -= restartKills;
		restartKills = 0;
		sm.stylePoints = stylePoints;
		if ((bool)MonoSingleton<ChallengeManager>.Instance)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeDone = challengeAlreadyDone && !MonoSingleton<ChallengeManager>.Instance.challengeFailedPermanently;
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = challengeAlreadyFailed || MonoSingleton<ChallengeManager>.Instance.challengeFailedPermanently;
		}
		if (succesfulHitters.Count > 0)
		{
			KillHitterCache instance2 = MonoSingleton<KillHitterCache>.Instance;
			if ((bool)instance2 && !instance2.ignoreRestarts)
			{
				foreach (int succesfulHitter in succesfulHitters)
				{
					instance2.RemoveId(succesfulHitter);
				}
			}
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		shud.ComboOver();
		shud.ResetAllFreshness();
		MonoSingleton<FistControl>.Instance.fistCooldown = 0f;
		if (doorsToUnlock.Length != 0)
		{
			Door[] array = doorsToUnlock;
			foreach (Door door in array)
			{
				if (!(door == null))
				{
					if (door.locked)
					{
						door.Unlock();
					}
					if (door.startOpen && !door.open)
					{
						door.Open();
					}
				}
			}
		}
		DestroyOnCheckpointRestart[] array2 = Object.FindObjectsOfType<DestroyOnCheckpointRestart>();
		if (array2 != null && array2.Length != 0)
		{
			DestroyOnCheckpointRestart[] array3 = array2;
			foreach (DestroyOnCheckpointRestart destroyOnCheckpointRestart in array3)
			{
				if (destroyOnCheckpointRestart.gameObject.activeInHierarchy && !destroyOnCheckpointRestart.dontDestroy)
				{
					Object.Destroy(destroyOnCheckpointRestart.gameObject);
				}
			}
		}
		Harpoon[] array4 = Object.FindObjectsOfType<Harpoon>();
		if (array4 != null && array4.Length != 0)
		{
			Harpoon[] array5 = array4;
			foreach (Harpoon harpoon in array5)
			{
				if (harpoon.gameObject.activeInHierarchy)
				{
					TimeBomb componentInChildren = harpoon.GetComponentInChildren<TimeBomb>();
					if ((bool)componentInChildren)
					{
						componentInChildren.dontExplode = true;
					}
					Object.Destroy(harpoon.gameObject);
				}
			}
		}
		DoorController[] array6 = Object.FindObjectsOfType<DoorController>();
		if (array6 != null && array6.Length != 0)
		{
			DoorController[] array7 = array6;
			for (int i = 0; i < array7.Length; i++)
			{
				array7[i].ForcePlayerOut();
			}
		}
		HookPoint[] array8 = Object.FindObjectsOfType<HookPoint>();
		if (array8 != null && array8.Length != 0)
		{
			HookPoint[] array9 = array8;
			foreach (HookPoint hookPoint in array9)
			{
				if (hookPoint.timer > 0f)
				{
					hookPoint.TimerStop();
				}
			}
		}
		MonoSingleton<CoinTracker>.Instance.Reset();
		if (newRooms.Count > 0)
		{
			ResetRoom();
		}
	}

	public void ResetRoom()
	{
		Vector3 position = newRooms[this.i].transform.position;
		newRooms[this.i].SetActive(value: false);
		Object.Destroy(newRooms[this.i]);
		newRooms[this.i] = Object.Instantiate(defaultRooms[this.i], position, defaultRooms[this.i].transform.rotation, defaultRooms[this.i].transform.parent);
		newRooms[this.i].SetActive(value: true);
		Bonus[] componentsInChildren = newRooms[this.i].GetComponentsInChildren<Bonus>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			Bonus[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateStatsManagerReference();
			}
		}
		if (this.i + 1 < defaultRooms.Count)
		{
			this.i++;
			ResetRoom();
			return;
		}
		if ((bool)toActivate)
		{
			toActivate.SetActive(value: true);
		}
		onRestart?.Invoke();
		if (!activated)
		{
			activated = true;
			if ((bool)graphic)
			{
				graphic.SetActive(value: false);
			}
		}
		player.transform.position = base.transform.position + base.transform.up * 1.25f;
		Rigidbody component = player.GetComponent<Rigidbody>();
		component.velocity = Vector3.zero;
		if (nm == null)
		{
			nm = MonoSingleton<NewMovement>.Instance;
		}
		CameraController cc = nm.cc;
		nm.rb.SetCustomGravityMode(useCustomGravity: false);
		cc.gravityRotation = Quaternion.identity;
		cc.gravityVec = Physics.gravity.normalized;
		cc.rotationOffset = Quaternion.identity;
		cc.transitionRotationZ = 0f;
		cc.transitionRotationZSmooth = 0f;
		cc.tiltRotationZ = 0f;
		cc.tiltRotationZSmooth = 0f;
		float num = base.transform.rotation.eulerAngles.y + 0.01f + additionalSpawnRotation;
		if ((bool)player && (bool)player.transform.parent && player.transform.parent.gameObject.CompareTag("Moving"))
		{
			num -= player.transform.parent.rotation.eulerAngles.y;
		}
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			cc.ResetCamera(num);
		}
		else
		{
			MonoSingleton<PlatformerMovement>.Instance.ResetCamera(num);
		}
		cc.ApplyRotations();
		nm.rb.SetCustomGravity(gravity);
		nm.rb.SetCustomGravityMode(useCustomGravity: true);
		nm.gc.heavyFall = false;
		cc.Transform(Matrix4x4.identity, gravity);
		MonoSingleton<CameraController>.Instance.activated = true;
		component.position = base.transform.position + base.transform.up * 1.25f;
		if (!nm.enabled)
		{
			nm.enabled = true;
		}
		nm.Respawn();
		nm.GetHealth(0, silent: true);
		nm.cc.StopShake();
		nm.ActivatePlayer();
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.transform.position = base.transform.position;
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
			MonoSingleton<PlatformerMovement>.Instance.playerModel.rotation = base.transform.rotation;
			if (additionalSpawnRotation != 0f)
			{
				MonoSingleton<PlatformerMovement>.Instance.playerModel.Rotate(Vector3.up, additionalSpawnRotation);
			}
			MonoSingleton<PlatformerMovement>.Instance.gameObject.SetActive(value: true);
			MonoSingleton<PlatformerMovement>.Instance.SnapCamera();
			MonoSingleton<PlatformerMovement>.Instance.Respawn();
			MonoSingleton<CrateCounter>.Instance.ResetUnsavedStuff();
		}
	}

	public void UpdateRooms()
	{
		Vector3 position = newRooms[this.i].transform.position;
		Object.Destroy(newRooms[this.i]);
		newRooms[this.i] = Object.Instantiate(defaultRooms[this.i], position, defaultRooms[this.i].transform.rotation, defaultRooms[this.i].transform.parent);
		newRooms[this.i].SetActive(value: true);
		Bonus[] componentsInChildren = newRooms[this.i].GetComponentsInChildren<Bonus>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			Bonus[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateStatsManagerReference();
			}
		}
		newRooms[this.i].GetComponent<GoreZone>().isNewest = true;
		if (this.i + 1 < defaultRooms.Count)
		{
			this.i++;
			UpdateRooms();
		}
		else
		{
			this.i = 0;
		}
	}

	public void InheritRoom(GameObject targetRoom)
	{
		new List<GameObject>();
		new List<GameObject>();
		defaultRooms.Add(targetRoom);
		int index = defaultRooms.IndexOf(targetRoom);
		GoreZone component = defaultRooms[index].GetComponent<GoreZone>();
		component.checkpoint = this;
		component.isNewest = false;
		RemoveOnTime[] componentsInChildren = defaultRooms[index].GetComponentsInChildren<RemoveOnTime>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		newRooms.Add(Object.Instantiate(defaultRooms[index], defaultRooms[index].transform.position, defaultRooms[index].transform.rotation, defaultRooms[index].transform.parent));
		Flammable[] componentsInChildren2 = defaultRooms[index].GetComponentsInChildren<Flammable>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].CancelInvoke("Pulse");
		}
		EnemyIdentifier[] componentsInChildren3 = defaultRooms[index].GetComponentsInChildren<EnemyIdentifier>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].CancelInvoke("Burn");
		}
		defaultRooms[index].gameObject.SetActive(value: false);
		newRooms[index].gameObject.SetActive(value: true);
		Bonus[] componentsInChildren4 = newRooms[index].GetComponentsInChildren<Bonus>(includeInactive: true);
		if (componentsInChildren4 != null && componentsInChildren4.Length != 0)
		{
			Bonus[] array = componentsInChildren4;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateStatsManagerReference();
			}
		}
		newRooms[index].GetComponent<GoreZone>().isNewest = true;
		defaultRooms[index].transform.position = new Vector3(defaultRooms[index].transform.position.x + 10000f, defaultRooms[index].transform.position.y, defaultRooms[index].transform.position.z);
	}

	public void ReactivateCheckpoint()
	{
		activated = false;
		firstTime = false;
		inDuringResetSafety = false;
		ReactivationEffect();
	}

	public void ReactivationEffect()
	{
		if (!activated && (bool)graphic && !ShouldBeOff())
		{
			graphic.SetActive(value: true);
			if (graphic.TryGetComponent<ScaleTransform>(out var _))
			{
				graphic.transform.localScale = new Vector3(graphic.transform.localScale.x, 0f, graphic.transform.localScale.z);
			}
			if (graphic.TryGetComponent<AudioSource>(out var component2))
			{
				component2.Play(tracked: true);
			}
			resetSafetyTimer = 0.5f;
		}
	}

	public void ApplyCurrentStyleAndKills()
	{
		ApplyCurrentKills();
		ApplyCurrentStyle();
	}

	public void ApplyCurrentKills()
	{
		restartKills = 0;
	}

	public void ApplyCurrentStyle()
	{
		stylePoints = sm.stylePoints;
	}

	public void AddCustomKill()
	{
		MonoSingleton<StatsManager>.Instance.kills++;
		restartKills++;
	}

	public void ChangeSpawnRotation(float degrees)
	{
		additionalSpawnRotation = degrees;
	}

	public void SetInvisibility(bool state)
	{
		invisible = state;
	}

	public void SetForceOff(bool state)
	{
		forceOff = state;
	}
}
