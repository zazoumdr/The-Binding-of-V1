using ULTRAKILL.Cheats;
using UnityEngine;

public class ChallengeTrigger : MonoBehaviour
{
	public ChallengeType type;

	public bool checkForNoEnemies;

	public bool evenIfPlayerDead;

	private bool colliderless;

	public bool disableOnExit;

	private void Start()
	{
		if (type == ChallengeType.Fail)
		{
			MonoSingleton<ChallengeDoneByDefault>.Instance.Prepare();
		}
		colliderless = GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null;
		if (colliderless && (evenIfPlayerDead || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Entered();
		}
	}

	private void OnEnable()
	{
		if (colliderless && (evenIfPlayerDead || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Entered();
		}
	}

	private void OnDisable()
	{
		if (colliderless && disableOnExit && base.gameObject.scene.isLoaded)
		{
			Exited();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && (!checkForNoEnemies || !DisableEnemySpawns.DisableArenaTriggers) && (evenIfPlayerDead || !MonoSingleton<NewMovement>.Instance.dead))
		{
			Entered();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (disableOnExit && other.gameObject.CompareTag("Player") && (!checkForNoEnemies || !DisableEnemySpawns.DisableArenaTriggers))
		{
			Exited();
		}
	}

	public void Entered()
	{
		if (type == ChallengeType.Fail)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
			MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
		}
		else if (type == ChallengeType.Succeed)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = false;
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
	}

	public void Exited()
	{
		if (type == ChallengeType.Fail)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = false;
			MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
		}
		else if (type == ChallengeType.Succeed)
		{
			MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
			MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
		}
	}
}
