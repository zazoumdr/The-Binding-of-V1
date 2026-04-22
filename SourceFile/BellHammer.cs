using UnityEngine;

public class BellHammer : MonoBehaviour
{
	[HideInInspector]
	public HingeJoint joint;

	public AudioSource ringSound;

	[HideInInspector]
	public Rigidbody rb;

	[HideInInspector]
	public bool gotValues;

	[HideInInspector]
	public Quaternion originalRotation;

	private Vector3 previousFrameVelocity;

	private Vector3 currentFrameVelocity;

	[HideInInspector]
	public int hittingLimit;

	public float speedForMaxVolume;

	public bool oneTimeEvent;

	[HideInInspector]
	public bool rung;

	public UltrakillEvent onRing;

	private void Start()
	{
		GetValues();
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			joint = GetComponent<HingeJoint>();
			rb = GetComponent<Rigidbody>();
			originalRotation = base.transform.localRotation;
		}
	}

	private void FixedUpdate()
	{
		currentFrameVelocity = rb.angularVelocity;
		if (currentFrameVelocity.magnitude > 0.001f && Vector3.Dot(currentFrameVelocity, previousFrameVelocity) <= 0f)
		{
			Ring();
		}
		else if (currentFrameVelocity.magnitude > 1f && previousFrameVelocity.magnitude < 0.01f && Vector3.Dot(currentFrameVelocity, previousFrameVelocity) <= 0.1f)
		{
			Ring();
		}
		previousFrameVelocity = currentFrameVelocity;
	}

	private void Ring()
	{
		float num = Mathf.Min(1f, Vector3.Distance(currentFrameVelocity, previousFrameVelocity) / speedForMaxVolume);
		AudioSource obj = Object.Instantiate<AudioSource>(ringSound, base.transform.position, Quaternion.identity);
		obj.volume *= num;
		obj.Play(tracked: true);
		if ((!rung || !oneTimeEvent) && num >= 0.5f)
		{
			onRing?.Invoke();
			rung = true;
		}
	}
}
