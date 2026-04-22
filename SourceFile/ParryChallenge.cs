using UnityEngine;

public class ParryChallenge : MonoBehaviour
{
	private void Awake()
	{
		if (TryGetComponent<Enemy>(out var component))
		{
			component.parryChallenge = this;
		}
	}

	public void Done()
	{
		MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
	}
}
