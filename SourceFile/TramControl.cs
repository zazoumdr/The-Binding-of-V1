using Train;
using UnityEngine;
using UnityEngine.UI;

public class TramControl : MonoBehaviour
{
	[SerializeField]
	private Tram targetTram;

	[Space]
	[SerializeField]
	private GameObject clickSound;

	[SerializeField]
	private GameObject clickDownSound;

	[SerializeField]
	private GameObject clickFailSound;

	[Space]
	[SerializeField]
	private int maxSpeedStep;

	[SerializeField]
	private int minSpeedStep;

	[SerializeField]
	private float speedMultiplier;

	[HideInInspector]
	public float zapAmount;

	[SerializeField]
	private Image[] speedIndicators;

	public Color speedOffColor;

	public Color speedOnColor;

	public float maxPlayerDistance = 15f;

	public int currentSpeedStep;

	private int lastSpeedStep;

	[SerializeField]
	private GameObject zapEffects;

	[SerializeField]
	private Light zapLight;

	[SerializeField]
	private SpriteRenderer zapSprite;

	[SerializeField]
	private AudioSource zapSound;

	private void Awake()
	{
		if ((bool)targetTram)
		{
			targetTram.controller = this;
		}
	}

	public void SpeedUp()
	{
		if (SpeedUp(1))
		{
			if ((bool)clickSound)
			{
				Object.Instantiate(clickSound, base.transform.position, Quaternion.identity, base.transform);
			}
		}
		else if ((bool)clickFailSound)
		{
			Object.Instantiate(clickFailSound, base.transform.position, Quaternion.identity, base.transform);
		}
	}

	public void SpeedDown()
	{
		if (SpeedDown(1))
		{
			if ((bool)clickSound)
			{
				Object.Instantiate(clickDownSound, base.transform.position, Quaternion.identity, base.transform);
			}
		}
		else if ((bool)clickFailSound)
		{
			Object.Instantiate(clickFailSound, base.transform.position, Quaternion.identity, base.transform);
		}
	}

	public bool SpeedUp(int amount)
	{
		if (!targetTram.poweredOn)
		{
			return false;
		}
		if (targetTram.currentPoint != null && targetTram.currentPoint.GetDestination() == null)
		{
			return false;
		}
		if (zapAmount > 0f)
		{
			currentSpeedStep = maxSpeedStep;
		}
		else if (currentSpeedStep < maxSpeedStep)
		{
			if (currentSpeedStep + amount <= maxSpeedStep)
			{
				currentSpeedStep += amount;
			}
			else
			{
				currentSpeedStep = maxSpeedStep;
			}
			return true;
		}
		return false;
	}

	public bool SpeedDown(int amount)
	{
		if (!targetTram.poweredOn)
		{
			return false;
		}
		if (targetTram.currentPoint != null && targetTram.currentPoint.GetDestination(forward: false) == null)
		{
			return false;
		}
		if (zapAmount > 0f)
		{
			currentSpeedStep = minSpeedStep;
		}
		else if (currentSpeedStep > minSpeedStep)
		{
			if (currentSpeedStep - amount >= minSpeedStep)
			{
				currentSpeedStep -= amount;
			}
			else
			{
				currentSpeedStep = minSpeedStep;
			}
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (targetTram == null || !base.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (zapAmount > 0f)
		{
			zapAmount = Mathf.MoveTowards(zapAmount, 0f, Time.deltaTime);
			targetTram.zapAmount = zapAmount;
			if (currentSpeedStep != 0)
			{
				currentSpeedStep = ((currentSpeedStep > 0) ? maxSpeedStep : minSpeedStep);
			}
			targetTram.speed = (float)currentSpeedStep * (speedMultiplier / 10f);
		}
		else
		{
			targetTram.speed = Mathf.MoveTowards(targetTram.speed, (float)currentSpeedStep * (speedMultiplier / 10f), speedMultiplier / 10f * Time.deltaTime);
		}
		UpdateZapEffects();
		if (currentSpeedStep != 0)
		{
			if (!targetTram.poweredOn)
			{
				currentSpeedStep = 0;
			}
			else if (targetTram.movementDirection == TramMovementDirection.Forward && !targetTram.canGoForward)
			{
				currentSpeedStep = 0;
				targetTram.speed = 0f;
			}
			else if (targetTram.movementDirection == TramMovementDirection.Backward && !targetTram.canGoBackward)
			{
				currentSpeedStep = 0;
				targetTram.speed = 0f;
			}
		}
		if (lastSpeedStep != currentSpeedStep)
		{
			lastSpeedStep = currentSpeedStep;
			UpdateSpeedIndicators();
		}
	}

	private void FixedUpdate()
	{
		if (maxPlayerDistance != 0f && Vector3.Distance(base.transform.position, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position) > maxPlayerDistance)
		{
			currentSpeedStep = 0;
		}
	}

	private void UpdateSpeedIndicators()
	{
		for (int i = 0; i < speedIndicators.Length; i++)
		{
			((Graphic)speedIndicators[i]).color = ((i == currentSpeedStep - minSpeedStep) ? speedOnColor : speedOffColor);
		}
	}

	public void Zap()
	{
		zapAmount = 5f;
		targetTram.zapAmount = zapAmount;
		UpdateZapEffects();
	}

	private void UpdateZapEffects()
	{
		if (zapAmount > 0f && !zapEffects.activeSelf)
		{
			zapEffects.SetActive(value: true);
		}
		else if (zapAmount <= 0f && zapEffects.activeSelf)
		{
			zapEffects.SetActive(value: false);
		}
		zapLight.intensity = Mathf.Lerp(0f, 10f, zapAmount);
		zapSprite.color = new Color(zapSprite.color.r, zapSprite.color.g, zapSprite.color.b, Mathf.Lerp(0f, 1f, zapAmount));
		zapSound.volume = Mathf.Lerp(0f, 0.5f, zapAmount);
		zapSound.SetPitch(Mathf.Lerp(0f, 1f, zapAmount));
	}
}
