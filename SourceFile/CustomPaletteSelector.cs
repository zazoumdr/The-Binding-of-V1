using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomPaletteSelector : MonoBehaviour
{
	[Space]
	[SerializeField]
	private GameObject menu;

	[SerializeField]
	private Transform container;

	[Space]
	[SerializeField]
	private Image templatePreviewImage;

	[SerializeField]
	private Text templateFileName;

	[SerializeField]
	private Button buttonTemplate;

	[Space]
	[SerializeField]
	private Image previewImage;

	private static string PalettePath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Palettes");

	private static void Init()
	{
		if (!Directory.Exists(PalettePath))
		{
			Directory.CreateDirectory(PalettePath);
		}
	}

	private void ResetMenu()
	{
		for (int i = 1; i < container.childCount; i++)
		{
			Object.Destroy(container.GetChild(i).gameObject);
		}
	}

	private void RefreshCurrentPreview()
	{
		Texture currentTexture = MonoSingleton<PostProcessV2_Handler>.Instance.CurrentTexture;
		if (!(currentTexture == null))
		{
			Sprite sprite = Sprite.Create((Texture2D)currentTexture, new Rect(0f, 0f, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.texture.filterMode = FilterMode.Point;
			previewImage.sprite = sprite;
		}
	}

	public void ShowMenu()
	{
		BuildMenu();
		RefreshCurrentPreview();
		menu.SetActive(value: true);
	}

	private void BuildMenu()
	{
		ResetMenu();
		((Component)(object)buttonTemplate).gameObject.SetActive(value: false);
		foreach (string palette in (from a in Directory.GetFiles(PalettePath, "*", SearchOption.TopDirectoryOnly)
			where CustomTextures.AllowedExtensions.Contains<string>(Path.GetExtension(a))
			select a).Select(Path.GetFileName))
		{
			Texture2D txt = LoadPalette(palette);
			Sprite sprite = Sprite.Create(txt, new Rect(0f, 0f, txt.width, txt.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.texture.filterMode = FilterMode.Point;
			templatePreviewImage.sprite = sprite;
			templateFileName.text = Path.GetFileNameWithoutExtension(palette);
			Button obj = Object.Instantiate<Button>(buttonTemplate, container, false);
			((Component)(object)obj).gameObject.SetActive(value: true);
			((UnityEvent)(object)obj.onClick).AddListener((UnityAction)delegate
			{
				SetGamePalette(txt, palette);
			});
		}
	}

	private void SetGamePalette(Texture2D txt, string name)
	{
		MonoSingleton<PrefsManager>.Instance.SetStringLocal("colorPaletteTexture", name);
		RefreshCurrentPreview();
	}

	private static Texture2D LoadPalette(string name)
	{
		if (!File.Exists(Path.Combine(PalettePath, name)))
		{
			return null;
		}
		byte[] array = File.ReadAllBytes(Path.Combine(PalettePath, name));
		Texture2D obj = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Point
		};
		ImageConversion.LoadImage(obj, array);
		obj.name = name;
		return obj;
	}

	public static Texture2D LoadSavedPalette()
	{
		Init();
		string stringLocal = MonoSingleton<PrefsManager>.Instance.GetStringLocal("colorPaletteTexture");
		if (!string.IsNullOrEmpty(stringLocal))
		{
			return LoadPalette(stringLocal);
		}
		return null;
	}
}
