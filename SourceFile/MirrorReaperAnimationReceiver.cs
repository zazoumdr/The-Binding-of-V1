using UnityEngine;

public class MirrorReaperAnimationReceiver : MonoBehaviour
{
	[SerializeField]
	private MirrorReaper mr;

	public void LowDamageStart()
	{
		mr.LowDamageStart();
	}

	public void VerticalDamageStart()
	{
		mr.VerticalDamageStart();
	}

	public void HighDamageStart()
	{
		mr.HighDamageStart();
	}

	public void DamageStop()
	{
		mr.DamageStop();
	}

	public void StopAction()
	{
		mr.StopAction();
	}

	public void SpawnGroundWave()
	{
		mr.SpawnGroundWave();
	}

	public void PredictTarget()
	{
		mr.PredictTarget();
	}

	public void SpawnDecorativeProjectiles()
	{
		mr.SpawnDecorativeProjectiles();
	}

	public void SpawnProjectiles()
	{
		mr.SpawnProjectiles();
	}

	public void TeleportNow()
	{
		mr.TeleportNow();
	}

	public void StartMoving()
	{
		mr.StartMoving();
	}
}
