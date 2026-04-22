namespace ULTRAKILL.Portal;

public static class PortalDirectionExtensions
{
	public static PortalSide GetEntrySide(this PortalDirection direction)
	{
		if (direction != PortalDirection.EnterToExit)
		{
			return PortalSide.Exit;
		}
		return PortalSide.Enter;
	}

	public static PortalSide GetExitSide(this PortalDirection direction)
	{
		if (direction != PortalDirection.EnterToExit)
		{
			return PortalSide.Enter;
		}
		return PortalSide.Exit;
	}

	public static PortalDirection Reverse(this PortalDirection direction)
	{
		if (direction != PortalDirection.EnterToExit)
		{
			return PortalDirection.EnterToExit;
		}
		return PortalDirection.ExitToEnter;
	}
}
