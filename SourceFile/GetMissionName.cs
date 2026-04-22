using System.Collections.Generic;

public static class GetMissionName
{
	public static string GetMission(int missionNum)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return MapInfoBase.Instance.levelName;
		}
		if (missionNum == 0)
		{
			return "MAIN MENU";
		}
		return GetMissionNumberOnly(missionNum) + ": " + GetMissionNameOnly(missionNum);
	}

	public static string GetMissionNumberOnly(int missionNum)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return "";
		}
		return missionNum switch
		{
			1 => "0-1", 
			2 => "0-2", 
			3 => "0-3", 
			4 => "0-4", 
			5 => "0-5", 
			6 => "1-1", 
			7 => "1-2", 
			8 => "1-3", 
			9 => "1-4", 
			10 => "2-1", 
			11 => "2-2", 
			12 => "2-3", 
			13 => "2-4", 
			14 => "3-1", 
			15 => "3-2", 
			16 => "4-1", 
			17 => "4-2", 
			18 => "4-3", 
			19 => "4-4", 
			20 => "5-1", 
			21 => "5-2", 
			22 => "5-3", 
			23 => "5-4", 
			24 => "6-1", 
			25 => "6-2", 
			26 => "7-1", 
			27 => "7-2", 
			28 => "7-3", 
			29 => "7-4", 
			30 => "8-1", 
			31 => "8-2", 
			32 => "8-3", 
			33 => "8-4", 
			34 => "9-1", 
			35 => "9-2", 
			100 => "0-E", 
			101 => "1-E", 
			102 => "2-E", 
			103 => "3-E", 
			104 => "4-E", 
			105 => "5-E", 
			106 => "6-E", 
			107 => "7-E", 
			108 => "8-E", 
			109 => "9-E", 
			666 => "P-1", 
			667 => "P-2", 
			668 => "P-3", 
			_ => "", 
		};
	}

	public static string GetMissionNameOnly(int missionNum)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			return MapInfoBase.Instance.levelName;
		}
		return missionNum switch
		{
			0 => "MAIN MENU", 
			1 => "INTO THE FIRE", 
			2 => "THE MEATGRINDER", 
			3 => "DOUBLE DOWN", 
			4 => "A ONE-MACHINE ARMY", 
			5 => "CERBERUS", 
			6 => "HEART OF THE SUNRISE", 
			7 => "THE BURNING WORLD", 
			8 => "HALLS OF SACRED REMAINS", 
			9 => "CLAIR DE LUNE", 
			10 => "BRIDGEBURNER", 
			11 => "DEATH AT 20,000 VOLTS", 
			12 => "SHEER HEART ATTACK", 
			13 => "COURT OF THE CORPSE KING", 
			14 => "BELLY OF THE BEAST", 
			15 => "IN THE FLESH", 
			16 => "SLAVES TO POWER", 
			17 => "GOD DAMN THE SUN", 
			18 => "A SHOT IN THE DARK", 
			19 => "CLAIR DE SOLEIL", 
			20 => "IN THE WAKE OF POSEIDON", 
			21 => "WAVES OF THE STARLESS SEA", 
			22 => "SHIP OF FOOLS", 
			23 => "LEVIATHAN", 
			24 => "CRY FOR THE WEEPER", 
			25 => "AESTHETICS OF HATE", 
			26 => "GARDEN OF FORKING PATHS", 
			27 => "LIGHT UP THE NIGHT", 
			28 => "NO SOUND, NO MEMORY", 
			29 => "...LIKE ANTENNAS TO HEAVEN", 
			30 => "HURTBREAK WONDERLAND", 
			31 => "THROUGH THE MIRROR", 
			32 => "DISINTEGRATION LOOP", 
			33 => "FINAL FLIGHT", 
			34 => "???", 
			35 => "???", 
			100 => "THIS HEAT, AN EVIL HEAT", 
			101 => "...THEN FELL THE ASHES", 
			102 => "???", 
			103 => "???", 
			104 => "???", 
			105 => "???", 
			106 => "???", 
			107 => "???", 
			108 => "???", 
			109 => "???", 
			666 => "SOUL SURVIVOR", 
			667 => "WAIT OF THE WORLD", 
			668 => "???", 
			_ => "MISSION NAME NOT FOUND", 
		};
	}

	public static string GetSceneName(int missionNum)
	{
		return missionNum switch
		{
			1 => "Level 0-1", 
			2 => "Level 0-2", 
			3 => "Level 0-3", 
			4 => "Level 0-4", 
			5 => "Level 0-5", 
			6 => "Level 1-1", 
			7 => "Level 1-2", 
			8 => "Level 1-3", 
			9 => "Level 1-4", 
			10 => "Level 2-1", 
			11 => "Level 2-2", 
			12 => "Level 2-3", 
			13 => "Level 2-4", 
			14 => "Level 3-1", 
			15 => "Level 3-2", 
			16 => "Level 4-1", 
			17 => "Level 4-2", 
			18 => "Level 4-3", 
			19 => "Level 4-4", 
			20 => "Level 5-1", 
			21 => "Level 5-2", 
			22 => "Level 5-3", 
			23 => "Level 5-4", 
			24 => "Level 6-1", 
			25 => "Level 6-2", 
			26 => "Level 7-1", 
			27 => "Level 7-2", 
			28 => "Level 7-3", 
			29 => "Level 7-4", 
			30 => "Level 8-1", 
			31 => "Level 8-2", 
			32 => "Level 8-3", 
			33 => "Level 8-4", 
			34 => "Level 9-1", 
			35 => "Level 9-2", 
			100 => "Level 0-E", 
			101 => "Level 1-E", 
			102 => "Level 2-E", 
			103 => "Level 3-E", 
			104 => "Level 4-E", 
			105 => "Level 5-E", 
			106 => "Level 6-E", 
			107 => "Level 7-E", 
			108 => "Level 8-E", 
			109 => "Level 9-E", 
			666 => "Level P-1", 
			667 => "Level P-2", 
			668 => "Level P-3", 
			_ => "Main Menu", 
		};
	}

	public static IEnumerable<int> EnumerateMissionNumbers()
	{
		for (int i = 1; i <= 50; i++)
		{
			if (GetMissionNumberOnly(i) != "")
			{
				yield return i;
			}
		}
		for (int i = 100; i <= 150; i++)
		{
			if (GetMissionNumberOnly(i) != "")
			{
				yield return i;
			}
		}
		for (int i = 666; i <= 700; i++)
		{
			if (GetMissionNumberOnly(i) != "")
			{
				yield return i;
			}
		}
	}
}
