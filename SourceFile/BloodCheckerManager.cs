using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class BloodCheckerManager : MonoSingleton<BloodCheckerManager>
{
	public Canvas washingCanvas;

	public GameObject painterGUITemplate;

	public TextMeshProUGUI roomName;

	public TextMeshProUGUI activePainter;

	public TextMeshProUGUI toDoText;

	public TextMeshProUGUI cleanText;

	public TextMeshProUGUI litterCount;

	public Slider activePercentSlider;

	private string activePainterName;

	public GameObject finalDoorOpener;

	public List<GameObject> trackedRooms = new List<GameObject>();

	public int[] roomLitterForgiveness = new int[5];

	private Dictionary<GameObject, List<BloodAbsorber>> rooms = new Dictionary<GameObject, List<BloodAbsorber>>();

	private Dictionary<GameObject, HashSet<GoreSplatter>> roomGore = new Dictionary<GameObject, HashSet<GoreSplatter>>();

	private Dictionary<GameObject, HashSet<EnemyIdentifierIdentifier>> roomGibs = new Dictionary<GameObject, HashSet<EnemyIdentifierIdentifier>>();

	public Dictionary<BloodAbsorber, GameObject> toDoEntries = new Dictionary<BloodAbsorber, GameObject>();

	public Cubemap[] cleanedMaps = new Cubemap[5];

	private GameObject pondToDoEntry;

	public Pond pond;

	public HashSet<GameObject> pondLitter = new HashSet<GameObject>();

	public bool startedWashing;

	private int litterCheckIndex;

	public int[] roomLitterCounts = new int[5];

	public bool[] roomCompletions = new bool[5];

	private int totalLitterCount = 999;

	public List<GameObject> completedRoomStates = new List<GameObject>();

	[HideInInspector]
	public bool playerInPond;

	public bool higherAccuracy;

	public void HigherAccuracy(bool useHigherAccuracy)
	{
		higherAccuracy = useHigherAccuracy;
		foreach (List<BloodAbsorber> value in rooms.Values)
		{
			foreach (BloodAbsorber item in value)
			{
				item.ToggleHigherAccuracy(higherAccuracy);
			}
		}
	}

	private void Start()
	{
		foreach (GameObject trackedRoom in trackedRooms)
		{
			BloodAbsorber[] componentsInChildren = trackedRoom.GetComponentsInChildren<BloodAbsorber>();
			roomGore.Add(trackedRoom, new HashSet<GoreSplatter>());
			roomGibs.Add(trackedRoom, new HashSet<EnemyIdentifierIdentifier>());
			BloodAbsorber[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].owningRoom = trackedRoom;
			}
			rooms.Add(trackedRoom, componentsInChildren.ToList());
		}
		Transform parent = painterGUITemplate.transform.parent;
		painterGUITemplate.SetActive(value: false);
		foreach (KeyValuePair<GameObject, List<BloodAbsorber>> room in rooms)
		{
			int num = trackedRooms.IndexOf(room.Key);
			Cubemap cleanedMap = cleanedMaps[num];
			foreach (BloodAbsorber item in room.Value)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(painterGUITemplate, parent);
				gameObject.transform.GetChild(0).gameObject.SetActive(value: false);
				((TMP_Text)gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>()).text = item.painterName;
				gameObject.name = item.painterName;
				toDoEntries.Add(item, gameObject);
				item.cleanedMap = cleanedMap;
			}
		}
		pondToDoEntry = UnityEngine.Object.Instantiate(painterGUITemplate, parent);
		pondToDoEntry.transform.GetChild(0).gameObject.SetActive(value: false);
		((TMP_Text)pondToDoEntry.transform.GetChild(1).GetComponent<TextMeshProUGUI>()).text = "Pond";
		pondToDoEntry.name = "Pond";
		((Behaviour)(object)washingCanvas).enabled = false;
	}

	private void RemoveNullLitters()
	{
		litterCheckIndex = (litterCheckIndex + 1) % rooms.Count;
		GameObject key = trackedRooms[litterCheckIndex];
		roomGore[key].RemoveWhere((GoreSplatter x) => x == null || !x.gameObject.activeInHierarchy);
		roomGibs[key].RemoveWhere((EnemyIdentifierIdentifier x) => x == null || !x.gameObject.activeInHierarchy || x.transform.lossyScale == Vector3.zero);
		pondLitter.RemoveWhere((GameObject x) => x == null || !x.activeInHierarchy || x.transform.lossyScale == Vector3.zero);
		Invoke("RemoveNullLitters", 0.033f);
	}

	private void CheckLevelStates()
	{
		CheckLitterCounts();
		CheckLevelCompletion();
		Invoke("CheckLevelStates", 1f);
	}

	private void CheckLitterCounts()
	{
		int[] array = new int[5];
		roomLitterCounts.CopyTo(array, 0);
		foreach (KeyValuePair<GameObject, HashSet<GoreSplatter>> item in roomGore)
		{
			int num = trackedRooms.IndexOf(item.Key);
			roomLitterCounts[num] = item.Value.Count;
		}
		foreach (KeyValuePair<GameObject, HashSet<EnemyIdentifierIdentifier>> roomGib in roomGibs)
		{
			int num2 = trackedRooms.IndexOf(roomGib.Key);
			roomLitterCounts[num2] += roomGib.Value.Count;
		}
		int num3 = trackedRooms.IndexOf(pond.owningRoom);
		roomLitterCounts[num3] += pondLitter.Count;
		totalLitterCount = 0;
		for (int i = 0; i < roomLitterCounts.Length; i++)
		{
			roomLitterCounts[i] = Math.Max(0, roomLitterCounts[i] - roomLitterForgiveness[i]);
			if (roomLitterCounts[i] <= 0)
			{
				_ = array[i];
				_ = 0;
				GameObject gameObject = trackedRooms[i];
				foreach (GoreSplatter item2 in roomGore[gameObject])
				{
					item2.Repool();
				}
				foreach (EnemyIdentifierIdentifier item3 in roomGibs[gameObject])
				{
					if (item3.TryGetComponent<Collider>(out var component))
					{
						GibDestroyer.LimbBegone(component);
					}
				}
				if (gameObject == pond.owningRoom)
				{
					foreach (GameObject item4 in pondLitter)
					{
						Collider component3;
						if (item4.TryGetComponent<GoreSplatter>(out var component2))
						{
							component2.Repool();
						}
						else if (item4.TryGetComponent<Collider>(out component3))
						{
							GibDestroyer.LimbBegone(component3);
						}
					}
				}
			}
			totalLitterCount += roomLitterCounts[i];
		}
	}

	public void StartCheckingBlood()
	{
		if (startedWashing)
		{
			return;
		}
		startedWashing = true;
		foreach (List<BloodAbsorber> value in rooms.Values)
		{
			foreach (BloodAbsorber item in value)
			{
				item.StartCheckingFill();
			}
		}
		Invoke("RemoveNullLitters", 0.033f);
		Invoke("CheckLevelStates", 0.5f);
	}

	private void ToggleHigherAccuracy(bool isTrue)
	{
		foreach (List<BloodAbsorber> value in rooms.Values)
		{
			foreach (BloodAbsorber item in value)
			{
				item.ToggleHigherAccuracy(isTrue);
			}
		}
	}

	private void Update()
	{
		if (!startedWashing)
		{
			return;
		}
		((Behaviour)(object)washingCanvas).enabled = false;
		Transform transform = MonoSingleton<CameraController>.Instance.transform;
		bool flag = false;
		if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 50f, (int)LayerMaskDefaults.Get(LMD.Environment) | 0x10, QueryTriggerInteraction.Collide))
		{
			Pond component3;
			if (hitInfo.transform.gameObject.layer != 4)
			{
				hitInfo.transform.TryGetComponent<BloodAbsorber>(out var component);
				if (component == null && hitInfo.transform.TryGetComponent<BloodAbsorberChild>(out var component2))
				{
					component = component2.bloodGroup;
				}
				if (component != null)
				{
					UpdateDisplay(component);
					flag = true;
				}
			}
			else if (hitInfo.transform.TryGetComponent<Pond>(out component3))
			{
				flag = true;
				UpdateDisplay(null);
			}
		}
		if (!flag && playerInPond)
		{
			UpdateDisplay(null);
		}
	}

	private void CheckLevelCompletion()
	{
		bool flag = totalLitterCount <= 0;
		foreach (GameObject trackedRoom in trackedRooms)
		{
			flag &= IsRoomCompleted(trackedRoom);
		}
		if (flag)
		{
			finalDoorOpener.SetActive(value: true);
		}
	}

	private bool IsRoomCompleted(GameObject roomToCheck)
	{
		bool flag = true;
		foreach (BloodAbsorber item in rooms[roomToCheck])
		{
			toDoEntries[item].transform.GetChild(0).gameObject.SetActive(item.isCompleted);
			flag &= item.isCompleted;
		}
		if (roomToCheck == pond.owningRoom)
		{
			bool flag2 = pond.bloodFillAmount <= 0.001f;
			pondToDoEntry.transform.GetChild(0).gameObject.SetActive(flag2);
			flag = flag && flag2;
		}
		int num = trackedRooms.IndexOf(roomToCheck);
		flag &= roomLitterCounts[num] == 0;
		completedRoomStates[num].SetActive(flag);
		roomCompletions[num] = flag;
		return flag;
	}

	public void StoreBlood()
	{
		foreach (List<BloodAbsorber> value in rooms.Values)
		{
			foreach (BloodAbsorber item in value)
			{
				item.StoreBloodCopy();
			}
		}
		pond.StoreBlood();
	}

	public void RestoreBlood()
	{
		foreach (List<BloodAbsorber> value in rooms.Values)
		{
			foreach (BloodAbsorber item in value)
			{
				item.RestoreBloodCopy();
			}
		}
		pond.RestoreBlood();
	}

	public void UpdateDisplay(BloodAbsorber bA)
	{
		((Behaviour)(object)washingCanvas).enabled = !HideUI.Active;
		GameObject gameObject = null;
		if (bA != null)
		{
			if (bA.painterName != activePainterName)
			{
				((TMP_Text)activePainter).text = bA.painterName;
			}
			gameObject = bA.owningRoom;
		}
		else
		{
			gameObject = pond.owningRoom;
			((TMP_Text)activePainter).text = "Pond";
		}
		if (gameObject == null)
		{
			Debug.LogError("No room found on UpdateDisplay");
		}
		((TMP_Text)roomName).SetText(gameObject.name, true);
		pondToDoEntry.SetActive(value: false);
		foreach (GameObject value2 in toDoEntries.Values)
		{
			value2.SetActive(value: false);
		}
		int num = trackedRooms.IndexOf(gameObject);
		if (roomCompletions[num])
		{
			((TMP_Text)litterCount).transform.parent.gameObject.SetActive(value: false);
			((Component)(object)cleanText).gameObject.SetActive(value: true);
			((Component)(object)toDoText).gameObject.SetActive(value: false);
			return;
		}
		((TMP_Text)litterCount).transform.parent.gameObject.SetActive(value: true);
		((Component)(object)toDoText).gameObject.SetActive(value: true);
		int num2 = roomLitterCounts[num];
		((TMP_Text)litterCount).text = num2.ToString();
		foreach (BloodAbsorber item in rooms[gameObject])
		{
			toDoEntries[item].SetActive(value: true);
		}
		if (gameObject == pond.owningRoom)
		{
			pondToDoEntry.SetActive(value: true);
		}
		if (bA == null)
		{
			if (pond.bloodFillAmount <= 0.001f)
			{
				activePercentSlider.value = 100f;
			}
			else
			{
				activePercentSlider.value = (1f - pond.bloodFillAmount) * 100f;
			}
		}
		else if (bA.isCompleted)
		{
			activePercentSlider.value = 100f;
		}
		else
		{
			float value = (1f - bA.fillAmount) * 100f;
			activePercentSlider.value = value;
		}
	}

	public void AddPondGore(GoreSplatter litter)
	{
		pondLitter.Add(litter.gameObject);
		foreach (HashSet<GoreSplatter> value in roomGore.Values)
		{
			value.Remove(litter);
		}
	}

	public void AddPondGib(EnemyIdentifierIdentifier litter)
	{
		pondLitter.Add(litter.gameObject);
		foreach (HashSet<EnemyIdentifierIdentifier> value in roomGibs.Values)
		{
			value.Remove(litter);
		}
	}

	public void AddGoreToRoom(BloodAbsorber absorber, GoreSplatter litter)
	{
		pondLitter.Remove(litter.gameObject);
		foreach (HashSet<GoreSplatter> value in roomGore.Values)
		{
			value.Remove(litter);
		}
		roomGore[absorber.owningRoom].Add(litter);
	}

	public void AddGibToRoom(BloodAbsorber absorber, EnemyIdentifierIdentifier litter)
	{
		pondLitter.Remove(litter.gameObject);
		foreach (HashSet<EnemyIdentifierIdentifier> value in roomGibs.Values)
		{
			value.Remove(litter);
		}
		roomGibs[absorber.owningRoom].Add(litter);
	}
}
