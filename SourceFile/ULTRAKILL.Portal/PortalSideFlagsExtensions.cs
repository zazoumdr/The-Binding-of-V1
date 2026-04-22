namespace ULTRAKILL.Portal;

public static class PortalSideFlagsExtensions
{
	public static bool HasSide(this PortalSideFlags flags, PortalSide side)
	{
		if (side != PortalSide.Enter)
		{
			return flags.HasAllFlags(PortalSideFlags.Exit);
		}
		return flags.HasAllFlags(PortalSideFlags.Enter);
	}
}
