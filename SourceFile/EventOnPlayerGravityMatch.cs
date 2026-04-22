using UnityEngine;

public class EventOnPlayerGravityMatch : MonoBehaviour
{
	public float marginOfError = 0.25f;

	public bool inUpdate;

	public bool revertOnUnmatch = true;

	[HideInInspector]
	public bool active;

	public UltrakillEvent onMatch;

	private void Start()
	{
		CheckRotation(force: true);
	}

	private void Update()
	{
		if (inUpdate)
		{
			CheckRotation();
		}
	}

	private void CheckRotation(bool force = false)
	{
		bool flag = Vector3.Dot(base.transform.up * -1f, MonoSingleton<NewMovement>.Instance.rb.GetGravityDirection()) > 1f - marginOfError;
		if (force || flag != active)
		{
			if (flag)
			{
				onMatch.Invoke();
			}
			else if (revertOnUnmatch)
			{
				onMatch.Revert();
			}
			active = flag;
		}
	}
}
