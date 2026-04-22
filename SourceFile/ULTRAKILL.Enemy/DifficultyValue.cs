using System;

namespace ULTRAKILL.Enemy;

public struct DifficultyValue<T>
{
	private Func<GameDifficulty, T> getter;

	private GameDifficulty current;

	public T value;

	public void Set(GameDifficulty difficulty)
	{
		if (current != difficulty)
		{
			current = difficulty;
			value = getter(difficulty);
		}
	}

	public DifficultyValue(Func<GameDifficulty, T> getter, GameDifficulty difficulty)
	{
		this.getter = getter;
		current = difficulty;
		value = getter(difficulty);
	}
}
