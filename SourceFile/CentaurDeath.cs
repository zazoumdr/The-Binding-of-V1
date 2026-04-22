using System.Collections.Generic;
using UnityEngine;

public class CentaurDeath : MonoBehaviour
{
	public float forceStrength = 100f;

	public AnimationCurve timeCurve;

	public Texture burningTex;

	private MeshRenderer[] mRends;

	private List<Material> allMaterials = new List<Material>();

	private MaterialPropertyBlock propBlock;

	private int burnID = Shader.PropertyToID("_BurnTime");

	private int burnFadeID = Shader.PropertyToID("_BurnTimeFade");

	private float realTime;

	private float burnTime;

	private float burnTimeFade = 1f;

	private void Start()
	{
		propBlock = new MaterialPropertyBlock();
		propBlock.SetTexture("_BurningTex", burningTex);
		mRends = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		Bounds bounds = mRends[0].bounds;
		MeshRenderer[] array = mRends;
		foreach (MeshRenderer meshRenderer in array)
		{
			bounds.Encapsulate(meshRenderer.bounds);
		}
		array = mRends;
		foreach (MeshRenderer meshRenderer2 in array)
		{
			allMaterials.AddRange(meshRenderer2.materials);
			Vector4 value = bounds.size;
			propBlock.SetVector("_MeshScale", value);
			meshRenderer2.SetPropertyBlock(propBlock);
		}
		foreach (Material allMaterial in allMaterials)
		{
			allMaterial.EnableKeyword("BURNING");
		}
		burnTimeFade = 1f;
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].AddForceAtPosition(Vector3.one * forceStrength, bounds.center, ForceMode.Impulse);
		}
	}

	private void Update()
	{
		realTime += Time.deltaTime;
		burnTime += Time.deltaTime * 0.05f;
		float value = timeCurve.Evaluate(burnTime);
		if (realTime >= 10f)
		{
			burnTimeFade -= Time.deltaTime * 0.1f;
		}
		for (int i = 0; i < mRends.Length; i++)
		{
			MeshRenderer obj = mRends[i];
			obj.GetPropertyBlock(propBlock);
			propBlock.SetFloat(burnID, value);
			propBlock.SetFloat(burnFadeID, burnTimeFade);
			obj.SetPropertyBlock(propBlock);
		}
	}
}
