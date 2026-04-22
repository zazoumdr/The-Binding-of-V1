using UnityEngine;

public class Pond : MonoBehaviour, IBloodstainReceiver
{
	public GameObject owningRoom;

	public float bloodFillSpeed;

	public float bloodDrainSpeed = 0.1f;

	public Color surfaceBloodColor = Color.red;

	public Color underwaterBloodColor = Color.red;

	public float bloodFillAmount;

	private Color underwaterColor;

	private Color waterSurfaceColor;

	private Water waterComponent;

	public Renderer waterSurface;

	private MaterialPropertyBlock propertyBlock;

	public bool isDraining;

	public float bloodFillAmountCopy;

	private float lastBloodFillAmount = 9999f;

	private void Start()
	{
		waterComponent = GetComponent<Water>();
		underwaterColor = waterComponent.clr;
		waterSurfaceColor = waterSurface.sharedMaterials[0].GetColor("_Color");
		propertyBlock = new MaterialPropertyBlock();
		bloodFillAmountCopy = bloodFillAmount;
	}

	private void Update()
	{
		bloodFillAmount = Mathf.Clamp01(bloodFillAmount);
		if (isDraining)
		{
			bloodFillAmount -= bloodDrainSpeed * Time.deltaTime;
			bloodFillAmount = Mathf.Clamp01(bloodFillAmount);
		}
		if (bloodFillAmount != lastBloodFillAmount)
		{
			UpdateVisuals();
		}
		lastBloodFillAmount = bloodFillAmount;
	}

	public void StoreBlood()
	{
		bloodFillAmountCopy = bloodFillAmount;
	}

	public void RestoreBlood()
	{
		bloodFillAmount = bloodFillAmountCopy;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		Color value = Color.Lerp(waterSurfaceColor, surfaceBloodColor, bloodFillAmount);
		propertyBlock.SetColor("_Color", value);
		waterSurface.SetPropertyBlock(propertyBlock);
		Color newColor = Color.Lerp(underwaterColor, underwaterBloodColor, bloodFillAmount);
		waterComponent.UpdateColor(newColor);
	}

	public bool HandleBloodstainHit(in RaycastHit rhit)
	{
		bloodFillAmount += bloodFillSpeed;
		return true;
	}

	private void OnTriggerEnter(Collider col)
	{
		EnemyIdentifierIdentifier component2;
		UnderwaterController component3;
		if (col.TryGetComponent<GoreSplatter>(out var component))
		{
			component.bloodAbsorberCount++;
			MonoSingleton<BloodCheckerManager>.Instance.AddPondGore(component);
		}
		else if (col.TryGetComponent<EnemyIdentifierIdentifier>(out component2))
		{
			component2.bloodAbsorberCount++;
			MonoSingleton<BloodCheckerManager>.Instance.AddPondGib(component2);
		}
		else if (col.TryGetComponent<UnderwaterController>(out component3))
		{
			MonoSingleton<BloodCheckerManager>.Instance.playerInPond = true;
		}
	}

	private void OnTriggerExit(Collider col)
	{
		EnemyIdentifierIdentifier component2;
		UnderwaterController component3;
		if (col.TryGetComponent<GoreSplatter>(out var component))
		{
			component.bloodAbsorberCount--;
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
			{
				component.Invoke("RepoolIfNoAbsorber", StockMapInfo.Instance.gibRemoveTime);
			}
		}
		else if (col.TryGetComponent<EnemyIdentifierIdentifier>(out component2))
		{
			component2.bloodAbsorberCount--;
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
			{
				component2.SetupForHellBath();
			}
		}
		else if (col.TryGetComponent<UnderwaterController>(out component3))
		{
			MonoSingleton<BloodCheckerManager>.Instance.playerInPond = false;
		}
	}

	bool IBloodstainReceiver.HandleBloodstainHit(in RaycastHit hit)
	{
		return HandleBloodstainHit(in hit);
	}
}
