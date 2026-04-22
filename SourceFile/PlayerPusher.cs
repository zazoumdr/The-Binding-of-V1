using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
	public Vector3 force;

	public bool oneTime;

	[HideInInspector]
	public bool activated;

	private void OnTriggerEnter(Collider other)
	{
		if ((!oneTime || !activated) && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			activated = true;
			MonoSingleton<NewMovement>.Instance.rb.AddForce(force, ForceMode.VelocityChange);
		}
	}
}
