using System.Collections.Generic;
using plog;
using plog.Models;

namespace GameConsole.Commands;

public class Echo : ICommand, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Echo");

	public string Name => "Echo";

	public string Description => "Echo the given text";

	public string Command => "echo";

	public void Execute(Console con, string[] args)
	{
		Log.Info("Echoing: " + string.Join(" ", args), (IEnumerable<Tag>)null, (string)null, (object)null);
	}
}
