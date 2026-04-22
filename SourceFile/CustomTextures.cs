using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomTextures : DirectoryTreeBrowser<FileInfo>
{
	private enum EditMode
	{
		None,
		Grid,
		Skybox,
		Emission,
		Fog
	}

	[SerializeField]
	private Material[] gridMaterials;

	[SerializeField]
	private OutdoorLightMaster olm;

	[SerializeField]
	private Material skyMaterial;

	[SerializeField]
	private Texture defaultGlow;

	[SerializeField]
	private Texture[] defaultGridTextures;

	[SerializeField]
	private Texture defaultEmission;

	[SerializeField]
	private Texture[] defaultSkyboxes;

	[SerializeField]
	private GameObject gridWrapper;

	[SerializeField]
	private Button gridBtn;

	[SerializeField]
	private Button skyboxBtn;

	[SerializeField]
	private Button emissionBtn;

	[SerializeField]
	private Button fogBtn;

	private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

	private EditMode currentEditMode;

	private bool editBase = true;

	private bool editTop = true;

	private bool editTopRow = true;

	[SerializeField]
	private Button baseBtn;

	[SerializeField]
	private Image baseBtnFrame;

	[SerializeField]
	private Button topBtn;

	[SerializeField]
	private Image topBtnFrame;

	[SerializeField]
	private Button topRowBtn;

	[SerializeField]
	private Image topRowBtnFrame;

	[SerializeField]
	private Slider glowSlider;

	private static readonly int EmissiveTex = Shader.PropertyToID("_EmissiveTex");

	public static readonly string[] AllowedExtensions = new string[3] { ".png", ".jpg", ".jpeg" };

	private string TexturesPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Textures");

	protected override int maxPageLength => 14;

	protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(TexturesPath);

	public bool TryToLoad(string key)
	{
		if (!File.Exists(Path.Combine(TexturesPath, key)))
		{
			Debug.LogError("Tried to load an invalid texture! " + key);
			return false;
		}
		LoadTexture(key);
		return true;
	}

	public void SetEditMode(int m)
	{
		GoToBase();
		EditMode editMode = (currentEditMode = (EditMode)m);
		if ((bool)(UnityEngine.Object)(object)gridBtn)
		{
			((Selectable)gridBtn).interactable = editMode != EditMode.Grid;
			((Component)(object)gridBtn).GetComponent<ShopButton>().deactivated = editMode == EditMode.Grid;
			((Selectable)skyboxBtn).interactable = editMode != EditMode.Skybox;
			((Component)(object)skyboxBtn).GetComponent<ShopButton>().deactivated = editMode == EditMode.Skybox;
			((Selectable)emissionBtn).interactable = editMode != EditMode.Emission;
			((Component)(object)emissionBtn).GetComponent<ShopButton>().deactivated = editMode == EditMode.Emission;
			((Selectable)fogBtn).interactable = editMode != EditMode.Fog;
			((Component)(object)fogBtn).GetComponent<ShopButton>().deactivated = editMode == EditMode.Fog;
		}
	}

	public void SetGridEditMode(int num)
	{
		switch (num)
		{
		case 0:
			if (editBase)
			{
				editBase = false;
			}
			else
			{
				editBase = true;
			}
			break;
		case 1:
			if (editTop)
			{
				editTop = false;
			}
			else
			{
				editTop = true;
			}
			break;
		case 2:
			if (editTopRow)
			{
				editTopRow = false;
			}
			else
			{
				editTopRow = true;
			}
			break;
		}
		if (editBase)
		{
			((Graphic)baseBtnFrame).color = Color.red;
		}
		else
		{
			((Graphic)baseBtnFrame).color = Color.white;
		}
		if (editTop)
		{
			((Graphic)topBtnFrame).color = Color.red;
		}
		else
		{
			((Graphic)topBtnFrame).color = Color.white;
		}
		if (editTopRow)
		{
			((Graphic)topRowBtnFrame).color = Color.red;
		}
		else
		{
			((Graphic)topRowBtnFrame).color = Color.white;
		}
	}

	public void SetTexture(string key)
	{
		switch (currentEditMode)
		{
		case EditMode.Grid:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 0, key);
				gridMaterials[0].mainTexture = imageCache[key];
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 1, key);
				gridMaterials[1].mainTexture = imageCache[key];
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGrid_" + 2, key);
				gridMaterials[2].mainTexture = imageCache[key];
			}
			break;
		case EditMode.Emission:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 0, key);
				gridMaterials[0].SetTexture(EmissiveTex, imageCache[key]);
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 1, key);
				gridMaterials[1].SetTexture(EmissiveTex, imageCache[key]);
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customGlow_" + 2, key);
				gridMaterials[2].SetTexture(EmissiveTex, imageCache[key]);
			}
			break;
		case EditMode.Skybox:
			MonoSingleton<PrefsManager>.Instance.SetStringLocal("cyberGrind.customSkybox", key);
			skyMaterial.mainTexture = imageCache[key];
			olm?.UpdateSkyboxMaterial();
			break;
		}
	}

	public void SetGlowIntensity()
	{
		MonoSingleton<EndlessGrid>.Instance.glowMultiplier = glowSlider.value;
		MonoSingleton<EndlessGrid>.Instance.UpdateGlow();
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.glowIntensity", glowSlider.value);
	}

	private void Start()
	{
		string[] array = new int[3] { 0, 1, 2 }.Select((int i) => MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customGrid_" + i)).ToArray();
		string[] array2 = new int[3] { 0, 1, 2 }.Select((int i) => MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customGlow_" + i)).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			if (!string.IsNullOrEmpty(array[num]) && TryToLoad(array[num]))
			{
				gridMaterials[num].mainTexture = imageCache[array[num]];
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + num);
			}
		}
		for (int num2 = 0; num2 < array2.Length; num2++)
		{
			if (!string.IsNullOrEmpty(array2[num2]) && TryToLoad(array2[num2]))
			{
				gridMaterials[num2].SetTexture(EmissiveTex, imageCache[array2[num2]]);
				continue;
			}
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + num2);
			gridMaterials[num2].SetTexture("_EmissiveTex", defaultGlow);
		}
		string stringLocal = MonoSingleton<PrefsManager>.Instance.GetStringLocal("cyberGrind.customSkybox");
		if (!string.IsNullOrEmpty(stringLocal) && TryToLoad(stringLocal))
		{
			skyMaterial.mainTexture = imageCache[stringLocal];
			olm?.UpdateSkyboxMaterial();
		}
		float floatLocal = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.glowIntensity", -1f);
		if (floatLocal != -1f)
		{
			glowSlider.SetValueWithoutNotify(floatLocal);
			MonoSingleton<EndlessGrid>.Instance.glowMultiplier = glowSlider.value;
			MonoSingleton<EndlessGrid>.Instance.UpdateGlow();
		}
		if ((bool)(UnityEngine.Object)(object)gridBtn)
		{
			((Component)(object)gridBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetEditMode(1);
			};
			((Component)(object)emissionBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetEditMode(3);
			};
			((Component)(object)skyboxBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetEditMode(2);
			};
			((Component)(object)fogBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetEditMode(4);
			};
			((Component)(object)baseBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetGridEditMode(0);
			};
			((Component)(object)topRowBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetGridEditMode(2);
			};
			((Component)(object)topBtn).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetGridEditMode(1);
			};
		}
	}

	protected override Action BuildLeaf(FileInfo file, int indexInPage)
	{
		Texture2D texture2D = LoadTexture(file.FullName);
		GameObject btn = UnityEngine.Object.Instantiate(itemButtonTemplate, itemParent, worldPositionStays: false);
		Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
		sprite.texture.filterMode = FilterMode.Point;
		((UnityEventBase)(object)btn.GetComponent<Button>().onClick).RemoveAllListeners();
		((UnityEvent)(object)btn.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			SetTexture(file.FullName);
		});
		btn.GetComponent<Image>().sprite = sprite;
		btn.SetActive(value: true);
		return delegate
		{
			UnityEngine.Object.Destroy(btn);
		};
	}

	private Texture2D LoadTexture(string name)
	{
		if (imageCache.ContainsKey(name))
		{
			return imageCache[name];
		}
		byte[] array = File.ReadAllBytes(Path.Combine(TexturesPath, name));
		Texture2D texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false);
		texture2D.filterMode = FilterMode.Point;
		ImageConversion.LoadImage(texture2D, array);
		imageCache[name] = texture2D;
		return texture2D;
	}

	public void RemoveCustomPrefs()
	{
		for (int i = 0; i < 3; i++)
		{
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + i);
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + i);
		}
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customSkybox");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.glowIntensity");
	}

	public void ResetTexture()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.theme");
		switch (currentEditMode)
		{
		case EditMode.Grid:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 0);
				gridMaterials[0].mainTexture = defaultGridTextures[num];
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 1);
				gridMaterials[1].mainTexture = defaultGridTextures[num];
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGrid_" + 2);
				gridMaterials[2].mainTexture = defaultGridTextures[num];
			}
			break;
		case EditMode.Emission:
			if (editBase)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 0);
				gridMaterials[0].SetTexture(EmissiveTex, defaultEmission);
			}
			if (editTop)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 1);
				gridMaterials[1].SetTexture(EmissiveTex, defaultEmission);
			}
			if (editTopRow)
			{
				MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customGlow_" + 2);
				gridMaterials[2].SetTexture(EmissiveTex, defaultEmission);
			}
			break;
		case EditMode.Skybox:
			skyMaterial.mainTexture = defaultSkyboxes[num];
			olm?.UpdateSkyboxMaterial();
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.customSkybox");
			break;
		}
	}
}
