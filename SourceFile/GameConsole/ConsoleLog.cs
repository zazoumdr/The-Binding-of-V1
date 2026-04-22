using System;
using plog;
using plog.Models;

namespace GameConsole;

[Serializable]
public class ConsoleLog
{
	public Log log;

	public Logger source;

	public UnscaledTimeSince timeSinceLogged;

	public bool expanded;

	public ConsoleLog(Log log, Logger source)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		this.log = log;
		timeSinceLogged = 0f;
		this.source = source;
		if ((int)log.Level == 400 && log.StackTrace != null)
		{
			expanded = true;
		}
	}
}
