using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TeleportCheat : MonoBehaviour
{
	private class TeleportTarget
	{
		public string overrideName;

		public CheckPoint checkpoint;

		public FirstRoomPrefab firstRoom;

		public Transform target;
	}

	[SerializeField]
	private GameObject buttonTemplate;

	[SerializeField]
	private Color checkpointColor;

	[SerializeField]
	private Color roomColor;

	private List<string> checkpointNames = new List<string>();

	private void Start()
	{
		GenerateList();
	}

	private List<int> GetHierarchyPath(Transform t)
	{
		List<int> list = new List<int>();
		while (t != null)
		{
			list.Insert(0, t.GetSiblingIndex());
			t = t.parent;
		}
		return list;
	}

	private void GenerateList()
	{
		List<TeleportTarget> list = new List<TeleportTarget>();
		FirstRoomPrefab firstRoom = Object.FindFirstObjectByType<FirstRoomPrefab>();
		if ((bool)firstRoom)
		{
			GameObject gameObject = new GameObject("First Room Teleport Target");
			gameObject.transform.position = firstRoom.transform.position + new Vector3(0f, -9.75f, -1f);
			list.Add(new TeleportTarget
			{
				overrideName = "First Room",
				target = gameObject.transform,
				firstRoom = firstRoom
			});
		}
		List<CheckPoint> list2 = new List<CheckPoint>(Object.FindObjectsByType<CheckPoint>(FindObjectsSortMode.None));
		list2.Sort(delegate(CheckPoint a, CheckPoint b)
		{
			if (a == b)
			{
				return 0;
			}
			List<int> hierarchyPath = GetHierarchyPath(a.transform);
			List<int> hierarchyPath2 = GetHierarchyPath(b.transform);
			int num2 = Mathf.Min(hierarchyPath.Count, hierarchyPath2.Count);
			for (int i = 0; i < num2; i++)
			{
				if (hierarchyPath[i] != hierarchyPath2[i])
				{
					return hierarchyPath[i].CompareTo(hierarchyPath2[i]);
				}
			}
			return hierarchyPath.Count.CompareTo(hierarchyPath2.Count);
		});
		for (int num = 0; num < list2.Count; num++)
		{
			if (!list2[num].unteleportable)
			{
				list.Add(new TeleportTarget
				{
					target = list2[num].transform,
					checkpoint = list2[num]
				});
			}
		}
		foreach (TeleportTarget point in list)
		{
			GameObject obj = Object.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
			obj.GetComponentInChildren<TMP_Text>().text = ((!string.IsNullOrEmpty(point.overrideName)) ? point.overrideName : (point.checkpoint ? (point.checkpoint.toActivate ? ImproveCheckpointName(point.checkpoint.toActivate.name) : "<color=red>Missing toActivate</color>") : point.target.name));
			((Graphic)obj.GetComponentInChildren<TMP_Text>()).color = (point.checkpoint ? checkpointColor : roomColor);
			((UnityEvent)(object)obj.GetComponentInChildren<Button>().onClick).AddListener((UnityAction)delegate
			{
				Teleport(point.target, point.checkpoint);
				if ((bool)point.checkpoint)
				{
					point.checkpoint.toActivate.SetActive(value: true);
					if (point.checkpoint.doorsToUnlock.Length != 0)
					{
						Door[] doorsToUnlock = point.checkpoint.doorsToUnlock;
						foreach (Door door in doorsToUnlock)
						{
							if (door.locked)
							{
								door.Unlock();
							}
							if (door.startOpen)
							{
								door.Open();
							}
						}
					}
					point.checkpoint.onRestart?.Invoke();
				}
				if ((bool)firstRoom)
				{
					GameObject[] activatedRooms = firstRoom.mainDoor.activatedRooms;
					foreach (GameObject gameObject2 in activatedRooms)
					{
						if (gameObject2 != null)
						{
							gameObject2.SetActive(value: true);
						}
					}
				}
			});
			obj.SetActive(value: true);
		}
		buttonTemplate.SetActive(value: false);
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
		{
			base.gameObject.SetActive(value: false);
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
		}
	}

	private string ImproveCheckpointName(string original)
	{
		string text = original;
		if (original.Contains("- "))
		{
			text = original.Split('-')[^1];
		}
		if (checkpointNames.Contains(text))
		{
			for (int i = 2; i <= 99; i++)
			{
				if (!checkpointNames.Contains(text + $" ({i})"))
				{
					text += $" ({i})";
					break;
				}
			}
		}
		checkpointNames.Add(text);
		return text;
	}

	private void Teleport(Transform target, CheckPoint checkpoint = null)
	{
		NewMovement instance = MonoSingleton<NewMovement>.Instance;
		CameraController instance2 = MonoSingleton<CameraController>.Instance;
		if (instance.sliding)
		{
			instance.StopSlide();
		}
		if (instance.boost)
		{
			instance.boost = false;
		}
		instance.transform.position = target.position + target.up * 1.25f;
		instance.rb.position = instance.transform.position;
		instance.rb.velocity = Vector3.zero;
		instance.rb.SetCustomGravityMode(useCustomGravity: false);
		instance2.gravityRotation = Quaternion.identity;
		instance2.gravityVec = Physics.gravity.normalized;
		instance2.rotationOffset = Quaternion.identity;
		instance2.transitionRotationZ = 0f;
		instance2.transitionRotationZSmooth = 0f;
		instance2.tiltRotationZ = 0f;
		instance2.tiltRotationZSmooth = 0f;
		float num = target.rotation.eulerAngles.y + 0.01f;
		if ((bool)instance.transform.parent && instance.transform.parent.gameObject.CompareTag("Moving"))
		{
			num -= instance.transform.parent.rotation.eulerAngles.y;
		}
		instance2.ResetCamera(num);
		instance2.ApplyRotations();
		if (checkpoint != null && checkpoint.activated)
		{
			instance.rb.SetCustomGravity(checkpoint.gravity);
			instance.rb.SetCustomGravityMode(useCustomGravity: true);
			instance2.Transform(Matrix4x4.identity, checkpoint.gravity);
		}
		instance.gc.heavyFall = false;
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
		{
			MonoSingleton<PlatformerMovement>.Instance.transform.position = target.position;
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
			MonoSingleton<PlatformerMovement>.Instance.playerModel.rotation = target.rotation;
			MonoSingleton<PlatformerMovement>.Instance.SnapCamera();
		}
		base.gameObject.SetActive(value: false);
		MonoSingleton<OptionsManager>.Instance.UnFreeze();
		MonoSingleton<PlayerTracker>.Instance.LevelStart();
	}
}
