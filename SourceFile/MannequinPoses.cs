using UnityEngine;

public class MannequinPoses : MonoBehaviour
{
	private Animator anim;

	[SerializeField]
	private bool altar;

	[HideInInspector]
	public bool beenActivated;

	[HideInInspector]
	public int currentPose;

	private void Start()
	{
		anim = GetComponent<Animator>();
		if (altar)
		{
			anim.Play("Altar");
			base.enabled = false;
			return;
		}
		if (!beenActivated)
		{
			beenActivated = true;
			RandomPose();
		}
		else
		{
			ChangePose(currentPose);
		}
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", Random.Range(1f, 3f));
		if (Vector3.Dot(MonoSingleton<CameraController>.Instance.transform.forward, base.transform.position - MonoSingleton<CameraController>.Instance.transform.position) < -0.33f)
		{
			RandomPose();
		}
	}

	private void RandomPose()
	{
		ChangePose(Random.Range(1, 10));
	}

	private void ChangePose(int num)
	{
		currentPose = num;
		anim.SetInteger("TargetPose", currentPose);
	}
}
