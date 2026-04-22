using UnityEngine;

public class EnviroMaterialGetter : MonoBehaviour
{
	public bool oneTime = true;

	[HideInInspector]
	public Vector3 previousActivationPosition = Vector3.one * -9999f;

	public Vector3 getRayDirection = Vector3.down;

	public bool relative = true;

	[Space]
	public MeshRenderer[] targets;

	private void Start()
	{
		if (oneTime)
		{
			Activate();
		}
	}

	private void OnEnable()
	{
		if (!oneTime)
		{
			Activate();
		}
	}

	private void Activate()
	{
		if (Vector3.Distance(base.transform.position, previousActivationPosition) < 1f)
		{
			return;
		}
		previousActivationPosition = base.transform.position;
		Vector3 vector = getRayDirection.normalized;
		if (relative)
		{
			vector = base.transform.InverseTransformDirection(vector);
		}
		if (!MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position - vector, vector, 5f, out var hitSurfaceData))
		{
			if (oneTime)
			{
				base.enabled = false;
			}
			return;
		}
		if (targets == null || targets.Length == 0)
		{
			targets = GetComponentsInChildren<MeshRenderer>();
		}
		MeshRenderer[] array = targets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = hitSurfaceData.material;
		}
		if (oneTime)
		{
			base.enabled = false;
		}
	}
}
