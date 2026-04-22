using UnityEngine;
using UnityEngine.AddressableAssets;

public class BreakChunks : MonoBehaviour
{
	public AssetReference[] chunks;

	public bool getEnviroMaterial;

	public Vector3 getDirection = Vector3.down;

	public bool relativeDirection = true;

	private void Start()
	{
		if (chunks.Length == 0)
		{
			return;
		}
		GoreZone componentInParent = base.transform.GetComponentInParent<GoreZone>();
		Material mat = null;
		bool flag = getEnviroMaterial && GetMaterial(out mat);
		GameObject[] array = chunks.ToAssets();
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(array[i], base.transform.position, Random.rotation);
			Vector3 force = new Vector3(Random.Range(-45, 45), Random.Range(-45, 45), Random.Range(-45, 45));
			gameObject.GetComponent<Rigidbody>()?.AddForce(force, ForceMode.VelocityChange);
			if (componentInParent != null)
			{
				gameObject.transform.SetParent(componentInParent.gibZone);
			}
			if (flag && gameObject.TryGetComponent<MeshRenderer>(out var component))
			{
				component.material = mat;
			}
		}
	}

	private bool GetMaterial(out Material mat)
	{
		Vector3 vector = getDirection.normalized;
		if (relativeDirection)
		{
			vector = base.transform.InverseTransformDirection(vector);
		}
		Debug.Log("May need upgrading for blended surfaces", base.gameObject);
		if (!MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position - vector, vector, 5f, out var hitSurfaceData))
		{
			mat = hitSurfaceData.material;
			return false;
		}
		mat = hitSurfaceData.material;
		return true;
	}
}
