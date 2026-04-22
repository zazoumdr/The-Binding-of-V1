using System.Collections.Generic;
using plog;
using plog.Models;

namespace GameConsole.Commands;

public class Help : ICommand, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Help");

	public string Name => "Help";

	public string Description => "Helps you with things, does helpful things, lists things maybe??? Just a helpful pal.";

	public string Command => "help";

	public void Execute(Console con, string[] args)
	{
		if (args.Length != 0)
		{
			if (con.recognizedCommands.ContainsKey(args[0].ToLower()))
			{
				Log.Info("<b>" + args[0].ToLower() + "</b> - " + con.recognizedCommands[args[0].ToLower()].Description, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			else
			{
				Log.Info("Command not found.", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			return;
		}
		Log.Info("Listing recognized commands:", (IEnumerable<Tag>)null, (string)null, (object)null);
		foreach (KeyValuePair<string, ICommand> recognizedCommand in con.recognizedCommands)
		{
			Log.Info("<b>" + recognizedCommand.Key + "</b> - " + recognizedCommand.Value.Description, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}
}
