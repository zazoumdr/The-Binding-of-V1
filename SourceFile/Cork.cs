using System.Collections;
using UnityEngine;

public class Cork : MonoBehaviour
{
	public float wiggleTime = 2f;

	public float wiggleStrength = 1f;

	public GameObject vortex;

	public Pond tinter;

	public bool insideSuckZone;

	private Vector3 basePos;

	private float wiggleTimer;

	private Rigidbody rb;

	private Coroutine crt;

	private FloatOnWater floater;

	private bool disallowWiggle;

	private void Start()
	{
		basePos = base.transform.position;
		rb = GetComponent<Rigidbody>();
		floater = GetComponent<FloatOnWater>();
	}

	private void Update()
	{
		if (!insideSuckZone)
		{
			StopWiggle();
		}
	}

	public void StartWiggle()
	{
		if (!disallowWiggle && crt == null)
		{
			crt = StartCoroutine(Wiggle());
		}
	}

	public void StopWiggle()
	{
		if (!disallowWiggle)
		{
			base.transform.position = basePos;
			if (crt != null)
			{
				StopCoroutine(crt);
			}
			crt = null;
		}
	}

	private IEnumerator Wiggle()
	{
		wiggleTimer = 0f;
		while (wiggleTimer < wiggleTime)
		{
			base.transform.position = basePos + Random.onUnitSphere * wiggleStrength;
			wiggleTimer += Time.deltaTime;
			yield return null;
		}
		rb.isKinematic = false;
		rb.AddForce(Vector3.up);
		floater.enabled = true;
		disallowWiggle = true;
		yield return new WaitForSeconds(1f);
		vortex.SetActive(value: true);
		tinter.isDraining = true;
		Object.Destroy(this);
	}
}
