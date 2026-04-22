using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class EnemySimplifier : MonoBehaviour
{
	public enum MaterialState
	{
		normal,
		simplified,
		enraged,
		enragedSimplified
	}

	public bool neverOutlineAndRemoveSimplifier;

	public bool enemyScriptHandlesEnrage;

	public Transform enemyRootTransform;

	public List<int> radiantSubmeshesToIgnore = new List<int>();

	private Material currentMaterial;

	public Material enragedMaterial;

	[HideInInspector]
	public Material originalMaterial;

	[HideInInspector]
	public Material originalMaterial2;

	[HideInInspector]
	public Material originalMaterial3;

	public Material simplifiedMaterial;

	public Material simplifiedMaterial2;

	public Material simplifiedMaterial3;

	public Material enragedSimplifiedMaterial;

	private Renderer meshrenderer;

	[HideInInspector]
	public bool enraged;

	private OptionsManager oman;

	private GameObject player;

	private bool active = true;

	private bool simplify;

	private bool playerDistCheck;

	[HideInInspector]
	public EnemyType enemyColorType;

	public bool ignoreCustomColor;

	[HideInInspector]
	public EnemyIdentifier eid;

	[HideInInspector]
	public bool isHat;

	[HideInInspector]
	public DoubleRender radianceEffect;

	public Material[] matList;

	private bool hasSimplifiedMaterial;

	private int shouldBeOutlined;

	private int lastOutlineState;

	private MaterialState currentState;

	private MaterialState lastState;

	private Dictionary<MaterialState, Material> materialDict;

	private bool hasEnragedSimplified;

	private bool lastSandified;

	private bool lastBlessed;

	private MaterialPropertyBlock propBlock;

	private static readonly int HasSandBuff = Shader.PropertyToID("_HasSandBuff");

	private static readonly int NewSanded = Shader.PropertyToID("_Sanded");

	private static readonly int Blessed = Shader.PropertyToID("_Blessed");

	private static readonly int Outline = Shader.PropertyToID("_Outline");

	private static readonly int ForceOutline = Shader.PropertyToID("_ForceOutline");

	private void Awake()
	{
		oman = MonoSingleton<OptionsManager>.Instance;
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		if (enemyRootTransform == null)
		{
			enemyRootTransform = base.transform;
		}
		meshrenderer = GetComponent<Renderer>();
		currentMaterial = meshrenderer.material;
		originalMaterial = currentMaterial;
		if ((bool)enragedSimplifiedMaterial)
		{
			enragedSimplifiedMaterial = new Material(enragedSimplifiedMaterial);
		}
		if ((bool)simplifiedMaterial)
		{
			simplifiedMaterial = new Material(simplifiedMaterial);
		}
		if ((bool)simplifiedMaterial2)
		{
			simplifiedMaterial2 = new Material(simplifiedMaterial2);
		}
		if ((bool)simplifiedMaterial3)
		{
			simplifiedMaterial3 = new Material(simplifiedMaterial3);
		}
		matList = meshrenderer.materials;
		if (simplifiedMaterial2 != null && matList.Length > 1)
		{
			Material material = matList[1];
			originalMaterial2 = material;
		}
		if (simplifiedMaterial3 != null && matList.Length > 2)
		{
			Material material2 = matList[2];
			originalMaterial3 = material2;
		}
		if (!enragedMaterial)
		{
			enragedMaterial = originalMaterial;
		}
		materialDict = new Dictionary<MaterialState, Material>
		{
			{
				MaterialState.normal,
				originalMaterial
			},
			{
				MaterialState.simplified,
				simplifiedMaterial
			},
			{
				MaterialState.enraged,
				enragedMaterial
			},
			{
				MaterialState.enragedSimplified,
				enragedSimplifiedMaterial
			}
		};
		eid = GetComponentInParent<EnemyIdentifier>();
		if ((bool)eid && enemyColorType == EnemyType.Cerberus)
		{
			enemyColorType = eid.enemyType;
		}
		if ((bool)GetComponentInParent<SeasonalHats>())
		{
			isHat = true;
		}
		propBlock = new MaterialPropertyBlock();
		if (isHat && !eid)
		{
			Begone();
		}
	}

	private void Start()
	{
		oman = MonoSingleton<OptionsManager>.Instance;
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		TryRemoveSimplifier();
		if (enemyRootTransform == null)
		{
			enemyRootTransform = base.transform;
		}
		hasSimplifiedMaterial = simplifiedMaterial != null;
		hasEnragedSimplified = enragedSimplifiedMaterial != null;
		UpdateColors();
		SetOutline(forceUpdate: true);
		if (neverOutlineAndRemoveSimplifier)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetFloat("_Outline", 0f);
			materialPropertyBlock.SetFloat("_ForceOutline", 0f);
			meshrenderer.SetPropertyBlock(materialPropertyBlock);
			active = false;
			Object.Destroy(this);
		}
	}

	private void TryRemoveSimplifier()
	{
		if (!isHat)
		{
			return;
		}
		bool flag = true;
		EnemySimplifier[] componentsInChildren = eid.GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].isHat)
			{
				flag = false;
			}
		}
		if (flag)
		{
			Begone();
		}
	}

	private void OnEnable()
	{
		hasSimplifiedMaterial = simplifiedMaterial != null;
		UpdateColors();
	}

	private void Update()
	{
		if (active)
		{
			SetOutline(forceUpdate: false);
		}
	}

	public void SetOutline(bool forceUpdate)
	{
		playerDistCheck = Vector3.Distance(enemyRootTransform.position, player.transform.position) > oman.simplifiedDistance;
		if (!enemyScriptHandlesEnrage)
		{
			if (oman.simplifyEnemies && hasSimplifiedMaterial && playerDistCheck && !oman.outlinesOnly)
			{
				currentState = ((!enraged || !hasEnragedSimplified) ? MaterialState.simplified : MaterialState.enragedSimplified);
			}
			else
			{
				currentState = (enraged ? MaterialState.enraged : MaterialState.normal);
			}
		}
		bool flag = currentState != lastState;
		bool flag2 = false;
		bool flag3 = false;
		if ((bool)eid && !eid.dead)
		{
			flag3 = eid.blessed;
			if (eid.enemyType != EnemyType.Stalker && eid.sandified)
			{
				flag2 = true;
			}
		}
		if (flag2 != lastSandified || flag3 != lastBlessed)
		{
			flag = true;
		}
		lastSandified = flag2;
		lastBlessed = flag3;
		if (forceUpdate)
		{
			flag = true;
		}
		if (flag && !enemyScriptHandlesEnrage)
		{
			matList[0] = materialDict[currentState];
			if (simplifiedMaterial2 != null)
			{
				matList[1] = ((currentState == MaterialState.normal) ? originalMaterial2 : simplifiedMaterial2);
			}
			if (simplifiedMaterial3 != null)
			{
				matList[2] = ((currentState == MaterialState.normal) ? originalMaterial3 : simplifiedMaterial3);
			}
			meshrenderer.materials = matList;
		}
		shouldBeOutlined = ((oman.simplifyEnemies && playerDistCheck) ? 1 : 0);
		bool flag4 = shouldBeOutlined != lastOutlineState;
		if (forceUpdate)
		{
			Material[] array = matList;
			foreach (Material obj in array)
			{
				obj.SetFloat("_BlendOp1", 0f);
				obj.SetFloat("_SrcBlend1", 1f);
				obj.SetFloat("_DstBlend1", 0f);
				obj.SetFloat("_ForceOutlineBehind", 0f);
			}
		}
		if (flag4 || flag)
		{
			meshrenderer.GetPropertyBlock(propBlock);
			propBlock.SetFloat(HasSandBuff, flag2 ? 1 : 0);
			propBlock.SetFloat(NewSanded, flag2 ? 1 : 0);
			propBlock.SetFloat(Blessed, flag3 ? 1 : 0);
			propBlock.SetFloat(Outline, shouldBeOutlined);
			propBlock.SetFloat(ForceOutline, 0.5f);
			meshrenderer.SetPropertyBlock(propBlock);
		}
		lastState = currentState;
		lastOutlineState = shouldBeOutlined;
		if ((bool)eid && (eid.damageBuff || eid.speedBuff || eid.healthBuff || OptionsManager.forceRadiance))
		{
			if (radianceEffect == null)
			{
				radianceEffect = meshrenderer.gameObject.AddComponent<DoubleRender>();
				radianceEffect.subMeshesToIgnore = radiantSubmeshesToIgnore;
			}
			radianceEffect.SetOutline((oman.simplifyEnemies && playerDistCheck) ? 1 : 0);
		}
		else if ((bool)radianceEffect)
		{
			radianceEffect.RemoveEffect();
		}
	}

	public void UpdateColors()
	{
		if (!ignoreCustomColor)
		{
			if (hasSimplifiedMaterial)
			{
				simplifiedMaterial.color = MonoSingleton<ColorBlindSettings>.Instance.GetEnemyColor(enemyColorType);
			}
			if ((bool)enragedSimplifiedMaterial)
			{
				enragedSimplifiedMaterial.color = MonoSingleton<ColorBlindSettings>.Instance.enrageColor;
			}
		}
	}

	public void Begone()
	{
		active = false;
		if ((bool)meshrenderer)
		{
			matList[0] = originalMaterial;
			if (simplifiedMaterial2 != null)
			{
				matList[1] = originalMaterial2;
			}
			if (simplifiedMaterial3 != null)
			{
				matList[2] = originalMaterial3;
			}
			meshrenderer.materials = matList;
			propBlock.SetFloat("_Outline", 0f);
			propBlock.SetFloat("_ForceOutline", 0f);
			propBlock.SetFloat(HasSandBuff, 0f);
			propBlock.SetFloat(NewSanded, 0f);
			propBlock.SetFloat(Blessed, 0f);
			meshrenderer.SetPropertyBlock(propBlock);
		}
		if ((bool)radianceEffect)
		{
			radianceEffect.RemoveEffect();
		}
		Object.Destroy(this);
	}

	public void ChangeTexture(MaterialState stateToTarget, Texture newTexture)
	{
		materialDict[stateToTarget].SetTexture("_MainTex", newTexture);
	}

	public void ChangeMaterialNew(MaterialState stateToTarget, Material newMaterial)
	{
		materialDict[stateToTarget] = newMaterial;
		SetOutline(forceUpdate: true);
	}

	public void ChangeMaterial(Material oldMaterial, Material newMaterial)
	{
		bool flag = false;
		if (currentMaterial == oldMaterial)
		{
			flag = true;
		}
		newMaterial = new Material(newMaterial);
		if (oldMaterial == originalMaterial)
		{
			originalMaterial = newMaterial;
		}
		else if (oldMaterial == simplifiedMaterial)
		{
			simplifiedMaterial = newMaterial;
		}
		else if (oldMaterial == enragedMaterial)
		{
			enragedMaterial = newMaterial;
		}
		else if (oldMaterial == enragedSimplifiedMaterial)
		{
			enragedSimplifiedMaterial = newMaterial;
		}
		if (flag)
		{
			meshrenderer.material = newMaterial;
			currentMaterial = newMaterial;
		}
	}
}
