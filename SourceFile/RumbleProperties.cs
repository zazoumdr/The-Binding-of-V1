public static class RumbleProperties
{
	public static readonly RumbleKey Slide = new RumbleKey("rumble.slide");

	public static readonly RumbleKey Jump = new RumbleKey("rumble.jump");

	public static readonly RumbleKey FallImpact = new RumbleKey("rumble.fall_impact");

	public static readonly RumbleKey FallImpactHeavy = new RumbleKey("rumble.fall_impact_heave");

	public static readonly RumbleKey Dash = new RumbleKey("rumble.dash");

	public static readonly RumbleKey WhiplashThrow = new RumbleKey("rumble.whiplash.throw");

	public static readonly RumbleKey WhiplashPull = new RumbleKey("rumble.whiplash.pull");

	public static readonly RumbleKey GunFire = new RumbleKey("rumble.gun.fire");

	public static readonly RumbleKey GunFireStrong = new RumbleKey("rumble.gun.fire_strong");

	public static readonly RumbleKey GunFireProjectiles = new RumbleKey("rumble.gun.fire_projectiles");

	public static readonly RumbleKey RailcannonIdle = new RumbleKey("rumble.gun.railcannon_idle");

	public static readonly RumbleKey ShotgunCharge = new RumbleKey("rumble.gun.shotgun_charge");

	public static readonly RumbleKey RevolverCharge = new RumbleKey("rumble.gun.revolver_charge");

	public static readonly RumbleKey NailgunFire = new RumbleKey("rumble.gun.nailgun_fire");

	public static readonly RumbleKey Sawblade = new RumbleKey("rumble.gun.sawblade");

	public static readonly RumbleKey SuperSaw = new RumbleKey("rumble.gun.super_saw");

	public static readonly RumbleKey Magnet = new RumbleKey("rumble.magnet_released");

	public static readonly RumbleKey ParryFlash = new RumbleKey("rumble.parry_flash");

	public static readonly RumbleKey CoinToss = new RumbleKey("rumble.coin_toss");

	public static readonly RumbleKey Punch = new RumbleKey("rumble.punch");

	public static readonly RumbleKey WeaponWheelTick = new RumbleKey("rumble.weapon_wheel_tick");

	public static readonly RumbleKey[] All = new RumbleKey[21]
	{
		Slide, Jump, FallImpact, FallImpactHeavy, Dash, WhiplashThrow, WhiplashPull, GunFire, GunFireStrong, GunFireProjectiles,
		RailcannonIdle, ShotgunCharge, RevolverCharge, NailgunFire, Sawblade, SuperSaw, Magnet, ParryFlash, CoinToss, Punch,
		WeaponWheelTick
	};
}
