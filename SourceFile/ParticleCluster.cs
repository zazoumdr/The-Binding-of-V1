using UnityEngine;

public class ParticleCluster : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particles;

	private EmissionModule[] emissionModules;

	private void Awake()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		emissionModules = (EmissionModule[])(object)new EmissionModule[particles.Length];
		for (int i = 0; i < particles.Length; i++)
		{
			emissionModules[i] = particles[i].emission;
		}
	}

	public void EmissionOn()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			((EmissionModule)(ref emissionModules[i])).enabled = true;
		}
	}

	public void EmissionOff()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			((EmissionModule)(ref emissionModules[i])).enabled = false;
		}
	}
}
