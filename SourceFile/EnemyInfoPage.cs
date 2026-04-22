using System.Collections.Generic;
using plog;
using plog.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyInfoPage : ListComponent<EnemyInfoPage>
{
	private static readonly Logger Log = new Logger("EnemyInfoPage");

	[SerializeField]
	private TMP_Text enemyPageTitle;

	[SerializeField]
	private TMP_Text enemyEntryTitle;

	[SerializeField]
	private TMP_Text enemyPageContent;

	[SerializeField]
	private Transform enemyPreviewWrapper;

	[SerializeField]
	private GameObject wickedNoise;

	[Space]
	[SerializeField]
	private Transform enemyList;

	[SerializeField]
	private GameObject buttonTemplate;

	[SerializeField]
	private Image buttonTemplateBackground;

	[SerializeField]
	private Image buttonTemplateForeground;

	[SerializeField]
	private Image buttonTemplateWickedNoise;

	[SerializeField]
	private Sprite lockedSprite;

	[Space]
	[SerializeField]
	private SpawnableObjectsDatabase objects;

	private SpawnableObject currentSpawnable;

	private void Start()
	{
		UpdateInfo();
	}

	public void UpdateInfo()
	{
		if (enemyList.childCount > 1)
		{
			for (int num = enemyList.childCount - 1; num > 0; num--)
			{
				Object.Destroy(enemyList.GetChild(num).gameObject);
			}
		}
		SpawnableObject[] enemies = objects.enemies;
		foreach (SpawnableObject spawnableObject in enemies)
		{
			if (spawnableObject == null)
			{
				Log.Warning("Spawnable object in enemy list is null!", (IEnumerable<Tag>)null, (string)null, (object)null);
				continue;
			}
			bool num2 = MonoSingleton<BestiaryData>.Instance.GetEnemy(spawnableObject.enemyType) >= 1;
			if (num2)
			{
				((Behaviour)(object)buttonTemplateWickedNoise).enabled = spawnableObject.enemyType == EnemyType.Wicked;
				((Graphic)buttonTemplateBackground).color = spawnableObject.backgroundColor;
				buttonTemplateForeground.sprite = spawnableObject.gridIcon;
			}
			else
			{
				((Behaviour)(object)buttonTemplateWickedNoise).enabled = false;
				((Graphic)buttonTemplateBackground).color = Color.gray;
				buttonTemplateForeground.sprite = lockedSprite;
			}
			GameObject gameObject = Object.Instantiate(buttonTemplate, enemyList);
			gameObject.SetActive(value: true);
			if (num2)
			{
				gameObject.GetComponentInChildren<ShopButton>().deactivated = false;
				((UnityEvent)(object)gameObject.GetComponentInChildren<Button>().onClick).AddListener((UnityAction)delegate
				{
					currentSpawnable = spawnableObject;
					DisplayInfo(spawnableObject);
				});
			}
			else
			{
				gameObject.GetComponentInChildren<ShopButton>().deactivated = true;
			}
		}
		buttonTemplate.SetActive(value: false);
	}

	private void SwapLayers(Transform target, int layer)
	{
		foreach (Transform item in target)
		{
			item.gameObject.layer = layer;
			if (item.childCount > 0)
			{
				SwapLayers(item, layer);
			}
		}
	}

	private void DisplayInfo(SpawnableObject source)
	{
		enemyPageTitle.text = source.objectName;
		enemyEntryTitle.text = source.objectName;
		string text = "<color=#FF4343>TYPE:</color> " + source.type + "\n\n<color=#FF4343>DATA:</color>\n";
		text = ((MonoSingleton<BestiaryData>.Instance.GetEnemy(source.enemyType) <= 1) ? (text + "???") : (text + source.description));
		text = text + "\n\n<color=#FF4343>STRATEGY:</color>\n" + source.strategy;
		enemyPageContent.text = text;
		enemyPageContent.rectTransform.localPosition = new Vector3(enemyPageContent.rectTransform.localPosition.x, 0f, enemyPageContent.rectTransform.localPosition.z);
		for (int i = 0; i < enemyPreviewWrapper.childCount; i++)
		{
			Object.Destroy(enemyPreviewWrapper.GetChild(i).gameObject);
		}
		if (source.enemyType == EnemyType.Wicked)
		{
			wickedNoise.SetActive(value: true);
			return;
		}
		wickedNoise.SetActive(value: false);
		GameObject gameObject = Object.Instantiate(source.preview, enemyPreviewWrapper);
		int layer = enemyPreviewWrapper.gameObject.layer;
		SwapLayers(gameObject.transform, layer);
		gameObject.layer = layer;
		gameObject.transform.localPosition = source.menuOffset;
		gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, source.menuScale);
		Spin spin = gameObject.AddComponent<Spin>();
		spin.spinDirection = new Vector3(0f, 1f, 0f);
		spin.speed = 10f;
	}

	public void DisplayInfo()
	{
		if (!(currentSpawnable == null))
		{
			DisplayInfo(currentSpawnable);
		}
	}

	public void UndisplayInfo()
	{
		currentSpawnable = null;
	}
}
