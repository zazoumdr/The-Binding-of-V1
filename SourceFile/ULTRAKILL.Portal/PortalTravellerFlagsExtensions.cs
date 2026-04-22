namespace ULTRAKILL.Portal;

public static class PortalTravellerFlagsExtensions
{
	public static bool HasFlags(this PortalTravellerFlags flags, PortalTravellerFlags otherFlags)
	{
		return flags.HasAllFlags(otherFlags);
	}

	public static bool HasType(this PortalTravellerFlags flags, PortalTravellerType type)
	{
		return flags.HasFlags(type.ToFlags());
	}
}
