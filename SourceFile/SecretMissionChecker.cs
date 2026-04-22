using UnityEngine;

public class SecretMissionChecker : MonoBehaviour
{
	public bool requireCompletion = true;

	public bool primeMission;

	public int secretMission;

	public UltrakillEvent onMissionGet;

	private void Start()
	{
		if (primeMission && GameProgressSaver.GetPrime(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"), secretMission) == 2)
		{
			onMissionGet.Invoke();
		}
		else if (!primeMission)
		{
			int num = GameProgressSaver.GetSecretMission(secretMission);
			if (requireCompletion && num == 2)
			{
				onMissionGet.Invoke();
			}
			else if (!requireCompletion && num >= 1)
			{
				onMissionGet.Invoke();
			}
		}
	}
}
