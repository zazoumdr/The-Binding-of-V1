using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorBlindGet : MonoBehaviour
{
	public HudColorType hct;

	private Image img;

	private Text txt;

	private Light lit;

	private SpriteRenderer sr;

	private TMP_Text txt2;

	private ParticleSystem ps;

	private bool gotTarget;

	public bool variationColor;

	public int variationNumber;

	public bool customColorRenderer;

	private Renderer rend;

	private MaterialPropertyBlock block;

	private void Start()
	{
		UpdateColor();
	}

	private void OnEnable()
	{
		UpdateColor();
	}

	public void UpdateColor()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		if (!gotTarget)
		{
			GetTarget();
		}
		Color color = (variationColor ? MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationNumber] : MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(hct));
		if ((bool)rend)
		{
			rend.GetPropertyBlock(block);
			block.SetColor("_CustomColor1", color);
			rend.SetPropertyBlock(block);
			return;
		}
		if ((bool)(Object)(object)img)
		{
			((Graphic)img).color = color;
		}
		if ((bool)(Object)(object)txt)
		{
			((Graphic)txt).color = color;
		}
		if ((bool)(Object)(object)txt2)
		{
			((Graphic)txt2).color = color;
		}
		if ((bool)lit)
		{
			lit.color = color;
		}
		if ((bool)sr)
		{
			sr.color = color;
		}
		if ((bool)(Object)(object)ps)
		{
			MainModule main = ps.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(color);
		}
	}

	private void GetTarget()
	{
		gotTarget = true;
		if (customColorRenderer)
		{
			rend = GetComponent<Renderer>();
			block = new MaterialPropertyBlock();
		}
		img = GetComponent<Image>();
		txt = GetComponent<Text>();
		txt2 = GetComponent<TMP_Text>();
		lit = GetComponent<Light>();
		sr = GetComponent<SpriteRenderer>();
		ps = GetComponent<ParticleSystem>();
	}
}
