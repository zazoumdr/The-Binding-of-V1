using UnityEngine;

public class Elevator : MonoBehaviour
{
	public Transform[] stops;

	public GameObject[] buttons;

	[HideInInspector]
	public int targetStop;

	private bool activated;

	private Rigidbody rb;

	[SerializeField]
	private Door doors;

	[SerializeField]
	private AudioSource moveAud;

	private float moveAudOrigPitch;

	[SerializeField]
	private AudioSource dingAud;

	public float speed;

	private float currentSpeed;

	private bool waitingForDoors;

	private TimeSince doorsFailsafe;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if ((bool)(Object)(object)moveAud)
		{
			moveAudOrigPitch = moveAud.GetPitch();
		}
	}

	private void FixedUpdate()
	{
		if (!activated)
		{
			return;
		}
		if (waitingForDoors)
		{
			if ((bool)doors && !doors.isFullyClosed && !((float)doorsFailsafe > 3f))
			{
				return;
			}
			waitingForDoors = false;
		}
		currentSpeed = Mathf.MoveTowards(currentSpeed, speed, speed * Time.fixedDeltaTime);
		float num = Mathf.Abs(stops[targetStop].position.y - base.transform.position.y);
		if (num < speed / 2f)
		{
			float num2 = speed * (num / (speed / 2f) + 0.1f);
			currentSpeed = Mathf.Clamp(currentSpeed, 0f - num2, num2);
		}
		if ((bool)(Object)(object)moveAud)
		{
			moveAud.SetPitch(Mathf.Abs(currentSpeed) / speed * moveAudOrigPitch);
			if (!moveAud.isPlaying)
			{
				moveAud.Play(tracked: true);
			}
		}
		Vector3 vector = new Vector3(base.transform.position.x, stops[targetStop].position.y, base.transform.position.z);
		rb.MovePosition(Vector3.MoveTowards(base.transform.position, vector, currentSpeed * Time.fixedDeltaTime));
		if (rb.position == vector)
		{
			activated = false;
			if ((bool)doors)
			{
				doors.Open();
			}
			Door componentInChildren = stops[targetStop].GetComponentInChildren<Door>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Open();
			}
			if ((bool)(Object)(object)dingAud)
			{
				dingAud.Play(tracked: true);
			}
			if ((bool)(Object)(object)moveAud)
			{
				moveAud.Stop();
			}
		}
	}

	public void MoveToFloor(int target)
	{
		if (target != targetStop)
		{
			activated = true;
			Door componentInChildren = stops[targetStop].GetComponentInChildren<Door>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Close(force: true);
			}
			float num = Mathf.Sign(stops[targetStop].position.y - base.transform.position.y);
			float num2 = Mathf.Sign(stops[target].position.y - base.transform.position.y);
			if (num != num2)
			{
				currentSpeed *= -1f;
			}
			targetStop = target;
			if ((bool)doors)
			{
				waitingForDoors = true;
				doors.Close(force: true);
				doorsFailsafe = 0f;
			}
			UpdateButtons(target);
		}
	}

	public void TeleportToFloor(int target)
	{
		targetStop = target;
		base.transform.position = new Vector3(base.transform.position.x, stops[target].position.y, base.transform.position.z);
		if ((bool)doors)
		{
			doors.Open();
		}
		Door componentInChildren = stops[targetStop].GetComponentInChildren<Door>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Open();
		}
		if ((bool)(Object)(object)moveAud)
		{
			moveAud.Stop();
		}
		currentSpeed = 0f;
		UpdateButtons(target);
	}

	private void UpdateButtons(int target = -1)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (!(buttons[i] == null))
			{
				buttons[i].SetActive(i != target);
			}
		}
	}
}
