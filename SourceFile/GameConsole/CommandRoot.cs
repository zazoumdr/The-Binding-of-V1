using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GameConsole.CommandTree;
using plog.Models;

namespace GameConsole;

public abstract class CommandRoot : ICommand
{
	private delegate bool ParseMyThing(string s, out object result);

	public class PrefReference
	{
		public string Key;

		public Type Type;

		public bool Local;

		public string Default;
	}

	private Branch root;

	private const string KeyColor = "#db872c";

	private const string TypeColor = "#879fff";

	private const string ValueColor = "#4ac246";

	public abstract string Name { get; }

	public abstract string Description { get; }

	public string Command => root.name;

	public Branch Root => root;

	protected abstract Branch BuildTree(Console con);

	public CommandRoot(Console con)
	{
		root = BuildTree(con);
	}

	public void Execute(Console con, string[] args)
	{
		Queue<string> args2 = new Queue<string>(args.Where((string arg) => arg != ""));
		var (text, branch) = FindLongestMatchingBranch(root, args2, con);
		if (text != null && branch != null && !TryFindCorrectLeaf(text, branch, args2, con))
		{
			PrintUsage(con, text, branch);
		}
	}

	private bool TryFindCorrectLeaf(string soFar, Branch branch, Queue<string> args, Console con)
	{
		bool result = false;
		Node[] children = branch.children;
		foreach (Node node in children)
		{
			if (node is Branch)
			{
				continue;
			}
			if (node.requireCheats && con.CheatBlocker())
			{
				return true;
			}
			Leaf leaf = node as Leaf;
			ParameterInfo[] parameters = leaf.onExecute.Method.GetParameters();
			Queue<string> queue = new Queue<string>(args);
			if (args.Count != parameters.Length)
			{
				continue;
			}
			Dictionary<ParameterInfo, object> dictionary = new Dictionary<ParameterInfo, object>();
			try
			{
				ParameterInfo[] array = parameters;
				foreach (ParameterInfo parameterInfo in array)
				{
					Type parameterType = parameterInfo.ParameterType;
					string text = queue.Dequeue();
					if (parameterType == typeof(bool))
					{
						dictionary[parameterInfo] = bool.Parse(text);
						continue;
					}
					if (parameterType == typeof(int))
					{
						dictionary[parameterInfo] = int.Parse(text);
						continue;
					}
					if (parameterType == typeof(float))
					{
						dictionary[parameterInfo] = float.Parse(text);
						continue;
					}
					if (parameterType == typeof(string))
					{
						dictionary[parameterInfo] = text;
						continue;
					}
					if (parameterType == typeof(string[]))
					{
						List<string> list = new List<string> { text };
						list.AddRange(args);
						dictionary[parameterInfo] = list.ToArray();
						break;
					}
					if (parameterType.IsSubclassOf(typeof(Enum)))
					{
						dictionary[parameterInfo] = Enum.Parse(parameterType, text);
						continue;
					}
					throw new ArgumentException($"{soFar} has an unsupported parameter type: {parameterType}");
				}
			}
			catch (FormatException)
			{
				dictionary.Clear();
				continue;
			}
			leaf.onExecute?.DynamicInvoke(dictionary.Values.ToArray());
			result = true;
			break;
		}
		return result;
	}

	private void PrintUsage(Console con, string soFar, Branch branch)
	{
		Console.Log.Info("Usage: " + soFar + " <subcommand>", (IEnumerable<Tag>)null, (string)null, (object)null);
		Console.Log.Info("Subcommands:", (IEnumerable<Tag>)null, (string)null, (object)null);
		Node[] children = branch.children;
		foreach (Node obj in children)
		{
			if (obj is Branch branch2)
			{
				Console.Log.Info("- " + soFar + " " + branch2.name, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			if (obj is Leaf leaf)
			{
				Console.Log.Info(leaf.onExecute.Method.GetParameters().Aggregate("- " + soFar, (string acc, ParameterInfo param) => $"{acc} <{param.Name}: <color=grey>{param.ParameterType}</color>>"), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}
	}

	public (string soFar, Branch branch) FindLongestMatchingBranch(Branch root, Queue<string> args, Console con = null, Func<Branch, string, bool> matches = null)
	{
		string text = root.name;
		Branch branch = root;
		bool flag = false;
		while (!flag)
		{
			flag = true;
			if (args.Count == 0)
			{
				break;
			}
			Node[] children = branch.children;
			foreach (Node node in children)
			{
				if (!(node is Branch branch2))
				{
					continue;
				}
				bool num;
				if (matches == null)
				{
					if (matches != null)
					{
						continue;
					}
					num = branch2.name == args.Peek();
				}
				else
				{
					num = matches(branch2, args.Peek());
				}
				if (num)
				{
					if (con != null && node.requireCheats && con.CheatBlocker())
					{
						return (soFar: null, branch: null);
					}
					text = text + " " + args.Dequeue();
					branch = branch2;
					flag = false;
					break;
				}
			}
		}
		return (soFar: text, branch: branch);
	}

	public static Branch Branch(string name, params Node[] children)
	{
		return new Branch(name, children);
	}

	public static Branch Branch(string name, bool requireCheats, params Node[] children)
	{
		return new Branch(name, requireCheats, children);
	}

	public static Branch Leaf(string name, Action onExecute, bool requireCheats = false)
	{
		return new Branch(name, requireCheats, new Leaf(onExecute, requireCheats));
	}

	public static Leaf Leaf(Action onExecute, bool requireCheats = false)
	{
		return new Leaf(onExecute, requireCheats);
	}

	public static Branch Leaf<T>(string name, Action<T> onExecute, bool requireCheats = false)
	{
		return new Branch(name, requireCheats, new Leaf(onExecute, requireCheats));
	}

	public static Leaf Leaf<T>(Action<T> onExecute, bool requireCheats = false)
	{
		return new Leaf(onExecute, requireCheats);
	}

	public static Branch Leaf<T, U>(string name, Action<T, U> onExecute, bool requireCheats = false)
	{
		return new Branch(name, new Leaf(onExecute, requireCheats));
	}

	public static Leaf Leaf<T, U>(Action<T, U> onExecute, bool requireCheats = false)
	{
		return new Leaf(onExecute, requireCheats);
	}

	public static Branch Leaf<T, U, V>(string name, Action<T, U, V> onExecute, bool requireCheats = false)
	{
		return new Branch(name, new Leaf(onExecute, requireCheats));
	}

	public static Leaf Leaf<T, U, V>(Action<T, U, V> onExecute, bool requireCheats = false)
	{
		return new Leaf(onExecute, requireCheats);
	}

	public Branch BuildPrefsEditor(List<PrefReference> pref)
	{
		return Leaf("prefs", delegate
		{
			Console.Log.Info("Available prefs:", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (PrefReference item in pref)
			{
				string text = (item.Local ? "<color=red>LOCAL</color>" : string.Empty);
				if (item.Type == typeof(int))
				{
					string text2 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? (item.Local ? MonoSingleton<PrefsManager>.Instance.GetIntLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetInt(item.Key)).ToString() : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
					Console.Log.Info("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text2 + "</color>   [<color=#879fff>int</color>] " + text, (IEnumerable<Tag>)null, (string)null, (object)null);
				}
				else if (item.Type == typeof(float))
				{
					string text3 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? (item.Local ? MonoSingleton<PrefsManager>.Instance.GetFloatLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetFloat(item.Key)).ToString(CultureInfo.InvariantCulture) : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
					Console.Log.Info("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text3 + "</color>   [<color=#879fff>float</color>] " + text, (IEnumerable<Tag>)null, (string)null, (object)null);
				}
				else if (item.Type == typeof(bool))
				{
					string text4 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? ((item.Local ? MonoSingleton<PrefsManager>.Instance.GetBoolLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetBool(item.Key)) ? "True" : "False") : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
					Console.Log.Info("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text4 + "</color>   [<color=#879fff>float</color>] " + text, (IEnumerable<Tag>)null, (string)null, (object)null);
				}
				else if (item.Type == typeof(string))
				{
					string text5 = (item.Local ? MonoSingleton<PrefsManager>.Instance.GetStringLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetString(item.Key));
					Console.Log.Info("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>\"" + (string.IsNullOrEmpty(text5) ? item.Default : text5) + "\"</color>   [<color=#879fff>float</color>] " + text, (IEnumerable<Tag>)null, (string)null, (object)null);
				}
				else
				{
					Console.Log.Info("Pref " + item.Key + " is type " + item.Type.Name + " (Unrecognized)", (IEnumerable<Tag>)null, (string)null, (object)null);
				}
			}
			Console.Log.Info("You can use `<color=#7df59d>prefs set <type> <value></color>` to change a pref", (IEnumerable<Tag>)null, (string)null, (object)null);
			Console.Log.Info("or `<color=#7df59d>prefs set_local <type> <value></color>` to change a <color=#db872c>local</color> pref. (it matters)", (IEnumerable<Tag>)null, (string)null, (object)null);
		});
	}

	public Branch BoolMenu(string commandKey, Func<bool> valueGetter, Action<bool> valueSetter, bool inverted = false, bool requireCheats = false)
	{
		return Branch(commandKey, requireCheats, Leaf("toggle", delegate
		{
			bool flag = !valueGetter();
			valueSetter(flag);
			Console.Log.Info("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(flag, inverted) + "</color>", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), Leaf("on", delegate
		{
			valueSetter(!inverted);
			Console.Log.Info("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(!inverted, inverted) + "</color>", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), Leaf("off", delegate
		{
			valueSetter(inverted);
			Console.Log.Info("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(inverted, inverted) + "</color>", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), Leaf("read", delegate
		{
			Console.Log.Info("The current value is <color=#4ac246>" + GetStateName(valueGetter(), inverted) + "</color>", (IEnumerable<Tag>)null, (string)null, (object)null);
		}));
	}

	private string GetStateName(bool value, bool inverted)
	{
		if (!inverted)
		{
			if (!value)
			{
				return "off";
			}
			return "on";
		}
		if (!value)
		{
			return "on";
		}
		return "off";
	}
}
