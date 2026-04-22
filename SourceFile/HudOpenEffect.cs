using UnityEngine;

public class HudOpenEffect : MonoBehaviour
{
	private RectTransform tran;

	[HideInInspector]
	public Vector2 originalDimensions;

	[HideInInspector]
	public Vector2 targetDimensions;

	[HideInInspector]
	public bool gotValues;

	public bool animating;

	public float speed = 30f;

	[HideInInspector]
	public float originalSpeed;

	public bool skip;

	public bool reverse;

	public bool YFirst;

	public bool dontUseScale;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (tran == null)
		{
			tran = GetComponent<RectTransform>();
		}
		if (!gotValues)
		{
			originalDimensions = (dontUseScale ? tran.sizeDelta : new Vector2(tran.localScale.x, tran.localScale.y));
			targetDimensions = originalDimensions;
			originalSpeed = speed;
			gotValues = true;
		}
	}

	private void OnEnable()
	{
		ResetValues();
	}

	private void Update()
	{
		if (!animating)
		{
			return;
		}
		float num = (dontUseScale ? (tran.sizeDelta.x / originalDimensions.x) : tran.localScale.x);
		float num2 = (dontUseScale ? (tran.sizeDelta.y / originalDimensions.y) : tran.localScale.y);
		if (!skip)
		{
			if (YFirst && num2 != targetDimensions.y)
			{
				num2 = Mathf.MoveTowards(num2, targetDimensions.y, Time.unscaledDeltaTime * ((Mathf.Abs(targetDimensions.y - num2) + 0.1f) * speed));
			}
			else if (num != targetDimensions.x)
			{
				num = Mathf.MoveTowards(num, targetDimensions.x, Time.unscaledDeltaTime * ((Mathf.Abs(targetDimensions.x - num) + 0.1f) * speed));
			}
			else if (num2 != targetDimensions.y)
			{
				num2 = Mathf.MoveTowards(num2, targetDimensions.y, Time.unscaledDeltaTime * ((Mathf.Abs(targetDimensions.y - num2) + 0.1f) * speed));
			}
		}
		else
		{
			num = targetDimensions.x;
			num2 = targetDimensions.y;
		}
		if (dontUseScale)
		{
			tran.sizeDelta = new Vector2(num * originalDimensions.x, num2 * originalDimensions.y);
		}
		else
		{
			tran.localScale = new Vector3(num, num2, tran.localScale.z);
		}
		if (num == targetDimensions.x && num2 == targetDimensions.y)
		{
			animating = false;
			if (num == 0f && num2 == 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public Vector2 GetOriginalDimensions()
	{
		Initialize();
		return originalDimensions;
	}

	public void Force()
	{
		Initialize();
	}

	public void ResetValues()
	{
		ResetValues(null);
	}

	public void ResetValues(Vector2? inheritedOriginalDimensions)
	{
		if (inheritedOriginalDimensions.HasValue)
		{
			originalDimensions = inheritedOriginalDimensions.Value;
			gotValues = true;
		}
		Initialize();
		speed = originalSpeed;
		if (!skip)
		{
			if (reverse)
			{
				Reverse(speed);
			}
			if (dontUseScale)
			{
				tran.sizeDelta = originalDimensions;
			}
			else
			{
				tran.localScale = new Vector3(reverse ? 1f : 0.05f, reverse ? 1f : 0.05f, tran.localScale.z);
			}
			animating = true;
		}
	}

	public void Reverse(float newSpeed = 10f)
	{
		targetDimensions = new Vector2(0.025f, 0f);
		speed = newSpeed;
		animating = true;
	}
}
