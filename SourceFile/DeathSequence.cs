using UnityEngine;

public class DeathSequence : MonoBehaviour
{
	private UnscaledTimeSince timeSinceDeath;

	[SerializeField]
	private GameObject deathScreen;

	private bool sequenceOver;

	private AudioSource aud;

	private TextAppearByLines tabl;

	private void OnEnable()
	{
		if ((Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		if (tabl == null)
		{
			tabl = GetComponentInChildren<TextAppearByLines>();
		}
		if (!sequenceOver)
		{
			timeSinceDeath = 0f;
			MonoSingleton<TimeController>.Instance.controlPitch = false;
			MonoSingleton<TimeController>.Instance.SetAllPitch(1f);
			MonoSingleton<PostProcessV2_Handler>.Instance.DeathEffect(isDead: true);
			sequenceOver = false;
		}
	}

	private void Update()
	{
		if (!sequenceOver)
		{
			if ((float)timeSinceDeath < 2f)
			{
				float value = (float)timeSinceDeath * 0.5f;
				Shader.SetGlobalFloat("_Sharpness", value);
				Shader.SetGlobalFloat("_Deathness", value);
				MonoSingleton<TimeController>.Instance.SetAllPitch(1f - (float)timeSinceDeath / 2f);
			}
			else
			{
				EndSequence();
			}
		}
	}

	public void EndSequence()
	{
		sequenceOver = true;
		deathScreen.SetActive(value: true);
		aud.Stop();
		tabl.Stop();
		MonoSingleton<TimeController>.Instance.controlPitch = false;
		MonoSingleton<TimeController>.Instance.SetAllPitch(0f);
	}

	private void OnDisable()
	{
		timeSinceDeath = 0f;
		sequenceOver = false;
		if (base.gameObject.scene.isLoaded)
		{
			MonoSingleton<TimeController>.Instance.controlPitch = true;
		}
		Shader.SetGlobalFloat("_Sharpness", 0f);
		Shader.SetGlobalFloat("_Deathness", 0f);
		MonoSingleton<PostProcessV2_Handler>.Instance.DeathEffect(isDead: false);
		deathScreen.SetActive(value: false);
	}
}
