using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using UnityEngine;

public class ProjectileSpread : MonoBehaviour
{
	private GameObject projectile;

	public float spreadAmount;

	public int projectileAmount;

	public float timeUntilDestroy;

	public bool parried;

	public bool dontSpawn;

	[Obsolete("This field is obsolete. Use targetHandle instead.")]
	public EnemyTarget target;

	public TargetHandle targetHandle;

	[HideInInspector]
	public List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	private void Start()
	{
		if (!dontSpawn && spreadAmount > 0f)
		{
			Projectile componentInChildren = GetComponentInChildren<Projectile>();
			if (componentInChildren == null)
			{
				Debug.LogError("ProjectileSpread: No Projectile component found in children.");
				return;
			}
			if (targetHandle != null)
			{
				componentInChildren.targetHandle = targetHandle;
			}
			else if (target != null)
			{
				componentInChildren.target = target;
			}
			projectile = componentInChildren.gameObject;
			GameObject gameObject = new GameObject();
			gameObject.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			for (int i = 0; i <= projectileAmount; i++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(projectile, gameObject.transform.position + gameObject.transform.up * 0.1f, gameObject.transform.rotation);
				obj.transform.Rotate(Vector3.right * spreadAmount);
				obj.transform.SetParent(base.transform, worldPositionStays: true);
				gameObject.transform.Rotate(Vector3.forward * (360 / projectileAmount));
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		if (timeUntilDestroy == 0f)
		{
			timeUntilDestroy = 5f;
		}
		Invoke("Remove", timeUntilDestroy);
	}

	public void ParriedProjectile()
	{
		parried = true;
		CancelInvoke("NoLongerParried");
		Invoke("NoLongerParried", 1f);
	}

	private void NoLongerParried()
	{
		parried = false;
	}

	private void Remove()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
