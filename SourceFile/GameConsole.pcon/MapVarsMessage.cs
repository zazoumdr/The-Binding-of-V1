using System.Collections.Generic;
using System.Globalization;
using Logic;
using pcon.core.Interfaces;

namespace GameConsole.pcon;

public class MapVarsMessage : ISend
{
	private const string Type = "ultrakill.mapvars";

	public List<MapVarField> variables;

	public bool clear;

	public string type => "ultrakill.mapvars";

	public MapVarsMessage(VarStore store)
	{
		clear = true;
		variables = new List<MapVarField>();
		foreach (KeyValuePair<string, int> item in store.intStore)
		{
			variables.Add(new MapVarField
			{
				name = item.Key,
				value = new MapVarValue
				{
					value = item.Value.ToString(),
					type = typeof(int).FullName
				}
			});
		}
		foreach (KeyValuePair<string, bool> item2 in store.boolStore)
		{
			variables.Add(new MapVarField
			{
				name = item2.Key,
				value = new MapVarValue
				{
					value = item2.Value,
					type = typeof(bool).FullName
				}
			});
		}
		foreach (KeyValuePair<string, float> item3 in store.floatStore)
		{
			variables.Add(new MapVarField
			{
				name = item3.Key,
				value = new MapVarValue
				{
					value = item3.Value.ToString(CultureInfo.InvariantCulture),
					type = typeof(float).FullName
				}
			});
		}
		foreach (KeyValuePair<string, string> item4 in store.stringStore)
		{
			variables.Add(new MapVarField
			{
				name = item4.Key,
				value = new MapVarValue
				{
					value = item4.Value,
					type = typeof(string).FullName
				}
			});
		}
	}
}
