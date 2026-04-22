using UnityEngine;
using UnityEngine.UI;

public class PuzzlePanel : MonoBehaviour
{
	public GameObject[] tileTypePrefabs;

	public Color[] tileColors;

	public GameObject currentPanel;

	public TileType tileType;

	public TileColor tileColor;

	private Image img;

	private Sprite defaultSprite;

	[SerializeField]
	private Sprite activeSprite;

	private bool activated;

	private PuzzleController pc;

	[HideInInspector]
	public PuzzleLine pl;

	private ControllerPointer pointer;

	private void Start()
	{
		img = GetComponent<Image>();
		defaultSprite = img.sprite;
		pc = GetComponentInParent<PuzzleController>();
		pl = base.transform.GetChild(0).GetComponent<PuzzleLine>();
		if (pl != null)
		{
			pl.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		if (!TryGetComponent<ControllerPointer>(out pointer))
		{
			pointer = base.gameObject.AddComponent<ControllerPointer>();
		}
		pointer.OnEnter.AddListener(delegate
		{
			pc.Hovered(this);
		});
		pointer.OnPressed.AddListener(delegate
		{
			pc.Clicked(this);
		});
		pointer.OnReleased.AddListener(pc.Unclicked);
	}

	public void Activate(TileColor color)
	{
		if (tileType == TileType.End)
		{
			base.transform.GetChild(0).GetComponent<Image>().fillCenter = true;
		}
		activated = true;
		Color color2 = pl.TranslateColor(color);
		((Graphic)img).color = new Color(color2.r, color2.g, color2.b, 1f);
		img.sprite = activeSprite;
	}

	public void DeActivate()
	{
		if (tileType == TileType.End)
		{
			base.transform.GetChild(0).GetComponent<Image>().fillCenter = false;
		}
		activated = false;
		((Graphic)img).color = new Color(1f, 1f, 1f, 1f);
		img.sprite = defaultSprite;
	}
}
