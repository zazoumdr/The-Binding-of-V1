using UnityEngine;

public class SummonedSwords : MonoBehaviour
{
	public EnemyTarget target;

	private bool inFormation;

	private SummonedSwordFormation formation;

	private Projectile[] swords;

	public float speed = 1f;

	[HideInInspector]
	public EnemyTarget targetEnemy;

	private int difficulty;

	private bool spinning;

	private void Start()
	{
		swords = GetComponentsInChildren<Projectile>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		Invoke("Begin", 5f * speed);
	}

	private void FixedUpdate()
	{
		if (!inFormation)
		{
			if (target != null)
			{
				base.transform.position = target.position;
			}
			base.transform.Rotate(Vector3.up, Time.deltaTime * 720f * speed, Space.Self);
		}
		else if (formation == SummonedSwordFormation.Spiral)
		{
			base.transform.position = target.position;
			if (spinning)
			{
				base.transform.Rotate(Vector3.up, Time.deltaTime * 720f * speed, Space.Self);
			}
		}
	}

	private void Begin()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Projectile[] array;
		if (difficulty > 3)
		{
			inFormation = true;
			target = targetEnemy;
			base.transform.position = target.position;
			array = swords;
			foreach (Projectile projectile in array)
			{
				if ((bool)projectile)
				{
					if (projectile.TryGetComponent<Collider>(out var component))
					{
						component.enabled = false;
					}
					projectile.target = target;
					projectile.transform.localPosition += projectile.transform.forward * 5f;
					projectile.transform.Rotate(Vector3.up, 180f, Space.World);
				}
			}
			spinning = true;
			Invoke("StopSpinning", 0.75f * speed);
			Invoke("SpiralStab", 1f * speed);
			return;
		}
		array = swords;
		foreach (Projectile projectile2 in array)
		{
			if ((bool)projectile2)
			{
				Object.Instantiate(projectile2.explosionEffect, projectile2.transform.position, Quaternion.identity);
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void SpiralStab()
	{
		Projectile[] array = swords;
		foreach (Projectile projectile in array)
		{
			if ((bool)projectile)
			{
				if (projectile.TryGetComponent<Collider>(out var component))
				{
					component.enabled = true;
				}
				projectile.ignoreEnvironment = false;
				projectile.speed = 150f;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void StopSpinning()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		spinning = false;
		Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, base.transform.position, Quaternion.identity);
		Projectile[] array = swords;
		foreach (Projectile projectile in array)
		{
			if ((bool)projectile)
			{
				Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, projectile.transform.position, projectile.transform.rotation).transform.localScale *= 10f;
				projectile.transform.SetParent(base.transform.parent, worldPositionStays: true);
				projectile.unparryable = false;
				projectile.undeflectable = false;
			}
		}
	}
}
