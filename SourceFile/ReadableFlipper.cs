using UnityEngine;

public class ReadableFlipper : MonoBehaviour
{
	public void PickUp()
	{
		MonoSingleton<ScanningStuff>.Instance.readingText.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
	}

	public void PutDown()
	{
		MonoSingleton<ScanningStuff>.Instance.readingText.rectTransform.localScale = new Vector3(1f, 1f, 1f);
	}
}
