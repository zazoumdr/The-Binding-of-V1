using System.Collections.Generic;
using UnityEngine;

public class LightningStrikeDecorative_Animated : MonoBehaviour
{
	[Header("Animations")]
	[SerializeField]
	private List<Texture2DArray> buildupAnim;

	[SerializeField]
	private float buildupTime = 0.75f;

	[SerializeField]
	private List<Texture2DArray> strikeAnim;

	[SerializeField]
	private float strikeTime = 1f;

	[SerializeField]
	private AnimatedTexture lightningAnim;

	[Header("Lightning Effects")]
	[SerializeField]
	private Light flash;

	[SerializeField]
	private AudioSource thunder;

	[HideInInspector]
	public float origPitch;

	[Header("Cooldown Settings")]
	[SerializeField]
	private float minInitialCooldown = 5f;

	[SerializeField]
	private float maxInitialCooldown = 60f;

	[SerializeField]
	private float minAfterCooldown = 25f;

	[SerializeField]
	private float maxAfterCooldown = 60f;

	[Header("Start Settings")]
	public bool flashOnStart;

	private float originalFlashIntensity;

	private float time;

	private float cooldown;

	private bool flashing;

	private bool inBuildup;

	private bool inStrike;

	private int atlasIndex;

	private float originalScale;

	private void Start()
	{
		originalFlashIntensity = flash.intensity;
		flash.intensity = 0f;
		time = 0f;
		lightningAnim.AddTime(time);
		cooldown = Random.Range(minInitialCooldown, maxInitialCooldown);
		originalScale = lightningAnim.transform.localScale.x;
		if (origPitch == 0f)
		{
			origPitch = thunder.GetPitch();
		}
		if (flashOnStart)
		{
			FlashStart();
		}
	}

	private void Update()
	{
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		if (cooldown == 0f && !flashing)
		{
			FlashStart();
		}
		if (!flashing)
		{
			return;
		}
		if (inBuildup)
		{
			time = Mathf.MoveTowards(time, 1f, Time.deltaTime / buildupTime);
			lightningAnim.AddTime(time);
			flash.intensity = 0f;
			if (time >= 1f)
			{
				inBuildup = false;
				inStrike = true;
				time = 0f;
				lightningAnim.arrayTex = strikeAnim[atlasIndex];
				thunder.SetPitch(Random.Range(origPitch - 0.2f, origPitch + 0.2f));
				thunder.Play(tracked: true);
				flash.intensity = originalFlashIntensity;
			}
		}
		else if (inStrike)
		{
			time = Mathf.MoveTowards(time, 1f, Time.deltaTime / strikeTime);
			lightningAnim.AddTime(time);
			float t = Mathf.InverseLerp(0f, 1f, time);
			flash.intensity = Mathf.Lerp(originalFlashIntensity, 0f, t);
			if (time >= 1f && flash.intensity == 0f)
			{
				flashing = false;
				inStrike = false;
				time = 1f;
				lightningAnim.AddTime(time);
				cooldown = Random.Range(minInitialCooldown, maxInitialCooldown);
			}
		}
	}

	private void FlashStart()
	{
		flashing = true;
		inBuildup = true;
		inStrike = false;
		time = 0f;
		bool flag = Random.value > 0.5f;
		Vector3 localScale = lightningAnim.transform.localScale;
		localScale.x = (flag ? (0f - originalScale) : originalScale);
		lightningAnim.transform.localScale = localScale;
		atlasIndex = Random.Range(0, buildupAnim.Count);
		lightningAnim.arrayTex = buildupAnim[atlasIndex];
	}
}
