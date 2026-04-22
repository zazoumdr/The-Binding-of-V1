using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBG : MonoBehaviour
{
	private RawImage img;

	private Coroutine coroutine;

	private void OnEnable()
	{
		img = GetComponent<RawImage>();
		coroutine = StartCoroutine(Animate());
	}

	private void OnDisable()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		coroutine = null;
	}

	private IEnumerator Animate()
	{
		while (true)
		{
			((Behaviour)(object)img).enabled = true;
			img.uvRect = new Rect(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)), img.uvRect.size);
			yield return new WaitForSeconds(0.5f);
			((Behaviour)(object)img).enabled = false;
			yield return new WaitForSeconds(0.1f);
		}
	}
}
