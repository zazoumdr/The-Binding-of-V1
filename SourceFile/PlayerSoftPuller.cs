using UnityEngine;

public class PlayerSoftPuller : MonoBehaviour
{
	public float pullAmount = 0.1f;

	private float currentPullAmount;

	public bool useX = true;

	public bool useY = true;

	public bool useZ = true;

	private int playerIsIn;

	private void Start()
	{
		currentPullAmount = pullAmount;
	}

	private void OnDisable()
	{
		playerIsIn = 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			playerIsIn++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			playerIsIn--;
		}
	}

	private void FixedUpdate()
	{
		if (currentPullAmount != pullAmount)
		{
			currentPullAmount = Mathf.MoveTowards(currentPullAmount, pullAmount, Time.fixedDeltaTime * pullAmount / 10f);
		}
		NewMovement instance = MonoSingleton<NewMovement>.Instance;
		if (playerIsIn != 0 && !instance.gc.onGround && !instance.TeleportFixedFramesPending)
		{
			Vector3 vector = base.transform.position - MonoSingleton<NewMovement>.Instance.transform.position;
			vector -= MonoSingleton<NewMovement>.Instance.rb.velocity / 2f;
			vector = new Vector3(useX ? vector.x : 0f, useY ? vector.y : 0f, useZ ? vector.z : 0f);
			MonoSingleton<NewMovement>.Instance.rb.AddForce(vector * currentPullAmount, ForceMode.VelocityChange);
		}
	}

	public void Dip()
	{
		currentPullAmount = 0f;
	}
}
