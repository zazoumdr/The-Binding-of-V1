using System.Collections;
using UnityEngine;

public class BlinkAnimTex : MonoBehaviour
{
	public float blinkDelay = 0.1f;

	public float randomMin = 3f;

	public float randomMax = 5f;

	private float randomTime;

	private float time;

	private AnimatedTexture animTex;

	private void Start()
	{
		NewRandomTime();
		animTex = GetComponent<AnimatedTexture>();
		animTex.SetArraySlice(0);
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (time >= randomTime)
		{
			NewRandomTime();
			StartCoroutine(DoBlink());
		}
	}

	private IEnumerator DoBlink()
	{
		for (int i = 0; i < animTex.arrayTex.depth + 1; i++)
		{
			yield return new WaitForSeconds(blinkDelay);
			animTex.SetArraySlice(i % animTex.arrayTex.depth);
		}
	}

	private void NewRandomTime()
	{
		randomTime = Random.Range(randomMin, randomMax);
		time = 0f;
	}
}
