public interface IEnemyWeapon
{
	void UpdateTarget(EnemyTarget target);

	void Fire(bool instantExplode = false);

	void AltFire(bool instantExplode = false);

	void CancelAltCharge();
}
