using UnityEngine;

public class DualWield : MonoBehaviour
{
	private GunControl gc;

	private PowerUpMeter meter;

	public float juiceAmount;

	private bool juiceGiven;

	private GameObject copyTarget;

	private GameObject currentWeapon;

	public float delay;

	private Vector3 defaultPosition;

	private void Awake()
	{
		gc = MonoSingleton<GunControl>.Instance;
		meter = MonoSingleton<PowerUpMeter>.Instance;
	}

	private void Start()
	{
		defaultPosition = base.transform.localPosition;
		if (juiceAmount == 0f)
		{
			juiceAmount = 30f;
		}
		if (meter.juice < juiceAmount)
		{
			meter.latestMaxJuice = juiceAmount;
			meter.juice = juiceAmount;
		}
		meter.powerUpColor = new Color(1f, 0.6f, 0f);
		juiceGiven = true;
		MonoSingleton<FistControl>.Instance.forceNoHold++;
		gc.dualWieldCount++;
		if ((bool)gc.currentWeapon)
		{
			WeaponPos componentInChildren = gc.currentWeapon.GetComponentInChildren<WeaponPos>();
			if ((bool)componentInChildren)
			{
				componentInChildren.CheckPosition();
			}
			UpdateWeapon(gc.currentWeapon);
		}
	}

	private void OnEnable()
	{
		gc.OnWeaponChange += UpdateWeapon;
	}

	private void OnDisable()
	{
		gc.OnWeaponChange -= UpdateWeapon;
	}

	private void Update()
	{
		if (juiceGiven && meter.juice <= 0f)
		{
			EndPowerUp();
		}
	}

	private void UpdateWeapon(GameObject newObject)
	{
		if ((bool)currentWeapon)
		{
			Object.Destroy(currentWeapon);
		}
		if ((bool)gc.currentWeapon && gc.currentWeapon.TryGetComponent<WeaponIdentifier>(out var component))
		{
			copyTarget = gc.currentWeapon;
			currentWeapon = Object.Instantiate(gc.currentWeapon, base.transform);
			if (currentWeapon.TryGetComponent<WeaponIdentifier>(out component))
			{
				component.delay = delay;
				component.duplicate = true;
				base.transform.localPosition = defaultPosition + component.duplicateOffset;
			}
		}
		else
		{
			copyTarget = null;
		}
		if ((bool)copyTarget)
		{
			currentWeapon.SetActive(newObject.activeInHierarchy);
		}
	}

	public void EndPowerUp()
	{
		if ((bool)gc.currentWeapon)
		{
			WeaponPos componentInChildren = gc.currentWeapon.GetComponentInChildren<WeaponPos>();
			if ((bool)componentInChildren)
			{
				componentInChildren.CheckPosition();
			}
		}
		if (MonoSingleton<FistControl>.Instance.forceNoHold > 0)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
		}
		gc.dualWieldCount--;
		Object.Destroy(base.gameObject);
	}
}
