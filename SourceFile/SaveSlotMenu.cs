using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveSlotMenu : MonoBehaviour
{
	public class SlotData
	{
		public bool exists;

		public int highestLvlNumber;

		public int highestDifficulty;

		public override string ToString()
		{
			if (!exists)
			{
				return "EMPTY";
			}
			return GetMissionName.GetMission(highestLvlNumber) + " " + ((highestLvlNumber <= 0) ? string.Empty : ("(" + MonoSingleton<PresenceController>.Instance.diffNames[highestDifficulty] + ")"));
		}
	}

	public const int Slots = 5;

	private static readonly Color ActiveColor = new Color(1f, 0.66f, 0f);

	[SerializeField]
	private SlotRowPanel templateRow;

	[SerializeField]
	private Button closeButton;

	[FormerlySerializedAs("consentWrapper")]
	[SerializeField]
	private GameObject reloadConsentWrapper;

	[SerializeField]
	private TMP_Text wipeConsentContent;

	[SerializeField]
	private GameObject wipeConsentWrapper;

	private int queuedSlot;

	private SlotRowPanel[] slots;

	private void OnEnable()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (slots != null)
		{
			return;
		}
		List<SlotRowPanel> list = new List<SlotRowPanel>();
		SlotData[] array = GameProgressSaver.GetSlots();
		for (int i = 0; i < 5; i++)
		{
			SlotRowPanel newRow = Object.Instantiate(templateRow, templateRow.transform.parent);
			newRow.slotIndex = i;
			newRow.gameObject.SetActive(value: true);
			UpdateSlotState(newRow, array[i]);
			((UnityEvent)(object)newRow.selectButton.onClick).AddListener((UnityAction)delegate
			{
				SelectSlot(newRow.slotIndex);
			});
			((UnityEvent)(object)newRow.deleteButton.onClick).AddListener((UnityAction)delegate
			{
				ClearSlot(newRow.slotIndex);
			});
			list.Add(newRow);
		}
		Navigation navigation;
		for (int num = 0; num < 5; num++)
		{
			Button selectButton = list[num].selectButton;
			navigation = default(Navigation);
			((Navigation)(ref navigation)).mode = (Mode)4;
			((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)((num > 0) ? list[num - 1].selectButton : null);
			((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)((num + 1 < 5) ? list[num + 1].selectButton : closeButton);
			((Navigation)(ref navigation)).selectOnLeft = (Selectable)(object)list[num].deleteButton;
			((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)list[num].deleteButton;
			((Selectable)selectButton).navigation = navigation;
			Button deleteButton = list[num].deleteButton;
			navigation = default(Navigation);
			((Navigation)(ref navigation)).mode = (Mode)4;
			((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)((num > 0) ? list[num - 1].deleteButton : null);
			((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)((num + 1 < 5) ? list[num + 1].deleteButton : closeButton);
			((Navigation)(ref navigation)).selectOnLeft = (Selectable)(object)list[num].selectButton;
			((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)list[num].selectButton;
			((Selectable)deleteButton).navigation = navigation;
		}
		Button obj = closeButton;
		navigation = default(Navigation);
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)list[4].selectButton;
		((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)list[0].selectButton;
		((Selectable)obj).navigation = navigation;
		slots = list.ToArray();
		templateRow.gameObject.SetActive(value: false);
	}

	public void ReloadMenu()
	{
		if (slots != null)
		{
			SlotRowPanel[] array = slots;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i].gameObject);
			}
			slots = null;
		}
		wipeConsentWrapper.SetActive(value: false);
		reloadConsentWrapper.SetActive(value: false);
		OnEnable();
	}

	private void UpdateSlotState(SlotRowPanel targetPanel, SlotData data)
	{
		bool flag = GameProgressSaver.currentSlot == targetPanel.slotIndex;
		((Graphic)targetPanel.backgroundPanel).color = (flag ? ActiveColor : Color.black);
		((Graphic)targetPanel.slotNumberLabel).color = (flag ? Color.black : (data.exists ? Color.white : Color.red));
		((Graphic)targetPanel.stateLabel).color = (flag ? Color.black : (data.exists ? Color.white : Color.red));
		((Selectable)targetPanel.selectButton).interactable = !flag;
		((Component)(object)targetPanel.selectButton).GetComponentInChildren<TMP_Text>().text = (flag ? "SELECTED" : "SELECT");
		((Selectable)targetPanel.deleteButton).interactable = data.exists;
		targetPanel.slotNumberLabel.text = $"Slot {targetPanel.slotIndex + 1}";
		targetPanel.stateLabel.text = data.ToString();
	}

	public void ConfirmReload()
	{
		GameProgressSaver.SetSlot(queuedSlot);
		SceneHelper.LoadScene("Main Menu");
	}

	public void ConfirmWipe()
	{
		int currentSlot = GameProgressSaver.currentSlot;
		GameProgressSaver.WipeSlot(queuedSlot);
		if (currentSlot == queuedSlot)
		{
			SceneHelper.LoadScene("Main Menu");
		}
		else
		{
			ReloadMenu();
		}
	}

	public void CancelConsent()
	{
		reloadConsentWrapper.SetActive(value: false);
		wipeConsentWrapper.SetActive(value: false);
	}

	private void SelectSlot(int slot)
	{
		queuedSlot = slot;
		reloadConsentWrapper.SetActive(value: true);
	}

	private void ClearSlot(int slot)
	{
		queuedSlot = slot;
		wipeConsentContent.text = $"Are you sure you want to <color=red>DELETE SLOT {slot + 1}</color>?";
		wipeConsentWrapper.SetActive(value: true);
	}
}
