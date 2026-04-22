using UnityEngine;
using UnityEngine.UI;

public class IconPackSprite : MonoBehaviour
{
	[SerializeField]
	private Sprite[] sprites;

	public void Start()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("iconPack");
		GetComponent<Image>().sprite = ((sprites.Length > num) ? sprites[num] : sprites[0]);
	}
}
