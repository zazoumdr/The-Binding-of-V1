using UnityEngine;

public class AnimatedTexture : MonoBehaviour
{
	[SerializeField]
	private bool randomFrame;

	[SerializeField]
	private bool manualTrigger;

	[SerializeField]
	private float delay = 0.0666f;

	[SerializeField]
	private Texture2D[] framePool;

	[SerializeField]
	public Texture2DArray arrayTex;

	[SerializeField]
	private TextureType textureType;

	private TimeSince counter;

	private int selector;

	private MaterialPropertyBlock block;

	private Renderer renderer;

	private LineRendererPortalHelper lrPortalHelper;

	private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

	private static readonly int EmissiveTexID = Shader.PropertyToID("_EmissiveTex");

	private Texture2D arrayIndexTexture;

	private int texID;

	private void OnValidate()
	{
		Setup();
		SetTexture();
	}

	private void Awake()
	{
		Setup();
	}

	private void Start()
	{
		lrPortalHelper = GetComponent<LineRendererPortalHelper>();
	}

	private void Setup()
	{
		switch (textureType)
		{
		case TextureType.Main:
			texID = MainTexID;
			break;
		case TextureType.Emissive:
			texID = EmissiveTexID;
			break;
		}
		block = new MaterialPropertyBlock();
		renderer = GetComponent<Renderer>();
		renderer.GetPropertyBlock(block);
		counter = 0f;
		if (arrayTex != null && (arrayIndexTexture == null || arrayIndexTexture.width != arrayTex.width || arrayIndexTexture.height != arrayTex.height || arrayIndexTexture.format != arrayTex.format))
		{
			arrayIndexTexture = new Texture2D(arrayTex.width, arrayTex.height, arrayTex.format, mipChain: false);
			arrayIndexTexture.filterMode = FilterMode.Point;
		}
	}

	private void Update()
	{
		if (!manualTrigger && (float)counter > delay)
		{
			if (randomFrame)
			{
				SetArraySlice(Random.Range(0, arrayTex.depth));
			}
			else
			{
				SetTexture();
			}
			counter = 0f;
		}
	}

	private void SetTexture()
	{
		renderer.GetPropertyBlock(block);
		if (framePool.Length != 0)
		{
			if (selector >= framePool.Length)
			{
				selector = 0;
			}
			block.SetTexture(texID, framePool[selector]);
		}
		else
		{
			if (arrayTex == null)
			{
				Debug.Log("MISMIDMSIMFSIMFISMDS " + base.gameObject.name, base.gameObject);
			}
			if (selector >= arrayTex.depth)
			{
				selector = 0;
			}
			Graphics.CopyTexture(arrayTex, selector, arrayIndexTexture, 0);
			block.SetTexture(texID, arrayIndexTexture);
		}
		renderer.SetPropertyBlock(block);
		selector++;
		UpdatePortalHelper();
	}

	public void AddTime(float newTime)
	{
		newTime *= (float)(arrayTex.depth - 1);
		Graphics.CopyTexture(arrayTex, Mathf.RoundToInt(newTime), arrayIndexTexture, 0);
		renderer.GetPropertyBlock(block);
		block.SetTexture(texID, arrayIndexTexture);
		renderer.SetPropertyBlock(block);
	}

	public void SetArraySlice(int slice)
	{
		Graphics.CopyTexture(arrayTex, slice, arrayIndexTexture, 0);
		renderer.GetPropertyBlock(block);
		block.SetTexture(texID, arrayIndexTexture);
		renderer.SetPropertyBlock(block);
		UpdatePortalHelper();
	}

	private void UpdatePortalHelper()
	{
		if (!(lrPortalHelper == null))
		{
			lrPortalHelper.UpdatePropertyBlock(block);
		}
	}
}
