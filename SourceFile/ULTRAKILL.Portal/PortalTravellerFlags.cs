using System;

namespace ULTRAKILL.Portal;

[Flags]
public enum PortalTravellerFlags
{
	None = 0,
	Player = 2,
	PlayerProjectile = 4,
	Enemy = 8,
	EnemyProjectile = 0x10,
	Other = 0x20
}
