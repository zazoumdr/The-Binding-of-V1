using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

public class JsonBinding
{
	public string path;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool isComposite;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public Dictionary<string, string> parts;

	private JsonBinding()
	{
	}

	public static List<JsonBinding> FromAction(InputAction action, string group)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		List<JsonBinding> list = new List<JsonBinding>();
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding val = action.bindings[i];
			JsonBinding jsonBinding = new JsonBinding();
			if (!action.BindingHasGroup(i, group))
			{
				continue;
			}
			if (((InputBinding)(ref val)).isComposite)
			{
				jsonBinding.path = ((InputBinding)(ref val)).GetNameOfComposite();
				jsonBinding.isComposite = true;
				jsonBinding.parts = new Dictionary<string, string>();
				while (i + 1 < action.bindings.Count)
				{
					InputBinding val2 = action.bindings[i + 1];
					if (((InputBinding)(ref val2)).isPartOfComposite)
					{
						i++;
						InputBinding val3 = action.bindings[i];
						Debug.Log("BLEURHG " + ((InputBinding)(ref val3)).name);
						Debug.Log(((InputBinding)(ref val3)).path);
						Debug.Log(((InputBinding)(ref val3)).isPartOfComposite);
						jsonBinding.parts.Add(((InputBinding)(ref val3)).name, ((InputBinding)(ref val3)).path);
						continue;
					}
					break;
				}
			}
			else
			{
				jsonBinding.path = ((InputBinding)(ref val)).path;
			}
			list.Add(jsonBinding);
		}
		return list;
	}
}
