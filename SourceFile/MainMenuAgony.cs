using System.Collections;
using System.Linq;
using plog;
using UnityEngine;

public class MainMenuAgony : MonoBehaviour
{
	private static readonly Logger Log = new Logger("MainMenuAgony");

	public static bool isAgonyOpen = true;

	[SerializeField]
	private GameObject agonyButton;

	[Space]
	[SerializeField]
	private GameObject normalLights;

	[SerializeField]
	private GameObject agonyLights;

	[Space]
	[SerializeField]
	private GameObject[] agonyMenus;

	[SerializeField]
	private GameObject mainMenu;

	private void Awake()
	{
		isAgonyOpen = false;
		GameObject[] array = agonyMenus;
		for (int i = 0; i < array.Length; i++)
		{
			Object.DestroyImmediate(array[i]);
		}
	}

	private void Start()
	{
	}

	private IEnumerator CloseMainMenuDelayed()
	{
		yield return null;
		mainMenu.SetActive(value: false);
	}

	private void Update()
	{
		bool flag = isAgonyOpen;
		normalLights.SetActive(!flag);
		agonyLights.SetActive(flag);
	}

	public void OpenAgony(bool restore = false)
	{
		isAgonyOpen = true;
		if (restore)
		{
			agonyMenus.Last().SetActive(value: true);
		}
		else
		{
			agonyMenus.First().SetActive(value: true);
		}
		mainMenu.SetActive(value: false);
	}

	public void CloseAgony()
	{
		isAgonyOpen = false;
		GameObject[] array = agonyMenus;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		mainMenu.SetActive(value: true);
	}
}
