public static class GameDifficultyExtensions
{
	public static string GetDifficultyName(this GameDifficulty difficulty)
	{
		return difficulty switch
		{
			GameDifficulty.Harmless => "Harmless", 
			GameDifficulty.Lenient => "Lenient", 
			GameDifficulty.Standard => "Standard", 
			GameDifficulty.Violent => "Violent", 
			GameDifficulty.Brutal => "Brutal", 
			GameDifficulty.UKMD => "Ultrakill Must Die", 
			_ => "None", 
		};
	}
}
