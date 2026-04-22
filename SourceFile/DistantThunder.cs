using UnityEngine;

public class DistantThunder : MonoBehaviour
{
	private Renderer rend;

	private Light[] lights;

	private float[] lightIntensities;

	private AudioSource aud;

	public float delay;

	public float delayRandomizer;

	public float firstTimeDelay = -1f;

	public float fadeSpeed = 1f;

	private MaterialPropertyBlock block;

	private Color clr;

	private float fade;

	private float originalPitch;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		aud = GetComponent<AudioSource>();
		lights = GetComponentsInChildren<Light>();
		if (lights.Length != 0)
		{
			lightIntensities = new float[lights.Length];
			for (int i = 0; i < lightIntensities.Length; i++)
			{
				lightIntensities[i] = lights[i].intensity;
				lights[i].intensity = 0f;
			}
		}
		if ((bool)(Object)(object)aud)
		{
			originalPitch = aud.GetPitch();
		}
		block = new MaterialPropertyBlock();
		UpdateColor();
		Invoke("Thunder", (firstTimeDelay >= 0f) ? firstTimeDelay : (delay + Random.Range(0f - delayRandomizer, delayRandomizer)));
	}

	private void Update()
	{
		if (fade > 0f)
		{
			fade = Mathf.MoveTowards(fade, 0f, fadeSpeed * Time.deltaTime);
			UpdateColor();
		}
	}

	private void Thunder()
	{
		fade = 1f;
		UpdateColor();
		if ((bool)(Object)(object)aud)
		{
			aud.SetPitch(originalPitch + Random.Range(-0.1f, 0.1f));
			aud.Play(tracked: true);
		}
		Invoke("Thunder", delay + Random.Range(0f - delayRandomizer, delayRandomizer));
	}

	private void UpdateColor()
	{
		if ((bool)rend)
		{
			clr = Color.white * fade;
			rend.GetPropertyBlock(block, 0);
			block.SetColor(UKShaderProperties.Color, clr);
			rend.SetPropertyBlock(block, 0);
		}
		if (lights != null && lights.Length != 0)
		{
			for (int i = 0; i < lightIntensities.Length; i++)
			{
				lights[i].intensity = lightIntensities[i] * fade;
			}
		}
	}

	public void ForceThunder()
	{
		CancelInvoke("Thunder");
		Thunder();
	}
}
