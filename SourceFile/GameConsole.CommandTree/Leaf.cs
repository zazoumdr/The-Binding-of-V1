using System;

namespace GameConsole.CommandTree;

public class Leaf : Node
{
	public readonly Delegate onExecute;

	public Leaf(Delegate onExecute, bool requireCheats)
		: base(requireCheats)
	{
		this.onExecute = onExecute;
	}

	public Leaf(Delegate onExecute)
	{
		this.onExecute = onExecute;
	}
}
