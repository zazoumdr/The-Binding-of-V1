using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;

public static class WaveUtils
{
	private static readonly Logger Log = new Logger("WaveUtils");

	public static bool IsWaveSelectable(int waveToCheck, int highestWave)
	{
		return highestWave >= waveToCheck * 2;
	}

	public static bool IsValidStartingWave(int wave)
	{
		Log.Info("Checking wave validity: " + wave, (IEnumerable<Tag>)null, (string)null, (object)null);
		if (wave < 0)
		{
			return false;
		}
		return wave % 5 == 0;
	}

	public static int GetSafeStartingWave(int requestedWave)
	{
		if (IsValidStartingWave(requestedWave))
		{
			return requestedWave;
		}
		Log.Warning("Invalid starting wave format", (IEnumerable<Tag>)null, (string)null, (object)null);
		return 0;
	}

	public static int? GetHighestWaveForDifficulty(int difficulty)
	{
		CyberRankData bestCyber = GameProgressSaver.GetBestCyber();
		if (bestCyber != null && bestCyber.preciseWavesByDifficulty.Length > difficulty)
		{
			return Mathf.FloorToInt(bestCyber.preciseWavesByDifficulty[difficulty]);
		}
		return null;
	}
}
