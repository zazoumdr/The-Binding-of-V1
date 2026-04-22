using System;
using System.Collections.Generic;
using SettingsMenu.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SettingsMenu.Components;

[DefaultExecutionOrder(-100)]
public class SettingsPageBuilder : MonoBehaviour
{
	[SerializeField]
	public SettingsMenuAssets assets;

	[Space]
	[SerializeField]
	private SettingsPage page;

	[SerializeField]
	private Transform targetContainer;

	public Selectable navigationButtonSelectable;

	public Selectable[] customTopSelectables;

	public Selectable[] customBottomSelectables;

	[Space]
	[FormerlySerializedAs("buttonCallbacks")]
	public List<SettingsButtonEvent> buttonEvents;

	public List<SettingsGroupInterrupt> groupInterrupts;

	private bool pageBuilt;

	private bool selectAfterBuild;

	private GamepadObjectSelector gamepadObjectSelector;

	private Dictionary<SettingsItem, SettingsBuilderBase> createdInstances;

	private Dictionary<SettingsGroup, List<ISettingsGroupUser>> groups;

	private List<Selectable> selectableRows;

	private void Awake()
	{
		gamepadObjectSelector = GetComponent<GamepadObjectSelector>();
		if (!(page == null))
		{
			BuildPage(page);
		}
	}

	private void Start()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDestroy()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		foreach (KeyValuePair<SettingsGroup, List<ISettingsGroupUser>> group in groups)
		{
			if (group.Key.preferenceKey.key == key)
			{
				bool groupEnabled = group.Key.GetEnabled();
				UpdateGroupUsers(group.Key, groupEnabled);
			}
		}
	}

	private void OnValidate()
	{
		if (page == null)
		{
			return;
		}
		if (buttonEvents == null)
		{
			buttonEvents = new List<SettingsButtonEvent>();
		}
		List<SettingsItem> buttonItems = new List<SettingsItem>();
		SettingsCategory[] categories = page.categories;
		for (int i = 0; i < categories.Length; i++)
		{
			foreach (SettingsItem item2 in categories[i].items)
			{
				if (item2.itemType == SettingsItemType.Button)
				{
					buttonItems.Add(item2);
				}
			}
		}
		buttonEvents.RemoveAll((SettingsButtonEvent x) => !buttonItems.Contains(x.buttonItem));
		foreach (SettingsItem item in buttonItems)
		{
			if (buttonEvents.Find((SettingsButtonEvent x) => x.buttonItem == item).buttonItem == null)
			{
				buttonEvents.Add(new SettingsButtonEvent
				{
					buttonItem = item,
					onClickEvent = new UnityEvent()
				});
			}
		}
	}

	private void BuildPage(SettingsPage settingsPage)
	{
		if (targetContainer == null)
		{
			return;
		}
		createdInstances = new Dictionary<SettingsItem, SettingsBuilderBase>();
		groups = new Dictionary<SettingsGroup, List<ISettingsGroupUser>>();
		selectableRows = new List<Selectable>();
		foreach (Transform item in targetContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		SettingsCategory[] categories = settingsPage.categories;
		foreach (SettingsCategory settingsCategory in categories)
		{
			SettingsCategoryBuilder settingsCategoryBuilder = UnityEngine.Object.Instantiate(assets.categoryTitlePrefab, targetContainer);
			string label = settingsCategory.GetLabel(capitalize: false);
			settingsCategoryBuilder.gameObject.name = label;
			settingsCategoryBuilder.ConfigureFrom(settingsCategory, this);
			foreach (SettingsItem item2 in settingsCategory.items)
			{
				if (item2.platformRequirements == null || item2.platformRequirements.Check())
				{
					SettingsItemBuilder settingsItemBuilder = UnityEngine.Object.Instantiate(assets.itemPrefab, targetContainer);
					settingsItemBuilder.ConfigureFrom(item2, settingsCategory, this);
					settingsItemBuilder.name = item2.GetLabel(capitalize: false);
				}
			}
		}
		foreach (KeyValuePair<SettingsGroup, List<ISettingsGroupUser>> group in groups)
		{
			bool flag = group.Key.GetEnabled();
			foreach (ISettingsGroupUser item3 in group.Value)
			{
				item3.UpdateGroupStatus(flag, group.Key.togglingMode);
			}
		}
		RefreshSelectableNavigation();
		pageBuilt = true;
		if (selectAfterBuild)
		{
			SetSelected();
			selectAfterBuild = false;
		}
	}

	public void SetSelected()
	{
		if (!pageBuilt)
		{
			selectAfterBuild = true;
		}
		else if ((bool)gamepadObjectSelector)
		{
			gamepadObjectSelector.Activate();
			gamepadObjectSelector.SetTop();
		}
	}

	public void RefreshSelectableNavigation()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		List<Selectable> list = new List<Selectable>(selectableRows);
		list.InsertRange(0, customTopSelectables);
		list.AddRange(customBottomSelectables);
		if (list.Count == 0)
		{
			return;
		}
		Selectable val = null;
		Selectable val2 = null;
		foreach (Selectable item in list)
		{
			if (!((UnityEngine.Object)(object)item == null) && ((Component)(object)item).gameObject.activeInHierarchy && item.IsInteractable())
			{
				if ((UnityEngine.Object)(object)val == null)
				{
					val = item;
				}
				if ((UnityEngine.Object)(object)val2 != null)
				{
					Navigation navigation = val2.navigation;
					((Navigation)(ref navigation)).selectOnDown = item;
					val2.navigation = navigation;
					((Navigation)(ref navigation)).mode = (Mode)4;
					Navigation navigation2 = item.navigation;
					((Navigation)(ref navigation2)).mode = (Mode)4;
					((Navigation)(ref navigation2)).selectOnUp = val2;
					item.navigation = navigation2;
				}
				val2 = item;
			}
		}
		if ((UnityEngine.Object)(object)val != null && (UnityEngine.Object)(object)val2 != null)
		{
			Navigation navigation3 = val.navigation;
			((Navigation)(ref navigation3)).selectOnUp = val2;
			val.navigation = navigation3;
			Navigation navigation4 = val2.navigation;
			((Navigation)(ref navigation4)).selectOnDown = val;
			val2.navigation = navigation4;
		}
		if (gamepadObjectSelector != null && (UnityEngine.Object)(object)val != null)
		{
			gamepadObjectSelector.SetMainTarget(val);
		}
	}

	public void AddBuilderInstance(SettingsBuilderBase builder, SettingsItem item)
	{
		if (createdInstances == null)
		{
			createdInstances = new Dictionary<SettingsItem, SettingsBuilderBase>();
		}
		createdInstances[item] = builder;
	}

	public void AddToGroup(SettingsGroup group, ISettingsGroupUser builder)
	{
		if (groups == null)
		{
			groups = new Dictionary<SettingsGroup, List<ISettingsGroupUser>>();
		}
		if (!groups.ContainsKey(group))
		{
			groups[group] = new List<ISettingsGroupUser>();
		}
		groups[group].Add(builder);
	}

	public void AddSelectableRow(Selectable selectable)
	{
		if (selectableRows == null)
		{
			selectableRows = new List<Selectable>();
		}
		selectableRows.Add(selectable);
	}

	public Selectable GetFirstSelectable()
	{
		if (selectableRows == null)
		{
			return null;
		}
		foreach (Selectable selectableRow in selectableRows)
		{
			if (!((UnityEngine.Object)(object)selectableRow == null) && ((Component)(object)selectableRow).gameObject.activeInHierarchy && selectableRow.IsInteractable())
			{
				return selectableRow;
			}
		}
		return null;
	}

	public Selectable GetLastSelectable()
	{
		if (selectableRows == null)
		{
			return null;
		}
		for (int num = selectableRows.Count - 1; num >= 0; num--)
		{
			Selectable val = selectableRows[num];
			if (!((UnityEngine.Object)(object)val == null) && ((Component)(object)val).gameObject.activeInHierarchy && val.IsInteractable())
			{
				return val;
			}
		}
		return null;
	}

	public void ConfirmGroupEnabled(SettingsGroup group)
	{
		SetGroupEnabled(group, groupEnabled: true, noInterrupts: true);
	}

	public void SetGroupEnabled(SettingsGroup group, bool groupEnabled, bool noInterrupts = false)
	{
		List<SettingsGroupInterrupt> list = groupInterrupts;
		if (list != null && list.Count > 0 && groupEnabled && !noInterrupts)
		{
			foreach (SettingsGroupInterrupt groupInterrupt in groupInterrupts)
			{
				if (!(groupInterrupt.group != group))
				{
					groupInterrupt.onEnableEvent.Invoke();
					if (groupInterrupt.suppressDefaultEnable)
					{
						return;
					}
				}
			}
		}
		group.SetEnabledBool(groupEnabled);
	}

	private void UpdateGroupUsers(SettingsGroup group, bool groupEnabled)
	{
		if (groups == null || !groups.TryGetValue(group, out var value))
		{
			return;
		}
		foreach (ISettingsGroupUser item in value)
		{
			item.UpdateGroupStatus(groupEnabled, group.togglingMode);
		}
		RefreshSelectableNavigation();
	}

	public bool TryGetItemBuilderInstance<T>(SettingsItem item, out T builder) where T : SettingsBuilderBase
	{
		builder = null;
		if (createdInstances == null)
		{
			return false;
		}
		if (createdInstances.TryGetValue(item, out var value))
		{
			builder = value as T;
			return builder != null;
		}
		return false;
	}

	public void SetSelectedItem(SettingsItem item)
	{
		if (createdInstances != null && createdInstances.TryGetValue(item, out var value))
		{
			value.SetSelected();
		}
	}
}
