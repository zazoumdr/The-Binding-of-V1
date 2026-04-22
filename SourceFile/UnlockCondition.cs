using System;
using System.Linq;

[Serializable]
public abstract class UnlockCondition
{
	[Serializable]
	public class HasCompletedLevelChallenge : UnlockCondition
	{
		public int levelIndex = 1;

		public override bool conditionMet => GameProgressSaver.GetRank(levelIndex, returnNull: true)?.challenge ?? false;

		public override string description => "COMPLETE CHALLENGE FOR " + GetMissionName.GetMissionNumberOnly(levelIndex);
	}

	[Serializable]
	public class HasSeenEnemy : UnlockCondition
	{
		public EnemyType enemy;

		public override bool conditionMet => true;

		public override string description => "ENCOUNTER AN UNKNOWN FOE";
	}

	[Serializable]
	public class HasReachedLevel : UnlockCondition
	{
		public int levelIndex = 1;

		public override bool conditionMet => GameProgressSaver.GetRank(levelIndex, returnNull: true) != null;

		public override string description => "REACH " + GetMissionName.GetMissionNumberOnly(levelIndex);
	}

	[Serializable]
	public class HasCompletedLevel : UnlockCondition
	{
		public int levelIndex = 1;

		public override bool conditionMet
		{
			get
			{
				int num = levelIndex;
				if (num >= 100)
				{
					if (num < 666)
					{
						return GameProgressSaver.GetEncoreProgress(0) > levelIndex - 100;
					}
					return GameProgressSaver.GetPrime(0, levelIndex - 665) > 0;
				}
				return GameProgressSaver.GetRank(levelIndex, returnNull: true).ranks.Aggregate(seed: false, (bool acc, int rank) => acc || rank > -1);
			}
		}

		public override string description => "COMPLETE " + GetMissionName.GetMissionNumberOnly(levelIndex);
	}

	[Serializable]
	public class HasCompletedSecretLevel : UnlockCondition
	{
		public int secretLevelIndex = 1;

		public override bool conditionMet => GameProgressSaver.GetSecretMission(secretLevelIndex) == 2;

		public override string description => "COMPLETE " + secretLevelIndex + "-S";
	}

	[Serializable]
	public class HasObtainedWeapon : UnlockCondition
	{
		public string gearName;

		public override bool conditionMet => GameProgressSaver.CheckGear(gearName) > 0;

		public override string description => "UNLOCK A WEAPON YET UNDISCOVERED";
	}

	public abstract bool conditionMet { get; }

	public abstract string description { get; }

	public UnlockCondition()
	{
	}
}
