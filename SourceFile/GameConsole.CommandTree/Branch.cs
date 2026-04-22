using System;
using System.Linq;

namespace GameConsole.CommandTree;

public class Branch : Node
{
	public readonly string name;

	public readonly Node[] children;

	public Branch(string name, bool requireCheats, params Node[] children)
		: base(requireCheats)
	{
		this.name = name;
		this.children = children;
	}

	public Branch(string name, params Node[] children)
	{
		this.name = name;
		this.children = children;
	}

	public Branch(string name, bool requireCheats = false, params Delegate[] onLeafExecutes)
		: base(requireCheats)
	{
		this.name = name;
		Node[] array = onLeafExecutes.Select((Delegate onExecute) => new Leaf(onExecute, requireCheats)).ToArray();
		children = array;
	}

	public Branch(string name, params Delegate[] onLeafExecutes)
	{
		this.name = name;
		Node[] array = onLeafExecutes.Select((Delegate onExecute) => new Leaf(onExecute, requireCheats: false)).ToArray();
		children = array;
	}
}
