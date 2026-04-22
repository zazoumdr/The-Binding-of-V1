using UnityEngine;

public class GetPlayerTagTest : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
		MonoBehaviour.print(array.Length);
		GameObject[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MonoBehaviour.print(array2[i].gameObject);
		}
	}
}
