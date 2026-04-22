using UnityEngine;

public class DoorBlocker : MonoBehaviour
{
	private AudioSource aud;

	private Door blockedDoor;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Door"))
		{
			Door componentInParent = collision.gameObject.GetComponentInParent<Door>();
			if (componentInParent != null)
			{
				blockedDoor = componentInParent;
			}
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			if ((Object)(object)aud != null)
			{
				aud.Play(tracked: true);
			}
			if (componentInParent != null)
			{
				componentInParent.Close();
			}
		}
	}

	private void OnDestroy()
	{
		if (blockedDoor != null && blockedDoor.gameObject.activeInHierarchy && base.gameObject.scene.isLoaded)
		{
			blockedDoor.Open(enemy: false, skull: true);
		}
	}
}
