using UnityEngine;

namespace ULTRAKILL.Cheats;

public class SpreadGasoline : ICheat
{
	private bool active;

	private GameObject asset;

	public string LongName => "Spread Gasoline";

	public string Identifier => "ultrakill.debug.spread-gasoline";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride => "Spawn Gasoline Projectiles";

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		if (asset == null)
		{
			GameObject gameObject = AssetHelper.LoadPrefab("Assets/Prefabs/Attacks and Projectiles/GasolineProjectile.prefab");
			if (gameObject == null || gameObject.Equals(null))
			{
				Debug.LogWarning("Failed to load projectile asset.\nRuntime key: Assets/Prefabs/Attacks and Projectiles/GasolineProjectile.prefab");
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to load gasoline projectile asset");
				return;
			}
			asset = gameObject;
		}
		SpawnProjectiles(asset);
	}

	private void SpawnProjectiles(GameObject projectilePrefab)
	{
		Vector3 position = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + Vector3.up * 4f;
		int num = 256;
		for (int i = 0; i < num; i++)
		{
			Quaternion quaternion = Random.rotation;
			if (quaternion.eulerAngles.x < 180f)
			{
				quaternion = Quaternion.Euler(180f, quaternion.eulerAngles.y, quaternion.eulerAngles.z);
			}
			GameObject gameObject = Object.Instantiate(projectilePrefab, position, quaternion);
			gameObject.SetActive(value: true);
			if (gameObject.TryGetComponent<Rigidbody>(out var component))
			{
				component.AddForce(quaternion * Vector3.forward * Random.Range(10f, 100f), ForceMode.Impulse);
			}
		}
	}

	public void Disable()
	{
	}
}
