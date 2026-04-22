using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance | SingletonFlags.PersistAutoInstance | SingletonFlags.DestroyDuplicates)]
public class SceneHelper : MonoSingleton<SceneHelper>
{
	public struct HitSurfaceData(SurfaceType surfaceType = SurfaceType.Generic, Color surfaceColor = default(Color))
	{
		public Material material = null;

		public RaycastHit hit = default(RaycastHit);

		public Mesh mesh = null;

		public bool useSecondaryBlend = false;

		public SurfaceType surfaceType = SurfaceType.Generic;

		public Color particleColor = default(Color);
	}

	public readonly struct PhysicsSceneObjectData
	{
		public Mesh Mesh { get; }

		public Material[] Materials { get; }

		public Vector3 Position { get; }

		public Quaternion Rotation { get; }

		public Vector3 Scale { get; }

		public int Layer { get; }

		public string Name { get; }

		public PhysicsSceneObjectData(Mesh mesh, Material[] materials, Vector3 position, Quaternion rotation, Vector3 scale, int layer, string name)
		{
			Mesh = mesh;
			Materials = materials;
			Position = position;
			Rotation = rotation;
			Scale = scale;
			Layer = layer;
			Name = name;
		}

		public static bool TryCreateFromObject(Transform transform, out PhysicsSceneObjectData data, bool ignorePhysicsChecks = false)
		{
			data = default(PhysicsSceneObjectData);
			MeshRenderer mr = null;
			if (!ignorePhysicsChecks && !IsValidForPhysicsScene(transform, out mr, out var _))
			{
				return false;
			}
			if (ignorePhysicsChecks && !transform.TryGetComponent<MeshRenderer>(out mr))
			{
				return false;
			}
			Mesh mesh = null;
			MeshCollider component2;
			MeshFilter component3;
			if (transform.TryGetComponent<PreservedOriginalMesh>(out var component))
			{
				mesh = component.mesh;
			}
			else if (transform.TryGetComponent<MeshCollider>(out component2))
			{
				mesh = component2.sharedMesh;
			}
			else if (transform.TryGetComponent<MeshFilter>(out component3))
			{
				mesh = component3.sharedMesh;
			}
			if (mesh == null || mr == null)
			{
				return false;
			}
			data = new PhysicsSceneObjectData(mesh, mr.sharedMaterials, transform.position, transform.rotation, transform.lossyScale, transform.gameObject.layer, transform.name);
			return true;
		}
	}

	private static LayerMask? _footstepLayerMask;

	public readonly HashSet<EnemyPersistentData> enemyPersistentData = new HashSet<EnemyPersistentData>();

	private readonly List<Material> reusableMaterials = new List<Material>();

	[SerializeField]
	private AssetReference finalRoomPit;

	[SerializeField]
	private GameObject loadingBlocker;

	[SerializeField]
	private TMP_Text loadingBar;

	[SerializeField]
	private GameObject preloadingBadge;

	[SerializeField]
	private GameObject eventSystem;

	[Space]
	[SerializeField]
	private AudioMixerGroup masterMixer;

	[SerializeField]
	private AudioMixerGroup musicMixer;

	[SerializeField]
	private AudioMixer allSound;

	[SerializeField]
	private AudioMixer goreSound;

	[SerializeField]
	private AudioMixer musicSound;

	[SerializeField]
	private AudioMixer doorSound;

	[SerializeField]
	private AudioMixer unfreezeableSound;

	[Space]
	[SerializeField]
	private EmbeddedSceneInfo embeddedSceneInfo;

	private Scene footstepScene;

	private PhysicsScene footstepPhysicsScene;

	public bool environmentalHitParticles = true;

	private readonly List<Color> reusableColors = new List<Color>();

	private readonly List<int> reusableTriangles = new List<int>();

	private static LayerMask footstepLayerMask
	{
		get
		{
			LayerMask valueOrDefault = _footstepLayerMask.GetValueOrDefault();
			if (!_footstepLayerMask.HasValue)
			{
				valueOrDefault = LayerMask.GetMask("Environment", "Outdoors", "EnvironmentBaked", "OutdoorsBaked");
				_footstepLayerMask = valueOrDefault;
			}
			return _footstepLayerMask.Value;
		}
	}

	public static bool IsPlayingCustom => GameStateManager.Instance.currentCustomGame != null;

	public static bool IsSceneRankless => Enumerable.Contains(MonoSingleton<SceneHelper>.Instance.embeddedSceneInfo.ranklessScenes, CurrentScene);

	public static int CurrentLevelNumber
	{
		get
		{
			if (!IsPlayingCustom)
			{
				return MonoSingleton<StatsManager>.Instance.levelNumber;
			}
			return GameStateManager.Instance.currentCustomGame.levelNumber;
		}
	}

	public static string CurrentScene { get; private set; }

	public static string LastScene { get; private set; }

	public static string PendingScene { get; private set; }

	public bool TryGetSurfaceData(Vector3 pos, out HitSurfaceData hitSurfaceData)
	{
		return TryGetSurfaceData(pos, Vector3.down, 5f, out hitSurfaceData);
	}

	public bool TryGetSurfaceData(Vector3 pos, Vector3 direction, float distance, out HitSurfaceData hitSurfaceData)
	{
		hitSurfaceData = new HitSurfaceData(SurfaceType.Generic, Color.white);
		_ = footstepPhysicsScene;
		if (footstepPhysicsScene.Raycast(pos, direction, out hitSurfaceData.hit, distance, footstepLayerMask, QueryTriggerInteraction.Ignore))
		{
			return TryGetRaycastHitSurfaceData(ref hitSurfaceData);
		}
		return false;
	}

	private bool TryGetRaycastHitSurfaceData(ref HitSurfaceData hitSurfaceData)
	{
		hitSurfaceData.surfaceType = SurfaceType.Generic;
		if (ResolveHitSurfaceData(ref hitSurfaceData))
		{
			ResolveSurfaceType(ref hitSurfaceData);
			return true;
		}
		return false;
	}

	private bool ResolveHitSurfaceData(ref HitSurfaceData hitSurfaceData)
	{
		hitSurfaceData.material = null;
		hitSurfaceData.mesh = null;
		hitSurfaceData.useSecondaryBlend = false;
		if (hitSurfaceData.hit.triangleIndex == -1)
		{
			return false;
		}
		Collider collider = hitSurfaceData.hit.collider;
		if (!collider || !collider.TryGetComponent<MeshRenderer>(out var component))
		{
			return false;
		}
		hitSurfaceData.mesh = ((MeshCollider)collider).sharedMesh;
		component.GetSharedMaterials(reusableMaterials);
		Material material = reusableMaterials[0];
		int num = hitSurfaceData.hit.triangleIndex * 3;
		int submesh = 0;
		int num2 = 0;
		if (hitSurfaceData.mesh.subMeshCount <= 1 || material.IsKeywordEnabled("STATIONARY_LIGHTING") || material.IsKeywordEnabled("STATIC_LIGHTING"))
		{
			hitSurfaceData.material = material;
		}
		else
		{
			int num3 = 0;
			for (int i = component.subMeshStartIndex; i < hitSurfaceData.mesh.subMeshCount; i++)
			{
				int num4 = num3 + hitSurfaceData.mesh.GetSubMesh(i).indexCount;
				if (num < num4)
				{
					submesh = i;
					num2 = num3;
					hitSurfaceData.material = reusableMaterials[i - component.subMeshStartIndex];
					reusableMaterials.Clear();
					break;
				}
				num3 = num4;
			}
		}
		if (hitSurfaceData.material != null && hitSurfaceData.material.IsKeywordEnabled("VERTEX_BLENDING"))
		{
			reusableColors.Clear();
			hitSurfaceData.mesh.GetColors(reusableColors);
			if (reusableColors.Count > 0)
			{
				reusableTriangles.Clear();
				hitSurfaceData.mesh.GetTriangles(reusableTriangles, submesh);
				int num5 = num - num2;
				float r = reusableColors[reusableTriangles[num5]].r;
				float r2 = reusableColors[reusableTriangles[num5 + 1]].r;
				float r3 = reusableColors[reusableTriangles[num5 + 2]].r;
				Vector3 barycentricCoordinate = hitSurfaceData.hit.barycentricCoordinate;
				float num6 = r * barycentricCoordinate.x + r2 * barycentricCoordinate.y + r3 * barycentricCoordinate.z;
				hitSurfaceData.useSecondaryBlend = num6 < 0.5f;
			}
			else
			{
				Debug.LogWarning("Material uses vertex blending but no vertex colors were found!", component.gameObject);
			}
		}
		return hitSurfaceData.material != null;
	}

	public void ResolveSurfaceType(ref HitSurfaceData hitSurfaceData)
	{
		if (hitSurfaceData.material.IsKeywordEnabled("STATIONARY_LIGHTING") || hitSurfaceData.material.IsKeywordEnabled("STATIC_LIGHTING"))
		{
			Debug.Log("TODO for Victoria");
			int num = hitSurfaceData.hit.triangleIndex * 3;
			int num2 = hitSurfaceData.mesh.triangles[num];
			_ = (Vector4)hitSurfaceData.mesh.uv7[num2];
		}
		if (hitSurfaceData.material.HasProperty(ShaderProperties.SurfaceType))
		{
			if (hitSurfaceData.useSecondaryBlend)
			{
				hitSurfaceData.surfaceType = (SurfaceType)Mathf.RoundToInt(hitSurfaceData.material.GetFloat(ShaderProperties.SecondarySurfaceType));
				hitSurfaceData.particleColor = hitSurfaceData.material.GetColor(ShaderProperties.SecondaryEnviroParticleColor);
			}
			else
			{
				hitSurfaceData.surfaceType = (SurfaceType)Mathf.RoundToInt(hitSurfaceData.material.GetFloat(ShaderProperties.SurfaceType));
				hitSurfaceData.particleColor = hitSurfaceData.material.GetColor(ShaderProperties.EnviroParticleColor);
			}
		}
	}

	public void CreateEnviroGibs(RaycastHit hit, int gibAmount = 3, float sizeMultiplier = 1f)
	{
		CreateEnviroGibs(hit.point + hit.normal, -hit.normal, 5f, gibAmount, sizeMultiplier);
	}

	public void CreateEnviroGibs(PhysicsCastResult hit, int gibAmount = 3, float sizeMultiplier = 1f)
	{
		CreateEnviroGibs(hit.point + hit.normal, -hit.normal, 5f, gibAmount, sizeMultiplier);
	}

	public void CreateEnviroGibs(ContactPoint hit, int gibAmount = 3, float sizeMultiplier = 1f)
	{
		CreateEnviroGibs(hit.point + hit.normal, -hit.normal, 5f, gibAmount, sizeMultiplier);
	}

	public void CreateEnviroGibs(Vector3 position, Vector3 direction, float distance, int gibAmount = 3, float sizeMultiplier = 1f)
	{
		if (sizeMultiplier <= 0f || !environmentalHitParticles || !TryGetSurfaceData(position, direction, distance, out var hitSurfaceData))
		{
			return;
		}
		SurfaceType surfaceType = hitSurfaceData.surfaceType;
		FootstepSet footstepSet = MonoSingleton<DefaultReferenceManager>.Instance.footstepSet;
		RaycastHit hit = hitSurfaceData.hit;
		if (gibAmount > 0 && footstepSet.TryGetEnviroGibs(surfaceType, out var enviroGibs) && enviroGibs != null && enviroGibs.Length != 0)
		{
			for (int i = 0; i < gibAmount; i++)
			{
				AsyncInstantiateOperation<GameObject> asyncOp = UnityEngine.Object.InstantiateAsync(enviroGibs[UnityEngine.Random.Range(0, enviroGibs.Length)], hit.point + hit.normal, UnityEngine.Random.rotation);
				asyncOp.completed += delegate
				{
					GameObject gameObject = asyncOp.Result[0];
					gameObject.transform.localScale *= sizeMultiplier;
					Renderer component2;
					if (gameObject.TryGetComponent<MeshRenderer>(out var component))
					{
						component.material = hitSurfaceData.material;
						if (hitSurfaceData.useSecondaryBlend)
						{
							component.material.SetFloat("_ForceSecondary", 1f);
						}
					}
					else if (gameObject.TryGetComponent<Renderer>(out component2))
					{
						component2.material.color = hitSurfaceData.particleColor;
					}
					if (gameObject.TryGetComponent<Rigidbody>(out var component3))
					{
						float num = 1.5f;
						if (i >= 3)
						{
							num /= (float)(i / 2);
						}
						Vector3 vector = hit.normal + new Vector3(UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num), UnityEngine.Random.Range(0f - num, num));
						component3.AddForce(vector.normalized * (1f + (sizeMultiplier - 1f) / 2f) * 30f, ForceMode.Impulse);
					}
				};
			}
		}
		if (!footstepSet.TryGetEnviroGibParticle(surfaceType, out var particle))
		{
			return;
		}
		AsyncInstantiateOperation<GameObject> asyncOp2 = UnityEngine.Object.InstantiateAsync(particle, hit.point, Quaternion.LookRotation(hit.normal));
		asyncOp2.completed += delegate
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			GameObject gameObject = asyncOp2.Result[0];
			EnviroGibModifier[] componentsInChildren = gameObject.GetComponentsInChildren<EnviroGibModifier>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (componentsInChildren[j] != null && componentsInChildren[j].increaseBurstEmission && componentsInChildren[j].TryGetComponent<ParticleSystem>(out var component))
					{
						EmissionModule emission = component.emission;
						if (((EmissionModule)(ref emission)).burstCount > 0)
						{
							componentsInChildren[j].transform.localScale /= sizeMultiplier;
							MainModule main = component.main;
							((MainModule)(ref main)).startSpeed = MultiplyCurve(((MainModule)(ref main)).startSpeed, sizeMultiplier);
							emission = component.emission;
							Burst burst = ((EmissionModule)(ref emission)).GetBurst(0);
							MinMaxCurve count = MultiplyCurve(((Burst)(ref burst)).count, sizeMultiplier);
							((Burst)(ref burst)).count = count;
							emission = component.emission;
							((EmissionModule)(ref emission)).SetBurst(0, burst);
						}
					}
				}
			}
			gameObject.transform.localScale *= sizeMultiplier;
			SetParticlesColors(componentsInChildren, ref hitSurfaceData);
		};
	}

	public void SetParticlesColors(GameObject target, ref HitSurfaceData hitSurfaceData)
	{
		SetParticlesColors(target.GetComponentsInChildren<EnviroGibModifier>(), ref hitSurfaceData);
	}

	public void SetParticlesColors(EnviroGibModifier[] modifiers, ref HitSurfaceData hitSurfaceData)
	{
		SetParticlesColors(modifiers, hitSurfaceData.particleColor);
	}

	public void SetParticlesColors(EnviroGibModifier[] modifiers, Color clr)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!(clr != Color.white))
		{
			return;
		}
		foreach (EnviroGibModifier enviroGibModifier in modifiers)
		{
			if ((bool)(UnityEngine.Object)(object)enviroGibModifier.particleSystem)
			{
				MainModule main = enviroGibModifier.particleSystem.main;
				MinMaxGradient startColor = ((MainModule)(ref main)).startColor;
				clr.a = ((MinMaxGradient)(ref startColor)).color.a;
				((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(clr);
			}
			if (enviroGibModifier.spriteRenderer != null)
			{
				clr.a = enviroGibModifier.spriteRenderer.color.a;
				enviroGibModifier.spriteRenderer.color = clr;
			}
		}
	}

	private MinMaxCurve MultiplyCurve(MinMaxCurve curve, float multiplier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((int)((MinMaxCurve)(ref curve)).mode == 1 || (int)((MinMaxCurve)(ref curve)).mode == 2)
		{
			((MinMaxCurve)(ref curve)).curveMultiplier = ((MinMaxCurve)(ref curve)).curveMultiplier * multiplier;
		}
		else if ((int)((MinMaxCurve)(ref curve)).mode == 3)
		{
			((MinMaxCurve)(ref curve)).constantMin = ((MinMaxCurve)(ref curve)).constantMin * multiplier;
			((MinMaxCurve)(ref curve)).constantMax = ((MinMaxCurve)(ref curve)).constantMax * multiplier;
		}
		else if ((int)((MinMaxCurve)(ref curve)).mode == 0)
		{
			((MinMaxCurve)(ref curve)).constant = ((MinMaxCurve)(ref curve)).constant * multiplier;
		}
		return curve;
	}

	private void OnEnable()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		if (string.IsNullOrEmpty(CurrentScene))
		{
			CurrentScene = SceneManager.GetActiveScene().name;
		}
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
		environmentalHitParticles = !MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableHitParticles");
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "disableHitParticles")
		{
			environmentalHitParticles = !(bool)value;
		}
	}

	private void OnDisable()
	{
		ClearPersistentEnemyRuntimeData();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	public bool IsSceneSpecial(string sceneName)
	{
		sceneName = SanitizeLevelPath(sceneName);
		if (embeddedSceneInfo == null)
		{
			return false;
		}
		return Enumerable.Contains(embeddedSceneInfo.specialScenes, sceneName);
	}

	public static bool IsStaticEnvironment(RaycastHit hit)
	{
		return IsStaticEnvironment(hit.collider);
	}

	public static bool IsStaticEnvironment(PhysicsCastResult hit)
	{
		return IsStaticEnvironment(hit.collider);
	}

	public static bool IsStaticEnvironment(Collider col)
	{
		if (col == null || col.isTrigger)
		{
			return false;
		}
		if (col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic)
		{
			return false;
		}
		if (!col.TryGetComponent<Renderer>(out var component) || !component.enabled)
		{
			return false;
		}
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		return ((1 << col.gameObject.layer) & (int)layerMask) != 0;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		EventSystem val = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
		if ((UnityEngine.Object)(object)val != null)
		{
			UnityEngine.Object.Destroy(((Component)(object)val).gameObject);
		}
		UnityEngine.Object.Instantiate(eventSystem);
		if (mode == LoadSceneMode.Single)
		{
			GameStateManager.Instance.SceneReset();
			SetUpFootstepPhysicsScene(scene);
		}
		ClearPersistentEnemyRuntimeData();
	}

	private void ClearPersistentEnemyRuntimeData()
	{
		foreach (EnemyPersistentData enemyPersistentDatum in enemyPersistentData)
		{
			enemyPersistentDatum.ResetRuntimeData();
		}
		enemyPersistentData.Clear();
	}

	public static bool IsValidForPhysicsScene(Transform transform, out MeshRenderer mr, out Collider col)
	{
		int layer = transform.gameObject.layer;
		mr = null;
		col = null;
		if (((int)footstepLayerMask & (1 << layer)) == 0)
		{
			return false;
		}
		if (!transform.TryGetComponent<Collider>(out col) || col.isTrigger || !transform.TryGetComponent<MeshRenderer>(out mr))
		{
			return false;
		}
		if (transform.TryGetComponent<Rigidbody>(out var component) && !component.isKinematic)
		{
			return false;
		}
		return true;
	}

	private GameObject CreatePhysicsSceneObject(PhysicsSceneObjectData data)
	{
		GameObject obj = new GameObject(data.Name);
		obj.layer = data.Layer;
		obj.transform.SetPositionAndRotation(data.Position, data.Rotation);
		obj.transform.localScale = data.Scale;
		MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = data.Mesh;
		meshCollider.convex = false;
		meshCollider.enabled = true;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.enabled = false;
		meshRenderer.sharedMaterials = data.Materials;
		return obj;
	}

	public GameObject AddToPhysicsScene(PhysicsSceneObjectData data, GameObject sourceObject = null)
	{
		if (!footstepScene.isLoaded)
		{
			Debug.LogError("Footstep scene is not initialized!");
			return null;
		}
		GameObject gameObject = CreatePhysicsSceneObject(data);
		SceneManager.MoveGameObjectToScene(gameObject, footstepScene);
		if (sourceObject == null)
		{
			return gameObject;
		}
		sourceObject.AddComponent<PhysicsSceneStateEnforcer>().SetMatchingObject(gameObject);
		return gameObject;
	}

	public GameObject AddMeshToPhysicsScene(Mesh mesh, Material[] materials, Vector3 position, Quaternion rotation, Vector3 scale, int layer, GameObject sourceObject = null)
	{
		string text = ((sourceObject != null) ? sourceObject.name : "Physics Mesh");
		PhysicsSceneObjectData data = new PhysicsSceneObjectData(mesh, materials, position, rotation, scale, layer, text);
		return AddToPhysicsScene(data, sourceObject);
	}

	private void SetUpFootstepPhysicsScene(Scene scene)
	{
		_ = footstepScene;
		if (footstepScene.isLoaded)
		{
			SceneManager.UnloadSceneAsync(footstepScene);
		}
		footstepScene = SceneManager.CreateScene(scene.name + " - Footsteps", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
		footstepPhysicsScene = footstepScene.GetPhysicsScene();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			Transform[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<Transform>(includeInactive: true);
			DuplicateColliders(componentsInChildren);
		}
	}

	private void DuplicateColliders(Transform[] transforms)
	{
		foreach (Transform transform in transforms)
		{
			if (PhysicsSceneObjectData.TryCreateFromObject(transform, out var data))
			{
				AddToPhysicsScene(data, transform.gameObject);
			}
		}
	}

	public static string SanitizeLevelPath(string scene)
	{
		if (scene.StartsWith("Assets/Scenes/"))
		{
			scene = scene.Substring("Assets/Scenes/".Length);
		}
		if (scene.EndsWith(".unity"))
		{
			scene = scene.Substring(0, scene.Length - ".unity".Length);
		}
		return scene;
	}

	public static void ShowLoadingBlocker()
	{
		MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(value: true);
	}

	public static void DismissBlockers()
	{
		MonoSingleton<SceneHelper>.Instance.loadingBlocker.SetActive(value: false);
		((Component)(object)MonoSingleton<SceneHelper>.Instance.loadingBar).gameObject.SetActive(value: false);
	}

	public static void LoadScene(string sceneName, bool noBlocker = false)
	{
		LoadSceneAsync(sceneName, noBlocker);
	}

	public static Coroutine LoadSceneAsync(string sceneName, bool noBlocker = false)
	{
		return MonoSingleton<SceneHelper>.Instance.StartCoroutine(MonoSingleton<SceneHelper>.Instance.LoadSceneCoroutine(sceneName, noBlocker));
	}

	private IEnumerator LoadSceneCoroutine(string sceneName, bool noSplash = false)
	{
		if (PendingScene != null)
		{
			yield break;
		}
		PendingScene = sceneName;
		Time.timeScale = 0f;
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!(monoBehaviour == null) && !(monoBehaviour.gameObject.scene.name == "DontDestroyOnLoad"))
			{
				monoBehaviour.CancelInvoke();
				monoBehaviour.enabled = false;
			}
		}
		sceneName = SanitizeLevelPath(sceneName);
		switch (sceneName)
		{
		default:
			_ = sceneName == "Custom Content";
			break;
		case "Main Menu":
		case "Tutorial":
		case "Credits":
		case "Endless":
			break;
		}
		Debug.Log("(LoadSceneAsync) Loading scene " + sceneName);
		loadingBlocker.SetActive(!noSplash);
		yield return null;
		if (CurrentScene != sceneName)
		{
			LastScene = CurrentScene;
		}
		CurrentScene = sceneName;
		if (MonoSingleton<MapVarManager>.Instance != null)
		{
			MonoSingleton<MapVarManager>.Instance.ReloadMapVars();
		}
		yield return Addressables.LoadSceneAsync((object)sceneName, LoadSceneMode.Single, true, 100);
		if ((bool)GameStateManager.Instance)
		{
			GameStateManager.Instance.currentCustomGame = null;
		}
		if ((bool)preloadingBadge)
		{
			preloadingBadge.SetActive(value: false);
		}
		if ((bool)loadingBlocker)
		{
			loadingBlocker.SetActive(value: false);
		}
		if ((bool)(UnityEngine.Object)(object)loadingBar)
		{
			((Component)(object)loadingBar).gameObject.SetActive(value: false);
		}
		PendingScene = null;
		if ((bool)MonoSingleton<AssistController>.Instance && (bool)MonoSingleton<TimeController>.Instance)
		{
			MonoSingleton<AssistController>.Instance.InitializeValues();
			MonoSingleton<TimeController>.Instance.InitializeValues();
		}
	}

	public static void RestartScene()
	{
		RestartSceneAsync();
	}

	public static Coroutine RestartSceneAsync()
	{
		if (string.IsNullOrEmpty(CurrentScene))
		{
			CurrentScene = SceneManager.GetActiveScene().name;
		}
		return LoadSceneAsync(CurrentScene);
	}

	public static void LoadPreviousScene()
	{
		string text = LastScene;
		if (string.IsNullOrEmpty(text))
		{
			text = "Main Menu";
		}
		LoadScene(text);
	}

	public static void SpawnFinalPitAndFinish()
	{
		FinalRoom finalRoom = UnityEngine.Object.FindObjectOfType<FinalRoom>();
		if (finalRoom != null)
		{
			if ((bool)finalRoom.doorOpener)
			{
				finalRoom.doorOpener.SetActive(value: true);
			}
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
		else
		{
			GameObject obj = UnityEngine.Object.Instantiate(AssetHelper.LoadPrefab(MonoSingleton<SceneHelper>.Instance.finalRoomPit));
			finalRoom = obj.GetComponent<FinalRoom>();
			obj.transform.position = new Vector3(50000f, -1000f, 50000f);
			MonoSingleton<NewMovement>.Instance.transform.position = finalRoom.dropPoint.position;
		}
	}

	public static void SetLoadingSubtext(string text)
	{
		if ((bool)MonoSingleton<SceneHelper>.Instance.loadingBlocker)
		{
			((Component)(object)MonoSingleton<SceneHelper>.Instance.loadingBar).gameObject.SetActive(value: true);
			MonoSingleton<SceneHelper>.Instance.loadingBar.text = text;
		}
	}

	public int? GetLevelIndexAfterIntermission(string intermissionScene)
	{
		if (embeddedSceneInfo == null)
		{
			return null;
		}
		IntermissionRelation[] intermissions = embeddedSceneInfo.intermissions;
		for (int i = 0; i < intermissions.Length; i++)
		{
			IntermissionRelation intermissionRelation = intermissions[i];
			if (intermissionRelation.intermissionScene == intermissionScene)
			{
				return intermissionRelation.nextLevelIndex;
			}
		}
		return null;
	}
}
