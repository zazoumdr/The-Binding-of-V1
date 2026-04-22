using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;

namespace GameConsole.Commands;

public class Exit : ICommand, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Exit");

	public string Name => "Exit";

	public string Description => "Quits the game.";

	public string Command => Name.ToLower();

	public void Execute(Console con, string[] args)
	{
		Log.Info("Goodbye \ud83d\udc4b", (IEnumerable<Tag>)null, (string)null, (object)null);
		Application.Quit();
	}
}
