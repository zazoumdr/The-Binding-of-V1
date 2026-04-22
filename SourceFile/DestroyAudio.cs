using UnityEngine;

public class DestroyAudio : MonoBehaviour
{
	public float time;

	private void Start()
	{
		Invoke("Delet", time);
	}

	private void Delet()
	{
		Object.Destroy((Object)(object)GetComponent<AudioSource>());
		Object.Destroy(this);
	}
}
