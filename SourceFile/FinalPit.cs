using UnityEngine;

public class FinalPit : MonoBehaviour
{
	private NewMovement nmov;

	private StatsManager sm;

	private Rigidbody rb;

	private bool rotationReady;

	private GameObject player;

	private bool infoSent;

	public bool rankless;

	public bool secondPit;

	public bool fakeEnd;

	public string targetLevelName;

	private int levelNumber;

	public bool musicFadeOut;

	private Quaternion targetRotation;

	public bool upsideDownFinishChallenge;

	private Quaternion currentWorldRotation;

	private void Start()
	{
		sm = MonoSingleton<StatsManager>.Instance;
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		targetRotation = Quaternion.Euler(base.transform.rotation.eulerAngles + Vector3.up * 0.01f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == player && (bool)MonoSingleton<NewMovement>.Instance && MonoSingleton<NewMovement>.Instance.hp > 0)
		{
			if (musicFadeOut)
			{
				MonoSingleton<MusicManager>.Instance.off = true;
			}
			MonoSingleton<FogFadeController>.Instance.FadeOut(fakeEnd);
			GameStateManager.Instance.RegisterState(new GameState("pit-falling", base.gameObject)
			{
				cursorLock = LockMode.Lock,
				cameraInputLock = LockMode.Lock
			});
			nmov = MonoSingleton<NewMovement>.Instance;
			nmov.gameObject.layer = 15;
			rb = nmov.rb;
			nmov.activated = false;
			nmov.levelOver = true;
			sm.HideShit();
			sm.StopTimer();
			if (nmov.sliding)
			{
				nmov.StopSlide();
			}
			if (upsideDownFinishChallenge && Vector3.Dot(nmov.rb.GetGravityDirection().normalized, Vector3.up) > 0f)
			{
				MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
			}
			currentWorldRotation = nmov.cc.transform.rotation;
			Quaternion rotation = nmov.cc.transform.rotation;
			nmov.ResetGravity();
			nmov.cc.transform.rotation = rotation;
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			nmov.cc.transitionRotationZ = 0f;
			nmov.cc.tiltRotationZ = 0f;
			if ((bool)MonoSingleton<PowerUpMeter>.Instance)
			{
				MonoSingleton<PowerUpMeter>.Instance.juice = 0f;
			}
			MonoSingleton<CrateCounter>.Instance?.SaveStuff();
			MonoSingleton<CrateCounter>.Instance?.CoinsToPoints();
			OutOfBounds[] array = Object.FindObjectsOfType<OutOfBounds>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
			DeathZone[] array2 = Object.FindObjectsOfType<DeathZone>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.SetActive(value: false);
			}
			if (!fakeEnd)
			{
				Invoke("SendInfo", 5f);
			}
		}
		else if (other.gameObject.CompareTag("Player") && (bool)MonoSingleton<PlatformerMovement>.Instance && !MonoSingleton<PlatformerMovement>.Instance.dead)
		{
			MonoSingleton<PlayerTracker>.Instance.ChangeToFPS();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!(other.gameObject == player) || !MonoSingleton<NewMovement>.Instance || MonoSingleton<NewMovement>.Instance.hp <= 0)
		{
			return;
		}
		if (nmov == null)
		{
			nmov = other.gameObject.GetComponent<NewMovement>();
			rb = nmov.rb;
		}
		if (other.transform.position.x != base.transform.position.x || other.transform.position.z != base.transform.position.z)
		{
			Vector3 vector = new Vector3(base.transform.position.x, other.transform.position.y, base.transform.position.z);
			float num = Vector3.Distance(other.transform.position, vector);
			other.transform.position = Vector3.MoveTowards(other.transform.position, vector, 1f + num * Time.deltaTime);
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		if (!rotationReady)
		{
			float num2 = Quaternion.Angle(currentWorldRotation, targetRotation);
			float maxDegreesDelta = Time.fixedDeltaTime * 10f * (num2 + 1f);
			currentWorldRotation = Quaternion.RotateTowards(currentWorldRotation, targetRotation, maxDegreesDelta);
			nmov.cc.transform.rotation = currentWorldRotation;
			Vector3 vector2 = nmov.cc.gravityRotation * Vector3.up;
			Vector3 vector3 = Vector3.ProjectOnPlane(currentWorldRotation * Vector3.forward, vector2);
			if (vector3.sqrMagnitude > 0.001f)
			{
				Quaternion rotation = Quaternion.LookRotation(vector3.normalized, vector2);
				player.transform.rotation = rotation;
				nmov.rb.rotation = rotation;
			}
			Vector3 vector4 = Quaternion.Inverse(nmov.cc.gravityRotation) * currentWorldRotation * Vector3.forward;
			nmov.cc.rotationY = Mathf.Atan2(vector4.x, vector4.z) * 57.29578f;
			nmov.cc.rotationX = Mathf.Asin(Mathf.Clamp(vector4.y, -1f, 1f)) * 57.29578f;
			if (num2 < 0.01f)
			{
				currentWorldRotation = targetRotation;
				nmov.cc.transform.rotation = currentWorldRotation;
				Vector3 vector5 = Quaternion.Inverse(nmov.cc.gravityRotation) * currentWorldRotation * Vector3.forward;
				nmov.cc.rotationY = Mathf.Atan2(vector5.x, vector5.z) * 57.29578f;
				nmov.cc.rotationX = Mathf.Asin(Mathf.Clamp(vector5.y, -1f, 1f)) * 57.29578f;
				nmov.cc.ApplyRotations();
				rotationReady = true;
			}
		}
		if (rotationReady && !infoSent && !fakeEnd)
		{
			SendInfo();
		}
	}

	private void SendInfo()
	{
		CancelInvoke();
		if (fakeEnd || infoSent)
		{
			return;
		}
		infoSent = true;
		if (!rankless)
		{
			FinalRank fr = sm.fr;
			if (!sm.infoSent)
			{
				levelNumber = MonoSingleton<StatsManager>.Instance.levelNumber;
				if (SceneHelper.IsPlayingCustom)
				{
					GameProgressSaver.SaveProgress(SceneHelper.CurrentLevelNumber);
				}
				else if (levelNumber >= 420)
				{
					GameProgressSaver.SaveProgress(0);
				}
				else if (levelNumber >= 100)
				{
					GameProgressSaver.SetEncoreProgress(levelNumber - 99);
				}
				else
				{
					GameProgressSaver.SaveProgress(levelNumber + 1);
				}
				fr.targetLevelName = targetLevelName;
			}
			if (secondPit)
			{
				fr.finalPitPos = base.transform.position;
				fr.reachedSecondPit = true;
			}
			if (!sm.infoSent)
			{
				sm.SendInfo();
			}
		}
		else if (secondPit)
		{
			GameProgressSaver.SetTutorial(beat: true);
			FinalRank fr2 = MonoSingleton<StatsManager>.Instance.fr;
			fr2.gameObject.SetActive(value: true);
			fr2.finalPitPos = base.transform.position;
			fr2.RanklessNextLevel(targetLevelName);
		}
	}
}
