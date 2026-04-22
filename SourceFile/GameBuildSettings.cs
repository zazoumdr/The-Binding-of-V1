using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class GameBuildSettings
{
	public string startScene;

	public bool noTutorial;

	private static GameBuildSettings _instance;

	public static GameBuildSettings Default => new GameBuildSettings
	{
		startScene = null,
		noTutorial = false
	};

	public static GameBuildSettings Agony => new GameBuildSettings
	{
		startScene = "Main Menu",
		noTutorial = true
	};

	public static GameBuildSettings SandboxOnly => new GameBuildSettings
	{
		startScene = "uk_construct",
		noTutorial = true
	};

	public static GameBuildSettings GetInstance()
	{
		if (_instance == null)
		{
			string path = Path.Combine(Application.streamingAssetsPath, "GameBuildSettings.json");
			if (File.Exists(path))
			{
				try
				{
					_instance = JsonConvert.DeserializeObject<GameBuildSettings>(File.ReadAllText(path));
				}
				catch (Exception arg)
				{
					Debug.LogError($"Failed to load GameBuildSettings: {arg}");
					_instance = Default;
				}
			}
			else
			{
				_instance = Default;
			}
		}
		return _instance;
	}
}
