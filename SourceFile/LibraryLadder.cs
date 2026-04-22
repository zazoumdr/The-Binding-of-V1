using UnityEngine;

public class LibraryLadder : MonoBehaviour
{
	public Rigidbody rb;

	public Transform leftClamp;

	public Transform rightClamp;

	private Vector3 startPos;

	private Transform rbTrans;

	private void Start()
	{
		rbTrans = rb.transform;
		startPos = rbTrans.localPosition;
	}

	private void FixedUpdate()
	{
		Vector3 vector = Vector3.Scale(rbTrans.InverseTransformVector(rb.velocity), new Vector3(1f, 0f, 0f));
		rb.velocity = rbTrans.TransformVector(vector);
		Vector3 localPosition = rbTrans.localPosition;
		float x = leftClamp.localPosition.x;
		float x2 = rightClamp.localPosition.x;
		localPosition.x = Mathf.Clamp(localPosition.x, x, x2);
		localPosition.y = startPos.y;
		localPosition.z = startPos.z;
		rbTrans.localPosition = localPosition;
	}
}
