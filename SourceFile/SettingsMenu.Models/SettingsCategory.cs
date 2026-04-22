using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SettingsMenu.Models;

[CreateAssetMenu(fileName = "SettingsCategory", menuName = "ULTRAKILL/Settings/Category")]
public class SettingsCategory : ScriptableObject
{
	[FormerlySerializedAs("label")]
	public string title;

	public string titleDecorator = "--";

	public string description;

	public SettingsGroup group;

	public List<SettingsItem> items;

	[HideInInspector]
	public List<SettingsItem> unusedItems;

	public string GetLabel(bool capitalize = true)
	{
		if (string.IsNullOrEmpty(title))
		{
			return string.Empty;
		}
		string text = title;
		if (capitalize)
		{
			text = text.ToUpper();
		}
		if (string.IsNullOrEmpty(titleDecorator))
		{
			return text;
		}
		return titleDecorator + " " + text + " " + titleDecorator;
	}
}
