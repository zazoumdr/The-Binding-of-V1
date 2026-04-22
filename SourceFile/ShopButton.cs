using System;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
	public bool deactivated;

	public bool failure;

	public GameObject clickSound;

	public GameObject failSound;

	public GameObject[] toActivate;

	public GameObject[] toDeactivate;

	public VariationInfo variationInfo;

	private ControllerPointer pointer;

	public event Action PointerClickSuccess;

	public event Action PointerClickFailure;

	public event Action PointerClickDeactivated;

	private void Awake()
	{
		if (!TryGetComponent<ControllerPointer>(out pointer))
		{
			pointer = base.gameObject.AddComponent<ControllerPointer>();
		}
		pointer.OnPressed.AddListener(OnPointerClick);
	}

	private void OnPointerClick()
	{
		if (deactivated)
		{
			this.PointerClickDeactivated?.Invoke();
		}
		else if (!failure)
		{
			Debug.Log("OnPointerClick passed");
			GameObject[] array = toActivate;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			array = toDeactivate;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			if (variationInfo != null)
			{
				variationInfo.WeaponBought();
			}
			if (clickSound != null)
			{
				UnityEngine.Object.Instantiate(clickSound, base.transform.position, Quaternion.identity);
			}
			this.PointerClickSuccess?.Invoke();
		}
		else if (failure && failSound != null)
		{
			UnityEngine.Object.Instantiate(failSound, base.transform.position, Quaternion.identity);
			this.PointerClickFailure?.Invoke();
		}
	}
}
