using System;
using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;

public class BurningVoxel : MonoBehaviour
{
	private const float BurnTime = 6f;

	private const float ExtinguishTime = 2f;

	private const float KeepPlayerDamageForFraction = 0.5f;

	private const float KeepDamageForFraction = 0.85f;

	[SerializeField]
	[HideInInspector]
	private VoxelProxy proxy;

	private HurtCooldownCollection hurtCooldownCollection;

	[SerializeField]
	[HideInInspector]
	private FireZone fireZone;

	[SerializeField]
	[HideInInspector]
	private Transform fireParticles;

	[SerializeField]
	[HideInInspector]
	private TimeSince timeSinceInitialized;

	private TimeSince? timeSinceStartedExtinguishing;

	public void Initialize(VoxelProxy proxy)
	{
		this.proxy = proxy;
		hurtCooldownCollection = MonoSingleton<StainVoxelManager>.Instance.SharedHurtCooldownCollection;
		GameObject fire = MonoSingleton<FireObjectPool>.Instance.GetFire(isSimple: true);
		fire.transform.SetParent(base.transform, worldPositionStays: false);
		fire.transform.localPosition = new Vector3(0f, 2.0625f, 0f);
		fire.transform.localRotation = Quaternion.identity;
		fireParticles = fire.transform;
		timeSinceInitialized = 0f;
		StartCoroutine(BurningCoroutine());
	}

	private void OnEnable()
	{
		if (!(proxy == null))
		{
			if (!timeSinceStartedExtinguishing.HasValue && (float)timeSinceInitialized > 6f)
			{
				timeSinceStartedExtinguishing = (float)timeSinceInitialized - 6f;
			}
			if (hurtCooldownCollection == null)
			{
				hurtCooldownCollection = MonoSingleton<StainVoxelManager>.Instance.SharedHurtCooldownCollection;
			}
			StopAllCoroutines();
			StartCoroutine(BurningCoroutine());
		}
	}

	public void Refuel()
	{
		StopAllCoroutines();
		timeSinceStartedExtinguishing = null;
		timeSinceInitialized = 0f;
		StartCoroutine(BurningCoroutine());
	}

	private void Remove()
	{
		StopAllCoroutines();
		ReturnFire();
		MonoSingleton<StainVoxelManager>.Instance.DoneBurning(proxy);
	}

	private void ReturnFire()
	{
		if (fireParticles != null && MonoSingleton<FireObjectPool>.Instance != null)
		{
			MonoSingleton<FireObjectPool>.Instance.ReturnFire(fireParticles.gameObject, isSimple: true);
		}
	}

	private IEnumerator BurningCoroutine()
	{
		if (!timeSinceStartedExtinguishing.HasValue)
		{
			if (fireZone == null)
			{
				fireZone = base.gameObject.AddComponent<FireZone>();
				fireZone.source = FlameSource.Napalm;
				fireZone.HurtCooldownCollection = hurtCooldownCollection;
				fireZone.playerDamage = 10;
				BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
				boxCollider.isTrigger = true;
				Vector3 size = new Vector3(2.75f, 5.5f, 2.75f);
				boxCollider.size = size;
				boxCollider.center = new Vector3(0f, 0.5f, 0f);
			}
			SetSize(1f);
			yield return new WaitForSeconds(Mathf.Max(0f, 6f - (float)timeSinceInitialized));
			while (NoWeaponCooldown.NoCooldown)
			{
				yield return null;
			}
			timeSinceStartedExtinguishing = 0f;
		}
		if (!timeSinceStartedExtinguishing.HasValue)
		{
			throw new Exception("timeSinceStartedExtinguishing is null. It shouldn't be.");
		}
		while ((float?)timeSinceStartedExtinguishing < 2f)
		{
			SetSize(1f - (float)timeSinceStartedExtinguishing.Value / 2f);
			if (fireZone.canHurtPlayer && (float?)timeSinceStartedExtinguishing / 2f > 0.5f)
			{
				fireZone.canHurtPlayer = false;
			}
			if (fireZone != null && (float?)timeSinceStartedExtinguishing / 2f > 0.85f)
			{
				UnityEngine.Object.Destroy(fireZone);
			}
			yield return null;
		}
		Remove();
	}

	private void SetSize(float size)
	{
		fireParticles.localScale = Vector3.one * 4f * size;
		proxy.SetStainSize(size);
	}
}
