using UnityEngine;

public class ElectricityLine : MonoBehaviour
{
	private LineRenderer lr;

	public Texture2DArray electricityArray;

	public float minWidth;

	public float maxWidth;

	public Gradient colors;

	private float cooldown;

	public float fadeSpeed;

	private float fadeLerp = 1f;

	private AnimatedTexture animatedTexture;

	private void Awake()
	{
		animatedTexture = GetComponent<AnimatedTexture>();
		if (animatedTexture == null)
		{
			Debug.LogError("This asset needs to be updated to the new electricity setup", base.gameObject);
		}
		animatedTexture.arrayTex = electricityArray;
	}

	private void Update()
	{
		fadeLerp = Mathf.MoveTowards(fadeLerp, 0f, Time.deltaTime * fadeSpeed);
		if (fadeLerp <= 0f)
		{
			base.gameObject.SetActive(value: false);
		}
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
			return;
		}
		cooldown = 0.05f;
		if (!lr)
		{
			lr = GetComponent<LineRenderer>();
		}
		animatedTexture.SetArraySlice(Random.Range(0, electricityArray.depth));
		lr.widthMultiplier = Random.Range(minWidth, maxWidth);
		lr.startColor = colors.Evaluate(Random.Range(0f, 1f));
		lr.endColor = colors.Evaluate(Random.Range(0f, 1f));
		lr.startColor = new Color(lr.startColor.r, lr.startColor.g, lr.startColor.b, lr.startColor.a * fadeLerp);
		lr.endColor = new Color(lr.endColor.r, lr.endColor.g, lr.endColor.b, lr.endColor.a * fadeLerp);
	}
}
