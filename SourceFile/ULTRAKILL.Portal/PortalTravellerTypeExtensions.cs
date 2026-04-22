namespace ULTRAKILL.Portal;

public static class PortalTravellerTypeExtensions
{
	public static PortalTravellerFlags ToFlags(this PortalTravellerType type)
	{
		return type switch
		{
			PortalTravellerType.PLAYER => PortalTravellerFlags.Player, 
			PortalTravellerType.PLAYER_PROJECTILE => PortalTravellerFlags.PlayerProjectile, 
			PortalTravellerType.ENEMY => PortalTravellerFlags.Enemy, 
			PortalTravellerType.ENEMY_PROJECTILE => PortalTravellerFlags.EnemyProjectile, 
			PortalTravellerType.OTHER => PortalTravellerFlags.Other, 
			_ => PortalTravellerFlags.None, 
		};
	}
}
