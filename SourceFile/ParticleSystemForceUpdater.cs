using System;
using UnityEngine;

public class ParticleSystemForceUpdater : MonoBehaviour
{
	private ParticleSystem ps;

	private Particle[] particles;

	private void Awake()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		ps = GetComponent<ParticleSystem>();
		if ((UnityEngine.Object)(object)ps == null)
		{
			throw new NullReferenceException("ParticleSystemForceUpdater has no ParticleSystem component!");
		}
		MainModule main = ps.main;
		particles = (Particle[])(object)new Particle[((MainModule)(ref main)).maxParticles];
	}

	public void ForceUpdate()
	{
		if ((bool)(UnityEngine.Object)(object)ps)
		{
			int num = ps.GetParticles(particles);
			ps.SetParticles(particles, num);
		}
	}
}
